using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace CJUMP
{
    public partial class Form1 : Form
    {
        private bool _enabled = false;
        private bool _isCrouchJumpRunning = false;

        private readonly object _stateLock = new object();
        private HashSet<Keys> _currentlyDown = new HashSet<Keys>();
        private bool _jumpHandled = false;
        private bool _duckPressedByMacro = false;

        private DateTime _lastPrintableKey = DateTime.MinValue;
        private readonly int _typingSuppressMs = 800;

        private System.Threading.Timer _focusTimer;
        private readonly int _autoTargetPid = 4876;
        private readonly string[] _autoTitleKeywords = new[] { "Counter-Strike Source", "Direct3D 9" };
        private readonly string[] _autoProcessNames = new[] { "cstrike_win64", "hl2" };
        private bool _autoFocusEnabled = true;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        private const int EM_SETMARGINS = 0xD3;
        private const int EC_LEFTMARGIN = 0x1;

        private string ConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cjump.cfg");

        private BindTextBox _activeBindBox;
        private string _activeBindPreviousText;

        public Form1()
        {
            InitializeComponent();

            try
            {
                var bytes = CJUMP.Properties.Resources.favicon;
                if (bytes != null && bytes.Length > 0 && picIcon != null)
                {
                    using (var ms = new MemoryStream(bytes))
                    {
                        Image srcImg = null;
                        try
                        {
                            using (var ic = new Icon(ms))
                            {
                                srcImg = ic.ToBitmap();
                            }
                        }
                        catch
                        {
                            try
                            {
                                ms.Position = 0;
                                srcImg = Image.FromStream(ms);
                            }
                            catch { srcImg = null; }
                        }

                        if (srcImg != null)
                        {
                            int size = Math.Min(picIcon.Width, picIcon.Height);
                            var circular = CreateCircularBitmap(new Bitmap(srcImg), size);
                            picIcon.Image = circular;
                            picIcon.SizeMode = PictureBoxSizeMode.Normal;
                        }
                    }
                }
            }
            catch { }

            DoubleBuffered = true;

            FixBindBoxSize(txtJumpKey);
            FixBindBoxSize(txtDuckKey);
            FixDelayTextBoxHeight(txtDelay);

            WireBindBoxes();

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );

            btnClose.IsCloseButton = true;
            btnMinimize.IsMinimizeButton = true;

            btnClose.Click += (_, __) => Close();
            btnMinimize.Click += (_, __) => WindowState = FormWindowState.Minimized;

            panelTitle.MouseDown += TitleBar_MouseDown;
            lblTitle.MouseDown += TitleBar_MouseDown;
            lblStatus.MouseDown += TitleBar_MouseDown;

            panelMain.MouseDown += TitleBar_MouseDown;
            groupBinds.MouseDown += TitleBar_MouseDown;
            groupTiming.MouseDown += TitleBar_MouseDown;

            lblJump.MouseDown += TitleBar_MouseDown;
            lblDuck.MouseDown += TitleBar_MouseDown;
            lblDelay.MouseDown += TitleBar_MouseDown;

            btnUpdate.Click += BtnUpdate_Click;

            LoadConfig();

            UpdateStatus(false);
            PositionTitleButtons();
            PositionFoundLabel();

            InputHook.KeyboardCaptured += GlobalKeyboardHandler;
            InputHook.KeyboardReleased += GlobalKeyboardReleasedHandler;
            InputHook.StartKeyboardCapture();

            StartFocusMonitor();

            this.Shown += (s, e) =>
            {
                ActiveControl = null;
                ApplyTextboxPadding(txtDelay);
                PositionFoundLabel();
            };
        }

        private void StartFocusMonitor()
        {
            _focusTimer = new System.Threading.Timer(_ =>
            {
                try
                {
                    var fg = GetForegroundWindow();
                    if (fg == IntPtr.Zero)
                    {
                        SetAutoEnabled(false);
                        UpdateFoundDisplay(false, 0);
                        return;
                    }

                    uint pid;
                    GetWindowThreadProcessId(fg, out pid);

                    bool shouldEnable = false;
                    uint foundPid = 0;

                    try
                    {
                        var proc = System.Diagnostics.Process.GetProcessById((int)pid);
                        if (proc != null)
                        {
                            string pname = proc.ProcessName ?? string.Empty;
                            foreach (var target in _autoProcessNames)
                            {
                                if (!string.IsNullOrEmpty(target) && string.Equals(pname, target, StringComparison.OrdinalIgnoreCase))
                                {
                                    shouldEnable = true;
                                    foundPid = pid;
                                    break;
                                }
                            }
                        }
                    }
                    catch { }

                    if (!shouldEnable)
                    {
                        if ((int)pid == _autoTargetPid && _autoTargetPid != 0)
                        {
                            shouldEnable = true;
                            foundPid = pid;
                        }
                        else
                        {
                            int len = GetWindowTextLength(fg);
                            if (len > 0)
                            {
                                var sb = new System.Text.StringBuilder(len + 1);
                                GetWindowText(fg, sb, sb.Capacity);
                                var title = sb.ToString();
                                foreach (var kw in _autoTitleKeywords)
                                {
                                    if (!string.IsNullOrEmpty(kw) && title.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        shouldEnable = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    SetAutoEnabled(shouldEnable);
                    UpdateFoundDisplay(foundPid != 0, foundPid);
                }
                catch { }
            }, null, 0, 500);
        }

        private void UpdateFoundDisplay(bool found, uint pid)
        {
            try
            {
                if (lblFound == null) return;

                string text = found ? $"CS:S found [PID {pid}]" : "CS:S not active";
                Color color = ThemeColors.Muted;

                if (lblFound.InvokeRequired)
                {
                    lblFound.BeginInvoke(new Action(() => { lblFound.Text = text; lblFound.ForeColor = color; PositionFoundLabel(); }));
                }
                else
                {
                    lblFound.Text = text;
                    lblFound.ForeColor = color;
                    PositionFoundLabel();
                }
            }
            catch { }
        }

        private void PositionFoundLabel()
        {
            try
            {
                if (lblFound == null || panelMain == null) return;

                lblFound.AutoSize = true;

                int left = (panelMain.ClientSize.Width - lblFound.Width) / 2;
                if (left < 0) left = 0;
                lblFound.Left = left;

                if (btnUpdate != null)
                {
                    int spaceTop = btnUpdate.Bottom;
                    int spaceBottom = panelMain.ClientSize.Height;
                    int available = Math.Max(0, spaceBottom - spaceTop);

                    int top = spaceTop + Math.Max(0, (available - lblFound.Height) / 2) + 5;

                    if (top + lblFound.Height > panelMain.ClientSize.Height)
                        top = Math.Max(0, panelMain.ClientSize.Height - lblFound.Height - 4);

                    lblFound.Top = top;
                }
                else
                {
                    int top = panelMain.ClientSize.Height - lblFound.Height - 8;
                    lblFound.Top = Math.Max(0, top);
                }
            }
            catch { }
        }

        private void SetAutoEnabled(bool enable)
        {
            if (!_autoFocusEnabled)
                return;

            if (enable == _enabled)
                return;

            if (InvokeRequired)
                BeginInvoke(new Action(() => UpdateStatus(enable)));
            else
                UpdateStatus(enable);
        }

        private void WireBindBoxes()
        {
            SetupBindBox(txtJumpKey);
            SetupBindBox(txtDuckKey);
        }

        private void SetupBindBox(BindTextBox box)
        {
            box.MouseDown += BindBox_MouseDown;
        }

        private void StartBindCapture(BindTextBox box)
        {
            CancelActiveBindCapture();

            _activeBindBox = box;
            _activeBindPreviousText = box.Text;

            box.ForeColor = ThemeColors.Muted;
            box.Text = "<press a key>";

            InputHook.KeyboardCaptured += InputHookOnKeyboardCaptured;
        }

        private void CommitBind(string bindText)
        {
            if (_activeBindBox == null)
                return;

            _activeBindBox.ForeColor = ThemeColors.Foreground;
            _activeBindBox.Text = bindText;

            _activeBindBox = null;
            _activeBindPreviousText = null;

            InputHook.KeyboardCaptured -= InputHookOnKeyboardCaptured;

            ActiveControl = null;
        }

        private void CancelActiveBindCapture()
        {
            if (_activeBindBox == null)
                return;

            _activeBindBox.ForeColor = ThemeColors.Foreground;
            _activeBindBox.Text = _activeBindPreviousText ?? string.Empty;

            _activeBindBox = null;
            _activeBindPreviousText = null;

            InputHook.KeyboardCaptured -= InputHookOnKeyboardCaptured;

            ActiveControl = null;
        }

        private void InputHookOnKeyboardCaptured(Keys key)
        {
            if (_activeBindBox == null)
                return;

            string keyName = KeyToDisplayString(key);
            CommitBind(keyName);
        }

        private void BindBox_MouseDown(object sender, MouseEventArgs e)
        {
            var box = sender as BindTextBox;
            if (box == null)
                return;

            if (_activeBindBox == null || _activeBindBox != box)
            {
                StartBindCapture(box);
                return;
            }

            string mouseName = MouseButtonToDisplayString(e.Button);
            if (mouseName != null)
            {
                CommitBind(mouseName);
            }
        }

        private void GlobalKeyboardReleasedHandler(Keys key)
        {
            lock (_stateLock)
            {
                _currentlyDown.Remove(key);

                string jumpBindText = txtJumpKey.Text.Trim();
                Keys? jumpKey = DisplayStringToKey(jumpBindText);
                if (jumpKey.HasValue && key == jumpKey.Value)
                {
                    _jumpHandled = false;
                }
            }
        }

        private void GlobalKeyboardHandler(Keys key)
        {
            if (_activeBindBox != null)
                return;

            lock (_stateLock)
            {
                if (_currentlyDown.Contains(key))
                    return;

                _currentlyDown.Add(key);
            }

            Keys pressedKey = key;

            string jumpBindText = txtJumpKey.Text.Trim();
            string duckBindText = txtDuckKey.Text.Trim();

            Keys? jumpKey = DisplayStringToKey(jumpBindText);
            Keys? duckKey = DisplayStringToKey(duckBindText);

            if ((!jumpKey.HasValue || key != jumpKey.Value))
            {
                bool isChatKey = (key == Keys.Y || key == Keys.U);
                if (isChatKey || (IsPrintableKey(key) && !IsWASDKey(key)))
                {
                    _lastPrintableKey = DateTime.UtcNow;
                }
            }

            if (!_enabled)
                return;

            if (!jumpKey.HasValue || !duckKey.HasValue)
                return;

            if ((DateTime.UtcNow - _lastPrintableKey).TotalMilliseconds < _typingSuppressMs)
                return;

            if (pressedKey != jumpKey.Value)
                return;

            int delayMs;
            if (!int.TryParse(txtDelay.Text.Trim(), out delayMs))
                delayMs = 850;

            delayMs = Math.Max(0, Math.Min(2000, delayMs));

            if (_isCrouchJumpRunning)
                return;

            lock (_stateLock)
            {
                if (_jumpHandled)
                    return;

                _jumpHandled = true;
                _isCrouchJumpRunning = true;
            }

            Task.Run(() =>
            {
                try
                {
                    Thread.Sleep(1);

                    lock (_stateLock)
                    {
                        if (!jumpKey.HasValue || !_currentlyDown.Contains(jumpKey.Value))
                        {
                            _isCrouchJumpRunning = false;
                            _jumpHandled = false;
                            return;
                        }
                    }

                    try
                    {
                        if (IsForegroundTarget())
                        {
                            KeyboardSender.KeyDownScancodeOnly(jumpKey.Value);
                            Thread.Sleep(5);
                            KeyboardSender.KeyUpScancodeOnly(jumpKey.Value);
                        }
                    }
                    catch { }

                    _duckPressedByMacro = true;
                    KeyboardSender.KeyDown(duckKey.Value);

                    Thread.Sleep(delayMs);

                    KeyboardSender.KeyUp(duckKey.Value);
                    _duckPressedByMacro = false;
                }
                finally
                {
                    _isCrouchJumpRunning = false;
                }
            });
        }

        private void TogglePause()
        {
            UpdateStatus(!_enabled);
        }

        private string KeyToDisplayString(Keys key)
        {
            switch (key)
            {
                case Keys.Space:
                    return "SPACE";
                case Keys.LControlKey:
                    return "LCTRL";
                case Keys.RControlKey:
                    return "RCTRL";
                case Keys.ControlKey:
                    return "CTRL";
                case Keys.LShiftKey:
                    return "LSHIFT";
                case Keys.RShiftKey:
                    return "RSHIFT";
                case Keys.ShiftKey:
                    return "SHIFT";
                case Keys.LMenu:
                    return "LALT";
                case Keys.RMenu:
                    return "RALT";
                case Keys.Menu:
                    return "ALT";
            }

            if (key >= Keys.D0 && key <= Keys.D9)
            {
                int digit = key - Keys.D0;
                return digit.ToString();
            }

            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                int digit = key - Keys.NumPad0;
                return $"NUMPAD{digit}";
            }

            return key.ToString().ToUpperInvariant();
        }

        private string MouseButtonToDisplayString(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left: return "MOUSE1";
                case MouseButtons.Right: return "MOUSE2";
                case MouseButtons.Middle: return "MOUSE3";
                case MouseButtons.XButton1: return "MOUSE4";
                case MouseButtons.XButton2: return "MOUSE5";
                default: return null;
            }
        }

        private Keys? DisplayStringToKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            name = name.Trim();

            string upper = name.ToUpperInvariant();

            if (upper.StartsWith("MOUSE"))
                return null;

            switch (upper)
            {
                case "SPACE":
                case "SPACEBAR":
                    return Keys.Space;
                case "LCTRL":
                case "LEFT CONTROL":
                    return Keys.LControlKey;
                case "RCTRL":
                case "RIGHT CONTROL":
                    return Keys.RControlKey;
                case "CTRL":
                case "CONTROL":
                    return Keys.ControlKey;
                case "LSHIFT":
                case "LEFT SHIFT":
                    return Keys.LShiftKey;
                case "RSHIFT":
                case "RIGHT SHIFT":
                    return Keys.RShiftKey;
                case "SHIFT":
                    return Keys.ShiftKey;
                case "LALT":
                case "LEFT ALT":
                    return Keys.LMenu;
                case "RALT":
                case "RIGHT ALT":
                    return Keys.RMenu;
                case "ALT":
                    return Keys.Menu;
                case "ENTER":
                case "RETURN":
                    return Keys.Enter;
                case "TAB":
                    return Keys.Tab;
                case "BACKSPACE":
                    return Keys.Back;
            }

            if (upper.Length == 1 && char.IsDigit(upper[0]))
            {
                int digit = upper[0] - '0';
                return (Keys)((int)Keys.D0 + digit);
            }

            if (upper.StartsWith("NUMPAD") && upper.Length == 7 && char.IsDigit(upper[6]))
            {
                int digit = upper[6] - '0';
                return (Keys)((int)Keys.NumPad0 + digit);
            }

            if (Enum.TryParse(name, true, out Keys parsed))
                return parsed;

            return null;
        }

        private void FixBindBoxSize(BindTextBox box)
        {
            if (box == null) return;
            box.Height = 21;
        }

        private void FixDelayTextBoxHeight(TextBox tb)
        {
            if (tb == null) return;
            tb.AutoSize = false;
            tb.Height = 18;
        }

        private void ApplyTextboxPadding(Control c)
        {
            if (c == null) return;
            SendMessage(c.Handle, EM_SETMARGINS, EC_LEFTMARGIN, 4);
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                CancelActiveBindCapture();

                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private bool IsForegroundTarget()
        {
            try
            {
                var fg = GetForegroundWindow();
                if (fg == IntPtr.Zero) return false;
                uint pid;
                GetWindowThreadProcessId(fg, out pid);
                if ((int)pid == _autoTargetPid) return true;
                int len = GetWindowTextLength(fg);
                if (len > 0)
                {
                    var sb = new System.Text.StringBuilder(len + 1);
                    GetWindowText(fg, sb, sb.Capacity);
                    var title = sb.ToString();
                    foreach (var kw in _autoTitleKeywords)
                    {
                        if (!string.IsNullOrEmpty(kw) && title.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private void PositionTitleButtons()
        {
            if (btnClose == null || btnMinimize == null || panelTitle == null)
                return;

            btnClose.Location = new Point(
                panelTitle.Width - btnClose.Width - 8,
                5
            );

            btnMinimize.Location = new Point(
                btnClose.Left - btnMinimize.Width - 4,
                5
            );
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PositionTitleButtons();
            PositionFoundLabel();
        }

        private void UpdateStatus(bool enabled)
        {
            _enabled = enabled;
            lblStatus.Text = enabled ? "[enabled]" : "[disabled]";
            lblStatus.ForeColor = enabled ? ThemeColors.Accent : Color.FromArgb(200, 60, 60);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            string jumpBind = txtJumpKey.Text.Trim();
            string duckBind = txtDuckKey.Text.Trim();

            int delayMs = 850;
            if (!int.TryParse(txtDelay.Text.Trim(), out delayMs))
                delayMs = 850;

            if (delayMs < 0) delayMs = 0;
            if (delayMs > 2000) delayMs = 2000;

            SaveConfig(jumpBind, duckBind, delayMs);
        }

        private void SaveConfig(string jump, string duck, int delayMs)
        {
            try
            {
                string[] lines =
                {
                    $"jump={jump}",
                    $"duck={duck}",
                    $"delay={delayMs}"
                };

                File.WriteAllLines(ConfigPath, lines);
            }
            catch { }
        }

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return;

                var lines = File.ReadAllLines(ConfigPath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length != 2) continue;

                    string key = parts[0].Trim().ToLowerInvariant();
                    string value = parts[1].Trim();

                    switch (key)
                    {
                        case "jump":
                            if (!string.IsNullOrEmpty(value))
                                txtJumpKey.Text = value;
                            break;
                        case "duck":
                            if (!string.IsNullOrEmpty(value))
                                txtDuckKey.Text = value;
                            break;
                        case "delay":
                            if (!string.IsNullOrEmpty(value))
                                txtDelay.Text = value;
                            break;
                    }
                }
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            using (var borderPen = new Pen(ThemeColors.Border))
                g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

            using (var accentPen = new Pen(ThemeColors.Accent))
                g.DrawLine(accentPen, 1, 1, Width - 2, 1);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            InputHook.KeyboardCaptured -= GlobalKeyboardHandler;
            InputHook.KeyboardCaptured -= InputHookOnKeyboardCaptured;
            InputHook.KeyboardReleased -= GlobalKeyboardReleasedHandler;

            InputHook.StopKeyboardCapture();

            try { _focusTimer?.Dispose(); } catch { }

            base.OnFormClosed(e);
        }

        private bool IsPrintableKey(Keys k)
        {
            if (k >= Keys.A && k <= Keys.Z) return true;
            if (k >= Keys.D0 && k <= Keys.D9) return true;
            if (k >= Keys.NumPad0 && k <= Keys.NumPad9) return true;

            switch (k)
            {
                case Keys.Space:
                case Keys.OemPeriod:
                case Keys.Oemcomma:
                case Keys.OemMinus:
                case Keys.Oemplus:
                case Keys.OemQuestion:
                case Keys.Oem1:
                case Keys.Oem7:
                case Keys.OemOpenBrackets:
                case Keys.Oem6:
                case Keys.Oem5:
                case Keys.Oem102:
                case Keys.Back:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsWASDKey(Keys k)
        {
            return k == Keys.W || k == Keys.A || k == Keys.S || k == Keys.D;
        }

        private Bitmap CreateCircularBitmap(Bitmap src, int diameter)
        {
            try
            {
                var dest = new Bitmap(diameter, diameter, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(dest))
                {
                    g.CompositingMode = CompositingMode.SourceOver;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;

                    g.Clear(Color.Transparent);

                    float srcAspect = (float)src.Width / src.Height;
                    float dstAspect = 1.0f;

                    RectangleF dstRect = new RectangleF(0, 0, diameter, diameter);
                    RectangleF srcRect;

                    if (srcAspect > dstAspect)
                    {
                        float srcW = src.Height * dstAspect;
                        float srcX = (src.Width - srcW) / 2f;
                        srcRect = new RectangleF(srcX, 0, srcW, src.Height);
                    }
                    else
                    {
                        float srcH = src.Width / dstAspect;
                        float srcY = (src.Height - srcH) / 2f;
                        srcRect = new RectangleF(0, srcY, src.Width, srcH);
                    }

                    using (var path = new GraphicsPath())
                    {
                        path.AddEllipse(0, 0, diameter, diameter);
                        g.SetClip(path);

                        g.DrawImage(src, dstRect, srcRect, GraphicsUnit.Pixel);

                        g.ResetClip();

                        using (var pen = new Pen(Color.FromArgb(120, 0, 0, 0), 1f))
                        {
                            pen.Alignment = PenAlignment.Center;
                            g.DrawEllipse(pen, 0.5f, 0.5f, diameter - 1f, diameter - 1f);
                        }
                    }
                }

                return dest;
            }
            catch
            {
                return src;
            }
        }
    }
}
