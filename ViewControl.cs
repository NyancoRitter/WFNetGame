using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFGame
{
	/// <summary>
	/// 表示倍率切り替え機能付き 表示域コントロール．
	/// - 表示物は Image 限定．これを整数倍に拡大表示するための領域．
	/// - 表示物サイズは固定である前提．
	/// </summary>
	public sealed partial class ViewControl : Control
	{
		private readonly int m_ContentW;	//表示物サイズ
		private readonly int m_ContentH;	//表示物サイズ
		private int m_Scale = 0;	//表示倍率
		private BufferedGraphics? m_BG;	//描画用バッファ

		/// <summary>private default ctor</summary>
		private ViewControl(){	InitializeComponent();	}

		/// <summary>
		/// 実用ctor.
		/// ※ここで指定した表示物サイズと <see cref="Draw"/> の引数画像サイズとを一致させること．
		/// </summary>
		/// <param name="contentW">表示したい物のx方向サイズ[pixel]</param>
		/// <param name="contentH">表示したい物のy方向サイズ[pixel]</param>
		public ViewControl( int contentW, int contentH )
			: this()
		{
			m_ContentW = contentW;
			m_ContentH = contentH;
			ViewScale = 1;
		}

		/// <summary>
		/// 表示倍率．
		/// 1以下の値を指定した場合には1（:等倍）を指定したものとみなされる．
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int ViewScale
		{
			get { return m_Scale; }
			set
			{
				int NewScale = Math.Max( 1, value );
				if( m_Scale==NewScale )return;

				m_Scale=NewScale;
				m_BG?.Dispose();

				this.Width = m_ContentW*m_Scale;
				this.Height = m_ContentH*m_Scale;

				m_BG = BufferedGraphicsManager.Current.Allocate(
					CreateGraphics(),
					this.ClientRectangle
				);
			}
		}

		/// <summary>
		/// 表示更新．
		/// 引数画像が表示域全体に引き延ばされて描画される．
		/// </summary>
		/// <param name="ContentImg">
		/// 表示内容．
		/// （この画像のサイズはctorで指定されたサイズであるという前提）
		/// </param>
		public void Draw( Image ContentImg )
		{
			var g = m_BG!.Graphics;
			g.InterpolationMode = InterpolationMode.NearestNeighbor;
			g.CompositingMode = CompositingMode.SourceCopy;	//若干，描画が早くなる？
			g.DrawImage( ContentImg, this.ClientRectangle );

			Invalidate();
			Update();
		}

		protected override void OnPaintBackground(PaintEventArgs pevent){	/* 領域全体を描画するからNOP */	}
		protected override void OnPaint(PaintEventArgs pe){	m_BG?.Render( pe.Graphics );	}
	}
}
