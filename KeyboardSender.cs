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

        // legacy fallback
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

            // attach our thread to the foreground thread so input is routed similarly
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

        // Scancode-only down (no PostMessage/keybd_event fallback)
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

        // Scancode-only up (no PostMessage/keybd_event fallback)
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

                // Also post a window message to foreground window as a fallback
                if (fg != IntPtr.Zero)
                {
                    int lParam = BuildLParamForKeyDown((int)sc);
                    PostMessage(fg, WM_KEYDOWN, new IntPtr((int)vk), new IntPtr(lParam));
                }

                // Legacy fallback press (helps some games)
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

                // Legacy fallback release (helps some games that miss SendInput keyup)
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
            int extended = 0; // handled by extended flag in WM message sometimes
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
            // Known explicit scan codes for common keys (set to BIOS/Set 1 scancodes as used by MapVirtualKey)
            switch (key)
            {
                case Keys.LControlKey: return 0x1D; // Left Control
                case Keys.RControlKey: return 0x1D; // Right Control uses same code with extended flag
                case Keys.LShiftKey: return 0x2A; // Left Shift
                case Keys.RShiftKey: return 0x36; // Right Shift
                case Keys.LMenu: return 0x38; // Left Alt
                case Keys.RMenu: return 0x38; // Right Alt uses same code with extended flag
                case Keys.Space: return 0x39;
                case Keys.Enter: return 0x1C;
                case Keys.Tab: return 0x0F;
                case Keys.Back: return 0x0E;
                // add other explicit mappings as needed
            }

            // fallback to MapVirtualKey for other keys
            uint vk = (uint)key;
            uint sc = MapVirtualKey(vk, MAPVK_VK_TO_VSC);
            if (sc == 0)
            {
                // as a final fallback, return 0 (no-op) — caller will still attempt to send the input
                return 0;
            }

            return sc;
        }

        private static bool IsExtendedKey(Keys key)
        {
            switch (key)
            {
                // keys that require the extended bit
                case Keys.RMenu:        // Right Alt
                case Keys.RControlKey:  // Right Ctrl
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
