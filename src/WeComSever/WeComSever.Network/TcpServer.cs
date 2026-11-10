using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Robot.Utils;
using WeComConsoleServer.Core;

namespace WeComSever.Network;

public class TcpServer
{
	private TcpListener _listener;

	private bool _isRunning;

	private int _port = 9090;

	private List<TcpClient> _clients = new List<TcpClient>();

	private readonly object _clientsLock = new object();

	private CancellationTokenSource _cancellationTokenSource;

	public int Port
	{
		get
		{
			return _port;
		}
		set
		{
			_port = value;
		}
	}

	public event Action ServerStarted;

	public event Action ServerStopped;

	public event Action<string> MessageReceived;

	public event Action<string> ClientConnected;

	public event Action<string> ClientDisconnected;

	public void Start()
	{
		if (_isRunning)
		{
			Logger.Info("TCP服务器已经在运行中");
			return;
		}
		try
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_listener = new TcpListener(IPAddress.Any, _port);
			_listener.Start();
			_isRunning = true;
			this.ServerStarted?.Invoke();
			Task.Run(() => AcceptClientsAsync(_cancellationTokenSource.Token));
		}
		catch (Exception ex)
		{
			Logger.Error("启动TCP服务器失败: " + ex.Message, ex);
			throw;
		}
	}

	public void Stop()
	{
		if (!_isRunning)
		{
			Logger.Info("TCP服务器已经停止");
			return;
		}
		try
		{
			_isRunning = false;
			_cancellationTokenSource?.Cancel();
			try
			{
				_listener?.Stop();
			}
			catch (Exception ex)
			{
				Logger.Error("停止监听器失败: " + ex.Message);
			}
			lock (_clientsLock)
			{
				foreach (TcpClient client in _clients)
				{
					try
					{
						client?.Close();
					}
					catch (Exception ex2)
					{
						Logger.Error("关闭客户端连接失败: " + ex2.Message);
					}
				}
				_clients.Clear();
			}
			Logger.Info("TCP服务器已停止");
			this.ServerStopped?.Invoke();
		}
		catch (Exception ex3)
		{
			Logger.Error("停止TCP服务器失败: " + ex3.Message, ex3);
		}
	}

	private async Task AcceptClientsAsync(CancellationToken cancellationToken)
	{
		try
		{
			while (_isRunning && !cancellationToken.IsCancellationRequested)
			{
				TcpClient client = null;
				try
				{
					client = await _listener.AcceptTcpClientAsync();
					string text = client.Client.RemoteEndPoint.ToString();
					Logger.Info("客户端已连接: " + text);
					this.ClientConnected?.Invoke(text);
					lock (_clientsLock)
					{
						_clients.Add(client);
					}
					HandleClientAsync(client, cancellationToken);
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				catch (Exception ex2)
				{
					Logger.Error("接受客户端连接时出错: " + ex2.Message, ex2);
					client?.Close();
				}
			}
		}
		catch (Exception ex3)
		{
			Logger.Error("AcceptClientsAsync异常: " + ex3.Message, ex3);
		}
	}

	private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
	{
		string clientEndpoint;
		try
		{
			clientEndpoint = client?.Client?.RemoteEndPoint?.ToString() ?? "unknown";
		}
		catch (ObjectDisposedException)
		{
			return;
		}
		byte[] buffer = new byte[4096];
		NetworkStream stream;
		try
		{
			stream = client.GetStream();
		}
		catch (ObjectDisposedException)
		{
			return;
		}
		catch (InvalidOperationException)
		{
			return;
		}
		try
		{
			while (_isRunning && !cancellationToken.IsCancellationRequested && client.Connected)
			{
				int num;
				try
				{
					num = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
				}
				catch (IOException)
				{
					break;
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				if (num > 0)
				{
					string text = Encoding.UTF8.GetString(buffer, 0, num);
					Logger.Info("收到来自 " + clientEndpoint + " 的消息: " + text);
					this.MessageReceived?.Invoke(text);
					await ProcessMessageAsync(client, text, cancellationToken);
					continue;
				}
				break;
			}
		}
		catch (ObjectDisposedException)
		{
		}
		catch (Exception ex8)
		{
			Logger.Error("处理客户端 " + clientEndpoint + " 消息时出错: " + ex8.Message, ex8);
		}
		finally
		{
			try
			{
				client?.Close();
				lock (_clientsLock)
				{
					_clients.Remove(client);
				}
				Logger.Info("客户端断开连接: " + clientEndpoint);
				this.ClientDisconnected?.Invoke(clientEndpoint);
			}
			catch (Exception ex9)
			{
				Logger.Error("关闭客户端连接失败: " + ex9.Message);
			}
		}
	}

	private async Task ProcessMessageAsync(TcpClient client, string message, CancellationToken cancellationToken)
	{
		try
		{
			dynamic val = JsonConvert.DeserializeObject(message);
			if (!((!val.ContainsKey("client")) ? true : false))
			{
				WeComHelper.SendMessage((int)val.client, val.ToString());
			}
		}
		catch (Exception ex)
		{
			Logger.Error("处理消息响应时出错: " + ex.Message, ex);
		}
	}

	public async Task BroadcastAsync(string message)
	{
		if (!_isRunning)
		{
			return;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(message);
		List<TcpClient> list = new List<TcpClient>();
		List<TcpClient> list2;
		lock (_clientsLock)
		{
			list2 = new List<TcpClient>(_clients);
		}
		foreach (TcpClient item in list2)
		{
			try
			{
				if (item != null && item.Connected)
				{
					item.GetStream().Write(bytes, 0, bytes.Length);
				}
				else
				{
					list.Add(item);
				}
			}
			catch (ObjectDisposedException)
			{
				list.Add(item);
			}
			catch (InvalidOperationException)
			{
				list.Add(item);
			}
			catch (Exception ex3)
			{
				Logger.Error("广播消息失败: " + ex3.Message);
				list.Add(item);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		lock (_clientsLock)
		{
			foreach (TcpClient item2 in list)
			{
				_clients.Remove(item2);
			}
		}
	}
}
