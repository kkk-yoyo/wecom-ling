using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using WebServerConsole.Utils;

namespace WeComSever;

public class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		WinApi.CreateLogConsole("微灵 运行日志");
	}

	protected override void OnExit(ExitEventArgs e)
	{
		WinApi.CloseLogConsole();
		base.OnExit(e);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		base.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
	}

	[STAThread]
	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public static void Main()
	{
		App app = new App();
		app.InitializeComponent();
		app.Run();
	}
}
