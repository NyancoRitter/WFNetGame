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
	/// <summary>T型のデータ群を管理するヘルパ</summary>
	/// <typeparam name="T">
	/// データの型．
	/// <see cref="Add"/>の引数に指定した処理によって再初期化できる（：使い回せる）ものであること．
	/// </typeparam>
	class Xs<T>
		where T : class, new()
	{
		//データ．
		//先頭から Size 個のみが「存在する」という扱い．
		private T[] m_Data;

		/// <summary>ctor．データ最大個数を指定する</summary>
		/// <param name="Capacity">最大個数</param>
		public Xs( int Capacity )
		{
			if( Capacity<=0 )throw new ArgumentOutOfRangeException( "Invalid Capacitiy" );
			//指定された個数だけ作り，使いまわす．
			m_Data = new T[Capacity];
			for( int i=0; i<Capacity; ++i ){	m_Data[i] = new T();	}
		}

		/// <summary>現在のデータ個数</summary>
		public int Size{	get;	private set; } = 0;
		/// <summary>最大データ個数（ctorで指定した値）</summary>
		public int Capacity => m_Data.Length;
		/// <summary>最大データ個数に達しているか否か</summary>
		public bool Full => (m_Data.Length==Size);
		/// <summary>クリア（現在のデータ個数を0にする）</summary>
		public void Clear(){	Size=0;	}

		/// <summary>
		/// データ追加．
		/// ただしすでに最大個数に達している場合には何もしない．
		///		<remarks>
		///		<see cref="Update"/> や <see cref="Paint"/> 内での処理順は，追加した順となるわけではない．
		///		</remarks>
		/// </summary>
		/// <param name="Initializer">データ初期化手段．「追加」されるデータを初期状態に設定する</param>
		public void Add( Action<T> Initializer )
		{
			if( Full )return;
			Initializer( m_Data[Size] );
			++Size;
		}

		/// <summary>
		/// 更新処理．
		/// - 引数に指定された処理を用いてのデータ更新
		/// - 不要データの削除
		/// </summary>
		/// <param name="Updater">
		/// データの更新手段．
		/// - 引数 : 更新対象データ
		/// - 戻り値：falseを返した場合，引数に渡されたデータは不要であることを示す（→削除される）
		/// </param>
		public void Update( Func<T,bool> Updater )
		{
			int i = 0;
			while( i<Size )
			{
				if( Updater( m_Data[i] ) )
				{	++i;	}
				else
				{//末尾要素とSWAPすることで「削除」とする（データの順序は保存されない）
					--Size;
					var Tmp = m_Data[i];
					m_Data[i] = m_Data[Size];
					m_Data[Size] = Tmp;
				}
			}
		}

		/// <summary>描画処理</summary>
		/// <param name="Painter">引数に渡されたされたデータを描画する手段</param>
		public void Paint( Action<T> Painter )
		{
			for( int i=0; i<Size; ++i )
			{	Painter( m_Data[i] );	}
		}
	}

	/// <summary>弾用データ（座標の 保持/更新 するだけ）</summary>
	class Bullet
	{
		/// <summary>中心座標</summary>
		public double cx{	get;	private set;	}
		/// <summary>中心座標</summary>
		public double cy{	get;	private set;	}

		//速度 [pixel / update]
		private double m_vx;
		private double m_vy;

		/// <summary>中心座標を整数に丸めた結果を返す</summary>
		public Point IntPos => new Point( (int)Math.Round(cx), (int)Math.Round(cy) );

		/// <summary>初期化</summary>
		/// <param name="x">初期位置</param>
		/// <param name="y">初期位置</param>
		/// <param name="spd">弾速</param>
		/// <param name="dir_deg">発射方向[degree] (真下方向を0とする）</param>
		public void Reset( double x, double y, double spd, double dir_deg )
		{
			cx = x;
			cy = y;
			double dir_rad = dir_deg * Math.PI / 180.0;
			m_vx = spd * Math.Sin(dir_rad);
			m_vy = spd * Math.Cos(dir_rad);
		}
		/// <summary>位置を速度分だけ更新</summary>
		public void Update(){	cx+=m_vx;	cy+=m_vy;	}
	}

	/// <summary>爆発表示用データ（座標の保持と，アニメーションカウンタを増やすことだけ）</summary>
	class Explosion
	{
		/// <summary>中心座標</summary>
		public Point Pos{	get;	private set;	} = new Point();
		/// <summary>アニメーションカウンタ</summary>
		public int iAnim{	get;	private set;	} = 0;

		/// <summary>初期化． <see cref="iAnim"/>の値は0になる</summary>
		/// <param name="pos">中心位置</param>
		public void Reset( Point pos ){	Pos = pos;	iAnim = 0;	}

		/// <summary> <see cref="iAnim"/>の値が1増加するだけ</summary>
		public void Update(){	++iAnim; }
	};

	//========================================================

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
		private const int EnemyBulletRadius = 5;	//敵の弾の半径

		//表示用定数
		private const int ExsplosionRadius = 8;	//爆発の半径
		private const int HPBarW = 4;
		private const int HPBarH = 8;

		//-----------------------------------
		//データ
		private Point m_MyPos;	//自機中心位置
		private int m_MyHP;	//自機HP
		private int m_MyRestReloadTime;	//残りリロード時間(これが0じゃないと弾を発射できない)
		private Xs<Bullet> m_MyBullets;	//こっちの弾
		
		private Point m_EnemyPos;	//敵の中心位置
		private int m_EnemyHP;	//敵のHP
		private Xs<Bullet> m_EnemyBullets;	//敵の弾
		private int m_EnemyBulletDirRange;	//敵の弾の発射方向バラつき範囲[deg]

		private Xs<Explosion> m_Explosions;	//爆発表示用

		private Random m_Rnd = new Random();	//乱数用
		private Font m_Font;	//文字列描画用
		private Bitmap m_MyShipImg;	//自機画像
		private Bitmap m_EnemyShipImg;	//敵画像

		private bool IsPlaying{	get;	set;	} = false;	//ゲームプレイ中か否か（いきなり始まってしまわないようにする用）
		
		//-----------------------------------

		/// <summary>ctor</summary>
		public TheGame()
		{
			m_MyBullets = new Xs<Bullet>( 3 );
			m_EnemyBullets = new Xs<Bullet>( 16 );
			m_Explosions = new Xs<Explosion>( 5 );

			m_Font = new Font( SystemFonts.DefaultFont.FontFamily, 16, FontStyle.Bold, GraphicsUnit.Pixel );
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

		/// <summary>塗りつぶし円を描画</summary>
		/// <param name="g">描画先</param>
		/// <param name="Br">ブラシ</param>
		/// <param name="c">中心座標</param>
		/// <param name="radius">半径</param>
		private static void FillCircle( Graphics g, Brush Br, Point c, int radius )
		{	g.FillEllipse( Br, c.X-radius, c.Y-radius, radius*2, radius*2 );	}

		/// <summary>円を描画</summary>
		/// <param name="g">描画先</param>
		/// <param name="pen">ペン</param>
		/// <param name="c">中心座標</param>
		/// <param name="radius">半径</param>
		private static void DrawCircle( Graphics g, Pen pen, Point c, int radius )
		{	g.DrawEllipse( pen, c.X-radius, c.Y-radius, radius*2, radius*2 );	}

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

		/// <summary>HP残量描画</summary>
		/// <param name="g">描画先</param>
		/// <param name="left">描画箇所（左上座標）</param>
		/// <param name="top">（左上座標）</param>
		/// <param name="HP">HP量</param>
		/// <param name="Br">ブラシ</param>
		private static void DrawHPBar( Graphics g, int left, int top, int HP, Brush Br )
		{
			if( HP<=0 )return;
			var Rect = new Rectangle( left, top, HPBarW, HPBarH );
			for( int i=0; i<HP; ++i )
			{
				g.FillRectangle( Br, Rect );
				Rect.Offset( HPBarW+2, 0 );
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
			m_MyBullets.Clear();

			m_EnemyPos = new Point( NewEnemyX(), EnemyRadius+8 );
			m_EnemyHP = 16;
			m_EnemyBullets.Clear();
			m_EnemyBulletDirRange = m_Rnd.Next( 30, 120 );	//プレイ毎にてきとーに変える

			m_Explosions.Clear();

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

			//自機移動
			if( Keyboard.IsKeyDown( Key.Z ) )
			{	m_MyPos.X = Math.Max( m_MyPos.X-MySpd, MyRadius ); }
			else if( Keyboard.IsKeyDown( Key.X ) )
			{	m_MyPos.X = Math.Min( m_MyPos.X+MySpd, Width-MyRadius );	}

			//自機の弾発射
			if( m_MyRestReloadTime > 0 )
			{	--m_MyRestReloadTime;	}
			else if( !m_MyBullets.Full  &&  Keyboard.IsKeyDown( Key.Space ) )
			{
				m_MyBullets.Add( bullet => bullet.Reset( m_MyPos.X,m_MyPos.Y,MyBulletSpd, 180 ) );
				m_MyRestReloadTime = 5;
			}

			{//こっちの弾の移動，敵との当たり関係の処理
				bool Hit = false;   //いずれかの弾が敵に当たったか？

				m_MyBullets.Update(
					bullet =>
					{
						bullet.Update();
						if( bullet.cy < 0 )return false;	//場外判定

						var P = bullet.IntPos;
						if( m_EnemyHP>0  &&  Collide( P, MyBulletRadius, m_EnemyPos, EnemyRadius ) )
						{
							--m_EnemyHP;
							Hit = true;
							m_Explosions.Add( e => e.Reset( P ) );
							return false;
						}

						return true;
					}
				);

				//弾が当たった場合，敵がどこかにワープする
				if( Hit )
				{	m_EnemyPos.X = NewEnemyX();	}
			}

			//敵の弾発射処理
			if( m_EnemyHP > 0 && (m_EnemyBullets.Size <= m_Rnd.Next(m_EnemyBullets.Capacity)) )
			{
				m_EnemyBullets.Add(
					bullet => bullet.Reset(
						m_EnemyPos.X, m_EnemyPos.Y,
						3.0 + m_Rnd.NextDouble() * 5.0, //テキトーに速度をばらけさせる
						(m_Rnd.NextDouble() - 0.5) * m_EnemyBulletDirRange  //テキトーに方向をばらけさせる
					)
				);
			}

			//敵の弾の移動，自機との当たり関係の処理
			m_EnemyBullets.Update(
				bullet =>
				{
					bullet.Update();
					if (bullet.cx < 0 || bullet.cx >= Width || bullet.cy >= Height)return false;  //場外判定

					var P = bullet.IntPos;
					if( m_MyHP > 0 && Collide( P, EnemyBulletRadius, m_MyPos, MyRadius ) )
					{//当たったとき
						--m_MyHP;
						m_Explosions.Add( e => e.Reset( P ) );
						return false;
					}
					return true;
				}
			);

			//爆発アニメーションの更新
			m_Explosions.Update(
				e => {	e.Update();	return ( e.iAnim<=2 );	}
			);
			return true;
		}

		/// <inheritdoc/>
		public void Paint(Graphics g)
		{
			g.Clear( Color.White );

			//自機，敵
			if( m_MyHP>0 ){	g.DrawImageUnscaled( m_MyShipImg, m_MyPos.X-MyRadius, m_MyPos.Y-MyRadius );	}
			if( m_EnemyHP>0 ){	g.DrawImageUnscaled( m_EnemyShipImg, m_EnemyPos.X-EnemyRadius, m_EnemyPos.Y-EnemyRadius );	}

			//爆発
			m_Explosions.Paint(
				e => 
				{
					if( e.iAnim <= 1 )FillCircle( g, Brushes.Red, e.Pos, ExsplosionRadius );
					else DrawCircle( g, Pens.Red, e.Pos, ExsplosionRadius );
				}
			);

			//弾
			m_MyBullets.Paint( bullet => FillCircle( g, Brushes.Blue, bullet.IntPos, MyBulletRadius ) );
			m_EnemyBullets.Paint( bullet => FillCircle( g, Brushes.Orange, bullet.IntPos, EnemyBulletRadius ) );
			
			//HP残量
			DrawHPBar( g, 2, Height-HPBarH-2, m_MyHP, Brushes.Blue );
			DrawHPBar( g, 2, 2, m_EnemyHP, Brushes.Olive );

			//状況表示
			if( !IsPlaying ){	DrawStrAtCenter( g, "= {Z,X,Space} to Start =" );	}
			else if( GameOver ){	DrawStrAtCenter( g, "= GAME OVER =" + Environment.NewLine + "( [R] key to Restart )" );	}
			else if( GameClear ){	DrawStrAtCenter( g, "= SAVED YOUR PLANET !! =" + Environment.NewLine + "( [R] key to Restart )" );	}
		}

		#endregion
	}
}
