using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Microsoft.Win32;
using Robot.Config;
using Robot.Utils;
using Spectre.Console;
using Spectre.Console.Rendering;
using WeComConsoleServer.Core;
using WeComSever.Models;
using WeComSever.Network;
using WeComSever.Utils;
using WebServerConsole.Utils;

namespace WeComSever;

public class MainWindow : Window, INotifyPropertyChanged, IComponentConnector
{
	private static MainWindow _instance;

	private static WeComHelper wecomHelper;

	public static TcpServer tcpServer;

	private bool _isServerRunning;

	private bool _isTcpServerRunning;

	private DateTime _startTime;

	private DateTime _tcpStartTime;

	private DispatcherTimer _timer;

	private string _statusText = "未运行";

	private string _statusBackground = "#FF6347";

	private string _serverAddress = "未启动";

	private string _runningTime = "--:--:--";

	private string _port = "8060";

	private string _tcpPort = "9090";

	private string _tcpStatusText = "未运行";

	private string _tcpStatusBackground = "#FF6347";

	private string _callbackUrl = "";

	private bool _isNpcRunning;

	private DispatcherTimer _npcStatusTimer;

	private string _npcServer = "";

	private string _npcVkey = "";

	private Process _npcProcess;

	private string _npcStatusText = "未运行";

	private string _npcStatusBackground = "#FF6347";

	private bool _isCleanerRunning;

	private DispatcherTimer _cleanerTimer;

	private DateTime? _cleanerNextRun;

	private int _cleanerTotalFiles;

	private int _cleanerTotalFolders;

	private string _cleanerFolderPath = "";

	private int _cronPresetIndex = 5;

	private string _cronExpression = "0 3 * * *";

	private string _cleanerStatusText = "未启动";

	private string _cleanerStatusBackground = "#FF6347";

	private string _cleanerNextRunText = "未启动";

	private string _cleanerStatsText = "已清理 0 个文件, 0 个文件夹";

	private string _cleanerLastCleanText = "暂无";

	private ObservableCollection<WeComClient> _weComClients = new ObservableCollection<WeComClient>();

	internal Button MinimizeButton;

	internal Button CloseButton;

	internal TextBox PortTextBox;

	internal Button StartButton;

	internal Button StopButton;

	internal TextBox CallbackUrlTextBox;

	internal Button SaveCallbackButton;

	internal TextBox TcpPortTextBox;

	internal Button StartTcpButton;

	internal Button StopTcpButton;

	internal Button LaunchWeComButton;

	internal ItemsControl WeComClientsList;

	internal TextBlock NoClientsText;

	internal TextBox NpcServerTextBox;

	internal PasswordBox NpcVkeyPasswordBox;

	internal Button StartNpcButton;

	internal Button StopNpcButton;

	internal ToggleButton ConsoleToggleButton;

	internal TextBlock ConsoleToggleText;

	private bool _contentLoaded;

	public string StatusText
	{
		get
		{
			return _statusText;
		}
		set
		{
			if (_statusText != value)
			{
				_statusText = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusText"));
			}
		}
	}

	public string StatusBackground
	{
		get
		{
			return _statusBackground;
		}
		set
		{
			if (_statusBackground != value)
			{
				_statusBackground = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusBackground"));
			}
		}
	}

	public string ServerAddress
	{
		get
		{
			return _serverAddress;
		}
		set
		{
			if (_serverAddress != value)
			{
				_serverAddress = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ServerAddress"));
			}
		}
	}

	public string RunningTime
	{
		get
		{
			return _runningTime;
		}
		set
		{
			if (_runningTime != value)
			{
				_runningTime = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RunningTime"));
			}
		}
	}

	public string Port
	{
		get
		{
			return _port;
		}
		set
		{
			if (_port != value)
			{
				_port = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Port"));
			}
		}
	}

	public string TcpPort
	{
		get
		{
			return _tcpPort;
		}
		set
		{
			if (_tcpPort != value)
			{
				_tcpPort = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TcpPort"));
			}
		}
	}

	public string TcpStatusText
	{
		get
		{
			return _tcpStatusText;
		}
		set
		{
			if (_tcpStatusText != value)
			{
				_tcpStatusText = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TcpStatusText"));
			}
		}
	}

	public string TcpStatusBackground
	{
		get
		{
			return _tcpStatusBackground;
		}
		set
		{
			if (_tcpStatusBackground != value)
			{
				_tcpStatusBackground = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TcpStatusBackground"));
			}
		}
	}

	public string CallbackUrl
	{
		get
		{
			return _callbackUrl;
		}
		set
		{
			if (_callbackUrl != value)
			{
				_callbackUrl = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CallbackUrl"));
			}
		}
	}

	public bool IsServerStopped => !_isServerRunning;

	public bool IsTcpServerStopped => !_isTcpServerRunning;

	public string NpcServer
	{
		get
		{
			return _npcServer;
		}
		set
		{
			if (_npcServer != value)
			{
				_npcServer = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NpcServer"));
			}
		}
	}

	public string NpcVkey
	{
		get
		{
			return _npcVkey;
		}
		set
		{
			if (_npcVkey != value)
			{
				_npcVkey = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NpcVkey"));
			}
		}
	}

	public string NpcStatusText
	{
		get
		{
			return _npcStatusText;
		}
		set
		{
			if (_npcStatusText != value)
			{
				_npcStatusText = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NpcStatusText"));
			}
		}
	}

