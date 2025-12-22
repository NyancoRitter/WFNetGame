using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WFGame
{
	/// <summary>敵の弾用（座標の 保持/更新 するだけ）</summary>
	class EnemyBullet
	{
		/// <summary>中心座標</summary>
		public double cx{	get;	private set;	}
		/// <summary>中心座標</summary>
		public double cy{	get;	private set;	}

		//速度 [pixel / update]
		private readonly double m_vx;
		private readonly double m_vy;

		/// <summary>ctor</summary>
		/// <param name="x">初期位置</param>
		/// <param name="y">初期位置</param>
		/// <param name="spd">弾速</param>
		/// <param name="dir_deg">発射方向[degree] (真下方向を0とする）</param>
		public EnemyBullet( double x, double y, double spd, double dir_deg )
		{
			cx = x;
			cy = y;
			double dir_rad = dir_deg * Math.PI / 180.0;
			m_vx = spd * Math.Sin(dir_rad);
			m_vy = spd * Math.Cos(dir_rad);
		}
		/// <summary>位置を速度分だけ更新</summary>
		public void Update(){	cx+=m_vx;	cy+=m_vy;	}
	};

	/// <summary>ゲーム実装</summary>
	internal class TheGame : IGame
	{
		//-----------------------------------
		//定数
		private const int MyRadius = 8;	//自機半径
		private const int MySpd = 4;	//自機移動速度

		private const int MyBulletRadius = 5;	//こっちの弾の半径
		private const int MyBulletSpd = 6;	//こっちの弾の速度

		private const int EnemyRadius = 25;	//敵の半径
		public const int EnemyBulletRadius = 5;	//敵の弾の半径

		//-----------------------------------
		//データ
		private Point m_MyPos;	//自機中心位置
		private int m_MyHP;	//自機HP
		private int m_MyRestReloadTime;	//残りリロード時間(これが0じゃないと弾を発射できない)
		private Bitmap m_MyShipImg;	//自機H画像
		
		private Point[] m_MyBulletsPos;	//こっちの弾の座標（この配列のサイズが，同時存在可能最大数）
		private int m_nMyBullets;	//現在存在するこっちの弾の数
		
		private Point m_EnemyPos;	//敵の中心位置
		private int m_EnemyHP;	//敵のHP
		private Bitmap m_EnemyShipImg;	//敵画像
		
		private EnemyBullet[] m_EnemyBullets;	//敵の弾（この配列のサイズが，同時存在可能最大数）
		private int m_nEnemyBullets;	//現在存在する敵の弾の数
		private int m_EnemyBulletDirRange;	//敵の弾の発射方向バラつき範囲[deg]

		private Random m_Rnd = new Random();	//乱数用
		private Font m_Font;	//文字列描画用

		private bool IsPlaying{	get;	set;	} = false;	//ゲームプレイ中か否か（いきなり始まってしまわないようにする用）
		
		//-----------------------------------

		/// <summary>ctor</summary>
		public TheGame()
		{
			m_Font = new Font( SystemFonts.DefaultFont.FontFamily, 16, FontStyle.Bold, GraphicsUnit.Pixel );

			m_MyBulletsPos = new Point[3];
			for( int i = 0; i < m_MyBulletsPos.Length; ++i ){ m_MyBulletsPos[i] = new Point(); }

			m_EnemyBullets = new EnemyBullet[16];

			m_MyShipImg = WFNetGame.Properties.Resources.MyShip;
			m_EnemyShipImg = WFNetGame.Properties.Resources.Enemy;
		}

		//-----------------------------------
		#region private

		/// <summary>ゲームクリア状態か否か</summary>
		private bool GameClear => ( m_EnemyHP<=0 );

		/// <summary>ゲームオーバー状態か否か</summary>
		private bool GameOver => ( m_MyHP<=0 );

		/// <summary>敵の新しいX位置を決める用</summary>
		/// <returns>乱数で決めたX座標</returns>
		private int NewEnemyX(){	return m_Rnd.Next( 10+EnemyRadius, Width-10-EnemyRadius );	}

		/// <summary>塗りつぶし円を描画</summary>
		/// <param name="g">描画先</param>
		/// <param name="Br">ブラシ</param>
		/// <param name="cx">中心座標x</param>
		/// <param name="cy">中心座標y</param>
		/// <param name="radius">半径</param>
		private static void FillCircle( Graphics g, Brush Br, int cx, int cy, int radius )
		{	g.FillEllipse( Br, cx-radius, cy-radius, radius*2, radius*2 );	}

		//↑の中心座標がdouble版
		private static void FillCircle( Graphics g, Brush Br, double cx, double cy, int radius )
		{	FillCircle( g, Br, (int)Math.Round(cx), (int)Math.Round(cy), radius );	}

		/// <summary>円同士の当たり判定用</summary>
		/// <param name="C1">円１の中心</param>
		/// <param name="r1">円１の半径</param>
		/// <param name="C2">円２の中心</param>
		/// <param name="r2">円２の半径</param>
		/// <returns>２つの円が重なっているか否か</returns>
		private static bool Collide( Point C1, int r1, Point C2, int r2 )
		{
			int dx = C1.X - C2.X;
			int dy = C1.Y - C2.Y;
			int SumR = r1 + r2;
			return ( dx*dx + dy*dy <= SumR*SumR );
		}

		/// <summary>文字列を中央に描画</summary>
		/// <param name="g">描画先</param>
		/// <param name="str">描画する文字列</param>
		private void DrawStrAtCenter( Graphics g, string str )
		{
			using( var SF = new StringFormat() )
			{
				SF.LineAlignment = StringAlignment.Center;
				SF.Alignment = StringAlignment.Center;
				g.DrawString( str, m_Font, Brushes.Black, new RectangleF(0,0,Width,Height), SF );
			}
		}

		#endregion
		//-----------------------------------
		#region IGame Impl

		/// <inheritdoc/>
		public int Width => 400;
		/// <inheritdoc/>
		public int Height => 300;

		/// <inheritdoc/>
		public void Restart()
		{
			m_MyPos = new Point( Width/2, Height-MyRadius-8 );
			m_MyHP = 6;
			m_MyRestReloadTime = 0;
			m_nMyBullets = 0;

			m_EnemyPos = new Point( NewEnemyX(), EnemyRadius+8 );
			m_EnemyHP = 16;
			m_nEnemyBullets = 0;
			m_EnemyBulletDirRange = m_Rnd.Next( 30, 120 );	//プレイ毎にてきとーに変える

			IsPlaying = false;
		}

		/// <inheritdoc/>
		public bool Update()
		{
			if( GameClear || GameOver )
			{
				if( Keyboard.IsKeyDown(Key.R) )  //Rキーでリセット
				{	Restart();	return true;	}

				return false;
			}

			if( !IsPlaying )
			{//操作に使ういずれかのキーを押したらゲーム開始
				IsPlaying = ( Keyboard.IsKeyDown(Key.Z) || Keyboard.IsKeyDown(Key.X) || Keyboard.IsKeyDown( Key.Space ) );
				return IsPlaying;
			}

			{//操作入力処理
				if( Keyboard.IsKeyDown( Key.Z ) )
				{	m_MyPos.X = Math.Max( m_MyPos.X-MySpd, MyRadius ); }
				else if( Keyboard.IsKeyDown( Key.X ) )
				{	m_MyPos.X = Math.Min( m_MyPos.X+MySpd, Width-MyRadius );	}

				if( m_MyRestReloadTime > 0 )
				{	--m_MyRestReloadTime;	}
				else if( m_nMyBullets < m_MyBulletsPos.Length  &&  Keyboard.IsKeyDown( Key.Space ) )
				{//弾の発射
					m_MyBulletsPos[m_nMyBullets].X = m_MyPos.X;
					m_MyBulletsPos[m_nMyBullets].Y = m_MyPos.Y;
					++m_nMyBullets;
					m_MyRestReloadTime = 5;
				}
			}
			{//こっちの弾の移動，敵との当たり関係の処理
				bool Hit = false;   //いずれかの弾が敵に当たったか？

				int i = 0;
				while( i < m_nMyBullets )
				{
					m_MyBulletsPos[i].Y -= MyBulletSpd;

					bool ShouldRemove = false;
					if( m_MyBulletsPos[i].Y < 0 )  //場外判定
					{ ShouldRemove = true; }
					else if( m_EnemyHP>0  &&  Collide( m_MyBulletsPos[i], MyBulletRadius, m_EnemyPos, EnemyRadius ) )
					{//敵に当たったとき
						--m_EnemyHP;
						Hit = true;
						ShouldRemove = true;
					}

					if( ShouldRemove )
					{
						m_MyBulletsPos[i] = m_MyBulletsPos[m_nMyBullets-1];
						--m_nMyBullets;
					}
					else
					{ ++i; }
				}

				//弾が当たった場合，敵がどこかにワープする
				if( Hit )
				{	m_EnemyPos.X = NewEnemyX();	}
			}

			if( m_EnemyHP>0 && ( m_nEnemyBullets <= m_Rnd.Next( m_EnemyBullets.Length ) ) )
			{//敵の弾発射処理
				m_EnemyBullets[m_nEnemyBullets] = new EnemyBullet(
					m_EnemyPos.X, m_EnemyPos.Y,
					3.0 + m_Rnd.NextDouble()*5.0,	//テキトーに速度をばらけさせる
					( m_Rnd.NextDouble() - 0.5 ) * m_EnemyBulletDirRange	//テキトーに方向をばらけさせる
				);
				++m_nEnemyBullets;
			}
			{//敵の弾の移動，自機との当たり関係の処理
				int i = 0;
				while( i < m_nEnemyBullets )
				{
					var EB = m_EnemyBullets[i];
					EB.Update();

					bool ShouldRemove = false;
					if( EB.cx < 0 || EB.cx >= Width || EB.cy >= Height )  //場外判定
					{ ShouldRemove = true; }
					else if( m_MyHP>0 && Collide( new Point( (int)Math.Round(EB.cx), (int)Math.Round(EB.cy) ), EnemyBulletRadius, m_MyPos, MyRadius ) )
					{//当たったとき
						--m_MyHP;
						ShouldRemove = true;
					}

					if( ShouldRemove )
					{
						m_EnemyBullets[i] = m_EnemyBullets[m_nEnemyBullets - 1];
						--m_nEnemyBullets;
					}
					else
					{ ++i; }
				}
			}
			return true;
		}

		/// <inheritdoc/>
		public void Paint(Graphics g)
		{
			g.Clear( Color.White );

			//自機，敵
			if( m_MyHP>0 ){	g.DrawImageUnscaled( m_MyShipImg, m_MyPos.X-MyRadius, m_MyPos.Y-MyRadius );	}
			if( m_EnemyHP>0 ){	g.DrawImageUnscaled( m_EnemyShipImg, m_EnemyPos.X-EnemyRadius, m_EnemyPos.Y-EnemyRadius );	}

			//弾
			for( int i=0; i<m_nMyBullets; ++i ){	FillCircle( g, Brushes.Blue, m_MyBulletsPos[i].X, m_MyBulletsPos[i].Y, MyBulletRadius );	}
			for( int i=0; i<m_nEnemyBullets; ++i ){	FillCircle( g, Brushes.Orange, m_EnemyBullets[i].cx, m_EnemyBullets[i].cy, EnemyBulletRadius );	}

			//HP残量
			const int HPBarW = 4;
			const int HPBarH = 8;
			if( m_MyHP>0 )
			{
				var Br = Brushes.Blue;
				var Rect = new Rectangle( 2, Height-HPBarH-2, HPBarW, HPBarH );
				for( int i=0; i<m_MyHP; ++i )
				{
					g.FillRectangle( Br, Rect );
					Rect.Offset( HPBarW+2, 0 );
				}
			}
			if( m_EnemyHP>0 )
			{
				var Br = Brushes.Olive;
				var Rect = new Rectangle( 2, 2, HPBarW, HPBarH );
				for( int i=0; i<m_EnemyHP; ++i )
				{
					g.FillRectangle( Br, Rect );
					Rect.Offset( HPBarW+2, 0 );
				}
			}

			//状況表示
			if( !IsPlaying ){	DrawStrAtCenter( g, "= {Z,X,Space} to Start =" );}
			if( GameOver ){	DrawStrAtCenter( g, "= GAME OVER =" + Environment.NewLine + "( [R] key to Restart )" );	}
			else if( GameClear ){	DrawStrAtCenter( g, "= SAVED YOUR PLANET !! =" + Environment.NewLine + "( [R] key to Restart )" );	}
		}

		#endregion
	}
}
