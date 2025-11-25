/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Vortice.Win32;

// ReSharper disable InconsistentNaming

namespace Pixelaria.Views.Direct2D
{
    public class PxlRenderForm : Form
    {
        private const int WM_SIZE = 0x0005;
        private const int SIZE_RESTORED = 0;
        private const int SIZE_MINIMIZED = 1;
        private const int SIZE_MAXIMIZED = 2;
        private const int WM_ACTIVATEAPP = 0x001C;
        private const int WM_POWERBROADCAST = 0x0218;
        private const int WM_MENUCHAR = 0x0120;
        private const int WM_SYSCOMMAND = 0x0112;
        private const uint PBT_APMRESUMESUSPEND = 7;
        private const uint PBT_APMQUERYSUSPEND = 0;
        private const int SC_MONITORPOWER = 0xF170;
        private const int SC_SCREENSAVE = 0xF140;
        private const int WM_DISPLAYCHANGE = 0x007E;
        private const int MNC_CLOSE = 1;
        private Size cachedSize;
        private FormWindowState previousWindowState;
        //private DisplayMonitor monitor;
        private bool isUserResizing;
        private bool allowUserResizing;
        private bool isBackgroundFirstDraw;
        private bool isSizeChangedWithoutResizeBegin;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PxlRenderForm"/> class.
        /// </summary>
        public PxlRenderForm()
        {
            ResizeRedraw = true;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            
            previousWindowState = FormWindowState.Normal;
            AllowUserResizing = true;
        }

        /// <summary>
        /// Occurs when [app activated].
        /// </summary>
        public event EventHandler<EventArgs> AppActivated;

        /// <summary>
        /// Occurs when [app deactivated].
        /// </summary>
        public event EventHandler<EventArgs> AppDeactivated;

        /// <summary>
        /// Occurs when [monitor changed].
        /// </summary>
        public event EventHandler<EventArgs> MonitorChanged;

        /// <summary>
        /// Occurs when [pause rendering].
        /// </summary>
        public event EventHandler<EventArgs> PauseRendering;

        /// <summary>
        /// Occurs when [resume rendering].
        /// </summary>
        public event EventHandler<EventArgs> ResumeRendering;

        /// <summary>
        /// Occurs when [screensaver].
        /// </summary>
        public event EventHandler<CancelEventArgs> Screensaver;

        /// <summary>
        /// Occurs when [system resume].
        /// </summary>
        public event EventHandler<EventArgs> SystemResume;

        /// <summary>
        /// Occurs when [system suspend].
        /// </summary>
        public event EventHandler<EventArgs> SystemSuspend;

        /// <summary>
        /// Occurs when [user resized].
        /// </summary>
        public event EventHandler<EventArgs> UserResized;

        /// <summary>
        /// Gets or sets a value indicating whether this form can be resized by the user. See remarks.
        /// </summary>
        /// <remarks>
        /// This property alters <see cref="Form.FormBorderStyle"/>, 
        /// for <c>true</c> value it is <see cref="FormBorderStyle.Sizable"/>, 
        /// for <c>false</c> - <see cref="FormBorderStyle.FixedSingle"/>.
        /// </remarks>
        /// <value><c>true</c> if this form can be resized by the user (by default); otherwise, <c>false</c>.</value>
        public bool AllowUserResizing
        {
            get => allowUserResizing;
            set
            {
                if (allowUserResizing == value)
                    return;

                allowUserResizing = value;
                MaximizeBox = allowUserResizing;
                FormBorderStyle = IsFullscreen
                    ? FormBorderStyle.None
                    : allowUserResizing ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
            }
        }

        /// <summary>
        /// Gets or sets a value indicationg whether the current render form is in fullscreen mode. See remarks.
        /// </summary>
        /// <remarks>
        /// If Toolkit is used, this property is set automatically,
        /// otherwise user should maintain it himself as it affects the behavior of <see cref="AllowUserResizing"/> property.
        /// </remarks>
        public bool IsFullscreen { get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.ResizeBegin"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResizeBegin(EventArgs e)
        {
            isUserResizing = true;

            base.OnResizeBegin(e);
            cachedSize = Size;
            OnPauseRendering(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.ResizeEnd"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            if (isUserResizing && cachedSize != Size)
            {
                OnUserResized(e);
            }

            isUserResizing = false;
            OnResumeRendering(e);
        }
        
        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!isBackgroundFirstDraw)
            {
                base.OnPaintBackground(e);
                isBackgroundFirstDraw = true;
            }
        }