	public string NpcStatusBackground
	{
		get
		{
			return _npcStatusBackground;
		}
		set
		{
			if (_npcStatusBackground != value)
			{
				_npcStatusBackground = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NpcStatusBackground"));
			}
		}
	}

	public bool IsNpcStopped => !_isNpcRunning;

	public string CleanerFolderPath
	{
		get
		{
			return _cleanerFolderPath;
		}
		set
		{
			if (_cleanerFolderPath != value)
			{
				_cleanerFolderPath = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CleanerFolderPath"));
			}
		}
	}

	public int CronPresetIndex
	{
		get
		{
			return _cronPresetIndex;
		}
		set
		{
			if (_cronPresetIndex != value)
			{
				_cronPresetIndex = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CronPresetIndex"));
			}
		}
	}

	public string CronExpression
	{
		get
		{
			return _cronExpression;
		}
		set
		{
			if (_cronExpression != value)
			{
				_cronExpression = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CronExpression"));
			}
		}
	}

	public string CleanerStatusText
	{
		get
		{
			return _cleanerStatusText;
		}
		set
		{
			if (_cleanerStatusText != value)
			{
				_cleanerStatusText = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CleanerStatusText"));
			}
		}
	}

	public string CleanerStatusBackground
	{
		get
		{
			return _cleanerStatusBackground;
		}
		set
		{
			if (_cleanerStatusBackground != value)
			{
				_cleanerStatusBackground = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CleanerStatusBackground"));
			}
		}
	}

	public string CleanerNextRunText
	{
		get
		{
			return _cleanerNextRunText;
		}
		set
		{
			if (_cleanerNextRunText != value)
			{
				_cleanerNextRunText = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CleanerNextRunText"));
			}
		}
	}

	public Visibility CleanerNextRunVisible
	{
		get
		{
			if (!_isCleanerRunning)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public string CleanerStatsText
	{
		get
		{
			return _cleanerStatsText;
		}
		set
		{
			if (_cleanerStatsText != value)
			{
				_cleanerStatsText = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CleanerStatsText"));
			}
		}
	}

	public string CleanerLastCleanText
	{
		get
		{
			return _cleanerLastCleanText;
		}
		set
		{
			if (_cleanerLastCleanText != value)
			{
				_cleanerLastCleanText = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CleanerLastCleanText"));
			}
		}
	}

	public bool IsCleanerStopped => !_isCleanerRunning;

	public ObservableCollection<WeComClient> WeComClients
	{
		get
		{
			return _weComClients;
		}
		set
		{
			if (_weComClients != value)
			{
				_weComClients = value;
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WeComClients"));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WeComClientCount"));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NoClientsVisibility"));
			}
		}
	}

	public int WeComClientCount => _weComClients?.Count((WeComClient c) => c.IsLoggedIn) ?? 0;

