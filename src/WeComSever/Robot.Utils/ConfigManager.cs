using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Robot.Utils;

public class ConfigManager
{
	private static readonly string ConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");

	private const int DefaultPort = 8060;

	private const int DefaultTcpPort = 9090;

	private const string PortKey = "Port";

	private const string CallbackUrlKey = "CallbackUrl";

	private const string DefaultCallbackUrl = "";

	private const string TcpPortKey = "TcpPort";

	private const string NpcServerKey = "NpcServer";

	private const string NpcVkeyKey = "NpcVkey";

	private static Dictionary<string, string> _configValues = new Dictionary<string, string>();

	private const string CleanerFolderPathKey = "CleanerFolderPath";

	private const string CleanerCronKey = "CleanerCron";

	private const string CleanerEnabledKey = "CleanerEnabled";

	private const string CleanerLastCleanTimeKey = "CleanerLastCleanTime";

	private const string DefaultCleanerCron = "0 3 * * *";

	public static string CallbackUrl { get; private set; }

	public static int GetTcpPort()
	{
		if (int.TryParse(GetValue("TcpPort", 9090.ToString()), out var result) && result > 0 && result < 65536)
		{
			return result;
		}
		return 9090;
	}

	public static void SetTcpPort(int port)
	{
		if (port > 0 && port < 65536)
		{
			SaveConfig("TcpPort", port.ToString());
		}
	}

	public static void LoadConfig()
	{
		try
		{
			_configValues.Clear();
			if (!File.Exists(ConfigFile))
			{
				SaveConfig("Port", 8060.ToString());
				SaveConfig("CallbackUrl", "");
				return;
			}
			string[] array = File.ReadAllLines(ConfigFile);
			foreach (string text in array)
			{
				if (!string.IsNullOrWhiteSpace(text) && !text.StartsWith("#"))
				{
					int num = text.IndexOf('=');
					if (num > 0)
					{
						string key = text.Substring(0, num).Trim();
						string value = text.Substring(num + 1).Trim();
						_configValues[key] = value;
					}
				}
			}
			if (!_configValues.ContainsKey("Port"))
			{
				_configValues["Port"] = 8060.ToString();
			}
			if (!_configValues.ContainsKey("CallbackUrl"))
			{
				_configValues["CallbackUrl"] = "";
			}
			if (!_configValues.ContainsKey("TcpPort"))
			{
				_configValues["TcpPort"] = 9090.ToString();
				SaveConfig();
			}
			CallbackUrl = GetValue("CallbackUrl", "");
			SaveConfig();
		}
		catch (Exception ex)
		{
			Logger.Error("加载配置文件失败: " + ex.Message, ex);
			_configValues["Port"] = 8060.ToString();
			_configValues["CallbackUrl"] = "";
			CallbackUrl = "";
		}
	}

	public static void SaveConfig()
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("# WeComServer配置文件");
			stringBuilder.AppendLine("# 自动生成，请勿手动修改");
			stringBuilder.AppendLine();
			foreach (KeyValuePair<string, string> configValue in _configValues)
			{
				stringBuilder.AppendLine(configValue.Key + "=" + configValue.Value);
			}
			File.WriteAllText(ConfigFile, stringBuilder.ToString());
		}
		catch (Exception ex)
		{
			Logger.Error("保存配置文件失败: " + ex.Message, ex);
		}
	}

	public static void SaveConfig(string key, string value)
	{
		_configValues[key] = value;
		if (key == "CallbackUrl")
		{
			CallbackUrl = value;
		}
		SaveConfig();
	}

	public static string GetValue(string key, string defaultValue = null)
	{
		if (_configValues.ContainsKey(key))
		{
			return _configValues[key];
		}
		return defaultValue;
	}

	public static int GetPort()
	{
		if (int.TryParse(GetValue("Port", 8060.ToString()), out var result) && result > 0 && result < 65536)
		{
			return result;
		}
		return 8060;
	}

	public static void SetPort(int port)
	{
		if (port > 0 && port < 65536)
		{
			SaveConfig("Port", port.ToString());
		}
	}

	public static string GetCallbackUrl()
	{
		return GetValue("CallbackUrl", "");
	}

	public static void SetCallbackUrl(string url)
	{
		if (!string.IsNullOrWhiteSpace(url) && (url.StartsWith("http://") || url.StartsWith("https://")))
		{
			SaveConfig("CallbackUrl", url);
		}
	}

	public static string GetNpcServer()
	{
		return GetValue("NpcServer", "");
	}

	public static void SetNpcServer(string server)
	{
		SaveConfig("NpcServer", server ?? "");
	}

	public static string GetNpcVkey()
	{
		return GetValue("NpcVkey", "");
	}

	public static void SetNpcVkey(string vkey)
	{
		SaveConfig("NpcVkey", vkey ?? "");
	}

	public static string GetCleanerFolderPath()
	{
		return GetValue("CleanerFolderPath", "");
	}

	public static void SetCleanerFolderPath(string path)
	{
		SaveConfig("CleanerFolderPath", path ?? "");
	}

	public static string GetCleanerCron()
	{
		return GetValue("CleanerCron", "0 3 * * *");
	}

	public static void SetCleanerCron(string cron)
	{
		SaveConfig("CleanerCron", cron ?? "0 3 * * *");
	}

	public static bool GetCleanerEnabled()
	{
		return GetValue("CleanerEnabled", "false").ToLower() == "true";
	}

	public static void SetCleanerEnabled(bool enabled)
	{
		SaveConfig("CleanerEnabled", enabled.ToString().ToLower());
	}

	public static string GetCleanerLastCleanTime()
	{
		return GetValue("CleanerLastCleanTime", "暂无");
	}

	public static void SetCleanerLastCleanTime(string time)
	{
		SaveConfig("CleanerLastCleanTime", time ?? "暂无");
	}
}