        /// <summary>
        /// Raises the Pause Rendering event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnPauseRendering(EventArgs e)
        {
            PauseRendering?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the Resume Rendering event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnResumeRendering(EventArgs e)
        {
            ResumeRendering?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the User resized event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnUserResized(EventArgs e)
        {
            UserResized?.Invoke(this, e);
        }

        private void OnMonitorChanged(EventArgs e)
        {
            MonitorChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the On App Activated event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnAppActivated(EventArgs e)
        {
            AppActivated?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the App Deactivated event
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnAppDeactivated(EventArgs e)
        {
            AppDeactivated?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the System Suspend event
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnSystemSuspend(EventArgs e)
        {
            SystemSuspend?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the System Resume event
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnSystemResume(EventArgs e)
        {
            SystemResume?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:Screensaver"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void OnScreensaver(CancelEventArgs e)
        {
            Screensaver?.Invoke(this, e);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (!isUserResizing && (isSizeChangedWithoutResizeBegin || cachedSize != Size))
            {
                isSizeChangedWithoutResizeBegin = false;
                cachedSize = Size;
                OnUserResized(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Override windows message loop handling.
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message"/> to process.</param>
        protected override void WndProc(ref Message m)
        {
            long wparam = m.WParam.ToInt64();

            switch (m.Msg)
            {
                case WM_SIZE:
                    if (wparam == SIZE_MINIMIZED)
                    {
                        previousWindowState = FormWindowState.Minimized;
                        OnPauseRendering(EventArgs.Empty);
                    }
                    else
                    {

                        UnsafeNativeMethods.GetClientRect(m.HWnd, out var rect);
                        if (rect.Bottom - rect.Top == 0)
                        {
                            // Rapidly clicking the task bar to minimize and restore a window
                            // can cause a WM_SIZE message with SIZE_RESTORED when 
                            // the window has actually become minimized due to rapid change
                            // so just ignore this message
                        }
                        else if (wparam == SIZE_MAXIMIZED)
                        {
                            if (previousWindowState == FormWindowState.Minimized)
                                OnResumeRendering(EventArgs.Empty);

                            previousWindowState = FormWindowState.Maximized;

                            OnUserResized(EventArgs.Empty);
                            //UpdateScreen();
                            cachedSize = Size;
                        }
                        else if (wparam == SIZE_RESTORED)
                        {
                            if (previousWindowState == FormWindowState.Minimized)
                                OnResumeRendering(EventArgs.Empty);

                            if (!isUserResizing && (Size != cachedSize || previousWindowState == FormWindowState.Maximized))
                            {
                                previousWindowState = FormWindowState.Normal;

                                // Only update when cachedSize is != 0
                                if (cachedSize != Size.Empty)
                                {
                                    isSizeChangedWithoutResizeBegin = true;
                                }
                            }
                            else
                                previousWindowState = FormWindowState.Normal;
                        }
                    }
                    break;
                case WM_ACTIVATEAPP:
                    if (wparam != 0)
                        OnAppActivated(EventArgs.Empty);
                    else
                        OnAppDeactivated(EventArgs.Empty);
                    break;
                case WM_POWERBROADCAST:
                    if (wparam == PBT_APMQUERYSUSPEND)
                    {
                        OnSystemSuspend(EventArgs.Empty);
                        m.Result = new IntPtr(1);
                        return;
                    }
                    else if (wparam == PBT_APMRESUMESUSPEND)
                    {
                        OnSystemResume(EventArgs.Empty);
                        m.Result = new IntPtr(1);
                        return;
                    }
                    break;
                case WM_MENUCHAR:
                    m.Result = new IntPtr(MNC_CLOSE << 16); // IntPtr(MAKELRESULT(0, MNC_CLOSE));
                    return;
                case WM_SYSCOMMAND:
                    wparam &= 0xFFF0;
                    if (wparam == SC_MONITORPOWER || wparam == SC_SCREENSAVE)
                    {
                        var e = new CancelEventArgs();
                        OnScreensaver(e);
                        if (e.Cancel)
                        {
                            m.Result = IntPtr.Zero;
                            return;
                        }
                    }
                    break;
                case WM_DISPLAYCHANGE:
                    OnMonitorChanged(EventArgs.Empty);
                    break;
            }

            base.WndProc(ref m);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == (Keys.Menu | Keys.Alt) || keyData == Keys.F10)
                return true;

            return base.ProcessDialogKey(keyData);
        }
    }

    /// <summary>
    /// Internal class to interact with Native Message
    /// </summary>
    internal class UnsafeNativeMethods
    {
        [DllImport("kernel32.dll", EntryPoint = "CreateFile", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr Create(
            string fileName,
            NativeFileAccess desiredAccess,
            NativeFileShare shareMode,
            IntPtr securityAttributes,
            NativeFileMode mode,
            NativeFileOptions flagsAndOptions,
            IntPtr templateFile);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TextMetric
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }


        [DllImport("user32.dll", EntryPoint = "PeekMessage")]
        public static extern IntPtr PeekMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin,
            int wMsgFilterMax, int wRemoveMsg);

        [DllImport("user32.dll", EntryPoint = "GetMessage")]
        public static extern IntPtr GetMessage(out NativeMessage lpMsg, IntPtr hWnd, int wMsgFilterMin,
            int wMsgFilterMax);

        [DllImport("user32.dll", EntryPoint = "TranslateMessage")]
        public static extern IntPtr TranslateMessage(ref NativeMessage lpMsg);

        [DllImport("user32.dll", EntryPoint = "DispatchMessage")]
        public static extern IntPtr DispatchMessage(ref NativeMessage lpMsg);

        public enum WindowLongType
        {
            WndProc = -4,
            HInstance = -6,
            HwndParent = -8,
            Style = -16,
            ExtendedStyle = -20,
            UserData = -21,
            Id = -12
        }

        public delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static IntPtr GetWindowLong(IntPtr hWnd, WindowLongType index)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, index);
            }
            return GetWindowLong64(hWnd, index);
        }

        [DllImport("user32.dll", EntryPoint = "GetFocus", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLong32(IntPtr hwnd, WindowLongType index);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLong64(IntPtr hwnd, WindowLongType index);

        public static IntPtr SetWindowLong(IntPtr hwnd, WindowLongType index, IntPtr wndProcPtr)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLong32(hwnd, index, wndProcPtr);
            }
            return SetWindowLongPtr64(hwnd, index, wndProcPtr);
        }

        [DllImport("user32.dll", EntryPoint = "SetParent", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetWindowLong32(IntPtr hwnd, WindowLongType index, IntPtr wndProc);
        
        public static bool ShowWindow(IntPtr hWnd, bool windowVisible)
        {
            return ShowWindow(hWnd, windowVisible ? 1 : 0) != IntPtr.Zero;
        }

        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Unicode)]
        private static extern IntPtr ShowWindow(IntPtr hWnd, int mCmdShow);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hwnd, WindowLongType index, IntPtr wndProc);

        [DllImport("user32.dll", EntryPoint = "CallWindowProc", CharSet = CharSet.Unicode)]
        public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetClientRect")]
        public static extern IntPtr GetClientRect(IntPtr hWnd, out RawRectangle lpRect);

        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandle", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    /// <summary>
    /// Native File access flags.
    /// </summary>
    [Flags]
    internal enum NativeFileAccess : uint
    {
        /// <summary>
        /// Read access.
        /// </summary>
        Read = 0x80000000,

        /// <summary>
        /// Write access.
        /// </summary>
        Write = 0x40000000,

        /// <summary>
        /// Read/Write Access,
        /// </summary>
        ReadWrite = Read | Write,

        /// <summary>
        /// Execute access.
        /// </summary>
        Execute = 0x20000000,

        /// <summary>
        /// All access
        /// </summary>
        All = 0x10000000
    }

    /// <summary>
    /// Native file share.
    /// </summary>
    [Flags]
    internal enum NativeFileShare : uint
    {
        /// <summary>
        /// None flag.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// Enables subsequent open operations on an object to request read access.
        /// Otherwise, other processes cannot open the object if they request read access.
        /// If this flag is not specified, but the object has been opened for read access, the function fails.
        /// </summary>
        Read = 0x00000001,

        /// <summary>
        /// Enables subsequent open operations on an object to request write access.
        /// Otherwise, other processes cannot open the object if they request write access.
        /// If this flag is not specified, but the object has been opened for write access, the function fails.
        /// </summary>
        Write = 0x00000002,

        /// <summary>
        /// Read and Write flags.
        /// </summary>
        ReadWrite = Read | Write,

        /// <summary>
        /// Enables subsequent open operations on an object to request delete access.
        /// Otherwise, other processes cannot open the object if they request delete access.
        /// If this flag is not specified, but the object has been opened for delete access, the function fails.
        /// </summary>
        Delete = 0x00000004
    }

    /// <summary>
    /// Native file creation disposition.
    /// </summary>
    internal enum NativeFileMode : uint
    {
        /// <summary>
        /// Creates a new file. The function fails if a specified file exists.
        /// </summary>
        CreateNew = 1,

        /// <summary>
        /// Creates a new file, always.
        /// If a file exists, the function overwrites the file, clears the existing attributes, combines the specified file attributes,
        /// and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security descriptor that the SECURITY_ATTRIBUTES structure specifies.
        /// </summary>
        Create = 2,

        /// <summary>
        /// Opens a file. The function fails if the file does not exist.
        /// </summary>
        Open = 3,

        /// <summary>
        /// Opens a file, always.
        /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
        /// </summary>
        OpenOrCreate = 4,

        /// <summary>
        /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does not exist.
        /// The calling process must open the file with the GENERIC_WRITE access right.
        /// </summary>
        Truncate = 5
    }

    /// <summary>
    /// Native file attributes.
    /// </summary>
    [Flags]
    internal enum NativeFileOptions : uint
    {
        /// <summary>
        /// None attribute.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// Read only attribute.
        /// </summary>
        Readonly = 0x00000001,

        /// <summary>
        /// Hidden attribute.
        /// </summary>
        Hidden = 0x00000002,

        /// <summary>
        /// System attribute.
        /// </summary>
        System = 0x00000004,

        /// <summary>
        /// Directory attribute.
        /// </summary>
        Directory = 0x00000010,

        /// <summary>
        /// Archive attribute.
        /// </summary>
        Archive = 0x00000020,

        /// <summary>
        /// Device attribute.
        /// </summary>
        Device = 0x00000040,

        /// <summary>
        /// Normal attribute.
        /// </summary>
        Normal = 0x00000080,

        /// <summary>
        /// Temporary attribute.
        /// </summary>
        Temporary = 0x00000100,

        /// <summary>
        /// Sparse file attribute.
        /// </summary>
        SparseFile = 0x00000200,

        /// <summary>
        /// ReparsePoint attribute.
        /// </summary>
        ReparsePoint = 0x00000400,

        /// <summary>
        /// Compressed attribute.
        /// </summary>
        Compressed = 0x00000800,

        /// <summary>
        /// Offline attribute.
        /// </summary>
        Offline = 0x00001000,

        /// <summary>
        /// Not content indexed attribute.
        /// </summary>
        NotContentIndexed = 0x00002000,

        /// <summary>
        /// Encrypted attribute.
        /// </summary>
        Encrypted = 0x00004000,

        /// <summary>
        /// Write through attribute.
        /// </summary>
        Write_Through = 0x80000000,

        /// <summary>
        /// Overlapped attribute.
        /// </summary>
        Overlapped = 0x40000000,

        /// <summary>
        /// No buffering attribute.
        /// </summary>
        NoBuffering = 0x20000000,

        /// <summary>
        /// Random access attribute.
        /// </summary>
        RandomAccess = 0x10000000,

        /// <summary>
        /// Sequential scan attribute.
        /// </summary>
        SequentialScan = 0x08000000,

        /// <summary>
        /// Delete on close attribute.
        /// </summary>
        DeleteOnClose = 0x04000000,

        /// <summary>
        /// Backup semantics attribute.
        /// </summary>
        BackupSemantics = 0x02000000,

        /// <summary>
        /// Post semantics attribute.
        /// </summary>
        PosixSemantics = 0x01000000,

        /// <summary>
        /// Open reparse point attribute.
        /// </summary>
        OpenReparsePoint = 0x00200000,

        /// <summary>
        /// Open no recall attribute.
        /// </summary>
        OpenNoRecall = 0x00100000,

        /// <summary>
        /// First pipe instance attribute.
        /// </summary>
        FirstPipeInstance = 0x00080000
    }

    /// <summary>
    /// Interop type for a Rectangle (4 ints).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [DebuggerDisplay("Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}")]
    internal struct RawRectangle
    {
        public RawRectangle(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// The left position.
        /// </summary>
        public int Left;

        /// <summary>
        /// The top position.
        /// </summary>
        public int Top;

        /// <summary>
        /// The right position
        /// </summary>
        public int Right;

        /// <summary>
        /// The bottom position.
        /// </summary>
        public int Bottom;

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty
        {
            get { return Left == 0 && Top == 0 && Right == 0 && Bottom == 0; }
        }
    }
}
