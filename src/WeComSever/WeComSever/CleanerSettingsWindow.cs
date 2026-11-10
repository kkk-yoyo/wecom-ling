using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Robot.Utils;
using WeComSever.Utils;

namespace WeComSever;

public class CleanerSettingsWindow : Window, IComponentConnector
{
	private MainWindow _mainWindow;

	private string _currentCronExpression;

	internal Border StatusBorder;

	internal TextBlock StatusText;

	internal System.Windows.Controls.TextBox FolderPathTextBox;

	internal System.Windows.Controls.ComboBox CronPresetComboBox;

	internal TextBlock StatsText;

	internal TextBlock LastCleanText;

	internal System.Windows.Controls.Button StartButton;

	internal System.Windows.Controls.Button StopButton;

	private bool _contentLoaded;

	public CleanerSettingsWindow(MainWindow mainWindow)
	{
		InitializeComponent();
		_mainWindow = mainWindow;
		InitializeCronPresets();
		LoadConfig();
		UpdateUI();
	}

	private void InitializeCronPresets()
	{
		CronPresetComboBox.Items.Clear();
		foreach (CronPreset preset in CronHelper.Presets)
		{
			CronPresetComboBox.Items.Add(preset.Name);
		}
	}

	private void LoadConfig()
	{
		FolderPathTextBox.Text = _mainWindow.CleanerFolderPath;
		string text = (_currentCronExpression = ConfigManager.GetCleanerCron());
		int selectedIndex = 0;
		for (int i = 0; i < CronHelper.Presets.Count; i++)
		{
			if (CronHelper.Presets[i].Expression == text)
			{
				selectedIndex = i;
				break;
			}
		}
		CronPresetComboBox.SelectedIndex = selectedIndex;
	}

	public void UpdateUI()
	{
		bool flag = !_mainWindow.IsCleanerStopped;
		if (flag)
		{
			StatusText.Text = "运行中";
			StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
		}
		else
		{
			StatusText.Text = "未启动";
			StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27272A"));
		}
		StartButton.Visibility = (flag ? Visibility.Collapsed : Visibility.Visible);
		StopButton.Visibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
		FolderPathTextBox.IsEnabled = !flag;
		CronPresetComboBox.IsEnabled = !flag;
		StatsText.Text = _mainWindow.CleanerStatsText;
		LastCleanText.Text = _mainWindow.CleanerLastCleanText;
	}

	private void Window_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			DragMove();
		}
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void BrowseFolder_Click(object sender, RoutedEventArgs e)
	{
		using FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		folderBrowserDialog.Description = "选择要清理的文件夹";
		folderBrowserDialog.ShowNewFolderButton = true;
		if (!string.IsNullOrEmpty(FolderPathTextBox.Text) && Directory.Exists(FolderPathTextBox.Text))
		{
			folderBrowserDialog.SelectedPath = FolderPathTextBox.Text;
		}
		if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
		{
			FolderPathTextBox.Text = folderBrowserDialog.SelectedPath;
		}
	}

	private void CronPresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (CronPresetComboBox.SelectedIndex >= 0 && CronPresetComboBox.SelectedIndex < CronHelper.Presets.Count)
		{
			CronPreset cronPreset = CronHelper.Presets[CronPresetComboBox.SelectedIndex];
			_currentCronExpression = cronPreset.Expression;
		}
	}

	private void CleanNow_Click(object sender, RoutedEventArgs e)
	{
		string text = FolderPathTextBox.Text;
		if (string.IsNullOrWhiteSpace(text))
		{
			System.Windows.MessageBox.Show("请先选择要清理的文件夹！", "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		if (!Directory.Exists(text))
		{
			System.Windows.MessageBox.Show("指定的文件夹不存在！", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
			return;
		}
		_mainWindow.CleanerFolderPath = text;
		_mainWindow.ExecuteCleanNow();
		UpdateUI();
	}

	private void StartCleaner_Click(object sender, RoutedEventArgs e)
	{
		string text = FolderPathTextBox.Text;
		if (string.IsNullOrWhiteSpace(text))
		{
			System.Windows.MessageBox.Show("请先选择要清理的文件夹！", "提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		if (!Directory.Exists(text))
		{
			System.Windows.MessageBox.Show("指定的文件夹不存在！", "错误", MessageBoxButton.OK, MessageBoxImage.Hand);
			return;
		}
		_mainWindow.CleanerFolderPath = text;
		_mainWindow.CronExpression = _currentCronExpression;
		_mainWindow.CronPresetIndex = CronPresetComboBox.SelectedIndex;
		ConfigManager.SetCleanerFolderPath(text);
		ConfigManager.SetCleanerCron(_currentCronExpression);
		_mainWindow.StartCleanerFromDialog();
		UpdateUI();
	}

	private void StopCleaner_Click(object sender, RoutedEventArgs e)
	{
		_mainWindow.StopCleanerFromDialog();
		UpdateUI();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/WeComSever;component/cleanersettingswindow.xaml", UriKind.Relative);
			System.Windows.Application.LoadComponent(this, resourceLocator);
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
			((CleanerSettingsWindow)target).MouseDown += Window_MouseDown;
			break;
		case 2:
			((System.Windows.Controls.Button)target).Click += CloseButton_Click;
			break;
		case 3:
			StatusBorder = (Border)target;
			break;
		case 4:
			StatusText = (TextBlock)target;
			break;
		case 5:
			FolderPathTextBox = (System.Windows.Controls.TextBox)target;
			break;
		case 6:
			((System.Windows.Controls.Button)target).Click += BrowseFolder_Click;
			break;
		case 7:
			CronPresetComboBox = (System.Windows.Controls.ComboBox)target;
			CronPresetComboBox.SelectionChanged += CronPresetComboBox_SelectionChanged;
			break;
		case 8:
			StatsText = (TextBlock)target;
			break;
		case 9:
			LastCleanText = (TextBlock)target;
			break;
		case 10:
			((System.Windows.Controls.Button)target).Click += CleanNow_Click;
			break;
		case 11:
			StartButton = (System.Windows.Controls.Button)target;
			StartButton.Click += StartCleaner_Click;
			break;
		case 12:
			StopButton = (System.Windows.Controls.Button)target;
			StopButton.Click += StopCleaner_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
