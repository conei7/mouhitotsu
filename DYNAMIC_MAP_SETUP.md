# 動的マップ生成システム セットアップ

## 概要

ステージごとにシーンを作成する代わりに、1つの `GameScene` で動的にマップを生成するシステム。

## 新規スクリプト

| ファイル | 役割 |
|----------|------|
| `StageData.cs` | ステージデータ構造（JSON保存用） |
| `StageDatabase.cs` | 組み込み＆ユーザーステージ管理 |
| `RuntimeMapLoader.cs` | 動的マップ生成 |

## セットアップ手順

### 1. GameScene 作成

1. **File > New Scene** → Empty Scene
2. **File > Save As** → `Assets/Scenes/GameScene.unity`

### 2. 必要なオブジェクトを配置

#### StageDatabase (シングルトン - TitleSceneに配置推奨)

1. TitleScene を開く
2. 空オブジェクト作成 → 名前: `StageDatabase`
3. **Add Component > StageDatabase**
4. シーン保存

#### RuntimeMapLoader (GameSceneに配置)

1. GameScene を開く
2. 空オブジェクト作成 → 名前: `RuntimeMapLoader`
3. **Add Component > RuntimeMapLoader**
4. Inspector で設定:
   - **Map Settings**: `Assets/Data/MapSettings.asset` をドラッグ
   - **Main Camera**: Main Camera をドラッグ
   - **Camera Padding**: `1`

#### GravityController (GameSceneに配置)

1. 空オブジェクト作成 → 名前: `GravityController`
2. **Add Component > GravityController**

#### Canvas (GameSceneに配置)

1. **右クリック > UI > Canvas**
2. **Canvas Scaler**:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`
3. GravityIndicatorUI などを配置

### 3. Main Camera 設定

1. Main Camera を選択
2. **Camera**:
   - Projection: `Orthographic`
   - Background: `#1a1a2e`

### 4. Build Settings 設定

**File > Build Settings** で以下の順序でシーンを追加:

```
0: TitleScene
1: GameScene        ← 新規追加
2: ClearScene
3: GameOverScene
4: EditorScene (任意)
```

### 5. 古いステージシーンの削除（任意）

Build Settings から `Stage1`, `Stage2` など個別のステージシーンを削除できる。
（ファイル自体は残しておいてバックアップとしても良い）

## 使い方

### タイトルからゲーム開始

TitleScene の Start ボタンから開始すると:
1. `StageManager.GoToStage(1)` が呼ばれる
2. `StageDatabase` から `stage_001` を取得
3. `GameScene` に遷移
4. `RuntimeMapLoader` がマップを動的生成

### ステージクリア時

1. ゴールに到達 → `ClearScene` に遷移
2. "Next Stage" を押す → `StageManager.GoToNextStage()`
3. `StageDatabase` から次のステージを取得
4. `GameScene` に遷移 → `RuntimeMapLoader` が次のマップを生成

## 組み込みステージの追加

`Assets/Resources/BuiltInStages.json` を編集:

```json
{
    "stages": [
        {
            "id": "stage_001",
            "name": "Stage 1",
            "mapText": "#####\n#S G#\n#####",
            "isBuiltIn": true,
            "stageNumber": 1,
            "cameraSize": 0,
            "cameraOffsetX": 0,
            "cameraOffsetY": 0,
            "createdAt": 0
        },
        // ... 追加ステージ
    ]
}
```

## マップ記号

| 記号 | 意味 |
|------|------|
| `#` | 壁/床 |
| `S` | スタート位置 |
| `G` | ゴール |
| `^` `v` `<` `>` | 重力スイッチ（上下左右） |
| `o` | 無重力スイッチ |
| `X` | トゲ |
| `B` | 押せる箱 |
| `0`～`9` | 壁ボタン（チャンネル0～9） |
| `a`～`j` | 切り替え壁ON（チャンネル0～9） |
| `k`～`t` | 切り替え壁OFF（チャンネル0～9） |
