using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Robot.Config;
using Robot.Utils;
using WeComSever;

namespace WeComConsoleServer.Core;

public class WeComHelper
{
	private delegate bool WxModuleInitWeChatSocket(AcceptCallbackFunc acceptCallback, RecvCallbackFunc recvCallback, CloseCallbackFunc closeCallback);

	private delegate void AcceptCallbackFunc(int dwClientId);

	private delegate void RecvCallbackFunc(int dwClientId, string message, int dwSize);

	private delegate void CloseCallbackFunc(int dwClientId);

	private delegate void WxModuleInjectWeChatMultiOpen(string dllPath, string weChatExePath);

	private delegate void WxModuleInjectWeChatPid(int pid, string dllPath);

	private delegate bool WxSendMessage(int client, string message);

	private static readonly string WxLoaderDllPath;

	private static readonly string WxSlaveDllPath;

	private static readonly IntPtr ModuleHandle;

	private static readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingRequests;

	private static readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingDownloadRequests;

	private static readonly ConcurrentDictionary<string, SemaphoreSlim> _syncLocks;

	private const int SyncRequestTimeoutMs = 60000;

	private static GCHandle acceptCallbackHandle;

	private static GCHandle recvCallbackHandle;

	private static GCHandle closeCallbackHandle;

	private static bool _socketInitialized;

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
	private static extern IntPtr LoadLibrary(string lpFileName);

	static WeComHelper()
	{
		_pendingRequests = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
		_pendingDownloadRequests = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
		_syncLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
		_socketInitialized = false;
		WxLoaderDllPath = AppDomain.CurrentDomain.BaseDirectory + "Loader.dll";
		WxSlaveDllPath = AppDomain.CurrentDomain.BaseDirectory + "Helper_5.0.0.6008.dll";
		ModuleHandle = LoadLibrary(WxLoaderDllPath);
		InitializeWeChatSocket();
		Process[] processes = Process.GetProcesses();
		int num = 0;
		Process[] array = processes;
		foreach (Process process in array)
		{
			if (process.ProcessName == "WXWork")
			{
				Logger.Info($"开始注入进程：{process.Id}");
				InjectWeChatPid(process.Id);
				num++;
			}
		}
		Logger.Info($"共找到{num}个微信进程,并进行注入");
	}

	~WeComHelper()
	{
		FreeCallbackHandles();
	}

	private static void FreeCallbackHandles()
	{
		if (acceptCallbackHandle.IsAllocated)
		{
			acceptCallbackHandle.Free();
		}
		if (recvCallbackHandle.IsAllocated)
		{
			recvCallbackHandle.Free();
		}
		if (closeCallbackHandle.IsAllocated)
		{
			closeCallbackHandle.Free();
		}
	}

	private static T GetDelegateForFunctionPointer<T>(IntPtr address)
	{
		return (T)(object)Marshal.GetDelegateForFunctionPointer(address, typeof(T));
	}

