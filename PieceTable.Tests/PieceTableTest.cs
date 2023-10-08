using System.Reflection;

namespace PieceTable.Tests
{	public class PieceTableTest : IDisposable
	{
		private ReadOnlyMemory<char> _text;
		private PieceTable _pieceTable;

		/// <summary>
		/// 開始処理。
		/// テストデータを作成する。
		/// </summary>
		public PieceTableTest()
		{
			_text = "1234567890".AsMemory();
			_pieceTable = new PieceTable(_text);
		}

		/// <summary>
		/// 終了処理。
		/// 現状は特になし。
		/// </summary>
		public void Dispose()
		{
		}

		[Fact]
		public void テキストの先頭にInsertができること()
		{
			_pieceTable.Insert("ABC", 0);

			Assert.Equal("ABC1234567890", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストの2文字目にInsertができること()
		{
			_pieceTable.Insert("ABC", 1);

			Assert.Equal("1ABC234567890", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストの末尾にInsertができること()
		{
			_pieceTable.Insert("ABC", _text.Length);

			Assert.Equal("1234567890ABC", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストの末尾から2文字目にInsertができること()
		{
			_pieceTable.Insert("ABC", _text.Length - 1);

			Assert.Equal("123456789ABC0", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストの中間にInsertができること()
		{
			_pieceTable.Insert("ABC", _text.Length / 2);

			Assert.Equal("12345ABC67890", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストに複数回Insertができること()
		{
			_pieceTable.Insert("ABC", 0);
			_pieceTable.Insert("DEF", 8);
			//挿入後なのでテキスト長を再取得する(実際の運用ではLengthは使わない)。
			_pieceTable.Insert("GHI", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("abc", 2);

			Assert.Equal("ABabcC12345DEF67890GHI", _pieceTable.GetAllText());
		}

		[Fact]
		public void 直前の編集箇所にInsertを行う際にキャッシュを利用しているか()
		{
			_pieceTable.Insert("ABC", 0);
			_pieceTable.Insert("ABC", 3);

			Assert.Equal("ABCABC1234567890", _pieceTable.GetAllText());
		}

		[Fact]
		public void キャッシュから先頭に向かって検索が行えること()
		{
			_text = "a123456789b123456789c123456789".AsMemory();
			_pieceTable = new PieceTable(_text);
			_pieceTable.Insert("cache", 15);
			_pieceTable.Insert("ABC", 10);

			Assert.Equal("a123456789ABCb1234cache56789c123456789", _pieceTable.GetAllText());
		}

		[Fact]
		public void キャッシュから末尾に向かって検索が行えること()
		{
			_text = "a123456789b123456789c123456789".AsMemory();
			_pieceTable = new PieceTable(_text);
			_pieceTable.Insert("cache", 15);
			_pieceTable.Insert("ABC", 25);

			Assert.Equal("a123456789b1234cache56789ABCc123456789", _pieceTable.GetAllText());
		}

		[Fact]
		public void エスケープシーケンスをInsertできること()
		{
			var esc = "\r\n\r\n\t\'\"\\";
			_pieceTable.Insert(esc, _text.Length / 2);

			Assert.Equal("12345\r\n\r\n\t\'\"\\67890", _pieceTable.GetAllText());
		}

		[Fact]

		public void テキストに連続して文字をInsertでき1文字ずつのPieceにしていないこと()
		{
			_pieceTable.Insert("A", 0);
			_pieceTable.Insert("B", 1);
			_pieceTable.Insert("C", 2);
			_pieceTable.Insert("D", 3);
			_pieceTable.Insert("E", 4);

			var endPosition = _pieceTable.GetAllText().Length;
			_pieceTable.Insert("F", endPosition);
			_pieceTable.Insert("G", endPosition + 1);
			_pieceTable.Insert("H", endPosition + 2);
			_pieceTable.Insert("I", endPosition + 3);
			_pieceTable.Insert("J", endPosition + 4);

			var middlePosition = _pieceTable.GetAllText().Length / 2;
			_pieceTable.Insert("K", middlePosition);
			_pieceTable.Insert("L", middlePosition + 1);
			_pieceTable.Insert("M", middlePosition + 2);
			_pieceTable.Insert("N", middlePosition + 3);
			_pieceTable.Insert("O", middlePosition + 4);

			//PrivateフィールドのPieceとそのプロパティを無理やり取得する。
			var pieces = _pieceTable.GetType().GetField("_pieces", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_pieceTable);
			int pieceCount = (int)pieces.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance).GetValue(pieces);

			Assert.Equal("ABCDE12345KLMNO67890FGHIJ", _pieceTable.GetAllText());
			Assert.Equal(5, pieceCount);
		}

		[Fact]
		public void テキストの末尾の1文字をDeleteできること()
		{
			_pieceTable.Delete(1, _text.Length);

			Assert.Equal("123456789", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストの末尾の複数文字をDeleteできること()
		{
			_pieceTable.Delete(3, _text.Length);

			Assert.Equal("1234567", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストの末尾から連続して1文字ずつDeleteできること()
		{
			_pieceTable.Delete(1, _text.Length);
			_pieceTable.Delete(1, _text.Length);
			_pieceTable.Delete(1, _text.Length);
			_pieceTable.Delete(1, _text.Length);
			_pieceTable.Delete(1, _text.Length);

			Assert.Equal("12345", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストの先頭の1文字をDeleteできること()
		{
			//start位置は要確認
			_pieceTable.Delete(1, 1);

			Assert.Equal("234567890", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストの先頭を含めた複数文字をDeleteできること()
		{
			_pieceTable.Delete(3, 3);

			Assert.Equal("4567890", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストの先頭から連続して1文字ずつDeleteできること()
		{
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(1, 1);

			Assert.Equal("67890", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストの中間の1文字をDeleteできること()
		{
			_pieceTable.Delete(1, _text.Length / 2);

			Assert.Equal("123467890", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストの中間の複数文字をDeleteできること()
		{
			_pieceTable.Delete(3, _text.Length / 2);

			Assert.Equal("1267890", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストに1文字のDeleteを複数回できること()
		{
			_pieceTable.Delete(1, _text.Length);
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(1, 5);

			Assert.Equal("2345789", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストに1文字および複数文字のDeleteを複数回できること()
		{
			_pieceTable.Delete(3, _text.Length);
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(3, 5);

			Assert.Equal("237", _pieceTable.GetAllText());
		}

		[Fact]
		public void 入力したテキストを全て削除した後に再度入力と削除ができること()
		{
			for (int i = 0; i < _text.Length; i++)
			{
				_pieceTable.Delete(1, (_text.Length - i));
			}

			_pieceTable.Insert("あいうえお", 0);
			_pieceTable.Delete(1, 5);

			Assert.Equal("あいうえ", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストにInsert後にDeleteができること()
		{
			_pieceTable.Insert("ABC", _text.Length / 2);
			_pieceTable.Insert("abc", _pieceTable.GetAllText().Length);
			_pieceTable.Delete(5, _pieceTable.GetAllText().Length);
			_pieceTable.Delete(2, 10);
			_pieceTable.Insert("DEF", _pieceTable.GetAllText().Length);
			_pieceTable.Delete(6, 10);
			Assert.Equal("1234EF", _pieceTable.GetAllText());
		}

		[Fact]
		public void テキストに連続してInsertとDeleteができること()
		{
			_pieceTable.Insert("A", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("B", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("C", _pieceTable.GetAllText().Length);
			_pieceTable.Delete(1, _pieceTable.GetAllText().Length);
			_pieceTable.Delete(1, _pieceTable.GetAllText().Length);
			_pieceTable.Insert("あ", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("い", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("う", _pieceTable.GetAllText().Length);
			_pieceTable.Delete(1, _pieceTable.GetAllText().Length);
			_pieceTable.Delete(1, _pieceTable.GetAllText().Length);
			_pieceTable.Insert("ア", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("イ", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("ウ", _pieceTable.GetAllText().Length);

			Assert.Equal("1234567890Aあアイウ", _pieceTable.GetAllText());
		}
	}
}