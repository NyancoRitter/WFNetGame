using System.Diagnostics;
using System.Net.Http.Headers;
using System.Windows.Forms;
using WFGame;

namespace WFNetGame
{
	/// <summary>メインフォーム</summary>
	public partial class MainForm : Form
	{
		//Scale メニュー用
		private readonly int[] m_ViewScales = { 1, 2, 3, 4 };	//表示倍率群
		private readonly ToolStripMenuItem[] m_ViewScaleMenuItems;
		//Timer Interval メニュー用
		private readonly int[] m_GameLoopIntervals = { 33, 62, 100, 250, 1000 };	//Interval値群[ms]
		private readonly ToolStripMenuItem[] m_GameLoopIntervalMenuItems;

		//現在のゲームループ間隔設定値[ms]
		private int SelectedGameLoopInterval { get; set; } = 9999;  //※初期値は m_GameLoopIntervals に無い値

		//時間計測手段：タイマインターバル調整用
		private Stopwatch m_SW = new Stopwatch();

		//ゲーム実装
		private IGame m_Game;

		//表示用
		private Bitmap m_ViewBitmap;    //m_Gameに描画させるための画像
		private Graphics m_Graphics_for_ViewBitmap; //m_ViewBitmapに対応したGraphics（都度作って捨てても良い気がするが）
		private ViewControl m_View; //表示域

		//-----------------------------------

		/// <summary>ctor</summary>
		public MainForm()
		{
			InitializeComponent();

			Main_menuStrip.CanOverflow = true;

			{//Scale Menu
				m_ViewScaleMenuItems = new ToolStripMenuItem[m_ViewScales.Length];
				for (int i = 0; i < m_ViewScales.Length; ++i)
				{
					int Scale = m_ViewScales[i];
					m_ViewScaleMenuItems[i] = new ToolStripMenuItem($"{Scale * 100}[%] (&{Scale})");
					{
						int index = i;
						m_ViewScaleMenuItems[i].Click += (object? sender, EventArgs e) => { ChangeScale(index); };
					}
				}
				Scale_toolStripMenuItem.DropDownItems.AddRange(m_ViewScaleMenuItems);
			}
			{//Interval Menu
				m_GameLoopIntervalMenuItems = new ToolStripMenuItem[m_GameLoopIntervals.Length];
				for (int i = 0; i < m_GameLoopIntervals.Length; ++i)
				{
					int Interval = m_GameLoopIntervals[i];
					int FPS = 1000 / Interval;
					m_GameLoopIntervalMenuItems[i] = new ToolStripMenuItem($"{Interval}[ms] ({FPS}FPS)");
					{
						int index = i;
						m_GameLoopIntervalMenuItems[i].Click += (object? sender, EventArgs e) => { ChangeTimerInterval(index); };
					}
				}
				TimerInterval_toolStripMenuItem.DropDownItems.AddRange(m_GameLoopIntervalMenuItems);
			}

			//ゲーム関係オブジェクト生成
			m_Game = new TheGame();
			m_ViewBitmap = new Bitmap(m_Game.Width, m_Game.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
			m_Graphics_for_ViewBitmap = Graphics.FromImage(m_ViewBitmap);

			//表示域準備
			m_View = new ViewControl(m_Game.Width, m_Game.Height);
			Main_toolStripContainer.ContentPanel.Controls.Add(m_View);
			m_View.Margin = new Padding(12);
			m_View.Left = m_View.Margin.Left;
			m_View.Top = m_View.Margin.Top;
			m_View.Visible = true;
		}

		//-----------------------------------
		#region private method

		/// <summary>表示倍率を変更（メニュー選択時処理用）</summary>
		/// <param name="ScaleIndex"> 配列 <see cref="m_ViewScales"/> のindexで指定する</param>
		private void ChangeScale(int ScaleIndex)
		{
			int SelectedScale = m_ViewScales[ScaleIndex];

			m_View.ViewScale = SelectedScale;
			ResizeToFit();
			UpdateView();

			for (int i = 0; i < m_ViewScaleMenuItems.Length; ++i)
			{ m_ViewScaleMenuItems[i].Checked = (i == ScaleIndex); }
		}

		/// <summary>タイマインターバルを変更（メニュー選択時処理用）</summary>
		/// <param name="IntervalIndex">配列 <see cref="m_GameLoopIntervals"/> のindexで指定する</param>
		private void ChangeTimerInterval(int IntervalIndex)
		{
			int SelectedInterval = m_GameLoopIntervals[IntervalIndex];
			if (SelectedGameLoopInterval == SelectedInterval) return;

			SelectedGameLoopInterval = SelectedInterval;
			GameLoop_timer.Interval = SelectedInterval;

			for (int i = 0; i < m_GameLoopIntervalMenuItems.Length; ++i)
			{ m_GameLoopIntervalMenuItems[i].Checked = (i == IntervalIndex); }
		}

		/// <summary>ゲームをリスタート</summary>
		private void RestartGame()
		{
			GameLoop_timer.Stop();

			m_Game.Restart();
			UpdateView();

			m_SW.Restart();
			GameLoop_timer.Start();
		}

		/// <summary>この Form のサイズをゲーム表示域サイズに合わせる</summary>
		private void ResizeToFit()
		{
			int dx = m_View.Width + m_View.Margin.Left + m_View.Margin.Right - Main_toolStripContainer.ContentPanel.Width;
			int dy = m_View.Height + m_View.Margin.Top + m_View.Margin.Bottom - Main_toolStripContainer.ContentPanel.Height;
			if (dx != 0) this.Width += dx;
			if (dy != 0) this.Height += dy;
		}

		/// <summary>表示更新</summary>
		private void UpdateView()
		{
			m_Game.Paint(m_Graphics_for_ViewBitmap);
			m_View.Draw(m_ViewBitmap);
		}

		#endregion
		//-----------------------------
		#region Event Handler

		//Load
		private void MainForm_Load(object sender, EventArgs e)
		{
			if( DesignMode )return;

			this.Text += " / " + Properties.Resources.APP_VERSION;

			ChangeScale(1);
			ChangeTimerInterval(1);
			RestartGame();
		}

		//FormClosing
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			GameLoop_timer.Stop();
			m_Graphics_for_ViewBitmap?.Dispose();
			m_ViewBitmap?.Dispose();
		}

		//ゲームメインループタイマ
		private void GameLoop_timer_Tick(object sender, EventArgs e)
		{
			GameLoop_timer.Stop();

			//ゲームの処理と描画
			if( m_Game.Update() )
			{	UpdateView();	}
			
			GameLoop_timer.Interval = (int)Math.Max( 1, SelectedGameLoopInterval - m_SW.ElapsedMilliseconds );
			GameLoop_timer.Start();
			m_SW.Restart();
		}

		//メニュー
		private void Reset_toolStripMenuItem_Click(object sender, EventArgs e){	RestartGame();	}

		#endregion
	}
}
