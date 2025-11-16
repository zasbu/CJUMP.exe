using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CJUMP
{
    public partial class Form1 : Form
    {
        private bool _enabled = false;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        private const int EM_SETMARGINS = 0xD3;
        private const int EC_LEFTMARGIN = 0x1;

        private string ConfigPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cjump.cfg");

        // active bind capture state
        private BindTextBox _activeBindBox;
        private string _activeBindPreviousText;

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;

            // fix textbox heights so text looks vertically centered
            FixBindBoxSize(txtJumpKey);
            FixBindBoxSize(txtDuckKey);
            FixBindBoxSize(txtPauseKey);
            FixDelayTextBoxHeight(txtDelay);

            // bind capture behavior for bind boxes
            WireBindBoxes();

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );

            // flag special buttons so DarkButton paints them right
            btnClose.IsCloseButton = true;
            btnMinimize.IsMinimizeButton = true;

            // title bar buttons
            btnClose.Click += (_, __) => Close();
            btnMinimize.Click += (_, __) => WindowState = FormWindowState.Minimized;

            // drag from top bar
            panelTitle.MouseDown += TitleBar_MouseDown;
            lblTitle.MouseDown += TitleBar_MouseDown;
            lblStatus.MouseDown += TitleBar_MouseDown;

            // drag from background areas as well
            panelMain.MouseDown += TitleBar_MouseDown;
            groupBinds.MouseDown += TitleBar_MouseDown;
            groupTiming.MouseDown += TitleBar_MouseDown;

            // drag from labels
            lblJump.MouseDown += TitleBar_MouseDown;
            lblDuck.MouseDown += TitleBar_MouseDown;
            lblPause.MouseDown += TitleBar_MouseDown;
            lblDelay.MouseDown += TitleBar_MouseDown;

            // logic hooks
            btnUpdate.Click += BtnUpdate_Click;
            btnPause.Click += BtnPause_Click;   // play/pause toggle

            // load config if it exists
            LoadConfig();

            UpdateStatus(false);
            PositionTitleButtons();

            // start global keyboard hook for runtime hotkeys
            InputHook.KeyboardCaptured += GlobalKeyboardHandler;
            InputHook.StartKeyboardCapture();

            this.Shown += (s, e) =>
            {
                // no initial focus highlight
                ActiveControl = null;

                // left padding on delay textbox only
                ApplyTextboxPadding(txtDelay);
            };
        }

        // ===== BIND INPUT WIRING =====

        private void WireBindBoxes()
        {
            SetupBindBox(txtJumpKey);
            SetupBindBox(txtDuckKey);
            SetupBindBox(txtPauseKey);
        }

        private void SetupBindBox(BindTextBox box)
        {
            box.MouseDown += BindBox_MouseDown;
        }

        private void StartBindCapture(BindTextBox box)
        {
            // cancel any existing capture first
            CancelActiveBindCapture();

            _activeBindBox = box;
            _activeBindPreviousText = box.Text;

            box.ForeColor = ThemeColors.Muted;
            box.Text = "<press a key>";

            // subscribe capture handler ONLY, hook is already running globally
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

            // if clicking a different box or no active capture: start capture
            if (_activeBindBox == null || _activeBindBox != box)
            {
                StartBindCapture(box);
                return;
            }

            // same box already in capture: treat click as mouse bind
            string mouseName = MouseButtonToDisplayString(e.Button);
            if (mouseName != null)
            {
                CommitBind(mouseName);
            }
        }

        // ===== GLOBAL HOTKEY HANDLER =====

        private void GlobalKeyboardHandler(Keys key)
        {
            // if we are currently capturing a bind, ignore runtime hotkeys
            if (_activeBindBox != null)
                return;

            string keyName = KeyToDisplayString(key);
            string pauseBind = txtPauseKey.Text.Trim();

            if (string.IsNullOrEmpty(pauseBind))
                return;

            if (string.Equals(keyName, pauseBind, StringComparison.OrdinalIgnoreCase))
            {
                if (IsDisposed) return;

                if (InvokeRequired)
                    BeginInvoke(new Action(TogglePause));
                else
                    TogglePause();
            }
        }

        private void TogglePause()
        {
            UpdateStatus(!_enabled);

            // later: actually hook/unhook the crouch-jump behavior here
        }

        // ===== KEY / MOUSE NAME MAPPING =====

        private string KeyToDisplayString(Keys key)
        {
            switch (key)
            {
                case Keys.Space:
                    return "SPACE";

                // Control
                case Keys.LControlKey:
                    return "LCTRL";
                case Keys.RControlKey:
                    return "RCTRL";
                case Keys.ControlKey:
                    return "CTRL";

                // Shift
                case Keys.LShiftKey:
                    return "LSHIFT";
                case Keys.RShiftKey:
                    return "RSHIFT";
                case Keys.ShiftKey:
                    return "SHIFT";

                // Alt
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

        // ===== TEXTBOX / WINDOW BEHAVIOR =====

        private void FixBindBoxSize(BindTextBox box)
        {
            box.Height = 21; // matches delay textbox visually
        }

        private void FixDelayTextBoxHeight(TextBox tb)
        {
            tb.AutoSize = false;
            tb.Height = 18;   // your current value
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
                // clicking background cancels bind capture and drags the window
                CancelActiveBindCapture();

                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
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
        }

        // ===== STATUS / CONFIG / PAUSE =====

        private void UpdateStatus(bool enabled)
        {
            _enabled = enabled;
            lblStatus.Text = enabled ? "[enabled]" : "[disabled]";
            lblStatus.ForeColor = enabled ? ThemeColors.Accent : Color.FromArgb(200, 60, 60);

            // play/pause button look
            if (btnPause != null)
            {
                // ▶ when disabled, ◼ when enabled
                btnPause.Text = enabled ? "◼" : "▶";
                btnPause.ForeColor = enabled ? ThemeColors.Accent : Color.FromArgb(150, 150, 150);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            // just save config, do NOT enable here

            string jumpBind = txtJumpKey.Text.Trim();
            string duckBind = txtDuckKey.Text.Trim();
            string pauseBind = txtPauseKey.Text.Trim();

            int delayMs = 850;
            if (!int.TryParse(txtDelay.Text.Trim(), out delayMs))
                delayMs = 850;

            if (delayMs < 0) delayMs = 0;
            if (delayMs > 2000) delayMs = 2000;

            SaveConfig(jumpBind, duckBind, pauseBind, delayMs);

            // no UpdateStatus(true) here anymore
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            TogglePause();
        }

        private void SaveConfig(string jump, string duck, string pause, int delayMs)
        {
            try
            {
                string[] lines =
                {
                    $"jump={jump}",
                    $"duck={duck}",
                    $"pause={pause}",
                    $"delay={delayMs}"
                };

                File.WriteAllLines(ConfigPath, lines);
            }
            catch
            {
                // ignore file errors for now
            }
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
                        case "pause":
                            if (!string.IsNullOrEmpty(value))
                                txtPauseKey.Text = value;
                            break;
                        case "delay":
                            if (!string.IsNullOrEmpty(value))
                                txtDelay.Text = value;
                            break;
                    }
                }
            }
            catch
            {
                // bad config; ignore and stick to defaults
            }
        }

        // ===== BORDER PAINT =====

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
            // unsubscribe runtime and capture handlers and unhook
            InputHook.KeyboardCaptured -= GlobalKeyboardHandler;
            InputHook.KeyboardCaptured -= InputHookOnKeyboardCaptured;

            InputHook.StopKeyboardCapture();

            base.OnFormClosed(e);
        }
    }
}
