using System.Text;

namespace PieceTable
{
	public sealed class PieceTable
	{
		private readonly ReadOnlyMemory<char> _originalText;
		private List<char> _additionalText;
		private LinkedList<Piece> _pieces;
		private int _fullTextLength;
		private EditingCache _editingCache;

		public PieceTable(ReadOnlyMemory<char> originalText)
		{
			_originalText = originalText;
			_additionalText = new();
			_pieces = new();
			_fullTextLength = _originalText.Length;

			if (!_originalText.IsEmpty) { _pieces.AddFirst(new Piece(Type.Original, new Span(0, _fullTextLength))); }
			_editingCache = new(_pieces.First, _fullTextLength);
		}

		/// <summary>
		/// 指定されたテキストをテキストデータへ挿入する。
		/// </summary>
		/// <param name="text">挿入するテキスト</param>
		/// <param name="insertPosition">テキストを挿入する位置</param>
		public void Insert(string text, int insertPosition)
		{
			//Pieceに入れるスタートとレングスのため、追加前後で文字数を取得
			var pieceStart = _additionalText.Count;
			_additionalText.AddRange(text.ToList());
			var pieceLength = _additionalText.Count - pieceStart;

			//編集箇所が前回と同様か
			if (_editingCache.PieceNode is not null)
			{
				//pieceが指定する_additionalTextの末尾の位置を取得
				int endPosition = _editingCache.PieceNode.Value.Span.Start + _editingCache.PieceNode.Value.Span.Length;

				//連続して挿入する際はPieceを新規作成せずにLengthを伸ばす。
				if (_editingCache.PositionInText == insertPosition && _editingCache.PieceNode.Value.Type is Type.Additional && endPosition == pieceStart)
				{
					_editingCache.PieceNode.Value.Span.Length += pieceLength;
				}
				else
				{
					var newPiece = new Piece(Type.Additional, new Span(pieceStart, pieceLength));
					_editingCache = SearchAndInsertText(newPiece, insertPosition, _editingCache);
				}
			}
			else
			{
				//そうでない時は挿入位置を検索後に挿入する。
				var newPiece = new Piece(Type.Additional, new Span(pieceStart, pieceLength));
				_editingCache = SearchAndInsertText(newPiece, insertPosition, _editingCache);
			}

			_editingCache.PositionInText = insertPosition + text.Length;

			//テキスト長を更新
			_fullTextLength += text.Length;
		}

		/// <summary>
		/// 挿入位置を検索し、新規Pieceを挿入する。
		/// </summary>
		/// <param name="newPiece">新規Piece</param>
		/// <param name="insertPosition">挿入位置</param>
		/// <param name="currentCache">前回の操作箇所</param>
		/// <returns></returns>
		private EditingCache SearchAndInsertText(Piece newPiece, int insertPosition, EditingCache currentCache)
		{
			var returnCache = new EditingCache(null, -1);

			//挿入位置がテキスト長の0か末尾にあるか
			if (insertPosition == 0)
			{
				returnCache.PieceNode = _pieces.AddFirst(newPiece);
			}
			else if (insertPosition >= _fullTextLength)
			{
				returnCache.PieceNode = _pieces.AddLast(newPiece);
			}
			else
			{
				LinkedListNode<Piece> targetNode = null;
				int positionInPiece = -1;

				//0か末尾以外なら、挿入位置がどのPieceのどの位置にあるかを調べる。
				if (currentCache.PieceNode is not null && currentCache.PositionInText == insertPosition)
				{
					//直近の編集箇所を操作する時はキャッシュを利用する。
					targetNode = currentCache.PieceNode;
					positionInPiece = targetNode.Value.Span.Length;
				}
				else
				{
					(targetNode, positionInPiece) = GetPieceAndPosition(insertPosition, _editingCache);
				}

				//Piece内の最初か末尾の場合は、その前後にPieceを差し込む
				if (0 == positionInPiece)
				{
					//最初
					returnCache.PieceNode = _pieces.AddBefore(targetNode, newPiece);
				}
				else if (targetNode.Value.Span.Length == positionInPiece)
				{
					//最後
					returnCache.PieceNode = _pieces.AddAfter(targetNode, newPiece);
				}
				else
				{
					//それ以外(途中のどこか)
					SplitPiece(targetNode, positionInPiece);
					returnCache.PieceNode = _pieces.AddAfter(targetNode, newPiece);
				}
			}

			return returnCache;
		}

