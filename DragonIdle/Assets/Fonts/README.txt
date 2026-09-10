日本語フォントについて / About the Japanese font
================================================

このゲームの UI は、実行時に OS へインストール済みの日本語フォントを探して使う
（游ゴシック、メイリオ、ヒラギノ、Noto Sans CJK など）。探索と選択は
Assets/Scripts/UI/UIStyle.cs の MainFont が行っている。

そのため:

  エディタ / Windows / macOS / Linux のビルド
      OS のフォントをそのまま使えるので、追加の作業は要らない。

  iOS / Android / WebGL / コンソール
      OS フォントはビルドに埋め込まれない。日本語がすべて豆腐（□）になる。
      画面上部に英字で警告バーが出るので、それが出たらこの手順を行う。

埋め込む手順
------------

1. 再配布できる日本語フォントを用意する。Noto Sans JP や源ノ角ゴシックは
   SIL Open Font License で、ゲームへの同梱が認められている。
   Noto Sans JP: https://fonts.google.com/noto/specimen/Noto+Sans+JP

2. `.ttf` または `.otf` をこのフォルダ（Assets/Fonts/）に置く。

3. Unity のインスペクタでそのフォントを選び、
   Character を「Dynamic」にしておく（既定でそうなっている）。

4. Assets/Scripts/UI/UIStyle.cs の MainFont を、置いたフォントを返すように変える。
   例:

       public static UnityEngine.Font MainFont
       {
           get
           {
               if (_mainFont == null)
               {
                   _mainFont = Resources.Load<UnityEngine.Font>("NotoSansJP-Regular");
                   JapaneseFontFound = _mainFont != null;
               }
               return _mainFont;
           }
       }

   Resources.Load を使う場合は、フォントを Assets/Resources/ 以下に置くこと。
   インスペクタで参照を差す作りにしてもよい。

TextMeshPro を使う場合
----------------------

同じフォントから Font Asset を生成し（Window → TextMeshPro → Font Asset Creator）、
Character Set に必要な文字を含める。日本語は文字数が多いので、
「Dynamic」の Atlas Population Mode にするか、常用漢字＋かな＋英数の範囲を指定する。
このゲームは uGUI の Text で組んであるので、TextMeshPro へ移すなら
UIFactory.Label の作りも合わせて変える必要がある。
