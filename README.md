# VRChat Screenshot Date/Time Fixer

## 概要
VRChatのスクリーンショットファイルのファイル名から日時情報を抽出し、以下の情報を更新するツールです。
- ファイルの作成日時と更新日時
- ファイルのExif情報（撮影日時）

## 機能
- VRChatのスクリーンショットファイル名（例：`VRChat_2023-01-01_12-30-45.123_1920x1080.png`）から日時情報を抽出
- ファイルの作成日時と更新日時を抽出した日時に更新
- 画像ファイルのExif情報（撮影日時）を更新
- 単一ファイルまたはディレクトリ単位での一括処理
- サブディレクトリの再帰的な処理に対応

## 必要条件
- .NET 8.0 ランタイム または .NET 8.0 SDK
- Windows 10/11 (Windows 限定)

## インストール方法

### システム要件

- **オペレーティングシステム**: Windows 10/11 (64ビット)
- **.NET ランタイム**: .NET 8.0 (自己完結型パッケージに含まれています)
- **ディスク容量**: 50MB以上の空き容量

### 方法1: 事前ビルド済みパッケージを使用する（推奨）

1. **最新リリースをダウンロード**
   - [リリースページ](https://github.com/momoandbanana22/VRCSSDateTimeFixer/releases)にアクセス
   - 最新の `VRCSSDateTimeFixer-{バージョン}-win-x64.zip` ファイルをダウンロード

2. **ZIPファイルを解凍**
   - ダウンロードしたZIPファイルを右クリックし、「すべて展開...」を選択
   - 展開先フォルダを選択（例：`C:\Program Files\VRCSSDateTimeFixer`）

3. **アプリケーションを実行**
   - コマンドプロンプトを開き、解凍したフォルダに移動して以下のコマンドを実行:
     ```
     .\VRChatScreenshotDateTimeFixer.exe "処理するファイルまたはフォルダのパス" [オプション]
     ```
   - 例: `VRChatScreenshotDateTimeFixer.exe "C:\Path\To\Screenshots" -r`

### 方法2: ソースからビルドする

#### 前提条件
- [.NET 8.0 SDK](https://dotnet.microsoft.com/ja-jp/download/dotnet/8.0)
- [Git](https://git-scm.com/download/win)

1. **リポジトリをクローン**
   ```bash
   git clone https://github.com/momoandbanana22/VRCSSDateTimeFixer.git
   cd VRCSSDateTimeFixer
   ```

2. **依存関係を復元**
   ```bash
   dotnet restore
   ```

3. **リリースビルドを実行**
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
   ```

4. **ビルドされた実行ファイル**
   - ビルドされた実行ファイルは `VRCSSDateTimeFixer\bin\Release\net8.0\win-x64\publish\` に生成されます
   - `-p:PublishTrimmed=true` オプションにより、使用されていないアセンブリが削除され、出力サイズが最適化されます

## アンインストール方法

### 事前ビルド済みパッケージを使用した場合
1. ダウンロードしたZIPファイルを削除
2. 展開先フォルダ（例：`C:\Program Files\VRCSSDateTimeFixer`）を削除

### ソースからビルドした場合
1. リポジトリのクローンを削除
2. ビルドで生成された `bin` と `obj` フォルダを削除
3. 必要に応じて、.NET 8.0 SDK をアンインストール

## 使い方

### 基本的な使い方
```
VRCSSDateTimeFixer <ファイルまたはディレクトリのパス> [オプション]
```

### オプション
- `-r, --recursive` : サブディレクトリを再帰的に処理します
- `--version`       : バージョン情報を表示
- `-?, -h, --help`  : ヘルプを表示

### 使用例

1. 単一のファイルを処理する場合:
   ```
   VRCSSDateTimeFixer C:\Path\To\VRChat_2023-01-01_12-30-45.123_1920x1080.png
   ```

2. ディレクトリ内の全ファイルを処理する場合:
   ```
   VRCSSDateTimeFixer C:\Path\To\Screenshots
   ```

3. サブディレクトリも含めて再帰的に処理する場合:
   ```
   VRCSSDateTimeFixer C:\Path\To\Screenshots -r
   ```

## サポートしているファイル形式
- 画像ファイル: .png, .jpg, .jpeg

## スキップされる条件（更新が行われない場合）

処理対象のそれぞれ（作成日時・最終更新日時・撮影日時[Exif]）がスキップされる主な条件を明記します。

- __共通（作成日時・最終更新日時・撮影日時に共通）__
  - __ファイルが存在しない__: パス不正や削除済み
  - __ファイル名から日時が抽出できない__: `VRChat_YYYY-MM-DD_hh-mm-ss.mmm_*` 形式に一致しない等（`FileNameValidator.GetDateTimeFromFileName()` で抽出不可）
  - __アクセス不能__: ファイルがロック中、アクセス権限不足、読み取り専用属性の復元時などで例外が発生した場合
  - __予期される例外__: OS 由来の I/O 例外等、内部でハンドリング対象とした例外が発生した場合

- __作成日時（Creation Time）/ 最終更新日時（Last Write Time）__
  - 上記の共通条件に該当すると、__個別に__ スキップされる可能性があります
    - 例: 作成日時の更新のみ失敗しても、最終更新日時は成功しうる（処理は個別にリトライ・設定を試みます）

- __撮影日時（Exif: DateTimeOriginal）__
  - 上記の共通条件に加えて、画像固有の理由でスキップされることがあります
    - __画像の読み込み／保存に失敗__: 壊れたファイル、未対応フォーマット
    - __画像の置換（保存）処理に失敗__: 一時ファイルから元ファイルへの置換時に I/O エラーが発生
    - 実装上は、一時ファイルを元ファイルと同一ディレクトリに作成し、同一ボリューム内で置換します（`File.Replace` の制約回避）。それでも置換が失敗した場合はスキップとなります。

処理結果の表示は、各項目の成功/失敗に応じて以下のように出力されます。
- 成功: 「更新済」
- 失敗（スキップ）: 「スキップ」

## ビルド手順
1. リポジトリをクローン
   ```bash
   git clone https://github.com/momoandbanana22/VRCSSDateTimeFixer.git
   cd VRCSSDateTimeFixer
   ```

2. 依存関係を復元
   ```bash
   dotnet restore
   ```

3. リリースビルドを実行
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
   ```

4. ビルドされた実行ファイルは `VRCSSDateTimeFixer\bin\Release\net8.0\win-x64\publish\` に生成されます

## ライセンス
このプロジェクトは [MIT ライセンス](LICENSE) の下で公開されています。

## 貢献について
バグレポートや機能要望、プルリクエストは歓迎します。

## 作者
[momoandbanana22](https://github.com/momoandbanana22)
