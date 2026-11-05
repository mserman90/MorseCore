using System.Runtime.InteropServices;

namespace MorseCore.AppCore.Radio;

public sealed record SerialKeyerConfig(
    string PortName,
    bool UseDtr = true,
    bool UseRts = false,
    bool Invert = false
);

public interface ISerialPortKeyer : IDisposable
{
    bool IsOpen { get; }
    void Open();
    void Close();
    void SetKeyDown(bool down);
    Task SendMorseAsync(string morseCode, int wpm = 20, CancellationToken cancellationToken = default);
}

public sealed partial class SerialPortKeyer : ISerialPortKeyer
{
    private readonly SerialKeyerConfig _config;
    private nint _handle = -1;
    private bool _isDisposed;

    public bool IsOpen => _handle != -1 && _handle != 0;

    public SerialKeyerConfig Config => _config;

    public SerialPortKeyer(SerialKeyerConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public static IReadOnlyList<string> GetAvailablePorts()
    {
        if (OperatingSystem.IsWindows())
        {
            var ports = new List<string>();
            for (int i = 1; i <= 64; i++)
            {
                string name = $"COM{i}";
                nint handle = Win32.CreateFile(
                    @"\\.\" + name,
                    Win32.GENERIC_READ | Win32.GENERIC_WRITE,
                    0,
                    0,
                    Win32.OPEN_EXISTING,
                    0,
                    0
                );

                if (handle != -1)
                {
                    ports.Add(name);
                    Win32.CloseHandle(handle);
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    // ERROR_ACCESS_DENIED (5) means the port exists and is currently open/busy
                    if (error == 5) ports.Add(name);
                }
            }
            return ports.AsReadOnly();
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var ports = new List<string>();
            try
            {
                if (Directory.Exists("/dev"))
                {
                    ports.AddRange(Directory.GetFiles("/dev", "ttyUSB*"));
                    ports.AddRange(Directory.GetFiles("/dev", "ttyACM*"));
                    ports.AddRange(Directory.GetFiles("/dev", "cu.usb*"));
                }
            }
            catch (Exception) { }
            return ports.AsReadOnly();
        }

        return Array.Empty<string>();
    }

    public void Open()
    {
        if (IsOpen) return;

        if (OperatingSystem.IsWindows())
        {
            string portPath = _config.PortName.StartsWith(@"\\.\", StringComparison.OrdinalIgnoreCase)
                ? _config.PortName
                : @"\\.\" + _config.PortName;

            _handle = Win32.CreateFile(
                portPath,
                Win32.GENERIC_READ | Win32.GENERIC_WRITE,
                0,
                0,
                Win32.OPEN_EXISTING,
                0,
                0
            );

            if (_handle == -1)
                throw new InvalidOperationException($"Failed to open serial port '{_config.PortName}'. Error code: {Marshal.GetLastWin32Error()}");
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            _handle = Posix.open(_config.PortName, Posix.O_RDWR | Posix.O_NOCTTY | Posix.O_NONBLOCK);
            if (_handle == -1)
                throw new InvalidOperationException($"Failed to open serial port '{_config.PortName}'.");
        }

        SetKeyDown(false);
    }

    public void Close()
    {
        if (!IsOpen) return;

        try { SetKeyDown(false); } catch { }

        if (OperatingSystem.IsWindows() && _handle != -1)
        {
            Win32.CloseHandle(_handle);
        }
        else if ((OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()) && _handle != -1)
        {
            Posix.close((int)_handle);
        }

        _handle = -1;
    }

    public void SetKeyDown(bool down)
    {
        if (!IsOpen) return;

        bool active = _config.Invert ? !down : down;

        if (OperatingSystem.IsWindows())
        {
            if (_config.UseDtr)
                Win32.EscapeCommFunction(_handle, active ? Win32.SETDTR : Win32.CLRDTR);
            if (_config.UseRts)
                Win32.EscapeCommFunction(_handle, active ? Win32.SETRTS : Win32.CLRRTS);
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            int flags = 0;
            if (_config.UseDtr) flags |= Posix.TIOCM_DTR;
            if (_config.UseRts) flags |= Posix.TIOCM_RTS;

            if (flags != 0)
            {
                int request = active ? Posix.TIOCMBIS : Posix.TIOCMBIC;
                Posix.ioctl((int)_handle, request, ref flags);
            }
        }
    }

    public async Task SendMorseAsync(string morseCode, int wpm = 20, CancellationToken cancellationToken = default)
    {
        if (!IsOpen) Open();

        wpm = Math.Clamp(wpm, 5, 60);
        int unitMs = 1200 / wpm;

        foreach (char ch in morseCode)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (ch)
            {
                case '.':
                    SetKeyDown(true);
                    await Task.Delay(unitMs, cancellationToken);
                    SetKeyDown(false);
                    await Task.Delay(unitMs, cancellationToken);
                    break;

                case '-':
                    SetKeyDown(true);
                    await Task.Delay(unitMs * 3, cancellationToken);
                    SetKeyDown(false);
                    await Task.Delay(unitMs, cancellationToken);
                    break;

                case ' ':
                    // Character space (3 units total, 1 already waited after previous symbol)
                    await Task.Delay(unitMs * 2, cancellationToken);
                    break;

                case '/':
                    // Word space (7 units total, 3 already waited)
                    await Task.Delay(unitMs * 4, cancellationToken);
                    break;
            }
        }

        SetKeyDown(false);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            Close();
            _isDisposed = true;
        }
    }

    private static class Win32
    {
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint OPEN_EXISTING = 3;

        public const uint SETRTS = 3;
        public const uint CLRRTS = 4;
        public const uint SETDTR = 5;
        public const uint CLRDTR = 6;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern nint CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            nint lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            nint hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(nint hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EscapeCommFunction(nint hFile, uint dwFunc);
    }

    private static class Posix
    {
        public const int O_RDWR = 0x0002;
        public const int O_NOCTTY = 0x0100;
        public const int O_NONBLOCK = 0x0800;

        public const int TIOCM_DTR = 0x002;
        public const int TIOCM_RTS = 0x004;
        public const int TIOCMBIS = 0x5416;
        public const int TIOCMBIC = 0x5417;

        [DllImport("libc", SetLastError = true)]
        public static extern nint open(string pathname, int flags);

        [DllImport("libc", SetLastError = true)]
        public static extern int close(int fd);

        [DllImport("libc", SetLastError = true)]
        public static extern int ioctl(int fd, int request, ref int arg);
    }
}
