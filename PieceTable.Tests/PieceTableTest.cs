using System.Reflection;

namespace PieceTable.Tests
{	public class PieceTableTest : IDisposable
	{
		private ReadOnlyMemory<char> _text;
		private PieceTable _pieceTable;

		/// <summary>
		/// �J�n�����B
		/// �e�X�g�f�[�^���쐬����B
		/// </summary>
		public PieceTableTest()
		{
			_text = "1234567890".AsMemory();
			_pieceTable = new PieceTable(_text);
		}

		/// <summary>
		/// �I�������B
		/// ����͓��ɂȂ��B
		/// </summary>
		public void Dispose()
		{
		}

		[Fact]
		public void �e�L�X�g�̐擪��Insert���ł��邱��()
		{
			_pieceTable.Insert("ABC", 0);

			Assert.Equal("ABC1234567890", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g��2�����ڂ�Insert���ł��邱��()
		{
			_pieceTable.Insert("ABC", 1);

			Assert.Equal("1ABC234567890", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g�̖�����Insert���ł��邱��()
		{
			_pieceTable.Insert("ABC", _text.Length);

			Assert.Equal("1234567890ABC", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g�̖�������2�����ڂ�Insert���ł��邱��()
		{
			_pieceTable.Insert("ABC", _text.Length - 1);

			Assert.Equal("123456789ABC0", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g�̒��Ԃ�Insert���ł��邱��()
		{
			_pieceTable.Insert("ABC", _text.Length / 2);

			Assert.Equal("12345ABC67890", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g�ɕ�����Insert���ł��邱��()
		{
			_pieceTable.Insert("ABC", 0);
			_pieceTable.Insert("DEF", 8);
			//�}����Ȃ̂Ńe�L�X�g�����Ď擾����(���ۂ̉^�p�ł�Length�͎g��Ȃ�)�B
			_pieceTable.Insert("GHI", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("abc", 2);

			Assert.Equal("ABabcC12345DEF67890GHI", _pieceTable.GetAllText());
		}

		[Fact]
		public void ���O�̕ҏW�ӏ���Insert���s���ۂɃL���b�V���𗘗p���Ă��邩()
		{
			_pieceTable.Insert("ABC", 0);
			_pieceTable.Insert("ABC", 3);

			Assert.Equal("ABCABC1234567890", _pieceTable.GetAllText());
		}

		[Fact]
		public void �L���b�V������擪�Ɍ������Č������s���邱��()
		{
			_text = "a123456789b123456789c123456789".AsMemory();
			_pieceTable = new PieceTable(_text);
			_pieceTable.Insert("cache", 15);
			_pieceTable.Insert("ABC", 10);

			Assert.Equal("a123456789ABCb1234cache56789c123456789", _pieceTable.GetAllText());
		}

		[Fact]
		public void �L���b�V�����疖���Ɍ������Č������s���邱��()
		{
			_text = "a123456789b123456789c123456789".AsMemory();
			_pieceTable = new PieceTable(_text);
			_pieceTable.Insert("cache", 15);
			_pieceTable.Insert("ABC", 25);

			Assert.Equal("a123456789b1234cache56789ABCc123456789", _pieceTable.GetAllText());
		}

		[Fact]
		public void �G�X�P�[�v�V�[�P���X��Insert�ł��邱��()
		{
			var esc = "\r\n\r\n\t\'\"\\";
			_pieceTable.Insert(esc, _text.Length / 2);

			Assert.Equal("12345\r\n\r\n\t\'\"\\67890", _pieceTable.GetAllText());
		}

		[Fact]

		public void �e�L�X�g�ɘA�����ĕ�����Insert�ł�1��������Piece�ɂ��Ă��Ȃ�����()
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

			//Private�t�B�[���h��Piece�Ƃ��̃v���p�e�B�𖳗����擾����B
			var pieces = _pieceTable.GetType().GetField("_pieces", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_pieceTable);
			int pieceCount = (int)pieces.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance).GetValue(pieces);

			Assert.Equal("ABCDE12345KLMNO67890FGHIJ", _pieceTable.GetAllText());
			Assert.Equal(5, pieceCount);
		}

		[Fact]
		public void �e�L�X�g�̖�����1������Delete�ł��邱��()
		{
			_pieceTable.Delete(1, _text.Length);

			Assert.Equal("123456789", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g�̖����̕���������Delete�ł��邱��()
		{
			_pieceTable.Delete(3, _text.Length);

			Assert.Equal("1234567", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g�̖�������A������1��������Delete�ł��邱��()
		{
			_pieceTable.Delete(1, _text.Length);
			_pieceTable.Delete(1, _text.Length);
			_pieceTable.Delete(1, _text.Length);
			_pieceTable.Delete(1, _text.Length);
			_pieceTable.Delete(1, _text.Length);

			Assert.Equal("12345", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g�̐擪��1������Delete�ł��邱��()
		{
			//start�ʒu�͗v�m�F
			_pieceTable.Delete(1, 1);

			Assert.Equal("234567890", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g�̐擪���܂߂�����������Delete�ł��邱��()
		{
			_pieceTable.Delete(3, 3);

			Assert.Equal("4567890", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g�̐擪����A������1��������Delete�ł��邱��()
		{
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(1, 1);

			Assert.Equal("67890", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g�̒��Ԃ�1������Delete�ł��邱��()
		{
			_pieceTable.Delete(1, _text.Length / 2);

			Assert.Equal("123467890", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g�̒��Ԃ̕���������Delete�ł��邱��()
		{
			_pieceTable.Delete(3, _text.Length / 2);

			Assert.Equal("1267890", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g��1������Delete�𕡐���ł��邱��()
		{
			_pieceTable.Delete(1, _text.Length);
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(1, 5);

			Assert.Equal("2345789", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g��1��������ѕ���������Delete�𕡐���ł��邱��()
		{
			_pieceTable.Delete(3, _text.Length);
			_pieceTable.Delete(1, 1);
			_pieceTable.Delete(3, 5);

			Assert.Equal("237", _pieceTable.GetAllText());
		}

		[Fact]
		public void ���͂����e�L�X�g��S�č폜������ɍēx���͂ƍ폜���ł��邱��()
		{
			for (int i = 0; i < _text.Length; i++)
			{
				_pieceTable.Delete(1, (_text.Length - i));
			}

			_pieceTable.Insert("����������", 0);
			_pieceTable.Delete(1, 5);

			Assert.Equal("��������", _pieceTable.GetAllText());
		}

		[Fact]
		public void �e�L�X�g��Insert���Delete���ł��邱��()
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
		public void �e�L�X�g�ɘA������Insert��Delete���ł��邱��()
		{
			_pieceTable.Insert("A", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("B", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("C", _pieceTable.GetAllText().Length);
			_pieceTable.Delete(1, _pieceTable.GetAllText().Length);
			_pieceTable.Delete(1, _pieceTable.GetAllText().Length);
			_pieceTable.Insert("��", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("��", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("��", _pieceTable.GetAllText().Length);
			_pieceTable.Delete(1, _pieceTable.GetAllText().Length);
			_pieceTable.Delete(1, _pieceTable.GetAllText().Length);
			_pieceTable.Insert("�A", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("�C", _pieceTable.GetAllText().Length);
			_pieceTable.Insert("�E", _pieceTable.GetAllText().Length);

			Assert.Equal("1234567890A���A�C�E", _pieceTable.GetAllText());
		}
	}
}