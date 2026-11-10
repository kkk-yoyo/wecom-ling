using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Robot.Common;
using Robot.Utils;
using WeComConsoleServer.Core;
using WebServerConsole.Utils;

namespace WebServerConsole.Controller;

public class ClientController : ApiController
{
	[HttpGet]
	public R<object> OpenWeCom()
	{
		new Thread((ThreadStart)delegate
		{
			WeComHelper.OpenWechatMutexTwo("");
		}).Start();
		Logger.Info("HTTP执行开启企业微信");
		return R<object>.Success("执行开启企业微信");
	}

	[HttpGet]
	public R<object> InjectAllWeCom()
	{
		List<uint> list = FindAllWeChatProcesses();
		foreach (uint item in list)
		{
			WeComHelper.InjectWeChatPid((int)item);
		}
		Logger.Info($"共找到{list.Count}个微信进程,并进行注入");
		return R<object>.Success($"共找到{list.Count}个微信进程,并进行注入");
	}

	[HttpGet]
	public R<object> ReInitSocket()
	{
		try
		{
			WeComHelper.ReInitializeSocket();
			Logger.Info("HTTP执行重新初始化 DLL Socket");
			return R<object>.Success("DLL Socket 监听已重新初始化");
		}
		catch (Exception ex)
		{
			Logger.Info("重新初始化 Socket 失败: " + ex.Message);
			throw new CustomException("重新初始化失败: " + ex.Message);
		}
	}

	public static List<uint> FindAllWeChatProcesses()
	{
		List<uint> list = new List<uint>();
		try
		{
			Logger.Info("开始查找所有企业微信进程");
			IntPtr intPtr = WinApi.CreateToolhelp32Snapshot(2u, 0u);
			if (intPtr == IntPtr.Zero)
			{
				Logger.Info("创建进程快照失败");
				return list;
			}
			WinApi.PROCESSENTRY32 lppe = default(WinApi.PROCESSENTRY32);
			lppe.dwSize = (uint)Marshal.SizeOf(lppe);
			if (WinApi.Process32First(intPtr, ref lppe))
			{
				do
				{
					if (lppe.szExeFile.ToLower().Equals("wxwork.exe"))
					{
						string processWindowTitle = GetProcessWindowTitle(lppe.th32ProcessID);
						Logger.Debug($"找到企业微信进程: {lppe.szExeFile} (PID: {lppe.th32ProcessID}), 窗口标题: '{processWindowTitle}'");
						if (processWindowTitle.Equals("企业微信", StringComparison.OrdinalIgnoreCase))
						{
							Logger.Debug($"确认为企业微信主进程: {lppe.szExeFile} (PID: {lppe.th32ProcessID})");
							list.Add(lppe.th32ProcessID);
						}
						else
						{
							Logger.Debug($"跳过子进程: {lppe.szExeFile} (PID: {lppe.th32ProcessID}), 标题: '{processWindowTitle}'");
						}
					}
				}
				while (WinApi.Process32Next(intPtr, ref lppe));
			}
			WinApi.CloseHandle(intPtr);
			Logger.Info($"总共找到 {list.Count} 个企业微信主进程.");
			return list;
		}
		catch (Exception ex)
		{
			Logger.Info("查找企业微信进程异常: " + ex.Message);
			return list;
		}
	}

