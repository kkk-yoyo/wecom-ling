using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace WebServerConsole.Utils;

public static class WinApi
{
	public delegate bool ConsoleCtrlDelegate(int ctrlType);

	public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	public struct PROCESSENTRY32
	{
		public uint dwSize;

		public uint cntUsage;

		public uint th32ProcessID;

		public IntPtr th32DefaultHeapID;

		public uint th32ModuleID;

		public uint cntThreads;

		public uint th32ParentProcessID;

		public int pcPriClassBase;

		public uint dwFlags;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szExeFile;
	}

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int CallbackDelegate(IntPtr param1, IntPtr param2, IntPtr param3);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate int DllFunctionDelegate(params IntPtr[] parameters);

	public const int SW_HIDE = 0;

	public const int SW_SHOW = 5;

	public const int SW_MINIMIZE = 6;

	public const int SW_RESTORE = 9;

	public const int STD_INPUT_HANDLE = -10;

	public const int STD_OUTPUT_HANDLE = -11;

	public const int STD_ERROR_HANDLE = -12;

	public const uint GENERIC_READ = 2147483648u;

	public const uint GENERIC_WRITE = 1073741824u;

	public const uint FILE_SHARE_READ = 1u;

	public const uint FILE_SHARE_WRITE = 2u;

	public const uint OPEN_EXISTING = 3u;

	public const uint SC_CLOSE = 61536u;

	public const uint MF_BYCOMMAND = 0u;

	public const int CTRL_C_EVENT = 0;

	public const int CTRL_BREAK_EVENT = 1;

	public const int CTRL_CLOSE_EVENT = 2;

	public const int CTRL_LOGOFF_EVENT = 5;

	public const int CTRL_SHUTDOWN_EVENT = 6;

	private static IntPtr _consoleHandle = IntPtr.Zero;

	private static bool _isConsoleVisible = false;

	private static bool _isLogConsoleCreated = false;

	private static ConsoleCtrlDelegate _consoleCtrlHandler;

	public const uint PAGE_READWRITE = 4u;

	public const uint FILE_MAP_ALL_ACCESS = 983071u;

	public const uint PROCESS_ALL_ACCESS = 2035711u;

	public const uint MEM_COMMIT = 4096u;

	public const uint MEM_RESERVE = 8192u;

	public const uint PAGE_EXECUTE_READWRITE = 64u;

	public const uint PROCESS_QUERY_INFORMATION = 1024u;

	public const uint PROCESS_VM_READ = 16u;

	public const uint TH32CS_SNAPPROCESS = 2u;

	public static bool IsLogConsoleCreated => _isLogConsoleCreated;

	public static bool IsConsoleVisible => _isConsoleVisible;

