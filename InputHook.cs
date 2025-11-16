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

        private static IntPtr _keyboardHookId = IntPtr.Zero;
        private static LowLevelKeyboardProc _keyboardProc = KeyboardHookCallback;

        public delegate void KeyboardCapturedHandler(Keys key);
        public static event KeyboardCapturedHandler KeyboardCaptured;

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

                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    var kbStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                    Keys key = (Keys)kbStruct.vkCode;

                    KeyboardCaptured?.Invoke(key);
                    // we let the key pass through normally:
                    // return (IntPtr)1; // would swallow it
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