		/// <summary>
		/// 指定された箇所のテキストを削除する。
		/// </summary>
		/// <param name="deleteLength">削除する文字数</param>
		/// <param name="startPosition">削除を開始する位置</param>
		public void Delete(int deleteLength, int startPosition)
		{

			LinkedListNode<Piece> targetNode = null;
			int positionInPiece = -1;

			bool isDeletionFromForward = false;

			if (startPosition >= _fullTextLength)
			{
				//末尾を消す時は最後のPieceを取得する。
				targetNode = _pieces.Last;
				positionInPiece = targetNode.Value.Span.Length;
			}
			else if (_editingCache.PieceNode is not null && startPosition == _editingCache.PositionInText)
			{
				//直前に操作した文字を削除する場合はキャッシュを利用する。
				targetNode = _editingCache.PieceNode;
				positionInPiece = _editingCache.PieceNode.Value.Span.Length;
			}
			else
			{
				//削除位置がどのPieceのどの位置にあるかを調べる。
				(targetNode, positionInPiece) = GetPieceAndPosition(startPosition, _editingCache);
			}

			//削除する文字数で処理を分岐
			if (deleteLength == 1)
			{
				_editingCache = DeleteSingleLetter(targetNode, positionInPiece, out isDeletionFromForward);
			}
			else
			{
				_editingCache = DeleteMultipleLetters(targetNode, positionInPiece, deleteLength, out isDeletionFromForward);
			}

			//キャッシュ末尾のテキスト内位置を設定。
			if (_editingCache.PieceNode is not null)
			{
				//Pieceの先頭を削除した時のみ、テキスト内位置がキャッシュの末尾になるように再設定。
				_editingCache.PositionInText = startPosition - deleteLength + (isDeletionFromForward ? targetNode.Value.Span.Length : 0);
			}

			//テキスト長を更新
			_fullTextLength -= deleteLength;
		}

		/// <summary>
		/// 指定された箇所のテキストを1文字削除する。
		/// </summary>
		/// <param name="targetNode">削除する文字を含むPiece</param>
		/// <param name="positionInPiece">削除する文字のPiece内の位置</param>
		/// <param name="isDeletionFromForward">先頭から削除するか</param>
		private EditingCache DeleteSingleLetter(LinkedListNode<Piece> targetNode, int positionInPiece, out bool isDeletionFromForward)
		{
			var returnCache = new EditingCache(null, -1);
			isDeletionFromForward = false;

			//Pieceが1文字だけなら削除する。
			if (targetNode.Value.Span.Length == 1)
			{
				_pieces.Remove(targetNode);
				return new EditingCache(null, 0);
			}

			if (positionInPiece == targetNode.Value.Span.Length)
			{
				//末尾を削除
				targetNode.Value.Span.Length -= 1;
			}
			else if (positionInPiece == 1)
			{
				//先頭1文字を削除
				targetNode.Value.Span.Start += 1;
				targetNode.Value.Span.Length -= 1;
				isDeletionFromForward = true;
			}
			else
			{
				SplitPiece(targetNode, positionInPiece);
				targetNode.Value.Span.Length -= 1;
			}

			returnCache.PieceNode = targetNode;
			return returnCache;
		}