	public Visibility NoClientsVisibility
	{
		get
		{
			if (WeComClientCount != 0)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public static string GetCallbackUrl()
	{
		return _instance?._callbackUrl ?? "";
	}

	public static string GetUserIdByClientId(int clientId)
	{
		try
		{
			return Application.Current?.Dispatcher?.Invoke(() => ((Application.Current.MainWindow as MainWindow)?.WeComClients?.FirstOrDefault((WeComClient c) => c.ClientId == clientId.ToString()))?.WeChatId ?? "") ?? "";
		}
		catch
		{
			return "";
		}
	}

	public MainWindow()
	{
		InitializeComponent();
		_instance = this;
		base.DataContext = this;
		ConfigManager.LoadConfig();
		Port = ConfigManager.GetPort().ToString();
		CallbackUrl = ConfigManager.GetCallbackUrl();
		TcpPort = ConfigManager.GetTcpPort().ToString();
		NpcServer = ConfigManager.GetNpcServer();
		NpcVkey = ConfigManager.GetNpcVkey();
		CleanerFolderPath = ConfigManager.GetCleanerFolderPath();
		CleanerLastCleanText = ConfigManager.GetCleanerLastCleanTime();
		LoadCronConfig();
		InitializeApp();
		InitializeServer();
		InitializeTcpServer();
		InitializeTimer();
		InitializeNpc();
		Task.Run(delegate
		{
			try
			{
				DisableWeComAutoUpdate();
				SetAutoStartup();
			}
			catch (Exception ex)
			{
				Logger.Error("后台初始化失败: " + ex.Message);
			}
		});
		UpdateButtonStates();
		if (Utility.IsRunningAsAdministrator())
		{
			base.Dispatcher.BeginInvoke((Action)delegate
			{
				try
				{
					int port = ConfigManager.GetPort();
					if (Logger.IsPortInUse(port))
					{
						Logger.Warning($"HTTP端口 {port} 已被占用，服务未启动");
						return;
					}
					Logger.Info("正在启动Web服务...");
					WebApiFactory.Server.StartWebServer();
				}
				catch (Exception ex)
				{
					Logger.Error("自动Web服务失败: " + ex.Message, ex);
				}
				AutoStartCleanerService();
			}, DispatcherPriority.Loaded);
		}
		wecomHelper = new WeComHelper();
		WebSocketServer.Start();
		UpdateConsoleButtonStates();
	}

	private void LoadCronConfig()
	{
		string cleanerCron = ConfigManager.GetCleanerCron();
		int num = 0;
		for (int i = 0; i < CronHelper.Presets.Count; i++)
		{
			if (CronHelper.Presets[i].Expression == cleanerCron)
			{
				num = i;
				break;
			}
		}
		CronPresetIndex = num;
		CronExpression = CronHelper.Presets[num].Expression;
	}

	private void InitializeApp()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (WinApi.IsLogConsoleCreated)
		{
			try
			{
				AnsiConsole.Write((IRenderable)(object)FigletTextExtensions.Color(HasJustificationExtensions.LeftJustified<FigletText>(new FigletText("WeCom Server")), (Color?)Color.Green));
			}
			catch
			{
			}
		}
		if (!Utility.IsRunningAsAdministrator())
		{
			MessageBox.Show("请使用管理员身份运行程序!", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
		else
		{
			Logger.Info("检测通过，当前以管理员身份运行");
		}
	}

	private void InitializeServer()
	{
		WebApiFactory.Server.StartSuccessfulCallback = delegate(WebServer t)
		{
			_isServerRunning = true;
			_startTime = DateTime.Now;
			base.Dispatcher.Invoke(delegate
			{
				ServerAddress = t.BaseAddress ?? "";
				StatusText = "运行中";
				StatusBackground = "#4CAF50";
				UpdateButtonStates();
				Logger.Info("服务已启动，监听地址: " + t.BaseAddress);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsServerStopped"));
			});
		};
		WebApiFactory.Server.RunEndCallback = delegate
		{
			_isServerRunning = false;
			base.Dispatcher.Invoke(delegate
			{
				ServerAddress = "未启动";
				StatusText = "未运行";
				StatusBackground = "#FF6347";
				RunningTime = "--:--:--";
				UpdateButtonStates();
				Logger.Info("服务已停止");
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsServerStopped"));
			});
		};
		int port = ConfigManager.GetPort();
		WebApiFactory.Server.BaseAddress = $"http://0.0.0.0:{port}/";
	}

	private void InitializeTcpServer()
	{
		tcpServer = new TcpServer();
		tcpServer.ServerStarted += delegate
		{
			_isTcpServerRunning = true;
			_tcpStartTime = DateTime.Now;
			base.Dispatcher.Invoke(delegate
			{
				TcpStatusText = "运行中";
				TcpStatusBackground = "#4CAF50";
				UpdateTcpButtonStates();
				Logger.Info("TCP服务已启动，监听端口: " + TcpPort);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTcpServerStopped"));
			});
		};
		tcpServer.ServerStopped += delegate
		{
			_isTcpServerRunning = false;
			base.Dispatcher.Invoke(delegate
			{
				TcpStatusText = "未运行";
				TcpStatusBackground = "#FF6347";
				UpdateTcpButtonStates();
				Logger.Info("TCP服务已停止");
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTcpServerStopped"));
			});
		};
		tcpServer.MessageReceived += delegate(string message)
		{
			base.Dispatcher.Invoke(delegate
			{
				Logger.Info("收到TCP消息: " + message);
			});
		};
		int tcpPort = ConfigManager.GetTcpPort();
		tcpServer.Port = tcpPort;
	}

	private void AutoStartTcpService()
	{
		try
		{
			if (!Utility.IsRunningAsAdministrator())
			{
				Logger.Warning("未以管理员身份运行，TCP服务未启动");
			}
			else if (!_isTcpServerRunning)
			{
				if (!int.TryParse(TcpPort, out var result) || result <= 0 || result > 65535)
				{
					Logger.Warning("TCP端口配置无效: " + TcpPort + "，服务未启动");
					return;
				}
				if (Logger.IsPortInUse(result))
				{
					Logger.Warning($"TCP端口 {result} 已被占用，服务未启动");
					return;
				}
				Logger.Info("正在自动启动TCP服务...");
				tcpServer.Port = result;
				tcpServer.Start();
			}
		}
		catch (Exception ex)
		{
			Logger.Error("自动启动TCP服务失败: " + ex.Message);
		}
	}

	private void InitializeTimer()
	{
		_timer = new DispatcherTimer();
		_timer.Interval = TimeSpan.FromSeconds(1.0);
		_timer.Tick += delegate
		{
			if (_isServerRunning)
			{
				TimeSpan timeSpan = DateTime.Now - _startTime;
				RunningTime = $"{timeSpan.Days}天 {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
			}
		};
		_timer.Start();
	}

	private void UpdateButtonStates()
	{
		StartButton.IsEnabled = !_isServerRunning;
		StopButton.IsEnabled = _isServerRunning;
	}

	private void UpdateTcpButtonStates()
	{
		StartTcpButton.IsEnabled = !_isTcpServerRunning;
		StopTcpButton.IsEnabled = _isTcpServerRunning;
	}

	private void StartButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (!Utility.IsRunningAsAdministrator())
			{
				MessageBox.Show("请使用管理员身份运行程序!", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			else if (!_isServerRunning)
			{
				if (!int.TryParse(Port, out var result) || result <= 0 || result > 65535)
				{
					MessageBox.Show("请输入有效的端口号（1-65535）", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
					return;
				}
				if (Logger.IsPortInUse(result))
				{
					MessageBox.Show($"端口 {result} 已被其他应用程序占用", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
					return;
				}
				ConfigManager.SetPort(result);
				WebApiFactory.Server.BaseAddress = $"http://0.0.0.0:{result}/";
				Logger.Info("正在启动服务...");
				WebApiFactory.Server.StartWebServer();
			}
		}
		catch (Exception ex)
		{
			Logger.Error("启动服务失败: " + ex.Message);
			MessageBox.Show("启动服务失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void StopButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (_isServerRunning)
			{
				Logger.Info("正在停止服务...");
				WebApiFactory.Server.StopWebServer();
			}
		}
		catch (Exception ex)
		{
			Logger.Error("停止服务失败: " + ex.Message);
			MessageBox.Show("停止服务失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void OpenWechatMutexTwo_Click(object sender, RoutedEventArgs e)
	{
		WeComHelper.OpenWechatMutexTwo("");
	}

	private void SaveCallbackButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			string text = CallbackUrl?.Trim();
			if (string.IsNullOrEmpty(text))
			{
				MessageBox.Show("回调地址不能为空", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			if (!text.StartsWith("http://") && !text.StartsWith("https://"))
			{
				MessageBox.Show("回调地址必须以 http:// 或 https:// 开头", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			if (!Uri.TryCreate(text, UriKind.Absolute, out var result))
			{
				MessageBox.Show("回调地址格式无效，请输入完整的 URL 地址\n例如: http://192.168.1.100:8080/callback", "格式错误", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			string host = result.Host;
			if (string.IsNullOrEmpty(host) || host.Length < 2)
			{
				MessageBox.Show("回调地址的主机名无效，请输入有效的 IP 地址或域名\n例如: http://192.168.1.100:8080/callback", "格式错误", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			if (IPAddress.TryParse(host, out var address))
			{
				if (address.AddressFamily == AddressFamily.InterNetwork)
				{
					byte[] addressBytes = address.GetAddressBytes();
					if (addressBytes[0] == 0 && addressBytes[1] == 0 && addressBytes[2] == 0)
					{
						MessageBox.Show("IP 地址 " + host + " 无效，请输入正确的服务器 IP 地址", "格式错误", MessageBoxButton.OK, MessageBoxImage.Exclamation);
						return;
					}
				}
			}
			else if (!host.Contains(".") && host != "localhost")
			{
				MessageBox.Show("主机名 '" + host + "' 格式无效，请输入有效的域名或 IP 地址\n例如: http://example.com:8080/callback", "格式错误", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			ConfigManager.SetCallbackUrl(text);
			MessageBox.Show("回调地址保存成功", "成功", MessageBoxButton.OK, MessageBoxImage.Asterisk);
			Logger.Info("回调地址配置已更新为: " + text);
		}
		catch (Exception ex)
		{
			Logger.Error("保存回调地址失败: " + ex.Message, ex);
			MessageBox.Show("保存回调地址失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void StartTcpButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (!Utility.IsRunningAsAdministrator())
			{
				MessageBox.Show("请使用管理员身份运行程序!", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			else if (!_isTcpServerRunning)
			{
				if (!int.TryParse(TcpPort, out var result) || result <= 0 || result > 65535)
				{
					MessageBox.Show("请输入有效的端口号（1-65535）", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
					return;
				}
				if (Logger.IsPortInUse(result))
				{
					MessageBox.Show($"端口 {result} 已被其他应用程序占用", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
					return;
				}
				ConfigManager.SetTcpPort(result);
				Logger.Info("正在启动TCP服务...");
				tcpServer.Port = result;
				tcpServer.Start();
			}
		}
		catch (Exception ex)
		{
			Logger.Error("启动TCP服务失败: " + ex.Message);
			MessageBox.Show("启动TCP服务失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void StopTcpButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (_isTcpServerRunning)
			{
				Logger.Info("正在停止TCP服务...");
				tcpServer.Stop();
			}
		}
		catch (Exception ex)
		{
			Logger.Error("停止TCP服务失败: " + ex.Message);
			MessageBox.Show("停止TCP服务失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void InitializeNpc()
	{
		_npcStatusTimer = new DispatcherTimer();
		_npcStatusTimer.Interval = TimeSpan.FromSeconds(2.0);
		_npcStatusTimer.Tick += delegate
		{
			UpdateNpcStatus();
		};
		_npcStatusTimer.Start();
		UpdateNpcStatus();
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			if (NpcVkeyPasswordBox != null)
			{
				NpcVkeyPasswordBox.Password = NpcVkey;
			}
		}, DispatcherPriority.Loaded);
	}

	private void DisableWeComAutoUpdate()
	{
		try
		{
			using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Tencent\\WXWork"))
			{
				if (registryKey != null)
				{
					registryKey.SetValue("NeedUpdateType", 0, RegistryValueKind.DWord);
					registryKey.SetValue("AutoUpdate", 0, RegistryValueKind.DWord);
				}
			}
			string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tencent", "WXWork", "global");
			if (Directory.Exists(text))
			{
				string path = Path.Combine(text, "as_iss.dat");
				if (File.Exists(path))
				{
					try
					{
						string text2 = File.ReadAllText(path);
						bool flag = false;
						if (text2.Contains("NeedUpdateType=1"))
						{
							text2 = text2.Replace("NeedUpdateType=1", "NeedUpdateType=0");
							flag = true;
						}
						if (flag)
						{
							File.WriteAllText(path, text2);
						}
					}
					catch
					{
					}
				}
			}
			string[] array = new string[3]
			{
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WXWork", "WXWorkUpdate.exe"),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "WXWork", "WXWorkUpdate.exe"),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WXWork", "WXWorkUpdate.exe")
			};
			foreach (string text3 in array)
			{
				if (!File.Exists(text3))
				{
					continue;
				}
				try
				{
					string text4 = text3 + ".disabled";
					if (!File.Exists(text4))
					{
						File.Move(text3, text4);
					}
				}
				catch
				{
				}
			}
			Logger.Info("已尝试禁用企业微信自动更新");
		}
		catch (Exception ex)
		{
			Logger.Error("禁用企业微信自动更新失败: " + ex.Message);
		}
	}

	private void SetAutoStartup()
	{
		try
		{
			string name = "微灵";
			string fileName = Process.GetCurrentProcess().MainModule.FileName;
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true);
			if (registryKey != null)
			{
				object value = registryKey.GetValue(name);
				if (value == null || value.ToString() != fileName)
				{
					registryKey.SetValue(name, fileName);
					Logger.Info("已设置开机自启动");
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error("设置开机自启动失败: " + ex.Message);
		}
	}

	private string GetNpcPath()
	{
		string text = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nps"), "npc.exe");
		if (!File.Exists(text))
		{
			ExtractNpcFromResource(text);
		}
		return text;
	}

	private void ExtractNpcFromResource(string targetPath)
	{
		try
		{
			string directoryName = Path.GetDirectoryName(targetPath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string text = "WeComSever.nps.npc.exe";
			using (Stream stream = executingAssembly.GetManifestResourceStream(text))
			{
				if (stream == null)
				{
					Logger.Error("嵌入式资源 " + text + " 不存在");
					return;
				}
				using FileStream destination = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
				stream.CopyTo(destination);
			}
			Logger.Info("已从嵌入式资源提取 npc.exe 到 " + targetPath);
		}
		catch (Exception ex)
		{
			Logger.Error("提取 npc.exe 失败: " + ex.Message, ex);
		}
	}

	private void UpdateNpcStatus()
	{
		try
		{
			if (Process.GetProcessesByName("npc").Length != 0)
			{
				_isNpcRunning = true;
				NpcStatusText = "运行中";
				NpcStatusBackground = "#4CAF50";
			}
			else
			{
				_isNpcRunning = false;
				NpcStatusText = "未运行";
				NpcStatusBackground = "#FF6347";
			}
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsNpcStopped"));
		}
		catch
		{
			_isNpcRunning = false;
			NpcStatusText = "未运行";
			NpcStatusBackground = "#FF6347";
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsNpcStopped"));
		}
	}

	private void StartNpcButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			string text = NpcServer?.Trim();
			string text2 = NpcVkeyPasswordBox?.Password?.Trim();
			if (string.IsNullOrEmpty(text))
			{
				MessageBox.Show("请输入服务器地址！", "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			if (!text.Contains(":"))
			{
				MessageBox.Show("服务器地址应包含端口号（如 1.2.3.4:8080）", "格式错误", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			if (string.IsNullOrEmpty(text2))
			{
				MessageBox.Show("请输入密钥！", "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			string npcPath = GetNpcPath();
			if (!File.Exists(npcPath))
			{
				MessageBox.Show("npc.exe 文件不存在，请将 npc.exe 放置到 nps 目录", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			ConfigManager.SetNpcServer(text);
			ConfigManager.SetNpcVkey(text2);
			NpcVkey = text2;
			KillAllNpcProcesses();
			string arguments = "-server=" + text + " -vkey=" + text2 + " -type=tcp";
			_npcProcess = new Process();
			_npcProcess.StartInfo.FileName = npcPath;
			_npcProcess.StartInfo.Arguments = arguments;
			_npcProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(npcPath);
			_npcProcess.StartInfo.UseShellExecute = false;
			_npcProcess.StartInfo.CreateNoWindow = true;
			_npcProcess.StartInfo.RedirectStandardOutput = true;
			_npcProcess.StartInfo.RedirectStandardError = true;
			_npcProcess.StartInfo.StandardOutputEncoding = Encoding.UTF8;
			_npcProcess.StartInfo.StandardErrorEncoding = Encoding.UTF8;
			_npcProcess.OutputDataReceived += delegate(object s, DataReceivedEventArgs args)
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Logger.Info("[NPC] " + args.Data);
				}
			};
			_npcProcess.ErrorDataReceived += delegate(object s, DataReceivedEventArgs args)
			{
				if (!string.IsNullOrEmpty(args.Data))
				{
					Logger.Warning("[NPC] " + args.Data);
				}
			};
			_npcProcess.EnableRaisingEvents = true;
			_npcProcess.Exited += delegate
			{
				base.Dispatcher.Invoke(delegate
				{
					Logger.Info("[NPC] 内网穿透进程已退出");
					UpdateNpcStatus();
				});
			};
			_npcProcess.Start();
			_npcProcess.BeginOutputReadLine();
			_npcProcess.BeginErrorReadLine();
			Logger.Info("内网穿透已启动，服务器: " + text);
			MessageBox.Show("内网穿透启动成功，日志将显示在日志控制台中", "成功", MessageBoxButton.OK, MessageBoxImage.Asterisk);
			UpdateNpcStatus();
		}
		catch (Exception ex)
		{
			Logger.Error("启动内网穿透失败: " + ex.Message, ex);
			MessageBox.Show("启动失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void StopNpcButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			if (_npcProcess != null && !_npcProcess.HasExited)
			{
				try
				{
					_npcProcess.Kill();
					_npcProcess.WaitForExit(2000);
				}
				catch
				{
				}
				finally
				{
					_npcProcess.Dispose();
					_npcProcess = null;
				}
			}
			KillAllNpcProcesses();
			Logger.Info("内网穿透已停止");
			MessageBox.Show("内网穿透已停止", "成功", MessageBoxButton.OK, MessageBoxImage.Asterisk);
			UpdateNpcStatus();
		}
		catch (Exception ex)
		{
			Logger.Error("停止内网穿透失败: " + ex.Message, ex);
			MessageBox.Show("停止失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void KillAllNpcProcesses()
	{
		try
		{
			Process[] processesByName = Process.GetProcessesByName("npc");
			foreach (Process process in processesByName)
			{
				try
				{
					process.Kill();
					process.WaitForExit(2000);
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	private void ConsoleToggleButton_Click(object sender, RoutedEventArgs e)
	{
		if (ConsoleToggleButton.IsChecked == true)
		{
			WinApi.ShowConsole();
			Logger.Info("已显示控制台窗口");
		}
		else
		{
			WinApi.HideConsole();
			Logger.Info("已隐藏控制台窗口");
		}
	}

	private void UpdateConsoleButtonStates()
	{
		ConsoleToggleButton.IsChecked = WinApi.IsConsoleVisible;
	}

	private void Window_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			DragMove();
		}
	}

	private void MinimizeButton_Click(object sender, RoutedEventArgs e)
	{
		base.WindowState = WindowState.Minimized;
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		if (_isServerRunning)
		{
			if (MessageBox.Show("服务正在运行中，确定要关闭程序吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
			{
				return;
			}
			try
			{
				WebApiFactory.Server.StopWebServer();
				return;
			}
			catch (Exception ex)
			{
				Logger.Error("停止服务失败: " + ex.Message);
				return;
			}
			finally
			{
				Close();
			}
		}
		Close();
	}

	public void AddWeComClient(string clientId, string weChatName, string weChatId)
	{
		base.Dispatcher.Invoke(delegate
		{
			WeComClient weComClient = WeComClients.FirstOrDefault((WeComClient c) => c.ClientId == clientId);
			if (weComClient != null)
			{
				weComClient.WeChatName = weChatName;
				weComClient.WeChatId = weChatId;
				weComClient.IsOnline = true;
				Logger.Info("更新企微客户端: " + weChatName + " (" + clientId + ")");
			}
			else
			{
				WeComClient item = new WeComClient(clientId, weChatName, weChatId);
				WeComClients.Add(item);
				Logger.Info("添加企微客户端: " + weChatName + " (" + clientId + ")");
			}
			UpdateClientCountProperties();
		});
	}

	public void RemoveWeComClient(string clientId)
	{
		base.Dispatcher.Invoke(delegate
		{
			WeComClient weComClient = WeComClients.FirstOrDefault((WeComClient c) => c.ClientId == clientId);
			if (weComClient != null)
			{
				WeComClients.Remove(weComClient);
				Logger.Info("移除企微客户端: " + weComClient.WeChatName + " (" + clientId + ")");
				UpdateClientCountProperties();
			}
		});
	}

	public void SetWeComClientOffline(string clientId)
	{
		base.Dispatcher.Invoke(delegate
		{
			WeComClient weComClient = WeComClients.FirstOrDefault((WeComClient c) => c.ClientId == clientId);
			if (weComClient != null)
			{
				weComClient.IsOnline = false;
				Logger.Info("企微客户端离线: " + weComClient.WeChatName + " (" + clientId + ")");
			}
		});
	}

	public void UpdateWeComClientLoginInfo(string clientId, string username, string userId, string avatar, string mobile, string corpId, string acctId, int pid)
	{
		base.Dispatcher.Invoke(delegate
		{
			WeComClient weComClient = WeComClients.FirstOrDefault((WeComClient c) => c.ClientId == clientId);
			if (weComClient != null)
			{
				weComClient.WeChatName = (string.IsNullOrEmpty(username) ? ("客户端 " + clientId) : username);
				weComClient.WeChatId = userId;
				weComClient.Avatar = avatar;
				weComClient.Mobile = mobile;
				weComClient.CorpId = corpId;
				weComClient.AcctId = acctId;
				weComClient.Pid = pid;
				weComClient.IsLoggedIn = true;
				weComClient.IsOnline = true;
				Logger.Info("企微客户端登录: " + username + " (" + clientId + "), userId=" + userId);
			}
			else
			{
				WeComClient item = new WeComClient(clientId, string.IsNullOrEmpty(username) ? ("客户端 " + clientId) : username, userId)
				{
					Avatar = avatar,
					Mobile = mobile,
					CorpId = corpId,
					AcctId = acctId,
					Pid = pid,
					IsLoggedIn = true,
					IsOnline = true
				};
				WeComClients.Add(item);
				Logger.Info("新增并登录企微客户端: " + username + " (" + clientId + "), userId=" + userId);
			}
			UpdateClientCountProperties();
		});
	}

	public void SetWeComClientLoggedOut(string clientId)
	{
		base.Dispatcher.Invoke(delegate
		{
			WeComClient weComClient = WeComClients.FirstOrDefault((WeComClient c) => c.ClientId == clientId);
			if (weComClient != null)
			{
				weComClient.IsLoggedIn = false;
				Logger.Info("企微客户端退出登录: " + weComClient.WeChatName + " (" + clientId + ")");
			}
		});
	}

	private void UpdateClientCountProperties()
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WeComClientCount"));
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NoClientsVisibility"));
	}

	private void CloseWeComClient_Click(object sender, RoutedEventArgs e)
	{
		object obj = (sender as Button)?.Tag;
		string clientId = obj as string;
		if (clientId != null)
		{
			WeComClient weComClient = WeComClients.FirstOrDefault((WeComClient c) => c.ClientId == clientId);
			if (weComClient != null && MessageBox.Show("确定要关闭客户端 \"" + weComClient.WeChatName + "\" 吗？", "确认关闭", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				RemoveWeComClient(clientId);
			}
		}
	}

	private void AutoStartCleanerService()
	{
		try
		{
			if (ConfigManager.GetCleanerEnabled() && !string.IsNullOrEmpty(CleanerFolderPath))
			{
				if (!Directory.Exists(CleanerFolderPath))
				{
					Logger.Warning("定时清理文件夹路径不存在，跳过自动启动: " + CleanerFolderPath);
					return;
				}
				if (!CronHelper.IsValid(CronExpression))
				{
					Logger.Warning("定时清理Cron表达式无效，跳过自动启动: " + CronExpression);
					return;
				}
				Logger.Info("检测到定时清理配置，正在自动启动...");
				StartCleanerTimer();
			}
		}
		catch (Exception ex)
		{
			Logger.Error("自动启动定时清理失败: " + ex.Message);
		}
	}

	private void StartCleanerTimer()
	{
		if (CronHelper.IsValid(CronExpression))
		{
			_cleanerNextRun = CronHelper.GetNextOccurrence(CronExpression, DateTime.Now);
			if (!_cleanerNextRun.HasValue)
			{
				MessageBox.Show("无法计算下次执行时间，请检查 Cron 表达式", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			_cleanerTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(1.0)
			};
			_cleanerTimer.Tick += CleanerTimer_Tick;
			_cleanerTimer.Start();
			_isCleanerRunning = true;
			CleanerStatusText = "运行中";
			CleanerStatusBackground = "#10B981";
			UpdateCleanerNextRunText();
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCleanerStopped"));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CleanerNextRunVisible"));
			ConfigManager.SetCleanerEnabled(enabled: true);
			Logger.Info("文件夹清理定时器已启动: Cron=" + CronExpression + ", 下次执行: " + _cleanerNextRun?.ToString("yyyy-MM-dd HH:mm:ss") + ", 路径: " + CleanerFolderPath);
		}
	}

	private void StopCleanerTimer()
	{
		if (_cleanerTimer != null)
		{
			_cleanerTimer.Stop();
			_cleanerTimer.Tick -= CleanerTimer_Tick;
			_cleanerTimer = null;
		}
		_isCleanerRunning = false;
		_cleanerNextRun = null;
		CleanerStatusText = "未启动";
		CleanerStatusBackground = "#FF6347";
		CleanerNextRunText = "未启动";
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCleanerStopped"));
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CleanerNextRunVisible"));
		ConfigManager.SetCleanerEnabled(enabled: false);
		Logger.Info("文件夹清理定时器已停止");
	}

	private void UpdateCleanerNextRunText()
	{
		if (!_isCleanerRunning || !_cleanerNextRun.HasValue)
		{
			CleanerNextRunText = "未启动";
		}
		else
		{
			CleanerNextRunText = CronHelper.FormatNextOccurrence(_cleanerNextRun);
		}
	}

	private void CleanerTimer_Tick(object sender, EventArgs e)
	{
		UpdateCleanerNextRunText();
		if (_cleanerNextRun.HasValue && DateTime.Now >= _cleanerNextRun.Value)
		{
			ExecuteCleanFolder();
			_cleanerNextRun = CronHelper.GetNextOccurrence(CronExpression, DateTime.Now);
			if (_cleanerNextRun.HasValue)
			{
				Logger.Info("下次清理时间: " + _cleanerNextRun?.ToString("yyyy-MM-dd HH:mm:ss"));
				return;
			}
			Logger.Warning("无法计算下次执行时间");
			StopCleanerTimer();
		}
	}

	private void ExecuteCleanFolder()
	{
		if (string.IsNullOrEmpty(CleanerFolderPath) || !Directory.Exists(CleanerFolderPath))
		{
			Logger.Warning("清理文件夹路径无效: " + CleanerFolderPath);
			return;
		}
		int num = 0;
		int num2 = 0;
		try
		{
			Logger.Info("开始清理文件夹: " + CleanerFolderPath);
			string[] files = Directory.GetFiles(CleanerFolderPath, "*", SearchOption.AllDirectories);
			foreach (string text in files)
			{
				try
				{
					File.Delete(text);
					num++;
				}
				catch (Exception ex)
				{
					Logger.Warning("删除文件失败: " + text + ", 错误: " + ex.Message);
				}
			}
			foreach (string item in from d in Directory.GetDirectories(CleanerFolderPath, "*", SearchOption.AllDirectories)
				orderby d.Length descending
				select d)
			{
				try
				{
					Directory.Delete(item, recursive: false);
					num2++;
				}
				catch (Exception ex2)
				{
					Logger.Warning("删除文件夹失败: " + item + ", 错误: " + ex2.Message);
				}
			}
			_cleanerTotalFiles += num;
			_cleanerTotalFolders += num2;
			CleanerStatsText = $"已清理 {_cleanerTotalFiles} 个文件, {_cleanerTotalFolders} 个文件夹";
			string cleanerLastCleanTime = (CleanerLastCleanText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			ConfigManager.SetCleanerLastCleanTime(cleanerLastCleanTime);
			Logger.Info($"文件夹清理完成: 删除了 {num} 个文件, {num2} 个文件夹");
		}
		catch (Exception ex3)
		{
			Logger.Error("清理文件夹时发生错误: " + ex3.Message, ex3);
		}
	}

	private void OpenCleanerSettings_Click(object sender, RoutedEventArgs e)
	{
		CleanerSettingsWindow cleanerSettingsWindow = new CleanerSettingsWindow(this);
		cleanerSettingsWindow.Owner = this;
		cleanerSettingsWindow.ShowDialog();
	}

	public void ExecuteCleanNow()
	{
		if (MessageBox.Show("确定要清理文件夹内的所有内容吗？\n\n路径: " + CleanerFolderPath + "\n\n此操作不可恢复！", "确认清理", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
		{
			ExecuteCleanFolder();
		}
	}

	public void StartCleanerFromDialog()
	{
		if (!CronHelper.IsValid(CronExpression))
		{
			MessageBox.Show("请输入有效的 Cron 表达式", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
		else
		{
			StartCleanerTimer();
		}
	}

	public void StopCleanerFromDialog()
	{
		StopCleanerTimer();
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		try
		{
			Logger.Info("正在关闭窗口，停止所有服务...");
			if (_isServerRunning)
			{
				try
				{
					WebApiFactory.Server.StopWebServer();
					Logger.Info("HTTP服务已停止");
				}
				catch (Exception ex)
				{
					Logger.Error("停止HTTP服务失败: " + ex.Message);
				}
			}
			if (_isTcpServerRunning)
			{
				try
				{
					tcpServer.Stop();
					Logger.Info("TCP服务已停止");
				}
				catch (Exception ex2)
				{
					Logger.Error("停止TCP服务失败: " + ex2.Message);
				}
			}
			try
			{
				WebSocketServer.Stop();
				Logger.Info("WebSocket服务已停止");
			}
			catch (Exception ex3)
			{
				Logger.Error("停止WebSocket服务失败: " + ex3.Message);
			}
			if (_npcStatusTimer != null)
			{
				_npcStatusTimer.Stop();
			}
			if (_cleanerTimer != null)
			{
				_cleanerTimer.Stop();
			}
			Logger.Info("所有服务已停止，窗口关闭");
		}
		catch (Exception ex4)
		{
			Logger.Error("关闭窗口时发生错误: " + ex4.Message);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/WeComSever;component/mainwindow.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			((MainWindow)target).MouseDown += Window_MouseDown;
			((MainWindow)target).Closing += Window_Closing;
			break;
		case 2:
			MinimizeButton = (Button)target;
			MinimizeButton.Click += MinimizeButton_Click;
			break;
		case 3:
			CloseButton = (Button)target;
			CloseButton.Click += CloseButton_Click;
			break;
		case 4:
			PortTextBox = (TextBox)target;
			break;
		case 5:
			StartButton = (Button)target;
			StartButton.Click += StartButton_Click;
			break;
		case 6:
			StopButton = (Button)target;
			StopButton.Click += StopButton_Click;
			break;
		case 7:
			CallbackUrlTextBox = (TextBox)target;
			break;
		case 8:
			SaveCallbackButton = (Button)target;
			SaveCallbackButton.Click += SaveCallbackButton_Click;
			break;
		case 9:
			TcpPortTextBox = (TextBox)target;
			break;
		case 10:
			StartTcpButton = (Button)target;
			StartTcpButton.Click += StartTcpButton_Click;
			break;
		case 11:
			StopTcpButton = (Button)target;
			StopTcpButton.Click += StopTcpButton_Click;
			break;
		case 12:
			LaunchWeComButton = (Button)target;
			LaunchWeComButton.Click += OpenWechatMutexTwo_Click;
			break;
		case 13:
			WeComClientsList = (ItemsControl)target;
			break;
		case 14:
			NoClientsText = (TextBlock)target;
			break;
		case 15:
			NpcServerTextBox = (TextBox)target;
			break;
		case 16:
			NpcVkeyPasswordBox = (PasswordBox)target;
			break;
		case 17:
			StartNpcButton = (Button)target;
			StartNpcButton.Click += StartNpcButton_Click;
			break;
		case 18:
			StopNpcButton = (Button)target;
			StopNpcButton.Click += StopNpcButton_Click;
			break;
		case 19:
			ConsoleToggleButton = (ToggleButton)target;
			ConsoleToggleButton.Click += ConsoleToggleButton_Click;
			break;
		case 20:
			ConsoleToggleText = (TextBlock)target;
			break;
		case 21:
			((Button)target).Click += OpenCleanerSettings_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
