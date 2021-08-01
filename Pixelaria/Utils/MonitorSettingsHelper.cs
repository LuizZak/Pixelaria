using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Source code lifted from https://github.com/rickbrew/RefreshRateWpf

namespace Pixelaria.Utils
{
    public static class MonitorSettingsHelper
    {
        /// <summary>
        /// Attempts to get the refresh rate of the monitor that contains a given Win32 form.
        /// </summary>
        /// <param name="form">The form to detect the monitor it is contained within</param>
        /// <returns>A display frequency in Hz, or <c>null</c>, in case an error occurred while retrieving the information</returns>
        public static int? GetRefreshRateForForm(Form form)
        {
            // 1. Get a monitor handle ("HMONITOR") for the window. 
            //    If the window is straddling more than one monitor, Windows will pick the "best" one.
            var hMonitor = MonitorFromWindow(form.Handle, MONITOR_DEFAULTTONEAREST);
            if (hMonitor == IntPtr.Zero)
                return null;

            // 2. Get more information about the monitor.
            var monitorInfo = new MONITORINFOEXW
            {
                cbSize = (uint)Marshal.SizeOf<MONITORINFOEXW>()
            };

            bool bResult = GetMonitorInfoW(hMonitor, ref monitorInfo);
            if (!bResult)
                return null;

            // 3. Get the current display settings for that monitor, which includes the resolution and refresh rate.
            bResult = EnumDisplaySettingsW(monitorInfo.szDevice, ENUM_CURRENT_SETTINGS, out var devMode);
            if (!bResult)
                return null;

            return (int)devMode.dmDisplayFrequency;
        }

        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo
        // ReSharper disable MemberCanBePrivate.Local

        // MonitorFromWindow
        private const uint MONITOR_DEFAULTTONEAREST = 2;
        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        // RECT
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        // MONITORINFOEX
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private unsafe struct MONITORINFOEXW
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        // GetMonitorInfo
        [DllImport("user32.dll", SetLastError = false, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfoW(
            IntPtr hMonitor,
            ref MONITORINFOEXW lpmi);

        // EnumDisplaySettings
        private const uint ENUM_CURRENT_SETTINGS = unchecked((uint)-1);

        [DllImport("user32.dll", SetLastError = false, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumDisplaySettingsW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszDeviceName,
            uint iModeNum,
            out DEVMODEW lpDevMode);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DEVMODEW
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;

            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;

            /*public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;
            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;*/
            // These next 4 int fields are a union with the above 8 shorts, but we don't need them right now
            public int dmPositionX;
            public int dmPositionY;
            public uint dmDisplayOrientation;
            public uint dmDisplayFixedOutput;

            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;

            public short dmLogPixels;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;

            public uint dmNupOrDisplayFlags;
            public uint dmDisplayFrequency;

            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;
            public uint dmPanningWidth;
            public uint dmPanningHeight;
        }

        // ReSharper restore InconsistentNaming
        // ReSharper restore IdentifierTypo
        // ReSharper restore MemberCanBePrivate.Local
    }
}