		/// <summary>
		/// 指定された箇所のテキストを複数文字削除する。
		/// </summary>
		/// <param name="targetNode">削除する文字を含むPiece</param>
		/// <param name="positionInPiece">削除する文字のPiece内の位置</param>
		/// <param name="deleteLength">削除する文字数</param>
		private EditingCache DeleteMultipleLetters(LinkedListNode<Piece> targetNode, int positionInPiece, int deleteLength, out bool isDeletionFromForward)
		{
			var returnCache = new EditingCache(null, -1);
			isDeletionFromForward = false;

			//Pieceの先頭を削除する時は、先に処理をする。
			if (targetNode.Value.Span.Length > positionInPiece && deleteLength == positionInPiece)
			{
				targetNode.Value.Span.Start += deleteLength;
				targetNode.Value.Span.Length -= deleteLength;
				isDeletionFromForward = true;

				returnCache.PieceNode = targetNode;
				return returnCache;
			}

			if (positionInPiece != targetNode.Value.Span.Length)
			{
				SplitPiece(targetNode, positionInPiece);
			}

			if (deleteLength <= (targetNode.Value.Span.Length - 1))
			{
				targetNode.Value.Span.Length -= deleteLength;
				returnCache.PieceNode = targetNode;
			}
			else if (deleteLength == targetNode.Value.Span.Length)
			{
				_pieces.Remove(targetNode);
				//Pieceを削除し切った時はキャッシュを保存しない。
				returnCache = new EditingCache(null, 0);
			}
			else
			{
				deleteLength -= targetNode.Value.Span.Length;
				var followingTargetNode = targetNode.Previous;
				positionInPiece = followingTargetNode.Value.Span.Length;

				//現在のPieceを削除し、再帰処理で残りの文字を削除する。
				_pieces.Remove(targetNode);
				returnCache = DeleteMultipleLetters(followingTargetNode, positionInPiece, deleteLength, out isDeletionFromForward);
			}

			return returnCache;
		}

		public string GetText()
		{
			return "";
		}

		/// <summary>
		/// 全テキストを取得する。
		/// </summary>
		/// <returns></returns>
		public string GetAllText()
		{
			var sb = new StringBuilder();
			foreach (var piece in _pieces)
			{
				if (piece.Type == Type.Original)
				{
					sb.Append(_originalText.Slice(piece.Span.Start, piece.Span.Length));
				}
				else
				{
					sb.Append(string.Concat(_additionalText.GetRange(piece.Span.Start, piece.Span.Length)));
				}
			}

			var result = sb.ToString();
			return result;
		}

		/// <summary>
		/// 指定した箇所のPieceとPiece内の位置を取得する。
		/// </summary>
		/// <param name="position">テキスト内の指定した位置</param>
		/// <param name="currentCache">前回の操作箇所</param>
		/// <returns></returns>
		private (LinkedListNode<Piece> targetPiece, int positionInPiece) GetPieceAndPosition(int position, EditingCache currentCache)
		{
			//キャッシュが存在しない時
			if (currentCache.PieceNode is null || currentCache.PositionInText == _fullTextLength)
			{
				if ((_fullTextLength / 2) >= position)
				{
					return SearchFromForward(position);
				}
				else
				{
					return SearchFromBackward(position);
				}
			}

			//先頭、末尾とキャッシュの中で、指定箇所に一番近い部分を探して検索する。
			if (currentCache.PositionInText > position)
			{
				if ((currentCache.PositionInText - position) > position)
				{
					return SearchFromForward(position);
				}
				else
				{
					return SearchFromCache(position, currentCache, false);
				}
			}
			else
			{
				if ((position - currentCache.PositionInText) > (_fullTextLength - position))
				{
					return SearchFromBackward(position);
				}
				else
				{
					return SearchFromCache(position, currentCache);
				}
			}
		}

		/// <summary>
		/// リストの前方からPieceを検索する。
		/// </summary>
		/// <param name="position">テキスト内の指定した位置</param>
		/// <returns></returns>
		private (LinkedListNode<Piece> targetPiece, int positionInPiece) SearchFromForward(int position)
		{
			int currentPosition = 0;
			var node = _pieces.First;

			while (node is not null)
			{
				if ((currentPosition + node.Value.Span.Length) >= position)
				{
					var positionInPiece = position - currentPosition;
					return (node, positionInPiece);
				}

				currentPosition += node.Value.Span.Length;
				node = node.Next;
			}

			return (null, -1);
		}

