# PieceTable
テキストデータを管理するためのデータ構造。  

このコード自体は別プロジェクトで、テキストエディタを自作するために作成したものです。  
フレームワークの選定や設計などを見直したいので、そちらの実装はストップしていますが、  
かなり苦労して作ったので別に切り出して公開しています。

## PieceTableについて
- テキストデータの効率的な操作に使われるデータ構造。  
	- その他に有名なものはGapBufferやRopeなど。
- 新旧問わず色々なテキストエディタのデータ保持に採用されている。
	- VSCodeで採用されているものもPieceTableだが、  
	改行位置を効率的に取得できるようにPieceTreeという独自実装になっている。


## 詳細
### 基本仕様
- 基本的に保持するデータはオリジナルテキスト、追加テキストとPiece。  
	- オリジナルテキストと追加テキストはcharの配列などで、  	それぞれ、開いたテキストファイルと、追加入力されたテキストを保持する。  
	- Pieceは操作の度に編集される配列であり、  
	どちらからテキストを取るかと、どこから何文字を取るかを保持する。
- テキストを取得する際は、Pieceに対しループ処理を実行し、  
オリジナルテキストと追加テキストをつなぎ合わせて取得する。  
PieceTableではテキスト追加と削除がO(1)、テキスト取得がO(N)の処理速度になる。  
(テキストデータで頻繁に行われる操作は追加と削除であり、取得は保存時のみ)
	- テキストを管理する際に直感的に浮かぶのは、テキストをstringで保持し、それを配列で管理すること。  
	しかし、これでは参照はO(1)だが、  
	値を追加する際に既存データを動かす必要がありO(N)になってしまう。  
	(VSCodeも当初は配列で管理しようとしていたらしいが)
- 直前に操作した箇所のPieceを保持するキャッシュを、独自に実装している。  
テキストを挿入する際、挿入個所を探すためにPieceの配列をループ処理で確認する必要があるが、  
テキストの中間にデータを頻繁に追加したり、長期間の操作を続けるとPieceの数が増え、  
挿入個所の検索に時間を要する可能性がある。  
テキストデータの特徴として、同じ箇所を集中して操作する点が挙げられるため、  
操作の度にキャッシュとして直前のPieceを保持し、その前後を操作する際はキャッシュを利用することで、  
操作時のパフォーマンスを改善している。  

### 実装  
PieceTable.csの各クラスとそのメソッドの役割を簡易的に記載。  

- PieceTable  
PieceTableの各データと操作用のメソッドを保持。  
Pieceの保持には双方向リスト(C#ではGenericのLinkedList)を使用。  
追加と削除が高速で、参照が低速なデータ構造。
	- Insert  
	テキストを挿入する。  
	→ 通常の入力 + ペーストなどを想定。  
	連続して操作している場合(続けて「あいうえお」などとタイプした時)は、  
	Pieceは1個で取得する文字数を増やすようにする。  
	(単純にPieceを5個作るとPieceが増えすぎてしまいパフォーマンスが下がるため)
		- SearchAndInsertText  
		テキスト挿入対象のPieceを探し、テキストを挿入する。
	- Delete  
	テキストを削除する。  
	→ 通常の削除と範囲指定して削除を想定。  
	1文字削除か複数文字削除かで処理を切り替える。  
	取得文字数が0になったPieceができた際はPiece自体を削除する。
		- DeleteSingleLetter  
		テキストを1文字削除する。  
		→ 通常の削除(BSとDelete)を想定。
		- DeleteMultipleLetters  
		テキストを複数文字削除する。  
		→ 範囲削除(範囲選択してBSやDelete)を想定。  
		複数文字削除の場合は、Pieceをまたぐ可能性がある(100文字削除で、10文字指定のPieceが複数個など)。  
		削除処理はPieceに対し実行するため、  
		削除する文字数を減らして、Pieceも変更しながらで再帰で複数回実行する。
	- GetPieceAndPosition  
	(InsertまたはDelete時に実行)  
	操作位置とテキスト長を比較して、目的のPieceを最短で検索できるメソッドを呼ぶ。  
	(テキスト全体が5000文字で、操作位置が1000文字目ならSearchFromForward()を実行)
	- SearchFromForward  
	Pieceの配列を先頭から検索し、条件に一致するPieceを探す。
	- SearchFromBackward  
	Pieceの配列を末尾から検索し、条件に一致するPieceを探す。
	- SearchFromCache  
	キャッシュしたPieceから先頭、または末尾に向かってPieceの配列を検索し、  
	条件に一致するPieceを探す。
	- SplitPiece  
	(テキストデータの中間を編集する際に実行)  
	Pieceを分割する。
	- GetAllText  
	Pieceに対しループ処理を行い、テキストを取得する。
- Piece  
オリジナル/追加テキストのどちらからテキストを取るかと、  
Span(取得するテキスト数)を保持する。
- Span  
読み取る文字数を指定する。
- EditingCache  
直前に操作したPieceと、そのテキスト位置を保持する。  
テキストデータは同じ箇所を集中的に操作するため、InsertとDeleteではまずこのキャッシュを確認して操作する。

## 改善が必要な点
- Undo/Redo処理が未実装  
PieceTableはテキストをImmutableに保持するため、その点では操作のやり直しは実装しやすい。  
実装するには、各操作(テキスト挿入や削除など)と操作箇所を、  
履歴として保持してUndo/Redoに応じた操作が必要。  
- 最終的なテキストを取得する際に、全体を取るしか方法が無い。  
極端に言えば、1文字操作した後にテキストを取得する場合でも、テキスト全体を取得する必要がある。  
そのためテキストを操作する処理(マークダウンに変換やスタイルかけたりなど？)をアプリケーションとして実装する際、  
パフォーマンス面で不安なところではある。

## 参考
- [Piece Table - Wikipedia](https://en.wikipedia.org/wiki/Piece_table)
- [Text Buffer Reimplementation - VSCode Blog](https://code.visualstudio.com/blogs/2018/03/23/text-buffer-reimplementation)
- [piece-table - Darren Burns](https://darrenburns.net/posts/piece-table/)
- [Text Editor Data Structures - invoke::thought()](https://cdacamar.github.io/data%20structures/algorithms/benchmarking/text%20editors/c++/editor-data-structures/)
- [Piece Chains - Catch22](https://www.catch22.net/tuts/neatpad/piece-chains/)
- [テキストエディタで使われがちなデータ構造 Piece Table の概要と実装 - A Memorandum](https://blog1.mammb.com/entry/2022/09/07/224202)
- [【C++】テキストエディタのバッファ データ構造・アルゴリズム【第１回】 - TECH PROjin](https://tech.pjin.jp/blog/2020/11/16/buffer-1)