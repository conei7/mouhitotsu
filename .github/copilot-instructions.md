## プロジェクト概要（2026-04-21時点）

### 何のゲームか

- 2Dのステージクリア型アクション/パズルゲーム。
- `TitleScene` から通常プレイ、ステージ選択、マップエディタへ遷移する。
- `GameScene` で動的にステージを生成し、`EditorScene` でマップを編集する。

### 構成の要点

- `StageManager` が通常プレイの進行と遷移を管理する。
- `StageDatabase` が組み込みステージとユーザーステージを扱う。
- `RuntimeMapLoader` が `StageData` からステージを生成する。
- `PlacementSystem` と `EditorCamera` がマップエディタの編集・カメラ操作を担う。

### 特別に気を付けるところ

- `TitleScene` のボタン配線は厳密に確認する。
  - `StartButton` → `TitleButton.OnStartClick`
  - `StageSelectButton` → `TitleButton.OnStageSelectClick`
  - マップエディタ用ボタン → `TitleButton.OnMapEditorClick`
- エディタのテストプレイ時と通常プレイでクリア後の遷移先が違う。
  - 通常プレイ: `ClearScene`
  - テストプレイ: `EditorScene`
- スマホ操作は 1本指と2本指を完全に分ける。
  - 1本指: 配置/削除
  - 2本指: パン/ズーム
  - 2本指中や解除直後に誤配置が起きないよう注意
- `MapTextCodec` で保存形式を圧縮している。
  - 新形式と旧形式の両方を読める前提で扱う。
- WebGL は保存/読み込みがファイル中心。
  - クリップボード前提に戻さない。

### 確認ポイント

1. タイトル画面の各ボタン遷移
2. エディタの 1本指配置/削除
3. エディタの 2本指パン/ズーム
4. 保存→読込の往復
5. 通常プレイとテストプレイでクリア後の遷移が分かれていること

最後に実装が完了したらツールのvscode_askQuestionsを使用してほかに確認すべきポイントがないか質問してください。