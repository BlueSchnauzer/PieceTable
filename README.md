# PieceTable
�e�L�X�g�f�[�^���Ǘ����邽�߂̃f�[�^�\���B  

���̃R�[�h���͕̂ʃv���W�F�N�g�ŁA�e�L�X�g�G�f�B�^�����삷�邽�߂ɍ쐬�������̂ł��B  
�t���[�����[�N�̑I���݌v�Ȃǂ������������̂ŁA������̎����̓X�g�b�v���Ă��܂����A  
���Ȃ��J���č�����̂ŕʂɐ؂�o���Č��J���Ă��܂��B

## PieceTable�ɂ���
- �e�L�X�g�f�[�^�̌����I�ȑ���Ɏg����f�[�^�\���B  
	- ���̑��ɗL���Ȃ��̂�GapBuffer��Rope�ȂǁB
- �V����킸�F�X�ȃe�L�X�g�G�f�B�^�̃f�[�^�ێ��ɍ̗p����Ă���B
	- VSCode�ō̗p����Ă�����̂�PieceTable�����A  
	���s�ʒu�������I�Ɏ擾�ł���悤��PieceTree�Ƃ����Ǝ������ɂȂ��Ă���B


## �ڍ�
### ��{�d�l
- ��{�I�ɕێ�����f�[�^�̓I���W�i���e�L�X�g�A�ǉ��e�L�X�g��Piece�B  
	- �I���W�i���e�L�X�g�ƒǉ��e�L�X�g��char�̔z��ȂǂŁA  	���ꂼ��A�J�����e�L�X�g�t�@�C���ƁA�ǉ����͂��ꂽ�e�L�X�g��ێ�����B  
	- Piece�͑���̓x�ɕҏW�����z��ł���A  
	�ǂ��炩��e�L�X�g����邩�ƁA�ǂ����牽��������邩��ێ�����B
- �e�L�X�g���擾����ۂ́APiece�ɑ΂����[�v���������s���A  
�I���W�i���e�L�X�g�ƒǉ��e�L�X�g���Ȃ����킹�Ď擾����B  
PieceTable�ł̓e�L�X�g�ǉ��ƍ폜��O(1)�A�e�L�X�g�擾��O(N)�̏������x�ɂȂ�B  
(�e�L�X�g�f�[�^�ŕp�ɂɍs���鑀��͒ǉ��ƍ폜�ł���A�擾�͕ۑ����̂�)
	- �e�L�X�g���Ǘ�����ۂɒ����I�ɕ����Ԃ̂́A�e�L�X�g��string�ŕێ����A�����z��ŊǗ����邱�ƁB  
	�������A����ł͎Q�Ƃ�O(1)�����A  
	�l��ǉ�����ۂɊ����f�[�^�𓮂����K�v������O(N)�ɂȂ��Ă��܂��B  
	(VSCode�������͔z��ŊǗ����悤�Ƃ��Ă����炵����)
- ���O�ɑ��삵���ӏ���Piece��ێ�����L���b�V�����A�Ǝ��Ɏ������Ă���B  
�e�L�X�g��}������ہA�}������T�����߂�Piece�̔z������[�v�����Ŋm�F����K�v�����邪�A  
�e�L�X�g�̒��ԂɃf�[�^��p�ɂɒǉ�������A�����Ԃ̑���𑱂����Piece�̐��������A  
�}�����̌����Ɏ��Ԃ�v����\��������B  
�e�L�X�g�f�[�^�̓����Ƃ��āA�����ӏ����W�����đ��삷��_���������邽�߁A  
����̓x�ɃL���b�V���Ƃ��Ē��O��Piece��ێ����A���̑O��𑀍삷��ۂ̓L���b�V���𗘗p���邱�ƂŁA  
���쎞�̃p�t�H�[�}���X�����P���Ă���B  

### ����  
PieceTable.cs�̊e�N���X�Ƃ��̃��\�b�h�̖������ȈՓI�ɋL�ځB  

- PieceTable  
PieceTable�̊e�f�[�^�Ƒ���p�̃��\�b�h��ێ��B  
Piece�̕ێ��ɂ͑o�������X�g(C#�ł�Generic��LinkedList)���g�p�B  
�ǉ��ƍ폜�������ŁA�Q�Ƃ��ᑬ�ȃf�[�^�\���B
	- Insert  
	�e�L�X�g��}������B  
	�� �ʏ�̓��� + �y�[�X�g�Ȃǂ�z��B  
	�A�����đ��삵�Ă���ꍇ(�����āu�����������v�Ȃǂƃ^�C�v������)�́A  
	Piece��1�Ŏ擾���镶�����𑝂₷�悤�ɂ���B  
	(�P����Piece��5����Piece�����������Ă��܂��p�t�H�[�}���X�������邽��)
		- SearchAndInsertText  
		�e�L�X�g�}���Ώۂ�Piece��T���A�e�L�X�g��}������B
	- Delete  
	�e�L�X�g���폜����B  
	�� �ʏ�̍폜�Ɣ͈͎w�肵�č폜��z��B  
	1�����폜�����������폜���ŏ�����؂�ւ���B  
	�擾��������0�ɂȂ���Piece���ł����ۂ�Piece���̂��폜����B
		- DeleteSingleLetter  
		�e�L�X�g��1�����폜����B  
		�� �ʏ�̍폜(BS��Delete)��z��B
		- DeleteMultipleLetters  
		�e�L�X�g�𕡐������폜����B  
		�� �͈͍폜(�͈͑I������BS��Delete)��z��B  
		���������폜�̏ꍇ�́APiece���܂����\��������(100�����폜�ŁA10�����w���Piece�������Ȃ�)�B  
		�폜������Piece�ɑ΂����s���邽�߁A  
		�폜���镶���������炵�āAPiece���ύX���Ȃ���ōċA�ŕ�������s����B
	- GetPieceAndPosition  
	(Insert�܂���Delete���Ɏ��s)  
	����ʒu�ƃe�L�X�g�����r���āA�ړI��Piece���ŒZ�Ō����ł��郁�\�b�h���ĂԁB  
	(�e�L�X�g�S�̂�5000�����ŁA����ʒu��1000�����ڂȂ�SearchFromForward()�����s)
	- SearchFromForward  
	Piece�̔z���擪���猟�����A�����Ɉ�v����Piece��T���B
	- SearchFromBackward  
	Piece�̔z��𖖔����猟�����A�����Ɉ�v����Piece��T���B
	- SearchFromCache  
	�L���b�V������Piece����擪�A�܂��͖����Ɍ�������Piece�̔z����������A  
	�����Ɉ�v����Piece��T���B
	- SplitPiece  
	(�e�L�X�g�f�[�^�̒��Ԃ�ҏW����ۂɎ��s)  
	Piece�𕪊�����B
	- GetAllText  
	Piece�ɑ΂����[�v�������s���A�e�L�X�g���擾����B
- Piece  
�I���W�i��/�ǉ��e�L�X�g�̂ǂ��炩��e�L�X�g����邩�ƁA  
Span(�擾����e�L�X�g��)��ێ�����B
- Span  
�ǂݎ�镶�������w�肷��B
- EditingCache  
���O�ɑ��삵��Piece�ƁA���̃e�L�X�g�ʒu��ێ�����B  
�e�L�X�g�f�[�^�͓����ӏ����W���I�ɑ��삷�邽�߁AInsert��Delete�ł͂܂����̃L���b�V�����m�F���đ��삷��B

## ���P���K�v�ȓ_
- Undo/Redo������������  
PieceTable�̓e�L�X�g��Immutable�ɕێ����邽�߁A���̓_�ł͑���̂�蒼���͎������₷���B  
��������ɂ́A�e����(�e�L�X�g�}����폜�Ȃ�)�Ƒ���ӏ����A  
�����Ƃ��ĕێ�����Undo/Redo�ɉ��������삪�K�v�B  
- �ŏI�I�ȃe�L�X�g���擾����ۂɁA�S�̂���邵�����@�������B  
�ɒ[�Ɍ����΁A1�������삵����Ƀe�L�X�g���擾����ꍇ�ł��A�e�L�X�g�S�̂��擾����K�v������B  
���̂��߃e�L�X�g�𑀍삷�鏈��(�}�[�N�_�E���ɕϊ���X�^�C����������ȂǁH)���A�v���P�[�V�����Ƃ��Ď�������ہA  
�p�t�H�[�}���X�ʂŕs���ȂƂ���ł͂���B

## �Q�l
- [Piece Table - Wikipedia](https://en.wikipedia.org/wiki/Piece_table)
- [Text Buffer Reimplementation - VSCode Blog](https://code.visualstudio.com/blogs/2018/03/23/text-buffer-reimplementation)
- [piece-table - Darren Burns](https://darrenburns.net/posts/piece-table/)
- [Text Editor Data Structures - invoke::thought()](https://cdacamar.github.io/data%20structures/algorithms/benchmarking/text%20editors/c++/editor-data-structures/)
- [Piece Chains - Catch22](https://www.catch22.net/tuts/neatpad/piece-chains/)
- [�e�L�X�g�G�f�B�^�Ŏg��ꂪ���ȃf�[�^�\�� Piece Table �̊T�v�Ǝ��� - A Memorandum](https://blog1.mammb.com/entry/2022/09/07/224202)
- [�yC++�z�e�L�X�g�G�f�B�^�̃o�b�t�@ �f�[�^�\���E�A���S���Y���y��P��z - TECH PROjin](https://tech.pjin.jp/blog/2020/11/16/buffer-1)