using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CJUMP
{
    internal static class InputHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYUP = 0x0105;

        private static IntPtr _keyboardHookId = IntPtr.Zero;
        private static LowLevelKeyboardProc _keyboardProc = KeyboardHookCallback;

        public delegate void KeyboardCapturedHandler(Keys key);
        public static event KeyboardCapturedHandler KeyboardCaptured;

        public delegate void KeyboardReleasedHandler(Keys key);
        public static event KeyboardReleasedHandler KeyboardReleased;

        public static void StartKeyboardCapture()
        {
            if (_keyboardHookId != IntPtr.Zero)
                return;

            _keyboardHookId = SetWindowsHookEx(
                WH_KEYBOARD_LL,
                _keyboardProc,
                IntPtr.Zero,
                0
            );
        }

        public static void StopKeyboardCapture()
        {
            if (_keyboardHookId == IntPtr.Zero)
                return;

            UnhookWindowsHookEx(_keyboardHookId);
            _keyboardHookId = IntPtr.Zero;
        }

        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();

                var kbStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                // ignore events that were injected by SendInput or other injection methods
                const uint LLKHF_INJECTED = 0x10;
                if ((kbStruct.flags & LLKHF_INJECTED) != 0)
                {
                    return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
                }

                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    Keys key = (Keys)kbStruct.vkCode;
                    KeyboardCaptured?.Invoke(key);
                }
                else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
                {
                    Keys key = (Keys)kbStruct.vkCode;
                    KeyboardReleased?.Invoke(key);
                }
            }

            return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        // ===== Interop =====

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            uint dwThreadId
        );

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam
        );
    }
}
