using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Spectre.Console;
using WebServerConsole.Utils;

namespace Robot.Utils;

public class Logger
{
	public delegate void LogEventHandler(string level, string message);

	private static object locks = new object();

	public static event LogEventHandler LogEvent;

	public static void Info(string message)
	{
		Log("INFO", message, "blue", ConsoleColor.Cyan);
	}

	public static void Debug(string message)
	{
		Log("DEBUG", message, "green", ConsoleColor.Green);
	}

	public static void Error(string message, Exception ex = null)
	{
		if (ex != null)
		{
			message = message + "\nException: " + ex.Message + "\nStack Trace: " + ex.StackTrace;
		}
		Log("ERROR", message, "red", ConsoleColor.Red);
	}

	public static void Warning(string message)
	{
		Log("WARN", message, "yellow", ConsoleColor.Yellow);
	}

	private static void Log(string level, string message, string color, ConsoleColor consoleColor)
	{
		string text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
		string text2 = Thread.CurrentThread.ManagedThreadId.ToString();
		string value = text + " [" + level + "] Thread:" + text2 + " --- " + message;
		if (WinApi.IsLogConsoleCreated)
		{
			try
			{
				string text3 = message.Replace("[", "[[").Replace("]", "]]");
				AnsiConsole.MarkupLine("[grey]" + text + "[/] [bold " + color + "]" + level + "[/] [grey]Thread:" + text2 + "[/] --- " + text3);
			}
			catch
			{
				try
				{
					Console.ForegroundColor = consoleColor;
					Console.WriteLine(value);
					Console.ResetColor();
				}
				catch
				{
				}
			}
		}
		Logger.LogEvent?.Invoke(level, message);
		lock (locks)
		{
			string text4 = Path.Combine(Directory.GetCurrentDirectory(), "logs");
			if (!Directory.Exists(text4))
			{
				Directory.CreateDirectory(text4);
			}
			using StreamWriter streamWriter = new StreamWriter(Path.Combine(text4, $"{DateTime.Now:yyyy-MM-dd}.log"), append: true);
			streamWriter.WriteLine(value);
		}
	}

	public static int GetPort()
	{
		int result;
		while (true)
		{
			AnsiConsole.Markup("[cyan]请输入你要监听的端口号 范围是(1-65535): [/]");
			string s = Console.ReadLine();
			if (int.TryParse(s, out result) && result >= 1 && result <= 65535)
			{
				if (!IsPortInUse(int.Parse(s)))
				{
					break;
				}
				AnsiConsole.MarkupLine($"[red]端口 {result} 已经被占用![/]");
			}
			else
			{
				AnsiConsole.MarkupLine("[red]无效的端口号。请输入一个介于1和65535之间的有效端口号.[/]");
			}
		}
		return result;
	}

	public static bool IsPortInUse(int port)
	{
		bool flag = true;
		try
		{
			TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
			tcpListener.Start();
			tcpListener.Stop();
		}
		catch (SocketException)
		{
			flag = false;
		}
		return !flag;
	}
}