	[DllImport("kernel32.dll")]
	public static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll")]
	public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("kernel32.dll")]
	public static extern bool AllocConsole();

	[DllImport("kernel32.dll")]
	public static extern bool FreeConsole();

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
	public static extern bool SetConsoleTitle(string lpConsoleTitle);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr GetStdHandle(int nStdHandle);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

	[DllImport("user32.dll")]
	public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

	[DllImport("user32.dll")]
	public static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handler, bool add);

	private static bool ConsoleCtrlHandler(int ctrlType)
	{
		if (ctrlType == 2)
		{
			HideConsole();
			return true;
		}
		return false;
	}

	public static bool CreateLogConsole(string title = "日志控制台", bool showImmediately = false)
	{
		if (_isLogConsoleCreated)
		{
			if (showImmediately && !_isConsoleVisible)
			{
				ShowConsole();
			}
			return true;
		}
		if (!AllocConsole())
		{
			return false;
		}
		SetConsoleTitle(title);
		Console.OutputEncoding = Encoding.UTF8;
		Console.InputEncoding = Encoding.UTF8;
		IntPtr intPtr = CreateFile("CONOUT$", 3221225472u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
		if (intPtr != new IntPtr(-1))
		{
			StreamWriter obj = new StreamWriter(new FileStream(new SafeFileHandle(intPtr, ownsHandle: false), FileAccess.Write), Encoding.UTF8)
			{
				AutoFlush = true
			};
			Console.SetOut(obj);
			Console.SetError(obj);
			SetStdHandle(-11, intPtr);
			SetStdHandle(-12, intPtr);
		}
		IntPtr intPtr2 = CreateFile("CONIN$", 2147483648u, 1u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
		if (intPtr2 != new IntPtr(-1))
		{
			Console.SetIn(new StreamReader(new FileStream(new SafeFileHandle(intPtr2, ownsHandle: false), FileAccess.Read)));
			SetStdHandle(-10, intPtr2);
		}
		_consoleHandle = GetConsoleWindow();
		_isLogConsoleCreated = true;
		DisableConsoleCloseButton();
		_consoleCtrlHandler = ConsoleCtrlHandler;
		SetConsoleCtrlHandler(_consoleCtrlHandler, add: true);
		if (!showImmediately)
		{
			ShowWindow(_consoleHandle, 0);
			_isConsoleVisible = false;
		}
		else
		{
			_isConsoleVisible = true;
		}
		return true;
	}

	private static void DisableConsoleCloseButton()
	{
		if (_consoleHandle == IntPtr.Zero)
		{
			_consoleHandle = GetConsoleWindow();
		}
		if (_consoleHandle != IntPtr.Zero)
		{
			IntPtr systemMenu = GetSystemMenu(_consoleHandle, bRevert: false);
			if (systemMenu != IntPtr.Zero)
			{
				DeleteMenu(systemMenu, 61536u, 0u);
			}
		}
	}

	public static void CloseLogConsole()
	{
		if (_isLogConsoleCreated)
		{
			if (_consoleCtrlHandler != null)
			{
				SetConsoleCtrlHandler(_consoleCtrlHandler, add: false);
				_consoleCtrlHandler = null;
			}
			FreeConsole();
			_isLogConsoleCreated = false;
			_isConsoleVisible = false;
			_consoleHandle = IntPtr.Zero;
		}
	}

	public static void HideConsole()
	{
		if (_isLogConsoleCreated)
		{
			if (_consoleHandle == IntPtr.Zero)
			{
				_consoleHandle = GetConsoleWindow();
			}
			if (_consoleHandle != IntPtr.Zero)
			{
				ShowWindow(_consoleHandle, 0);
				_isConsoleVisible = false;
			}
		}
	}

	public static void ShowConsole()
	{
		if (!_isLogConsoleCreated)
		{
			CreateLogConsole("日志控制台", showImmediately: true);
			return;
		}
		if (_consoleHandle == IntPtr.Zero)
		{
			_consoleHandle = GetConsoleWindow();
		}
		if (_consoleHandle != IntPtr.Zero)
		{
			ShowWindow(_consoleHandle, 5);
			_isConsoleVisible = true;
		}
	}

	public static bool ToggleConsole()
	{
		if (_isConsoleVisible)
		{
			HideConsole();
		}
		else
		{
			ShowConsole();
		}
		return _isConsoleVisible;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
	public static extern IntPtr LoadLibraryA(string lpFileName);

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
	public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

	[DllImport("kernel32.dll")]
	public static extern bool FreeLibrary(IntPtr hModule);

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
	public static extern IntPtr CreateFileMappingA(IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
	public static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

	[DllImport("kernel32.dll")]
	public static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

	[DllImport("kernel32.dll")]
	public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

	[DllImport("kernel32.dll")]
	public static extern bool CloseHandle(IntPtr hObject);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int GetWindowTextLength(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
	public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

	[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
	public static extern void CopyMemory(IntPtr dest, byte[] src, uint count);

	[DllImport("kernel32.dll")]
	public static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

	[DllImport("kernel32.dll")]
	public static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, uint dwFreeType);

	[DllImport("kernel32.dll")]
	public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

	[DllImport("kernel32.dll")]
	public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

	[DllImport("kernel32.dll")]
	public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesWritten);

	[DllImport("kernel32.dll")]
	public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

	[DllImport("kernel32.dll")]
	public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

	[DllImport("kernel32.dll")]
	public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
	public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
	public static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

	[DllImport("psapi.dll", SetLastError = true)]
	public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, StringBuilder lpBaseName, uint nSize);
}