		/// <summary>
		/// リストの後方からPieceを検索する。
		/// </summary>
		/// <param name="position">テキスト内の指定した位置</param>
		/// <returns></returns>
		private (LinkedListNode<Piece> targetPiece, int positionInPiece) SearchFromBackward(int position)
		{
			int currentPosition = _fullTextLength;
			var node = _pieces.Last;

			while (node is not null)
			{
				currentPosition -= node.Value.Span.Length;

				if (currentPosition <= position)
				{
					//後方からの場合は、現在位置を減算後にPiece内位置を取る。
					var positionInPiece = position - currentPosition;
					return (node, positionInPiece);
				}

				node = node.Previous;
			}

			return (null, -1);
		}

		/// <summary>
		/// キャッシュから末尾もしくは先頭に向かってPieceを検索する。
		/// </summary>
		/// <param name="position">テキスト内の指定した位置</param>
		/// <param name="currentCache">前回の操作箇所</param>
		/// <returns></returns>
		private (LinkedListNode<Piece> targetPiece, int positionInPiece) SearchFromCache(int position, EditingCache currentCache, bool searchToForward = true)
		{
			var node = currentCache.PieceNode;
			int currentPosition = 0;

			if (searchToForward)
			{
				//テキスト内位置がキャッシュ内のPieceの末尾にあるので一度巻き戻す。
				currentPosition = currentCache.PositionInText - node.Value.Span.Length;
				while (node is not null)
				{
					if ((currentPosition + node.Value.Span.Length) >= position)
					{
						var positionInPiece = position - currentPosition;
						return (node, positionInPiece);
					}

					currentPosition += node.Value.Span.Length;
					node = node.Next;
				}
			}
			else
			{
				currentPosition = currentCache.PositionInText;
				while (node is not null)
				{
					currentPosition -= node.Value.Span.Length;

					if (currentPosition <= position)
					{
						//後方からの場合は、現在位置を減算後にPiece内位置を取る。
						var positionInPiece = position - currentPosition;
						return (node, positionInPiece);
					}

					node = node.Previous;
				}
			}

			return (null, -1);
		}

		/// <summary>
		/// 指定したPieceを分割する。
		/// </summary>
		/// <param name="node">分割対象のPiece</param>
		/// <param name="splitPosition">Pieceの分割位置</param>
		private void SplitPiece(LinkedListNode<Piece> node, int splitPosition)
		{
			int splittedPieceLength = node.Value.Span.Length - splitPosition;

			node.Value.Span.Length = splitPosition;
			var splittedPiece = new Piece(node.Value.Type, new Span(splitPosition + node.Value.Span.Start, splittedPieceLength));

			_pieces.AddAfter(node, splittedPiece);
		}
	}

	/// <summary>
	/// Piece Table内のピースを表す。
	/// </summary>
	internal sealed class Piece
	{
		public Type Type;
		public Span Span;

		internal Piece(Type type, Span span)
		{
			Type = type;
			Span = span;
		}
	}

	/// <summary>
	/// バッファー内の範囲を保持する。
	/// </summary>
	internal sealed class Span
	{
		/// <summary>バッファーの読込開始位置</summary>
		public int Start;
		/// <summary>開始位置から読み込む長さ</summary>
		public int Length;

		internal Span(int start, int length)
		{
			Start = start;
			Length = length;
		}
	}

	internal enum Type
	{
		Original,
		Additional
	}

	/// <summary>
	/// 直近で編集したPieceの情報を保持する。
	/// </summary>
	internal sealed class EditingCache
	{
		/// <summary>直近に操作したPiece(Node形式) </summary>
		public LinkedListNode<Piece> PieceNode;
		/// <summary>Pieceの末尾の文字がテキストのどこにあるか</summary>
		public int PositionInText;

		internal EditingCache(LinkedListNode<Piece> pieceNode, int positionInText)
		{
			PieceNode = pieceNode;
			PositionInText = positionInText;
		}
	}
}
