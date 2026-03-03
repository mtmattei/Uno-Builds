using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;

namespace AgentNotifier.Services;

public class SystemTrayService : IDisposable
{
    private Window? _window;
    private bool _disposed;
    private bool _isIconCreated;
    private const int WM_USER = 0x0400;
    private const int WM_TRAYICON = WM_USER + 1;
    private const int NIM_ADD = 0x00000000;
    private const int NIM_MODIFY = 0x00000001;
    private const int NIM_DELETE = 0x00000002;
    private const int NIF_MESSAGE = 0x00000001;
    private const int NIF_ICON = 0x00000002;
    private const int NIF_TIP = 0x00000004;
    private const int NIF_INFO = 0x00000010;
    private const int NIIF_INFO = 0x00000001;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public int dwState;
        public int dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public int uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public int dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    private NOTIFYICONDATA _notifyIconData;
    private IntPtr _windowHandle;
    private IntPtr _iconHandle;

    public void Initialize(Window window)
    {
        _window = window;

        // Get the window handle
        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        _windowHandle = windowHandle;

        // Load a default application icon
        _iconHandle = LoadIcon(IntPtr.Zero, (IntPtr)32512); // IDI_APPLICATION

        // Create notify icon data
        _notifyIconData = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _windowHandle,
            uID = 1,
            uFlags = NIF_ICON | NIF_TIP | NIF_MESSAGE,
            uCallbackMessage = WM_TRAYICON,
            hIcon = _iconHandle,
            szTip = "Agent Notifier - Claude Agent Swarm Dashboard",
            szInfo = string.Empty,
            szInfoTitle = string.Empty
        };

        // Add the icon to system tray
        _isIconCreated = Shell_NotifyIcon(NIM_ADD, ref _notifyIconData);

        if (_isIconCreated)
        {
            System.Diagnostics.Debug.WriteLine("System tray icon created successfully");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Failed to create system tray icon");
        }
    }

    public void ShowWindow()
    {
        if (_window == null) return;

        _window.Activate();

        // Restore window if minimized
        if (_window.AppWindow?.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            if (presenter.State == Microsoft.UI.Windowing.OverlappedPresenterState.Minimized)
            {
                presenter.Restore();
            }
        }
    }

    public void HideWindow()
    {
        if (_window?.AppWindow?.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.Minimize();
        }
    }

    public void UpdateToolTip(string text)
    {
        if (!_isIconCreated) return;

        _notifyIconData.szTip = text.Length > 127 ? text.Substring(0, 127) : text;
        _notifyIconData.uFlags = NIF_TIP;
        Shell_NotifyIcon(NIM_MODIFY, ref _notifyIconData);
    }

    public void ShowBalloonTip(string title, string message)
    {
        if (!_isIconCreated) return;

        _notifyIconData.uFlags = NIF_INFO;
        _notifyIconData.szInfoTitle = title.Length > 63 ? title.Substring(0, 63) : title;
        _notifyIconData.szInfo = message.Length > 255 ? message.Substring(0, 255) : message;
        _notifyIconData.dwInfoFlags = NIIF_INFO;
        Shell_NotifyIcon(NIM_MODIFY, ref _notifyIconData);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_isIconCreated)
        {
            Shell_NotifyIcon(NIM_DELETE, ref _notifyIconData);
            _isIconCreated = false;
        }

        if (_iconHandle != IntPtr.Zero)
        {
            DestroyIcon(_iconHandle);
            _iconHandle = IntPtr.Zero;
        }
    }
}
