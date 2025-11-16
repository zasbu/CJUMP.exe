using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CJUMP
{
    partial class Form1
    {
        private IContainer components = null;

        // Title bar
        private Panel panelTitle;
        private Label lblTitle;
        private Label lblStatus;
        private DarkButton btnMinimize;
        private DarkButton btnClose;

        // Main area
        private Panel panelMain;
        private DarkGroupBox groupBinds;
        private DarkGroupBox groupTiming;

        // Binds controls
        private Label lblJump;
        private Label lblDuck;
        private Label lblPause;
        private BindTextBox txtJumpKey;
        private BindTextBox txtDuckKey;
        private BindTextBox txtPauseKey;

        // Timing controls
        private Label lblDelay;
        private TextBox txtDelay;

        // Bottom buttons
        private DarkButton btnUpdate;
        private DarkButton btnPause;   // new pause/play button

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelTitle = new Panel();
            btnClose = new DarkButton();
            btnMinimize = new DarkButton();
            lblStatus = new Label();
            lblTitle = new Label();
            panelMain = new Panel();
            groupBinds = new DarkGroupBox();
            lblJump = new Label();
            lblDuck = new Label();
            lblPause = new Label();
            txtJumpKey = new BindTextBox();
            txtDuckKey = new BindTextBox();
            txtPauseKey = new BindTextBox();
            groupTiming = new DarkGroupBox();
            lblDelay = new Label();
            txtDelay = new TextBox();
            btnUpdate = new DarkButton();
            btnPause = new DarkButton();
            panelTitle.SuspendLayout();
            panelMain.SuspendLayout();
            groupBinds.SuspendLayout();
            groupTiming.SuspendLayout();
            SuspendLayout();
            // 
            // panelTitle
            // 
            panelTitle.BackColor = Color.FromArgb(30, 30, 30);
            panelTitle.Controls.Add(btnClose);
            panelTitle.Controls.Add(btnMinimize);
            panelTitle.Controls.Add(lblStatus);
            panelTitle.Controls.Add(lblTitle);
            panelTitle.Dock = DockStyle.Top;
            panelTitle.Location = new Point(1, 1);
            panelTitle.Name = "panelTitle";
            panelTitle.Padding = new Padding(8, 4, 8, 4);
            panelTitle.Size = new Size(418, 28);
            panelTitle.TabIndex = 1;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.FromArgb(30, 30, 30);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Tahoma", 8.25F);
            btnClose.ForeColor = Color.FromArgb(230, 230, 230);
            btnClose.IsCloseButton = false;
            btnClose.IsMinimizeButton = false;
            btnClose.Location = new Point(387, 4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(26, 18);
            btnClose.TabIndex = 0;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = false;
            // 
            // btnMinimize
            // 
            btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimize.BackColor = Color.FromArgb(30, 30, 30);
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.Font = new Font("Tahoma", 8.25F);
            btnMinimize.ForeColor = Color.FromArgb(230, 230, 230);
            btnMinimize.IsCloseButton = false;
            btnMinimize.IsMinimizeButton = false;
            btnMinimize.Location = new Point(358, 4);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(26, 18);
            btnMinimize.TabIndex = 1;
            btnMinimize.Text = "_";
            btnMinimize.UseVisualStyleBackColor = false;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(200, 60, 60);
            lblStatus.Location = new Point(68, 7);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(63, 13);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "[disabled]";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.ForeColor = Color.FromArgb(230, 230, 230);
            lblTitle.Location = new Point(8, 7);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(62, 13);
            lblTitle.TabIndex = 3;
            lblTitle.Text = "CJUMP.exe";
            // 
            // panelMain
            // 
            panelMain.BackColor = Color.FromArgb(30, 30, 30);
            panelMain.Controls.Add(groupBinds);
            panelMain.Controls.Add(groupTiming);
            panelMain.Controls.Add(btnUpdate);
            panelMain.Controls.Add(btnPause);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(1, 29);
            panelMain.Name = "panelMain";
            panelMain.Padding = new Padding(8);
            panelMain.Size = new Size(418, 200);
            panelMain.TabIndex = 0;
            // 
            // groupBinds
            // 
            groupBinds.BackColor = Color.FromArgb(30, 30, 30);
            groupBinds.Controls.Add(lblJump);
            groupBinds.Controls.Add(lblDuck);
            groupBinds.Controls.Add(lblPause);
            groupBinds.Controls.Add(txtJumpKey);
            groupBinds.Controls.Add(txtDuckKey);
            groupBinds.Controls.Add(txtPauseKey);
            groupBinds.ForeColor = Color.FromArgb(150, 150, 150);
            groupBinds.Location = new Point(10, 18);
            groupBinds.Name = "groupBinds";
            groupBinds.Size = new Size(260, 110);
            groupBinds.TabIndex = 0;
            groupBinds.TabStop = false;
            groupBinds.Text = "binds";
            // 
            // lblJump
            // 
            lblJump.AutoSize = true;
            lblJump.ForeColor = Color.FromArgb(150, 150, 150);
            lblJump.Location = new Point(20, 27);
            lblJump.Name = "lblJump";
            lblJump.Size = new Size(58, 13);
            lblJump.TabIndex = 0;
            lblJump.Text = "+jump key";
            // 
            // lblDuck
            // 
            lblDuck.AutoSize = true;
            lblDuck.ForeColor = Color.FromArgb(150, 150, 150);
            lblDuck.Location = new Point(20, 51);
            lblDuck.Name = "lblDuck";
            lblDuck.Size = new Size(57, 13);
            lblDuck.TabIndex = 1;
            lblDuck.Text = "+duck key";
            // 
            // lblPause
            // 
            lblPause.AutoSize = true;
            lblPause.ForeColor = Color.FromArgb(150, 150, 150);
            lblPause.Location = new Point(21, 75);
            lblPause.Name = "lblPause";
            lblPause.Size = new Size(56, 13);
            lblPause.TabIndex = 2;
            lblPause.Text = "pause key";
            // 
            // txtJumpKey
            // 
            txtJumpKey.BackColor = Color.FromArgb(28, 28, 28);
            txtJumpKey.Font = new Font("Tahoma", 8.25F);
            txtJumpKey.ForeColor = Color.FromArgb(230, 230, 230);
            txtJumpKey.Location = new Point(110, 23);
            txtJumpKey.Name = "txtJumpKey";
            txtJumpKey.Size = new Size(110, 21);
            txtJumpKey.TabIndex = 3;
            txtJumpKey.Text = "SPACE";
            // 
            // txtDuckKey
            // 
            txtDuckKey.BackColor = Color.FromArgb(28, 28, 28);
            txtDuckKey.Font = new Font("Tahoma", 8.25F);
            txtDuckKey.ForeColor = Color.FromArgb(230, 230, 230);
            txtDuckKey.Location = new Point(110, 47);
            txtDuckKey.Name = "txtDuckKey";
            txtDuckKey.Size = new Size(110, 21);
            txtDuckKey.TabIndex = 4;
            txtDuckKey.Text = "CTRL";
            // 
            // txtPauseKey
            // 
            txtPauseKey.BackColor = Color.FromArgb(28, 28, 28);
            txtPauseKey.Font = new Font("Tahoma", 8.25F);
            txtPauseKey.ForeColor = Color.FromArgb(230, 230, 230);
            txtPauseKey.Location = new Point(110, 71);
            txtPauseKey.Name = "txtPauseKey";
            txtPauseKey.Size = new Size(110, 21);
            txtPauseKey.TabIndex = 5;
            txtPauseKey.Text = "F8";
            // 
            // groupTiming
            // 
            groupTiming.BackColor = Color.FromArgb(30, 30, 30);
            groupTiming.Controls.Add(lblDelay);
            groupTiming.Controls.Add(txtDelay);
            groupTiming.ForeColor = Color.FromArgb(150, 150, 150);
            groupTiming.Location = new Point(280, 18);
            groupTiming.Name = "groupTiming";
            groupTiming.Size = new Size(120, 110);
            groupTiming.TabIndex = 1;
            groupTiming.TabStop = false;
            groupTiming.Text = "timing";
            // 
            // lblDelay
            // 
            lblDelay.AutoSize = true;
            lblDelay.ForeColor = Color.FromArgb(150, 150, 150);
            lblDelay.Location = new Point(21, 35);
            lblDelay.Name = "lblDelay";
            lblDelay.Size = new Size(58, 13);
            lblDelay.TabIndex = 0;
            lblDelay.Text = "Delay (ms)";
            // 
            // txtDelay
            // 
            txtDelay.BackColor = Color.FromArgb(28, 28, 28);
            txtDelay.BorderStyle = BorderStyle.FixedSingle;
            txtDelay.Font = new Font("Tahoma", 9f, FontStyle.Regular);
            txtDelay.ForeColor = Color.FromArgb(230, 230, 230);
            txtDelay.Location = new Point(21, 53);
            txtDelay.Name = "txtDelay";
            txtDelay.Size = new Size(80, 21);
            txtDelay.TabIndex = 1;
            txtDelay.Text = "850";
            // 
            // btnUpdate
            // 
            btnUpdate.BackColor = Color.FromArgb(30, 30, 30);
            btnUpdate.FlatStyle = FlatStyle.Flat;
            btnUpdate.Font = new Font("Tahoma", 8.25F);
            btnUpdate.ForeColor = Color.FromArgb(230, 230, 230);
            btnUpdate.IsCloseButton = false;
            btnUpdate.IsMinimizeButton = false;
            btnUpdate.Location = new Point(109, 151);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(200, 24);
            btnUpdate.TabIndex = 2;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = false;
            // 
            // btnPause
            // 
            btnPause.BackColor = Color.FromArgb(30, 30, 30);
            btnPause.FlatStyle = FlatStyle.Flat;
            btnPause.Font = new Font("Segoe UI Symbol", 10.0f, FontStyle.Bold);
            btnPause.ForeColor = Color.FromArgb(150, 150, 150);
            btnPause.IsCloseButton = false;
            btnPause.IsMinimizeButton = false;
            btnPause.Location = new Point(317, 151);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(24, 24);
            btnPause.TabIndex = 3;
            btnPause.Text = "▶";
            btnPause.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.FromArgb(22, 22, 22);
            ClientSize = new Size(420, 230);
            Controls.Add(panelMain);
            Controls.Add(panelTitle);
            Font = new Font("Tahoma", 8.25F);
            ForeColor = Color.FromArgb(230, 230, 230);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Form1";
            Padding = new Padding(1);
            StartPosition = FormStartPosition.CenterScreen;
            panelTitle.ResumeLayout(false);
            panelTitle.PerformLayout();
            panelMain.ResumeLayout(false);
            groupBinds.ResumeLayout(false);
            groupTiming.ResumeLayout(false);
            groupTiming.PerformLayout();
            ResumeLayout(false);
        }
    }
}
