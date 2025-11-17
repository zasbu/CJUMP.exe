using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CJUMP
{
    internal static class KeyboardSender
    {
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_SCANCODE = 0x0008;

        private const uint MAPVK_VK_TO_VSC = 0;
        private const uint INPUT_KEYBOARD = 1;

        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUTUNION U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, [In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        private static extern UIntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private static void AttachToForegroundThread(out IntPtr fgWindow, out uint originalThreadId)
        {
            fgWindow = GetForegroundWindow();
            originalThreadId = 0;
            if (fgWindow == IntPtr.Zero)
                return;

            uint fgThread = GetWindowThreadProcessId(fgWindow, out _);
            uint currentThread = GetCurrentThreadId();

            if (fgThread != currentThread)
            {
                AttachThreadInput(currentThread, fgThread, true);
                originalThreadId = fgThread;
            }
        }

        private static void DetachFromForegroundThread(uint attachedThreadId)
        {
            if (attachedThreadId == 0)
                return;

            uint currentThread = GetCurrentThreadId();
            AttachThreadInput(currentThread, attachedThreadId, false);
        }

        public static void KeyDownScancodeOnly(Keys key)
        {
            uint sc = GetScanCode(key);
            uint flags = KEYEVENTF_SCANCODE;
            if (IsExtendedKey(key)) flags |= KEYEVENTF_EXTENDEDKEY;

            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)sc,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void KeyUpScancodeOnly(Keys key)
        {
            uint sc = GetScanCode(key);
            uint flags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
            if (IsExtendedKey(key)) flags |= KEYEVENTF_EXTENDEDKEY;

            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)sc,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void KeyDown(Keys key)
        {
            uint sc = GetScanCode(key);
            uint vk = (uint)key;

            uint flags = KEYEVENTF_SCANCODE;
            if (IsExtendedKey(key))
                flags |= KEYEVENTF_EXTENDEDKEY;

            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)sc,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };

            AttachToForegroundThread(out var fg, out var attachedThread);
            try
            {
                Thread.Sleep(1);
                SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));

                if (fg != IntPtr.Zero)
                {
                    int lParam = BuildLParamForKeyDown((int)sc);
                    PostMessage(fg, WM_KEYDOWN, new IntPtr((int)vk), new IntPtr(lParam));
                }

                try
                {
                    keybd_event((byte)vk, 0, 0, UIntPtr.Zero);
                }
                catch { }
            }
            finally
            {
                DetachFromForegroundThread(attachedThread);
            }
        }

        public static void KeyUp(Keys key)
        {
            uint sc = GetScanCode(key);
            uint vk = (uint)key;

            uint flags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
            if (IsExtendedKey(key))
                flags |= KEYEVENTF_EXTENDEDKEY;

            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new INPUTUNION
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = (ushort)sc,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            };

            AttachToForegroundThread(out var fg, out var attachedThread);
            try
            {
                Thread.Sleep(1);
                SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));

                if (fg != IntPtr.Zero)
                {
                    int lParam = BuildLParamForKeyUp((int)sc);
                    PostMessage(fg, WM_KEYUP, new IntPtr((int)vk), new IntPtr(lParam));
                }

                try
                {
                    keybd_event((byte)vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
                catch { }
            }
            finally
            {
                DetachFromForegroundThread(attachedThread);
            }
        }

        private static int BuildLParamForKeyDown(int scanCode)
        {
            int repeatCount = 1;
            int extended = 0;
            int lParam = (repeatCount & 0xFFFF) | ((scanCode & 0xFF) << 16) | (extended << 24);
            return lParam;
        }

        private static int BuildLParamForKeyUp(int scanCode)
        {
            int repeatCount = 1;
            int extended = 0;
            int prevState = 1 << 30;
            int transition = 1 << 31;
            int lParam = (repeatCount & 0xFFFF) | ((scanCode & 0xFF) << 16) | (extended << 24) | prevState | transition;
            return lParam;
        }

        private static uint GetScanCode(Keys key)
        {
            switch (key)
            {
                case Keys.LControlKey: return 0x1D;
                case Keys.RControlKey: return 0x1D;
                case Keys.LShiftKey: return 0x2A;
                case Keys.RShiftKey: return 0x36;
                case Keys.LMenu: return 0x38;
                case Keys.RMenu: return 0x38;
                case Keys.Space: return 0x39;
                case Keys.Enter: return 0x1C;
                case Keys.Tab: return 0x0F;
                case Keys.Back: return 0x0E;
            }

            uint vk = (uint)key;
            uint sc = MapVirtualKey(vk, MAPVK_VK_TO_VSC);
            if (sc == 0)
            {
                return 0;
            }

            return sc;
        }

        private static bool IsExtendedKey(Keys key)
        {
            switch (key)
            {
                case Keys.RMenu:
                case Keys.RControlKey:
                case Keys.Insert:
                case Keys.Delete:
                case Keys.Home:
                case Keys.End:
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.Right:
                case Keys.Left:
                case Keys.Up:
                case Keys.Down:
                case Keys.NumLock:
                case Keys.PrintScreen:
                case Keys.Divide:
                    return true;
                default:
                    return false;
            }
        }
    }
}