	private static string EncodeNonAsciiForDll(string input)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		try
		{
			return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(input), new JsonSerializerSettings
			{
				StringEscapeHandling = (StringEscapeHandling)1
			});
		}
		catch
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in input)
			{
				if (c > '\u007f')
				{
					stringBuilder.AppendFormat("\\u{0:X4}", (int)c);
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}
	}

	public static bool SendMessage(int client, string message)
	{
		string message2 = EncodeNonAsciiForDll(message);
		return GetDelegateForFunctionPointer<WxSendMessage>(IntPtr.Add(ModuleHandle, 22528))(client, message2);
	}

	public static string SendMessageSyncByPath(int client, string message, string savePath, int timeoutMs = 60000)
	{
		TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();
		_pendingDownloadRequests[savePath] = taskCompletionSource;
		TaskCompletionSource<string> value;
		try
		{
			string message2 = EncodeNonAsciiForDll(message);
			if (!GetDelegateForFunctionPointer<WxSendMessage>(IntPtr.Add(ModuleHandle, 22528))(client, message2))
			{
				_pendingDownloadRequests.TryRemove(savePath, out value);
				throw new Exception("发送消息失败");
			}
			Logger.Info("下载请求已发送: save_path=" + savePath + ", 等待回调...");
			if (taskCompletionSource.Task.Wait(timeoutMs))
			{
				return taskCompletionSource.Task.Result;
			}
			throw new TimeoutException($"下载请求等待响应超时 ({timeoutMs}ms), save_path={savePath}");
		}
		finally
		{
			_pendingDownloadRequests.TryRemove(savePath, out value);
		}
	}

	public static string SendMessageSyncWithLock(int client, string message, int type, int timeoutMs = 60000)
	{
		string text = $"{client}_{type}";
		SemaphoreSlim orAdd = _syncLocks.GetOrAdd(text, (string _) => new SemaphoreSlim(1, 1));
		if (!orAdd.Wait(timeoutMs))
		{
			throw new TimeoutException($"等待锁超时 ({timeoutMs}ms), lockKey={text}");
		}
		string key = $"{client}_{type}_{DateTime.Now.Ticks}";
		TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();
		_pendingRequests[key] = taskCompletionSource;
		TaskCompletionSource<string> value;
		try
		{
			string message2 = EncodeNonAsciiForDll(message);
			if (!GetDelegateForFunctionPointer<WxSendMessage>(IntPtr.Add(ModuleHandle, 22528))(client, message2))
			{
				_pendingRequests.TryRemove(key, out value);
				throw new Exception("发送消息失败");
			}
			if (taskCompletionSource.Task.Wait(timeoutMs))
			{
				return taskCompletionSource.Task.Result;
			}
			throw new TimeoutException($"等待响应超时 ({timeoutMs}ms)");
		}
		finally
		{
			_pendingRequests.TryRemove(key, out value);
			orAdd.Release();
		}
	}

	private static bool TryCompleteRequest(int clientId, string message, int type, JObject msgObj)
	{
		if (type == 11170 || type == 11171)
		{
			object obj;
			if (msgObj == null)
			{
				obj = null;
			}
			else
			{
				JToken obj2 = msgObj["data"];
				obj = ((obj2 == null) ? null : ((object)obj2[(object)"save_path"])?.ToString());
			}
			string text = (string)obj;
			if (string.IsNullOrEmpty(text))
			{
				string text2 = message;
				if (!string.IsNullOrEmpty(text2) && text2.Length > 500)
				{
					text2 = text2.Substring(0, 500) + "...";
				}
				Logger.Error($"下载回调缺少 save_path: client={clientId}, type={type}, message={text2}");
				return true;
			}
			if (_pendingDownloadRequests.TryRemove(text, out var value))
			{
				value.TrySetResult(message);
				Logger.Info("下载请求回调匹配成功: save_path=" + text);
				return true;
			}
			Logger.Error($"下载回调未匹配到待处理请求: client={clientId}, type={type}, save_path={text}");
			return true;
		}
		foreach (KeyValuePair<string, TaskCompletionSource<string>> pendingRequest in _pendingRequests)
		{
			if (pendingRequest.Key.StartsWith($"{clientId}_{type}_") && _pendingRequests.TryRemove(pendingRequest.Key, out var value2))
			{
				value2.TrySetResult(message);
				return true;
			}
		}
		return false;
	}

	private static void InitializeWeChatSocket()
	{
		if (_socketInitialized)
		{
			Logger.Info("Socket 已初始化，跳过重复初始化");
			return;
		}
		WxModuleInitWeChatSocket delegateForFunctionPointer = GetDelegateForFunctionPointer<WxModuleInitWeChatSocket>(IntPtr.Add(ModuleHandle, 19216));
		AcceptCallbackFunc acceptCallbackFunc = WxAcceptCallback;
		RecvCallbackFunc recvCallbackFunc = WxRecvCallback;
		CloseCallbackFunc closeCallbackFunc = WxCloseCallback;
		acceptCallbackHandle = GCHandle.Alloc(acceptCallbackFunc);
		recvCallbackHandle = GCHandle.Alloc(recvCallbackFunc);
		closeCallbackHandle = GCHandle.Alloc(closeCallbackFunc);
		delegateForFunctionPointer(acceptCallbackFunc, recvCallbackFunc, closeCallbackFunc);
		_socketInitialized = true;
		Logger.Info("DLL Socket 监听已初始化");
	}

	public static void ReInitializeSocket()
	{
		Logger.Info("正在重新初始化 DLL Socket 监听...");
		FreeCallbackHandles();
		_socketInitialized = false;
		InitializeWeChatSocket();
	}

	public static void OpenWechatMutexTwo(string path)
	{
		GetDelegateForFunctionPointer<WxModuleInjectWeChatMultiOpen>(IntPtr.Add(ModuleHandle, 20096))(WxSlaveDllPath, path);
	}

	public static void InjectWeChatPid(int pid)
	{
		GetDelegateForFunctionPointer<WxModuleInjectWeChatPid>(new IntPtr(ModuleHandle.ToInt64() + 20688))(pid, WxSlaveDllPath);
	}

	private static void WxAcceptCallback(int dwClientId)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0026: Expected O, but got Unknown
		var anon = new
		{
			type = 1000,
			data = new
			{
				clientId = dwClientId
			},
			message = "客户端上线"
		};
		JsonSerializerSettings val = new JsonSerializerSettings
		{
			ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver()
		};
		string message = JsonConvert.SerializeObject((object)anon, val);
		Logger.Info(message);
		NotifyStaticMessage(message);
		UpdateUIOnClientConnect(dwClientId);
	}

	private static void UpdateUIOnClientConnect(int clientId)
	{
		try
		{
			(Application.Current?.Dispatcher?.Invoke(() => Application.Current.MainWindow as MainWindow))?.AddWeComClient(clientId.ToString(), $"客户端 {clientId}", "");
		}
		catch (Exception ex)
		{
			Logger.Error("更新UI添加客户端失败: " + ex.Message, ex);
		}
	}

	private static void WxRecvCallback(int dwClient, string message, int dwSize)
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00e0: Expected O, but got Unknown
		JObject val = null;
		int num = 0;
		try
		{
			val = JObject.Parse(message);
			JToken obj = val["type"];
			num = ((obj != null) ? Extensions.Value<int>((IEnumerable<JToken>)obj) : 0);
		}
		catch
		{
		}
		if (TryCompleteRequest(dwClient, message, num, val))
		{
			Logger.Info($"同步请求响应: client={dwClient}, type={num}");
			return;
		}
		if (val != null)
		{
			switch (num)
			{
			case 11026:
				HandleUserLogin(dwClient, val);
				break;
			case 11027:
				HandleUserLogout(dwClient, val);
				break;
			}
		}
		JObject obj3 = JObject.Parse(message);
		obj3["client"] = JToken.op_Implicit(dwClient);
		JToken obj4 = obj3["data"];
		JObject val2 = (JObject)(object)((obj4 is JObject) ? obj4 : null);
		if (val2 != null && val2["receiver"] == null)
		{
			string userIdByClientId = MainWindow.GetUserIdByClientId(dwClient);
			if (!string.IsNullOrEmpty(userIdByClientId))
			{
				val2["receiver"] = JToken.op_Implicit(userIdByClientId);
			}
		}
		JsonSerializerSettings val3 = new JsonSerializerSettings
		{
			ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver()
		};
		string message2 = JsonConvert.SerializeObject((object)obj3, val3);
		Logger.Info(message2);
		NotifyStaticMessage(message2);
	}

	private static void HandleUserLogin(int clientId, JObject msgObj)
	{
		try
		{
			JToken val = msgObj["data"];
			if (val != null)
			{
				string text = ((object)val[(object)"username"])?.ToString() ?? "";
				string text2 = ((object)val[(object)"user_id"])?.ToString() ?? "";
				string avatar = ((object)val[(object)"avatar"])?.ToString() ?? "";
				string mobile = ((object)val[(object)"mobile"])?.ToString() ?? "";
				string corpId = ((object)val[(object)"corp_id"])?.ToString() ?? "";
				string acctId = ((object)val[(object)"acctid"])?.ToString() ?? "";
				JToken obj = val[(object)"pid"];
				int pid = ((obj != null) ? Extensions.Value<int>((IEnumerable<JToken>)obj) : 0);
				(Application.Current?.Dispatcher?.Invoke(() => Application.Current.MainWindow as MainWindow))?.UpdateWeComClientLoginInfo(clientId.ToString(), text, text2, avatar, mobile, corpId, acctId, pid);
				Logger.Info($"用户登录: clientId={clientId}, username={text}, userId={text2}");
			}
		}
		catch (Exception ex)
		{
			Logger.Error("处理用户登录事件失败: " + ex.Message, ex);
		}
	}

	private static void HandleUserLogout(int clientId, JObject msgObj)
	{
		try
		{
			(Application.Current?.Dispatcher?.Invoke(() => Application.Current.MainWindow as MainWindow))?.SetWeComClientLoggedOut(clientId.ToString());
			Logger.Info($"用户掉线: clientId={clientId}");
		}
		catch (Exception ex)
		{
			Logger.Error("处理用户掉线事件失败: " + ex.Message, ex);
		}
	}

	private static void WxCloseCallback(int dwClient)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0026: Expected O, but got Unknown
		var anon = new
		{
			type = -1000,
			data = new
			{
				clientId = dwClient
			},
			message = "客户端下线"
		};
		JsonSerializerSettings val = new JsonSerializerSettings
		{
			ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver()
		};
		string message = JsonConvert.SerializeObject((object)anon, val);
		Logger.Info(message);
		NotifyStaticMessage(message);
		UpdateUIOnClientDisconnect(dwClient);
	}

	private static void UpdateUIOnClientDisconnect(int clientId)
	{
		try
		{
			(Application.Current?.Dispatcher?.Invoke(() => Application.Current.MainWindow as MainWindow))?.RemoveWeComClient(clientId.ToString());
		}
		catch (Exception ex)
		{
			Logger.Error("更新UI移除客户端失败: " + ex.Message, ex);
		}
	}

	public static async void NotifyStaticMessage(string message)
	{
		MainWindow.tcpServer.BroadcastAsync(message);
		WebSocketServer.SendMessageToAllClients(message);
		string callbackUrl = MainWindow.GetCallbackUrl();
		if (!string.IsNullOrEmpty(callbackUrl))
		{
			PostJsonAsync(callbackUrl, message, 5);
		}
	}

	public static async Task<string> PostJsonAsync(string url, object data, int timeoutInSeconds)
	{
		StringContent content = new StringContent(JsonConvert.SerializeObject(data, new JsonSerializerSettings
		{
			ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver()
		}), Encoding.UTF8, "application/json");
		using (HttpClient httpClient = new HttpClient())
		{
			httpClient.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
			try
			{
				HttpResponseMessage obj = await httpClient.PostAsync(url, content);
				obj.EnsureSuccessStatusCode();
				return await obj.Content.ReadAsStringAsync();
			}
			catch (TaskCanceledException)
			{
				Logger.Info("通知一条消息超时");
			}
			catch (HttpRequestException ex2)
			{
				Logger.Info("通知消息错误: " + ex2.Message);
			}
		}
		return "";
	}
}
