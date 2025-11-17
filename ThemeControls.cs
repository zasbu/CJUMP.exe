using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Text;

namespace CJUMP
{
    public static class ThemeColors
    {
        public static readonly Color Background = Color.FromArgb(22, 22, 22);
        public static readonly Color PanelBackground = Color.FromArgb(30, 30, 30);
        public static readonly Color GroupBackground = Color.FromArgb(28, 28, 28);
        public static readonly Color Border = Color.FromArgb(90, 90, 90);
        public static readonly Color Accent = Color.FromArgb(120, 255, 0);
        public static readonly Color AccentSoft = Color.FromArgb(40, 60, 30);
        public static readonly Color Foreground = Color.FromArgb(230, 230, 230);
        public static readonly Color Muted = Color.FromArgb(150, 150, 150);
    }

    public class DarkGroupBox : GroupBox
    {
        public DarkGroupBox()
        {
            DoubleBuffered = true;
            ForeColor = ThemeColors.Muted;
            BackColor = ThemeColors.PanelBackground;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            Color parentBack = Parent?.BackColor ?? ThemeColors.PanelBackground;
            g.Clear(parentBack);

            string text = Text ?? string.Empty;
            var font = Font;

            if (string.IsNullOrEmpty(text))
            {
                using (var bg = new SolidBrush(ThemeColors.GroupBackground))
                    g.FillRectangle(bg, new Rectangle(5, 5, Width - 10, Height - 10));

                using (var p = new Pen(ThemeColors.Border))
                    g.DrawRectangle(p, new Rectangle(5, 5, Width - 10, Height - 10));

                return;
            }

            Size textSize = TextRenderer.MeasureText(text, font);

            int textLeft = 12;
            int textTop = 5;

            Rectangle textRect = new Rectangle(textLeft, textTop, textSize.Width, textSize.Height);

            int lineY = textRect.Top + textRect.Height / 2;

            Rectangle borderRect = new Rectangle(5, lineY, Width - 10, Height - lineY - 5);

            using (var bg = new SolidBrush(ThemeColors.GroupBackground))
                g.FillRectangle(bg, borderRect);

            using (var p = new Pen(ThemeColors.Border))
                g.DrawRectangle(p, borderRect);

            using (var strip = new SolidBrush(parentBack))
            {
                Rectangle stripRect = Rectangle.Inflate(textRect, 2, 1);
                g.FillRectangle(strip, stripRect);
            }

            TextRenderer.DrawText(g, text, font, textRect, ThemeColors.Muted, TextFormatFlags.Left | TextFormatFlags.Top);
        }
    }

    public class DarkButton : Button
    {
        private bool _hover;
        private bool _pressed;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(false)]
        public bool IsCloseButton { get; set; } = false;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(false)]
        public bool IsMinimizeButton { get; set; } = false;

        public DarkButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = ThemeColors.PanelBackground;
            ForeColor = ThemeColors.Foreground;
            Font = new Font("Tahoma", 8.25f, FontStyle.Regular);
            DoubleBuffered = true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            Rectangle r = ClientRectangle;
            Color fill;

            if (IsCloseButton)
            {
                if (_pressed)
                    fill = Color.FromArgb(120, 25, 25);
                else if (_hover)
                    fill = Color.FromArgb(180, 40, 40);
                else
                    fill = ThemeColors.PanelBackground;
            }
            else
            {
                if (_pressed)
                    fill = Color.FromArgb(28, 28, 28);
                else if (_hover)
                    fill = Color.FromArgb(45, 45, 45);
                else
                    fill = ThemeColors.PanelBackground;
            }

            using (var b = new SolidBrush(fill))
                g.FillRectangle(b, r);

            using (var p = new Pen(ThemeColors.Border))
                g.DrawRectangle(p, 0, 0, r.Width - 1, r.Height - 1);

            string text = Text ?? string.Empty;

            if (text.Length == 1)
            {
                Size textSize = TextRenderer.MeasureText(g, text, Font, r.Size, TextFormatFlags.NoPadding);

                int tx = r.X + (r.Width - textSize.Width) / 2;
                int ty = r.Y + (r.Height - textSize.Height) / 2;

                if (IsMinimizeButton)
                    ty -= 1;

                Rectangle textRect = new Rectangle(tx, ty, textSize.Width, textSize.Height);

                TextRenderer.DrawText(g, text, Font, textRect, ThemeColors.Foreground, TextFormatFlags.NoPadding);
            }
            else
            {
                Rectangle textRect = new Rectangle(r.X, r.Y, r.Width, r.Height);

                TextRenderer.DrawText(g, text, Font, textRect, ThemeColors.Foreground, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }

    public class BindTextBox : Control
    {
        public BindTextBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = ThemeColors.GroupBackground;
            ForeColor = ThemeColors.Foreground;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            TabStop = true;
            Size = new Size(110, 21);
        }

        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle r = ClientRectangle;

            using (var bg = new SolidBrush(BackColor))
                g.FillRectangle(bg, r);

            using (var p = new Pen(ThemeColors.Border))
                g.DrawRectangle(p, 0, 0, r.Width - 1, r.Height - 1);

            RectangleF textRect = new RectangleF(4, 0, r.Width - 8, r.Height);

            using (var b = new SolidBrush(ForeColor))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                g.DrawString(Text ?? string.Empty, Font, b, textRect, sf);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }
    }

    public class DarkCheckBox : CheckBox
    {
        public DarkCheckBox()
        {
            ForeColor = ThemeColors.Foreground;
            BackColor = Color.Transparent;
            Font = new Font("Tahoma", 8.25f);
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor ?? ThemeColors.PanelBackground);

            int boxSize = 13;
            int boxX = 0;
            int boxY = (Height - boxSize) / 2;

            Rectangle boxRect = new Rectangle(boxX, boxY, boxSize, boxSize);

            using (var b = new SolidBrush(ThemeColors.PanelBackground))
                e.Graphics.FillRectangle(b, boxRect);

            using (var p = new Pen(ThemeColors.Border))
                e.Graphics.DrawRectangle(p, boxRect);

            if (Checked)
            {
                using (var b = new SolidBrush(ThemeColors.Accent))
                {
                    Rectangle inner = Rectangle.Inflate(boxRect, -3, -3);
                    e.Graphics.FillRectangle(b, inner);
                }
            }

            Rectangle textRect = new Rectangle(boxRect.Right + 6, 0, Width - boxRect.Right - 6, Height);

            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, ThemeColors.Foreground, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }

    public class DarkComboBox : ComboBox
    {
        public DarkComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            ForeColor = ThemeColors.Foreground;
            BackColor = ThemeColors.PanelBackground;
            Font = new Font("Tahoma", 8.25f);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0) return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color back = selected ? ThemeColors.AccentSoft : ThemeColors.PanelBackground;
            using (var b = new SolidBrush(back))
                e.Graphics.FillRectangle(b, e.Bounds);

            string text = GetItemText(Items[e.Index]);

            TextRenderer.DrawText(e.Graphics, text, Font, e.Bounds, ThemeColors.Foreground, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }
}
