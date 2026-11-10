using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Robot.Utils;
using WeComConsoleServer.Core;

namespace Robot.Config;

internal class WebSocketServer
{
	private static List<WebSocket> _connectedSockets = new List<WebSocket>();

	private static HttpListener _listener;

	private static CancellationTokenSource _cancellationTokenSource;

	private static bool _isRunning = false;

	public static void Start()
	{
		if (_isRunning)
		{
			return;
		}
		_cancellationTokenSource = new CancellationTokenSource();
		new Thread((ThreadStart)delegate
		{
			string uri = "http://*:8061/message/";
			Task.Run(() => StartServer(uri, _cancellationTokenSource.Token)).GetAwaiter().GetResult();
		}).Start();
		_isRunning = true;
	}

	public static void Stop()
	{
		if (!_isRunning)
		{
			return;
		}
		try
		{
			_cancellationTokenSource?.Cancel();
			WebSocket[] array = _connectedSockets.ToArray();
			foreach (WebSocket webSocket in array)
			{
				try
				{
					if (webSocket.State == WebSocketState.Open)
					{
						webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None).Wait(1000);
					}
					webSocket.Dispose();
				}
				catch
				{
				}
			}
			_connectedSockets.Clear();
			_listener?.Stop();
			_listener?.Close();
			_listener = null;
			_isRunning = false;
		}
		catch (Exception ex)
		{
			Logger.Error("停止WebSocket服务器时出错: " + ex.Message);
		}
	}

	private static async Task ProcessMessageAsync(string message)
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

	private static async Task HandleWebSocketAsync(WebSocket webSocket, CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			_ = 1;
			try
			{
				byte[] buffer = new byte[1024];
				_connectedSockets.Add(webSocket);
				while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
				{
					WebSocketReceiveResult webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
					if (webSocketReceiveResult.MessageType == WebSocketMessageType.Text)
					{
						string text = Encoding.UTF8.GetString(buffer, 0, webSocketReceiveResult.Count);
						Console.WriteLine("Received: " + text);
						await ProcessMessageAsync(text);
					}
				}
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception ex2)
			{
				Console.WriteLine("Error: " + ex2.Message);
			}
		}
		finally
		{
			_connectedSockets.Remove(webSocket);
			try
			{
				if (webSocket.State == WebSocketState.Open)
				{
					await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
				}
			}
			catch
			{
			}
			webSocket.Dispose();
		}
	}

	public static async Task StartServer(string uri, CancellationToken cancellationToken = default(CancellationToken))
	{
		_listener = new HttpListener();
		_listener.Prefixes.Add(uri);
		_listener.Start();
		Logger.Info("Web Socket 连接地址: " + uri);
		try
		{
			while (!cancellationToken.IsCancellationRequested && _listener.IsListening)
			{
				HttpListenerContext httpListenerContext = await _listener.GetContextAsync();
				if (httpListenerContext.Request.IsWebSocketRequest)
				{
					WebSocket webSocket = (await httpListenerContext.AcceptWebSocketAsync(null)).WebSocket;
					Logger.Info("WebSocket 新连接已建立.");
					HandleWebSocketAsync(webSocket, cancellationToken);
				}
				else
				{
					httpListenerContext.Response.StatusCode = 400;
					httpListenerContext.Response.Close();
				}
			}
		}
		catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
		{
		}
		catch (ObjectDisposedException)
		{
		}
	}

	public static async Task SendMessageToAllClients(string message)
	{
		byte[] buffer = Encoding.UTF8.GetBytes(message);
		foreach (WebSocket connectedSocket in _connectedSockets)
		{
			if (connectedSocket.State == WebSocketState.Open)
			{
				await connectedSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
			}
		}
	}
}
