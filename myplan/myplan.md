# VRCSSDateTimeFixer Project Plan

- ユーザーは日本人なので、日本語で会話すること。
- 既存のコードを確認して進めて行くこと。 実装済み・未実装・テスト済み、テスト未を確認してから、進めて行くこと。

## 仕様
### VRChatのスクリーンショットのファイル名にある撮影日時を、そのファイルの「作成日時」「更新日時」「撮影日時（Exif/PNGメタデータ）」に反映する。
- コマンドラインで動作する。
  - パラメータでファイル名を指定されたら、そのファイルに対して処理を行う。
  - パラメータでディレクトリを指定されたら、そのディレクトリ内にあるファイルに対して処理を行う。サブディレクトリも再帰的に処理する。
  - 結果はコンソールに出力する。
    - ファイル名フォーマットが合致しないファイルは、スキップしたことを出力する。
    - １つのファイル毎に１行で、以下のように、進捗がわかるように表示する。
      - 最初に、ファイル名を表示する（改行しない）
      - 次に、ファイル名から取得した撮影日時を「：yyyy年mm月dd日 hh時mm分ss秒.xxx」形式で表示する（改行しない）
      - 次に、ファイル作成日時を更新する前に、「 作成日時：」を表示する（改行しない）
      - 次に、ファイル作成日時を更新した後に、「更新済」を表示する（改行しない）
      - 次に、ファイル更新日時を更新する前に、「 更新日時：」を表示する（改行しない）
      - 次に、ファイル更新日時を更新した後に、「更新済」を表示する（改行しない）
      - 次に、ファイル撮影日時を更新する前に、「 撮影日時：」を表示する（改行しない）
      - 次に、ファイル撮影日時を更新した後に、「更新済」を表示する（改行する）
    - エラーもコンソールに表示する。

- VRChatスクリーンショットのファイル名フォーマット例:
  - フォーマット1: VRChat_1080x1920_2022-08-31_21-54-39.227.png（1080x1920, 1920x1080 など縦横どちらもあり得る）
  - フォーマット2: VRChat_2022-10-14_23-44-13.389_7680x4320.png
- ファイル名判定・抽出時は "VRChat"・"x"・"png" などの大文字小文字を区別しないこと
- 例外: ファイル名のフォーマットが期待通りでない場合は、何もしないこと
- 解像度はユーザー設定に依存し、1～99999の整数値が縦横どちらにも入るため、縦横の組み合わせチェックは不要（両方が範囲内かのみ検証）

## Notes
- Visual Studio Community 2022 is the latest version as of Aug 2025 and supports .NET 8.0.
- For TDD, create one solution with a production project and a test project.
- Use "コンソール アプリ" (.NET, not .NET Framework) for cross-platform, modern development.
- Recommended: .NET 8.0 for both production and test projects; xUnit or NUnit for tests (xUnit preferred for new projects).
- Project name chosen: VRChatDateTimeFixer.
- "最上位レベルのステートメントを使用しない" should be ON for TDD (i.e., do not use top-level statements).
- "native AOT 発行を有効にする" can be enabled at distribution time, not during development.
- User has created the project and requested a review of its contents.
- Main project uses .NET 8.0, explicit Program class, and initial code is standard template ("Hello, World!").
- Next step: Add and configure xUnit test project (VRChatDateTimeFixer.Tests) targeting .NET 8.0 and referencing the main project.
- xUnit test project (VRChatDateTimeFixer.Tests) has been added, builds, and tests pass (green).
- テストコードはGiven-When-Then（GWT）形式で記述すること
- テストコメントは日本語で記述すること
- TDDサイクル（Red→Green→Refactor）を厳守すること
