using SharpGen.Runtime;
using System;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.Win32;

namespace PixDirectX.Rendering.DirectX
{
    internal unsafe static partial class Win32Native
    {
        [DllImport("user32")]
        public static extern RawBool PeekMessageW(NativeMessage* lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32")]
        public static extern int GetMessageW(NativeMessage* lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32")]
        public static extern RawBool TranslateMessage(NativeMessage* lpMsg);

        [DllImport("user32")]
        public static extern /*LRESULT*/IntPtr DispatchMessageW(NativeMessage* lpMsg);

        [DllImport("user32")]
        public static extern RawBool GetClientRect(IntPtr hWnd, RawRect* lpRect);
    }
}
