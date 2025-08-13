# VRCSSDateTimeFixer Project Plan

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
- ファイル名判定・抽出時は "VRChat"・"x"・"png" などの大文字小文字を区別しないことをテストリストに追加
- 解像度はユーザー設定に依存し、1～99999の整数値が縦横どちらにも入るため、縦横の組み合わせチェックは不要（両方が範囲内かのみ検証）

## Notes
- ユーザーは日本人なので、日本語で会話すること。
- プランモードで編集するファイルは、このmyplan.mdファイルです。 AIがこのファイルを編集します。
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
- User provided an initial TDD test list for review:
  - 判定: ファイル名のフォーマットを判定できること
  - 取得1: ファイル名から日時取得１（フォーマット１）できること
  - 取得2: ファイル名から日時取得２（フォーマット２）できること
  - 作成日時: 指定されたファイルの作成日時を指定された日時に変更できること
  - 更新日時: 指定されたファイルの更新日時を指定された日時に変更できること
  - 撮影日時: 指定されたファイルの撮影日時を指定された日時に変更できること
  - 例外: ファイル名のフォーマットが期待通りでない場合は、何もしないこと
- Plan to include edge case tests:
  - ファイル名に特殊文字が含まれる場合
  - ファイル名が非常に長い場合
  - ファイルが存在しない場合の処理
  - 読み取り専用ファイルを処理する場合
  - 書き込み権限がない場合のエラーハンドリング
  - 日時の境界値（うるう秒・うるう年・夏時間切替）
- TDDテストケースの優先順位:
  - 最重要（MVP）: ファイル名フォーマット検証（大文字小文字/解像度/日時）、日時抽出・設定（作成/更新/撮影）、異なる日時形式
  - 高優先度: エラーハンドリング（不正名/存在しない/読取専用ファイル）
  - 中優先度: パフォーマンス（大量ファイル/メモリ）
  - 低優先度: 特殊環境（ネットワークドライブ/異ファイルシステム）
  - 検討事項: バックアップ、並列処理、進捗表示
- テストコードはGiven-When-Then（GWT）形式で記述すること
- テストコメントは日本語で記述すること
- TDDサイクル（Red→Green→Refactor）を厳守すること
- コミットメッセージのprefix（test:, feat:, refactor:など）は内容に応じて一貫性を持って選ぶこと。特にテスト追加時はtest:を推奨。
- 過去のコミットメッセージも内容に応じて適切なprefixで一貫性を持って修正すること。ただし、修正時は1行目のprefixのみ変更し、2行目以降の本文や詳細説明は絶対に変更しないこと。
- 撮影日時（Exif/PNGメタデータ）もTDDサイクルで実装予定（ユーザー要望により追加）

## Task List
- [x] Decide on project structure and naming
- [x] Decide on .NET version and test framework
- [x] Create solution and projects in Visual Studio
- [x] Review created project contents
- [x] Add and configure xUnit test project
- [x] Generate .gitignore file
- [x] Make initial git commit
- [x] Review and refine initial TDD test list
- [x] Add and refine edge case tests to TDD plan
- [x] Add and refine case-insensitive matching tests to TDD plan
- [x] Begin first TDD test implementation
- [x] テストコードをGWT形式・日本語コメントにリファクタ
- [x] 日時抽出機能の実装が完了したことを確認
