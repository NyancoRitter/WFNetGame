using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFGame
{
	/// <summary>
	/// ゲーム内容実装用．
	/// MainForm から各メソッド等が呼ばれる．
	/// </summary>
	internal interface IGame
	{
		/// <summary>ゲーム内容の描画サイズ[pixel]．常に同じ値を返すこと．</summary>
		int Width{	get;	}
		/// <summary>ゲーム内容の描画サイズ[pixel]．常に同じ値を返すこと．</summary>
		int Height{ get; }

		/// <summary>
		/// ゲーム状態をリセットする．
		/// 少なくとも <see cref="Update"/> よりも前に１度は呼ばれる．
		/// </summary>
		void Restart();

		/// <summary>ゲーム更新処理</summary>
		/// <returns>
		/// 再描画の必要性．
		/// trueを返した場合，<see cref="Paint"/> が呼ばれる．
		/// </returns>
		bool Update();

		/// <summary>ゲーム状況描画処理</summary>
		/// <param name="g">
		/// 描画先．
		/// サイズが <see cref="Width"/> * <see cref="Height"/> [pixel]で PixelFormat が Format32bppRgb な Bitmap 
		/// に関連付けられた Graphics オブジェクトが渡される．
		/// </param>
		void Paint( System.Drawing.Graphics g );
	}
}
