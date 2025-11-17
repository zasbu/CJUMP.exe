using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CJUMP
{
    partial class Form1
    {
        private IContainer components = null;

        // Title bar controls
        private Panel panelTitle;
        private Label lblTitle;
        private Label lblStatus;
        private Label lblFound;
        private PictureBox picIcon;
        private DarkButton btnMinimize;
        private DarkButton btnClose;

        // Main area controls
        private Panel panelMain;
        private DarkGroupBox groupBinds;
        private DarkGroupBox groupTiming;

        // Binds controls
        private Label lblJump;
        private Label lblDuck;
        private BindTextBox txtJumpKey;
        private BindTextBox txtDuckKey;

        // Timing controls
        private Label lblDelay;
        private TextBox txtDelay;

        // Actions
        private DarkButton btnUpdate;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelTitle = new Panel();
            picIcon = new PictureBox();
            btnClose = new DarkButton();
            btnMinimize = new DarkButton();
            lblStatus = new Label();
            lblFound = new Label();
            lblTitle = new Label();
            panelMain = new Panel();
            groupBinds = new DarkGroupBox();
            lblJump = new Label();
            lblDuck = new Label();
            txtJumpKey = new BindTextBox();
            txtDuckKey = new BindTextBox();
            groupTiming = new DarkGroupBox();
            lblDelay = new Label();
            txtDelay = new TextBox();
            btnUpdate = new DarkButton();
            pictureBox1 = new PictureBox();
            panelTitle.SuspendLayout();
            ((ISupportInitialize)picIcon).BeginInit();
            panelMain.SuspendLayout();
            groupBinds.SuspendLayout();
            groupTiming.SuspendLayout();
            ((ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();

            panelTitle.BackColor = Color.FromArgb(30, 30, 30);
            panelTitle.Controls.Add(picIcon);
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

            picIcon.Location = new Point(8, 6);
            picIcon.Name = "picIcon";
            picIcon.Size = new Size(18, 18);
            picIcon.SizeMode = PictureBoxSizeMode.CenterImage;
            picIcon.TabIndex = 5;
            picIcon.TabStop = false;

            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.FromArgb(30, 30, 30);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Tahoma", 8.25F);
            btnClose.ForeColor = Color.FromArgb(230, 230, 230);
            btnClose.Location = new Point(387, 4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(26, 18);
            btnClose.TabIndex = 0;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = false;

            btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimize.BackColor = Color.FromArgb(30, 30, 30);
            btnMinimize.FlatStyle = FlatStyle.Flat;
            btnMinimize.Font = new Font("Tahoma", 8.25F);
            btnMinimize.ForeColor = Color.FromArgb(230, 230, 230);
            btnMinimize.Location = new Point(358, 4);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(26, 18);
            btnMinimize.TabIndex = 1;
            btnMinimize.Text = "_";
            btnMinimize.UseVisualStyleBackColor = false;

            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(200, 60, 60);
            lblStatus.Location = new Point(104, 8);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(64, 13);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "[disabled]";

            lblFound.AutoSize = true;
            lblFound.Font = new Font("Consolas", 8.25F); // use same clear UI font as rest of form
            lblFound.UseCompatibleTextRendering = true; // enable GDI+ rendering for smoother text
            lblFound.ForeColor = Color.FromArgb(150, 150, 150);
            lblFound.Location = new Point(169, 166);
            lblFound.Name = "lblFound";
            lblFound.Size = new Size(80, 14);
            lblFound.TabIndex = 4;
            lblFound.Text = "CS:S NOT ACTIVE";

            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Microsoft Sans Serif", 9F);
            lblTitle.ForeColor = Color.FromArgb(230, 230, 230);
            lblTitle.Location = new Point(30, 7);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(72, 15);
            lblTitle.TabIndex = 3;
            lblTitle.Text = "CJUMP.exe";

            panelMain.BackColor = Color.FromArgb(30, 30, 30);
            panelMain.Controls.Add(groupBinds);
            panelMain.Controls.Add(groupTiming);
            panelMain.Controls.Add(btnUpdate);
            panelMain.Controls.Add(lblFound);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(1, 29);
            panelMain.Name = "panelMain";
            panelMain.Padding = new Padding(8);
            panelMain.Size = new Size(418, 200);
            panelMain.TabIndex = 0;

            groupBinds.BackColor = Color.FromArgb(30, 30, 30);
            groupBinds.Controls.Add(lblJump);
            groupBinds.Controls.Add(lblDuck);
            groupBinds.Controls.Add(txtJumpKey);
            groupBinds.Controls.Add(txtDuckKey);
            groupBinds.ForeColor = Color.FromArgb(150, 150, 150);
            groupBinds.Location = new Point(10, 18);
            groupBinds.Name = "groupBinds";
            groupBinds.Size = new Size(260, 83);
            groupBinds.TabIndex = 0;
            groupBinds.TabStop = false;
            groupBinds.Text = "binds";

            lblJump.AutoSize = true;
            lblJump.ForeColor = Color.FromArgb(150, 150, 150);
            lblJump.Location = new Point(20, 27);
            lblJump.Name = "lblJump";
            lblJump.Size = new Size(58, 13);
            lblJump.TabIndex = 0;
            lblJump.Text = "+jump key";

            lblDuck.AutoSize = true;
            lblDuck.ForeColor = Color.FromArgb(150, 150, 150);
            lblDuck.Location = new Point(20, 51);
            lblDuck.Name = "lblDuck";
            lblDuck.Size = new Size(57, 13);
            lblDuck.TabIndex = 1;
            lblDuck.Text = "+duck key";

            txtJumpKey.BackColor = Color.FromArgb(28, 28, 28);
            txtJumpKey.Font = new Font("Tahoma", 8.25F);
            txtJumpKey.ForeColor = Color.FromArgb(230, 230, 230);
            txtJumpKey.Location = new Point(110, 23);
            txtJumpKey.Name = "txtJumpKey";
            txtJumpKey.Size = new Size(110, 21);
            txtJumpKey.TabIndex = 3;
            txtJumpKey.Text = "SPACE";

            txtDuckKey.BackColor = Color.FromArgb(28, 28, 28);
            txtDuckKey.Font = new Font("Tahoma", 8.25F);
            txtDuckKey.ForeColor = Color.FromArgb(230, 230, 230);
            txtDuckKey.Location = new Point(110, 47);
            txtDuckKey.Name = "txtDuckKey";
            txtDuckKey.Size = new Size(110, 21);
            txtDuckKey.TabIndex = 4;
            txtDuckKey.Text = "LCTRL";

            groupTiming.BackColor = Color.FromArgb(30, 30, 30);
            groupTiming.Controls.Add(lblDelay);
            groupTiming.Controls.Add(txtDelay);
            groupTiming.ForeColor = Color.FromArgb(150, 150, 150);
            groupTiming.Location = new Point(280, 18);
            groupTiming.Name = "groupTiming";
            groupTiming.Size = new Size(120, 83);
            groupTiming.TabIndex = 1;
            groupTiming.TabStop = false;
            groupTiming.Text = "timing";

            lblDelay.AutoSize = true;
            lblDelay.ForeColor = Color.FromArgb(150, 150, 150);
            lblDelay.Location = new Point(21, 27);
            lblDelay.Name = "lblDelay";
            lblDelay.Size = new Size(58, 13);
            lblDelay.TabIndex = 0;
            lblDelay.Text = "Delay (ms)";

            txtDelay.BackColor = Color.FromArgb(28, 28, 28);
            txtDelay.BorderStyle = BorderStyle.FixedSingle;
            txtDelay.Font = new Font("Tahoma", 9F);
            txtDelay.ForeColor = Color.FromArgb(230, 230, 230);
            txtDelay.Location = new Point(21, 45);
            txtDelay.Name = "txtDelay";
            txtDelay.Size = new Size(80, 22);
            txtDelay.TabIndex = 1;
            txtDelay.Text = "900";

            btnUpdate.BackColor = Color.FromArgb(30, 30, 30);
            btnUpdate.FlatStyle = FlatStyle.Flat;
            btnUpdate.Font = new Font("Tahoma", 8.25F);
            btnUpdate.ForeColor = Color.FromArgb(230, 230, 230);
            btnUpdate.Location = new Point(109, 123);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(200, 24);
            btnUpdate.TabIndex = 2;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = false;

            pictureBox1.Location = new Point(263, 5);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(100, 50);
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;

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
            ((ISupportInitialize)picIcon).EndInit();
            panelMain.ResumeLayout(false);
            panelMain.PerformLayout();
            groupBinds.ResumeLayout(false);
            groupBinds.PerformLayout();
            groupTiming.ResumeLayout(false);
            groupTiming.PerformLayout();
            ((ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        private PictureBox pictureBox1;
    }
}
