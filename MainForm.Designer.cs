namespace WFNetGame
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			Main_toolStripContainer = new ToolStripContainer();
			Main_menuStrip = new MenuStrip();
			Game_toolStripMenuItem = new ToolStripMenuItem();
			Reset_toolStripMenuItem = new ToolStripMenuItem();
			Scale_toolStripMenuItem = new ToolStripMenuItem();
			TimerInterval_toolStripMenuItem = new ToolStripMenuItem();
			GameLoop_timer = new System.Windows.Forms.Timer(components);
			Main_toolStripContainer.TopToolStripPanel.SuspendLayout();
			Main_toolStripContainer.SuspendLayout();
			Main_menuStrip.SuspendLayout();
			SuspendLayout();
			// 
			// Main_toolStripContainer
			// 
			// 
			// Main_toolStripContainer.ContentPanel
			// 
			Main_toolStripContainer.ContentPanel.BackColor = Color.Black;
			Main_toolStripContainer.ContentPanel.Size = new Size(539, 333);
			Main_toolStripContainer.Dock = DockStyle.Fill;
			Main_toolStripContainer.Location = new Point(0, 0);
			Main_toolStripContainer.Name = "Main_toolStripContainer";
			Main_toolStripContainer.Size = new Size(539, 357);
			Main_toolStripContainer.TabIndex = 0;
			Main_toolStripContainer.Text = "toolStripContainer1";
			// 
			// Main_toolStripContainer.TopToolStripPanel
			// 
			Main_toolStripContainer.TopToolStripPanel.Controls.Add(Main_menuStrip);
			// 
			// Main_menuStrip
			// 
			Main_menuStrip.Dock = DockStyle.None;
			Main_menuStrip.Items.AddRange(new ToolStripItem[] { Game_toolStripMenuItem, Scale_toolStripMenuItem, TimerInterval_toolStripMenuItem });
			Main_menuStrip.Location = new Point(0, 0);
			Main_menuStrip.Name = "Main_menuStrip";
			Main_menuStrip.Size = new Size(539, 24);
			Main_menuStrip.TabIndex = 0;
			Main_menuStrip.Text = "menuStrip1";
			// 
			// Game_toolStripMenuItem
			// 
			Game_toolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { Reset_toolStripMenuItem });
			Game_toolStripMenuItem.Name = "Game_toolStripMenuItem";
			Game_toolStripMenuItem.Overflow = ToolStripItemOverflow.AsNeeded;
			Game_toolStripMenuItem.Size = new Size(65, 20);
			Game_toolStripMenuItem.Text = "Game(&G)";
			// 
			// Reset_toolStripMenuItem
			// 
			Reset_toolStripMenuItem.Name = "Reset_toolStripMenuItem";
			Reset_toolStripMenuItem.Size = new Size(117, 22);
			Reset_toolStripMenuItem.Text = "Reset(&R)";
			Reset_toolStripMenuItem.Click += Reset_toolStripMenuItem_Click;
			// 
			// Scale_toolStripMenuItem
			// 
			Scale_toolStripMenuItem.Name = "Scale_toolStripMenuItem";
			Scale_toolStripMenuItem.Overflow = ToolStripItemOverflow.AsNeeded;
			Scale_toolStripMenuItem.Size = new Size(60, 20);
			Scale_toolStripMenuItem.Text = "Scale(&S)";
			// 
			// TimerInterval_toolStripMenuItem
			// 
			TimerInterval_toolStripMenuItem.Name = "TimerInterval_toolStripMenuItem";
			TimerInterval_toolStripMenuItem.Overflow = ToolStripItemOverflow.AsNeeded;
			TimerInterval_toolStripMenuItem.Size = new Size(104, 20);
			TimerInterval_toolStripMenuItem.Text = "Timer Interval(&T)";
			// 
			// GameLoop_timer
			// 
			GameLoop_timer.Tick += GameLoop_timer_Tick;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(539, 357);
			Controls.Add(Main_toolStripContainer);
			FormBorderStyle = FormBorderStyle.Fixed3D;
			MainMenuStrip = Main_menuStrip;
			Margin = new Padding(1);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "MainForm";
			Text = "WinForms(.NET) Test";
			FormClosing += MainForm_FormClosing;
			Load += MainForm_Load;
			Main_toolStripContainer.TopToolStripPanel.ResumeLayout(false);
			Main_toolStripContainer.TopToolStripPanel.PerformLayout();
			Main_toolStripContainer.ResumeLayout(false);
			Main_toolStripContainer.PerformLayout();
			Main_menuStrip.ResumeLayout(false);
			Main_menuStrip.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private ToolStripContainer Main_toolStripContainer;
		private MenuStrip Main_menuStrip;
		private ToolStripMenuItem Game_toolStripMenuItem;
		private ToolStripMenuItem Scale_toolStripMenuItem;
		private ToolStripMenuItem TimerInterval_toolStripMenuItem;
		private ToolStripMenuItem Reset_toolStripMenuItem;
		private System.Windows.Forms.Timer GameLoop_timer;
	}
}