	private static string GetProcessWindowTitle(uint processId)
	{
		try
		{
			List<string> windowTitles = new List<string>();
			WinApi.EnumWindows(delegate(IntPtr hWnd, IntPtr lParam)
			{
				WinApi.GetWindowThreadProcessId(hWnd, out var lpdwProcessId);
				if (lpdwProcessId == processId && WinApi.IsWindowVisible(hWnd))
				{
					int windowTextLength = WinApi.GetWindowTextLength(hWnd);
					if (windowTextLength > 0)
					{
						StringBuilder stringBuilder = new StringBuilder(windowTextLength + 1);
						WinApi.GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);
						string text = stringBuilder.ToString();
						if (!string.IsNullOrEmpty(text))
						{
							windowTitles.Add(text);
						}
					}
				}
				return true;
			}, IntPtr.Zero);
			foreach (string item in windowTitles)
			{
				if (item.Equals("微信", StringComparison.OrdinalIgnoreCase))
				{
					return item;
				}
			}
			return (windowTitles.Count > 0) ? windowTitles[0] : string.Empty;
		}
		catch (Exception ex)
		{
			Logger.Info("获取进程窗口标题异常: " + ex.Message);
			return string.Empty;
		}
	}

	[HttpPost]
	public IHttpActionResult ExecCommand([FromBody] dynamic model)
	{
		if ((!model.ContainsKey("client")))
		{
			throw new CustomException("请填写完整属性");
		}
		int num = (int)model.client;
		int num2 = 0;
		if (model.ContainsKey("type"))
		{
			num2 = (int)model.type;
		}
		switch (num2)
		{
		case 11170:
			return (IHttpActionResult)DownloadC2cCdnFile(num, model);
		case 11171:
			return (IHttpActionResult)DownloadWxCdnFile(num, model);
		default:
		{
			bool flag = false;
			if (model.ContainsKey("getData"))
			{
				flag = (bool)model.getData;
			}
			try
			{
				if (flag)
				{
					string text = WeComHelper.SendMessageSyncWithLock(num, model.ToString(), num2);
					if (!string.IsNullOrEmpty(text))
					{
						dynamic val = JsonConvert.DeserializeObject(text);
						if (val.data != null)
						{
							return (IHttpActionResult)Ok(R<object>.Success(val.data));
						}
						return (IHttpActionResult)Ok(R<object>.Success(val));
					}
					return (IHttpActionResult)(object)((ApiController)this).Ok<R<object>>(R<object>.Success(text));
				}
				if ((bool)WeComHelper.SendMessage(num, model.ToString()))
				{
					return (IHttpActionResult)(object)((ApiController)this).Ok<R<object>>(R<object>.Success("指令已发送"));
				}
				throw new CustomException("发送指令失败");
			}
			catch (Exception ex)
			{
				Logger.Info("执行指令异常: " + ex.Message);
				throw new CustomException("执行失败: " + ex.Message);
			}
		}
		}
	}

	private IHttpActionResult DownloadC2cCdnFile(int client, dynamic model)
	{
		if ((!model.ContainsKey("data")))
		{
			throw new CustomException("请填写 data 属性");
		}
		dynamic val = model.data;
		if (!val.ContainsKey("aes_key") || !val.ContainsKey("file_id") || !val.ContainsKey("file_size") || !val.ContainsKey("file_type"))
		{
			throw new CustomException("data 中请填写完整属性: aes_key, file_id, file_size, file_type");
		}
		string aes_key = (string)val.aes_key;
		string text = (string)val.file_id;
		int file_size = (int)val.file_size;
		int num = (int)val.file_type;
		string text2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_cdn");
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
		}
		string fileExtension = GetFileExtension(num);
		string text3 = $"{Guid.NewGuid()}{fileExtension}";
		string text4 = Path.Combine(text2, text3);
		try
		{
			string message = JsonConvert.SerializeObject((object)new
			{
				type = 11170,
				data = new
				{
					aes_key = aes_key,
					file_id = text,
					save_path = text4,
					file_size = file_size,
					file_type = num
				}
			});
			Logger.Info($"C2cCDN下载请求: client={client}, file_id={text}, save_path={text4}");
			string text5 = WeComHelper.SendMessageSyncByPath(client, message, text4);
			if (string.IsNullOrEmpty(text5))
			{
				throw new DownloadException("下载请求超时或失败");
			}
			JToken obj = JObject.Parse(text5)["data"];
			int? obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				JToken obj3 = obj[(object)"error_code"];
				obj2 = ((obj3 != null) ? new int?(Extensions.Value<int>((IEnumerable<JToken>)obj3)) : ((int?)null));
			}
			int num2 = obj2 ?? (-1);
			if (num2 != 0)
			{
				throw new DownloadException($"下载失败, error_code: {num2}");
			}
			if (!File.Exists(text4))
			{
				throw new DownloadException("文件下载失败，本地文件不存在");
			}
			byte[] array = File.ReadAllBytes(text4);
			try
			{
				File.Delete(text4);
				Logger.Info("已删除临时文件: " + text4);
			}
			catch (Exception ex)
			{
				Logger.Info("删除临时文件失败: " + ex.Message);
			}
			HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
			httpResponseMessage.Content = new ByteArrayContent(array);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(num));
			httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
			{
				FileName = text3
			};
			httpResponseMessage.Content.Headers.ContentLength = array.Length;
			Logger.Info($"C2cCDN下载成功: file_id={text}, size={array.Length}");
			return (IHttpActionResult)(object)((ApiController)this).ResponseMessage(httpResponseMessage);
		}
		catch (DownloadException)
		{
			throw;
		}
		catch (Exception ex3)
		{
			Logger.Info("C2cCDN下载异常: " + ex3.Message);
			if (File.Exists(text4))
			{
				try
				{
					File.Delete(text4);
				}
				catch
				{
				}
			}
			throw new DownloadException("下载失败: " + ex3.Message);
		}
	}

	private IHttpActionResult DownloadWxCdnFile(int client, dynamic model)
	{
		if ((!model.ContainsKey("data")))
		{
			throw new CustomException("请填写 data 属性");
		}
		dynamic val = model.data;
		if (!val.ContainsKey("url") || !val.ContainsKey("auth_key") || !val.ContainsKey("aes_key") || !val.ContainsKey("size") || !val.ContainsKey("download_type"))
		{
			throw new CustomException("data 中请填写完整属性: url, auth_key, aes_key, size, download_type");
		}
		string text = (string)val.url;
		string auth_key = (string)val.auth_key;
		string aes_key = (string)val.aes_key;
		int size = (int)val.size;
		int num = (int)val.download_type;
		GetFileTypeInfo(num, out var extension, out var contentType);
		string text2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_cdn");
		if (!Directory.Exists(text2))
		{
			Directory.CreateDirectory(text2);
		}
		string text3 = $"{Guid.NewGuid()}{extension}";
		string text4 = Path.Combine(text2, text3);
		try
		{
			string message = JsonConvert.SerializeObject((object)new
			{
				type = 11171,
				data = new
				{
					url = text,
					auth_key = auth_key,
					aes_key = aes_key,
					size = size,
					save_path = text4
				}
			});
			Logger.Info($"WxCDN下载请求: client={client}, download_type={num}, url={text.Substring(0, Math.Min(100, text.Length))}..., save_path={text4}");
			string text5 = WeComHelper.SendMessageSyncByPath(client, message, text4);
			if (string.IsNullOrEmpty(text5))
			{
				throw new DownloadException("下载请求超时或失败");
			}
			JToken obj = JObject.Parse(text5)["data"];
			int? obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				JToken obj3 = obj[(object)"error_code"];
				obj2 = ((obj3 != null) ? new int?(Extensions.Value<int>((IEnumerable<JToken>)obj3)) : ((int?)null));
			}
			int num2 = obj2 ?? (-1);
			if (num2 != 0)
			{
				throw new DownloadException($"下载失败, error_code: {num2}");
			}
			if (!File.Exists(text4))
			{
				throw new DownloadException("文件下载失败，本地文件不存在");
			}
			byte[] array = File.ReadAllBytes(text4);
			try
			{
				File.Delete(text4);
				Logger.Info("已删除临时文件: " + text4);
			}
			catch (Exception ex)
			{
				Logger.Info("删除临时文件失败: " + ex.Message);
			}
			HttpResponseMessage httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
			httpResponseMessage.Content = new ByteArrayContent(array);
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
			httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
			{
				FileName = text3
			};
			httpResponseMessage.Content.Headers.ContentLength = array.Length;
			Logger.Info($"WxCDN下载成功: size={array.Length}");
			return (IHttpActionResult)(object)((ApiController)this).ResponseMessage(httpResponseMessage);
		}
		catch (DownloadException)
		{
			throw;
		}
		catch (Exception ex3)
		{
			Logger.Info("WxCDN下载异常: " + ex3.Message);
			if (File.Exists(text4))
			{
				try
				{
					File.Delete(text4);
				}
				catch
				{
				}
			}
			throw new DownloadException("下载失败: " + ex3.Message);
		}
	}

	private string GetFileExtension(int fileType)
	{
		switch (fileType)
		{
		case 1:
		case 2:
		case 3:
			return ".jpg";
		case 4:
			return ".mp4";
		case 5:
			return ".dat";
		default:
			return ".dat";
		}
	}

	private string GetContentType(int fileType)
	{
		switch (fileType)
		{
		case 1:
		case 2:
		case 3:
			return "image/jpeg";
		case 4:
			return "video/mp4";
		default:
			return "application/octet-stream";
		}
	}

	private void GetFileTypeInfo(int downloadType, out string extension, out string contentType)
	{
		switch (downloadType)
		{
		case 1:
			extension = ".jpg";
			contentType = "image/jpeg";
			break;
		case 2:
			extension = ".mp4";
			contentType = "video/mp4";
			break;
		case 3:
			extension = ".amr";
			contentType = "audio/amr";
			break;
		default:
			extension = ".dat";
			contentType = "application/octet-stream";
			break;
		}
	}
}
