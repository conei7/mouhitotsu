# もうひとつ (Mouhitotsu) - セットアップガイド

"もう一つの重力"を操る2Dアクションパズルゲーム。

---

## 目次

1. [前提条件](#前提条件)
2. [レイヤー設定](#レイヤー設定)
3. [スクリプト一覧](#スクリプト一覧)
4. [シーン作成](#シーン作成)
5. [GameScene セットアップ](#gamescene-セットアップ)
6. [Build Settings](#build-settings)
7. [操作方法](#操作方法)
8. [切り替え壁とボタン セットアップ](#切り替え壁とボタン-セットアップ)

---

## 前提条件

- Unity 2022.3 以降
- DOTween（Asset Store からインポート）
- TextMeshPro（Package Manager からインストール済み）

---

## レイヤー設定

**Edit > Project Settings > Tags and Layers**

| User Layer | 名前 |
|------------|------|
| 8 | `Ground` |
| 9 | `Player` |

---

## スクリプト一覧

全て `Assets/Scripts/` 以下に配置済み

| パス | 役割 |
|------|------|
| `Player/CharacterBase.cs` | プレイヤー移動・ジャンプ・ゴール判定 |
| `Core/GameManager.cs` | ゲーム状態管理・シーン遷移 |
| `Core/GravityController.cs` | 重力システム管理（通常+追加重力、重ねがけ対応） |
| `Core/StageManager.cs` | ステージ進行管理（進捗保存） |
| `Camera/GravityCamera.cs` | 重力に合わせてカメラ回転・プレイヤー追従 |
| `Audio/AudioManager.cs` | BGM・SE管理、音量設定保存 |
| `Obstacles/Goal.cs` | ゴールエリア判定 |
| `Obstacles/Spike.cs` | トゲ（触れると死亡） |
| `Obstacles/FallZone.cs` | 落下死判定エリア |
| `Obstacles/GravitySwitch.cs` | 重力切替スイッチ（オンオフトグル対応） |
| `Obstacles/GravityOrb.cs` | 一時的重力オーブ |
| `UI/GravityIndicatorUI.cs` | 重力インジケーターUI |
| `UI/ClearScreenUI.cs` | クリア画面UI（次のステージ/リトライ/タイトル） |
| `UI/VolumeSettingsUI.cs` | 音量設定UI |
| `UI/TitleButton.cs` | タイトルのStartボタン |
| `UI/RetryButton.cs` | リトライボタン |
| `UI/ToTitleButton.cs` | タイトルへ戻るボタン |

---

## プレハブ作成

マップ生成システムを使用するには、以下のプレハブを作成する必要があります。

### フォルダ構成

```
Assets/
└── Prefabs/
    ├── Player.prefab
    ├── Wall.prefab
    ├── Goal.prefab
    └── Switches/
        ├── SwitchUp.prefab
        ├── SwitchDown.prefab
        ├── SwitchLeft.prefab
        └── SwitchRight.prefab
```

### Player プレハブ

1. **Hierarchy > 右クリック > 2D Object > Sprites > Square**
2. 名前を `Player` に変更
3. Transform:
   - Scale: `(0.8, 0.8, 1)` ← ブロックより少し小さく
4. **Add Component > Rigidbody 2D**
   - Gravity Scale: `1`
   - Freeze Rotation: `Z` にチェック
5. **Add Component > Box Collider 2D**
6. **Add Component > Character Base**
   - Jump Height Blocks: `3`
   - Ground Layer: `Ground`
7. Layer を `Player` に設定
8. **Project ウィンドウ（Assets/Prefabs/）にドラッグ** → プレハブ化
9. Hierarchy のオブジェクトを削除

### Wall プレハブ

1. **Hierarchy > 右クリック > 2D Object > Sprites > Square**
2. 名前を `Wall` に変更
3. Transform:
   - Scale: `(1, 1, 1)`
4. **Add Component > Box Collider 2D**
5. Layer を `Ground` に設定
6. SpriteRenderer の Color を好みの色に（例: グレー `#666666`）
7. **Project にドラッグ** → プレハブ化
8. Hierarchy のオブジェクトを削除

### Goal プレハブ

1. **Hierarchy > 右クリック > 2D Object > Sprites > Square**
2. 名前を `Goal` に変更
3. Transform:
   - Scale: `(1, 1, 1)`
4. **Add Component > Box Collider 2D**
   - `Is Trigger` にチェック ← 重要！
5. **Add Component > Goal**
6. SpriteRenderer の Color を黄色系に（例: `#FFFF00`）
7. **Project にドラッグ** → プレハブ化
8. Hierarchy のオブジェクトを削除

### 重力スイッチ プレハブ（4つ）

各方向ごとに作成します。以下は「上」の例：

1. **Hierarchy > 右クリック > 2D Object > Sprites > Square**
2. 名前を `SwitchUp` に変更
3. Transform:
   - Scale: `(1, 1, 1)`
4. **Add Component > Box Collider 2D**
   - `Is Trigger` にチェック
5. **Add Component > Gravity Switch**
   - Gravity Direction: `Up`
   - Gravity Strength: `1`
6. SpriteRenderer の Color をシアン系に（例: `#00FFFF`）
7. **Project（Assets/Prefabs/Switches/）にドラッグ** → プレハブ化
8. Hierarchy のオブジェクトを削除

同様に以下も作成：
- `SwitchDown` (Direction: Down, Color: 青系)
- `SwitchLeft` (Direction: Left, Color: マゼンタ系)
- `SwitchRight` (Direction: Right, Color: 緑系)

### Spike プレハブ（オプション）

1. **Hierarchy > 右クリック > 2D Object > Sprites > Square**
2. 名前を `Spike` に変更
3. Transform:
   - Scale: `(1, 1, 1)`
4. **Add Component > Box Collider 2D** → `Is Trigger` にチェック
5. **Add Component > Spike**
6. SpriteRenderer の Color を赤に（`#FF0000`）
7. **Project にドラッグ** → プレハブ化
8. Hierarchy のオブジェクトを削除

### FallZone プレハブ（オプション）

落下死エリア用。ステージ下部に配置して使用。

1. **Hierarchy > 右クリック > 2D Object > Sprites > Square**
2. 名前を `FallZone` に変更
3. Transform:
   - Scale: `(20, 1, 1)` ← 横長にする（ステージ幅に合わせて調整）
4. **Add Component > Box Collider 2D** → `Is Trigger` にチェック
5. **Add Component > Fall Zone**
6. SpriteRenderer の Color を透明な赤に（`#FF000050`）
7. **Project にドラッグ** → プレハブ化
8. Hierarchy のオブジェクトを削除

---

## MapSettings の設定

### MapSettings アセット作成

1. **Project ウィンドウで右クリック > Create > Mouhitotsu > Map Settings**
2. 名前を `MapSettings` に変更
3. 場所は `Assets/Data/` 推奨

### プレハブの設定

1. 作成した `MapSettings` を選択
2. Inspector 右上の **🔒（ロック）** をクリック（これで選択が固定される）
3. 各欄に先ほど作成したプレハブをドラッグ：

| 欄 | 設定するプレハブ |
|---|---|
| Wall Prefab | Wall |
| Player Prefab | Player |
| Goal Prefab | Goal |
| Switch Up Prefab | SwitchUp |
| Switch Down Prefab | SwitchDown |
| Switch Left Prefab | SwitchLeft |
| Switch Right Prefab | SwitchRight |
| Spike Prefab | Spike（あれば） |

4. **🔒** をもう一度クリックして解除

---

## ステージ作成ワークフロー

### Step 1: マップテキストを作成

1. **VSCode** で `Assets/Data/Maps/Stage1.txt` を開く（等幅フォントで見やすい）
2. マップを記述：

```
####################
#                  #
#                 G#
#                ###
#                  #
#       ###        #
#       ###        #
# S   ^ ###    >   #
####################
```

3. 保存

### Step 2: MapData アセットを作成

1. Unity で **Project > 右クリック > Create > Mouhitotsu > Map Data**
2. 名前を `Stage1` に変更
3. 場所は `Assets/Data/Maps/` 推奨

### Step 3: マップテキストを貼り付け

1. 作成した `Stage1` (MapData) を選択
2. Inspector の **Map Text** 欄に、VSCode で作成したテキストを貼り付け
3. Stage Name: `Stage 1`
4. Stage Number: `1`

### Step 4: ステージシーンを作成

1. **File > New Scene** → Empty Scene
2. **File > Save As** → `Assets/Scenes/Stage1.unity`

### Step 5: MapGenerator を配置

1. Hierarchy で **右クリック > Create Empty**
2. 名前を `MapGenerator` に変更
3. **Add Component > Map Generator**
4. Inspector で設定：
   - **Map Data**: `Stage1` (作成したMapData)
   - **Map Settings**: `MapSettings`
   - **Main Camera**: Main Camera

### Step 6: マップ生成

1. MapGenerator の Inspector で **「🔨 マップ生成」** ボタンをクリック
2. マップが生成される！
3. シーンを保存 (`Ctrl + S`)

### Step 7: Build Settings に追加

1. **File > Build Settings**
2. `Stage1` シーンをドラッグして追加

### Step 8: テストプレイ

1. **Play** ボタンを押してテスト
2. 問題があれば MapData のテキストを修正して再生成

---

## シーン作成


### TitleScene

1. **File > New Scene** → Empty Scene
2. **File > Save As** → `Assets/Scenes/TitleScene.unity`
3. **Canvas 作成**:
   - Hierarchy 右クリック > UI > Canvas
   - Canvas Scaler > UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`
4. **タイトルテキスト**:
   - Canvas 右クリック > UI > Text - TextMeshPro
   - 名前: `TitleText`
   - Pos: `(0, 100, 0)`, Size: `600 x 120`
   - Text: `もうひとつ`, Font Size: `72`, Alignment: Center
5. **スタートボタン**:
   - Canvas 右クリック > UI > Button - TextMeshPro
   - 名前: `StartButton`
   - Pos: `(0, -50, 0)`, Size: `200 x 60`
   - Text: `Start`, Font Size: `36`
   - Add Component > `TitleButton`
   - Button の OnClick > `+` > StartButton をドラッグ > `TitleButton.OnStartClick`

### ClearScene

1. **File > New Scene** → Save As `Assets/Scenes/ClearScene.unity`
2. **Canvas 作成**（同様設定）
3. **空オブジェクト作成**: 名前 `ClearScreenManager`
   - Add Component > `ClearScreenUI`
4. **ステージテキスト**: Pos `(0, 150, 0)`, Text: `Stage 1`, Font Size: `36`
   - ClearScreenUI の `Stage Text` にドラッグ
5. **クリアテキスト**: Pos `(0, 50, 0)`, Text: `CLEAR!`, Font Size: `80`, Color: `#4ade80`
   - ClearScreenUI の `Message Text` にドラッグ
6. **Next Stageボタン**: Pos `(0, -50, 0)`, Size: `200 x 60`, Text: `Next Stage`
   - Button の OnClick > `+` > ClearScreenManager をドラッグ > `ClearScreenUI.OnNextStageClick`
   - ClearScreenUI の `Next Stage Button` にドラッグ（自動表示/非表示用）
7. **Retryボタン**: Pos `(-120, -130, 0)`, Size: `180 x 60`, Text: `Retry`
   - Button の OnClick > `ClearScreenUI.OnRetryClick`
8. **Titleボタン**: Pos `(120, -130, 0)`, Size: `180 x 60`, Text: `Title`
   - Button の OnClick > `ClearScreenUI.OnTitleClick`

### GameOverScene

1. **File > New Scene** → Save As `Assets/Scenes/GameOverScene.unity`
2. **Canvas 作成**
3. **テキスト**: Pos `(0, 80, 0)`, Text: `Game Over`, Font Size: `72`, Color: `#ef4444`
4. **Retryボタン**: Pos `(-120, -50, 0)`, Size: `180 x 60`, Text: `Retry`
   - Add Component > `RetryButton`, OnClick > `RetryButton.OnRetryClick`
5. **Titleボタン**: Pos `(120, -50, 0)`, Size: `180 x 60`, Text: `Title`
   - Add Component > `ToTitleButton`, OnClick > `ToTitleButton.OnTitleClick`

---

## GameScene セットアップ

### シーン作成

1. **File > New Scene** → Save As `Assets/Scenes/GameScene.unity`

### Main Camera 設定

1. Main Camera を選択（なければ右クリック > Camera）
2. **Transform**: Position `(0, 0, -10)`
3. **Camera**:
   - Projection: `Orthographic`
   - Size: `5`
   - Background: `#1a1a2e`（ダークネイビー）
4. **Add Component > PlayerCamera**

### GameManager 作成

1. 右クリック > Create Empty
2. 名前: `GameManager`
3. Position: `(0, 0, 0)`
4. **Add Component > GameManager**

### GravityController 作成

1. 右クリック > Create Empty
2. 名前: `GravityController`
3. Position: `(0, 0, 0)`
4. **Add Component > GravityController**
5. Inspector設定:
   - Primary Gravity: `(0, -9.81)`
   - Secondary Gravity Multiplier: `0.5`（調整可能）

### Player 作成

1. 右クリック > 2D Object > Sprites > Square
2. 名前: `Player`
3. **Layer**: `Player`
4. **Transform**:
   - Position: `(0, 1, 0)`
   - Scale: `(0.8, 0.8, 1)`
5. **Sprite Renderer**: Color `#4a90d9`
6. **BoxCollider2D**: Size `(1, 1)`
7. **Add Component > Rigidbody2D**:
   - Collision Detection: `Continuous`
   - Constraints > Freeze Rotation: ☑️ Z
8. **Add Component > CharacterBase**
9. **子オブジェクト作成**:
   - Player 右クリック > Create Empty
   - 名前: `GroundCheck`
   - Position: `(0, -0.5, 0)`
10. **CharacterBase 設定**:
    - Ground Check: `GroundCheck` をドラッグ
    - Ground Layer: `Ground` を選択

### 床 (Ground)

1. 右クリック > 2D Object > Sprites > Square
2. 名前: `Ground`
3. **Layer**: `Ground`
4. **Transform**:
   - Position: `(0, -2, 0)`
   - Scale: `(30, 1, 1)`
5. **Sprite Renderer**: Color `#666666`
6. **BoxCollider2D**: デフォルト

### GravitySwitch (上重力)

1. 右クリック > 2D Object > Sprites > Square
2. 名前: `GravitySwitch_Up`
3. **Transform**:
   - Position: `(3, -1, 0)`
   - Scale: `(1, 0.3, 1)`
4. **Sprite Renderer**: Color `#00ffff`
5. **BoxCollider2D**: ☑️ Is Trigger
6. **Add Component > GravitySwitch**:
   - Gravity Direction: `Up`
   - Switch Color: Cyan

### GravitySwitch (リセット)

1. GravitySwitch_Up を **Ctrl+D**
2. 名前: `GravitySwitch_None`
3. **Transform**: Position `(6, -1, 0)`
4. **Sprite Renderer**: Color `#888888`
5. **GravitySwitch**:
   - Gravity Direction: `None`
   - Switch Color: Gray

### Goal

1. 右クリック > 2D Object > Sprites > Square
2. 名前: `Goal`
3. **Transform**:
   - Position: `(10, 3, 0)`（高い位置に配置）
   - Scale: `(2, 3, 1)`
4. **Sprite Renderer**:
   - Color: `#4ade80`, Alpha `100`
   - Order in Layer: `-1`
5. **BoxCollider2D**: ☑️ Is Trigger
6. **Add Component > Goal**

### FallZone

1. 右クリック > Create Empty
2. 名前: `FallZone`
3. **Transform**: Position `(0, -10, 0)`
4. **Add Component > BoxCollider2D**:
   - ☑️ Is Trigger
   - Size: `(100, 2)`
5. **Add Component > FallZone**

### 参照設定

1. **GameManager** を選択:
   - Player: `Player` をドラッグ
2. **Main Camera (PlayerCamera)** を選択:
   - Player: `Player` をドラッグ

---

## Build Settings

**File > Build Settings**

1. 各シーンを開いて Add Open Scenes:
   - `TitleScene` (Index 0)
   - `GameScene`
   - `ClearScene`
   - `GameOverScene`

---

## 操作方法

| キー | アクション |
|------|-----------|
| A / ← | 左移動 |
| D / → | 右移動 |
| W / ↑ / Space | ジャンプ |
| R | リトライ |

---

## ゲームルール

- **追加重力**: スイッチで方向を切り替え
- **ベクトル合成**: 通常重力＋追加重力（0.5倍）
- **クリア条件**: ゴールに到達
- **ゲームオーバー**: 落下死・トゲに触れる

---

## 重力設定のカスタマイズ

GravityController で以下を調整可能:

| パラメータ | 説明 | デフォルト |
|-----------|------|-----------|
| Primary Gravity | 通常重力 | (0, -9.81) |
| Secondary Gravity Strength | 追加重力の強度倍率 | 1.0 |
| Max Secondary Magnitude | 追加重力の最大強度（重ねがけ上限） | 3.0 |

StreamingAssets### 重ねがけシステム

スイッチを踏むたびに追加重力が**加算**されます。

```
例:
上スイッチ → 追加重力: (0, 1)
上スイッチ → 追加重力: (0, 2)  ← 上方向が強くなる
右スイッチ → 追加重力: (1, 2)  ← 右上方向に！
```

### GravitySwitch モード

| SwitchMode | 説明 |
|------------|------|
| Add | 重ねがけ（加算）- デフォルト |
| Set | 上書き（従来の動作） |
| Clear | 追加重力をリセット |

---

## GravityIndicatorUI セットアップ

重力の方向と強さを画面に表示するUIです。

### 1. Canvas作成（または既存を使用）

1. Hierarchy 右クリック > UI > Canvas
2. Canvas を選択
3. **Canvas Scaler** を設定:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`
   - Match: `0.5`

### 2. インジケーター背景作成

1. Canvas 右クリック > UI > Image
2. 名前: `GravityIndicator`
3. **Rect Transform** を設定:
   - Anchor Preset: 左上（Alt+Shiftを押しながらクリックで位置も設定）
   - Pos X: `80`, Pos Y: `-80`
   - Width: `120`, Height: `120`
4. **Image** コンポーネント:
   - Color: `#000000`（黒）, Alpha: `128`（半透明）

### 3. 矢印画像を作成

**GravityIndicator** の子として3つの矢印を作成します。

#### ArrowPrimary（通常重力 - グレー）

1. GravityIndicator 右クリック > UI > Image
2. 名前: `ArrowPrimary`
3. **Rect Transform**:
   - Pos X: `0`, Pos Y: `0`, Pos Z: `0`
   - Width: `10`, Height: `50`
   - **Pivot**: X: `0.5`, Y: `0` ← **重要！**
4. **Image**:
   - Color: `#808080`（グレー）

#### ArrowSecondary（追加重力 - シアン）

1. GravityIndicator 右クリック > UI > Image
2. 名前: `ArrowSecondary`
3. **Rect Transform**:
   - Pos X: `0`, Pos Y: `0`, Pos Z: `0`
   - Width: `12`, Height: `50`
   - **Pivot**: X: `0.5`, Y: `0` ← **重要！**
4. **Image**:
   - Color: `#00FFFF`（シアン）

#### ArrowCombined（合成重力 - オレンジ）

1. GravityIndicator 右クリック > UI > Image
2. 名前: `ArrowCombined`
3. **Rect Transform**:
   - Pos X: `0`, Pos Y: `0`, Pos Z: `0`
   - Width: `8`, Height: `60`
   - **Pivot**: X: `0.5`, Y: `0` ← **重要！**
4. **Image**:
   - Color: `#FF8000`（オレンジ）

### 4. 情報テキスト（オプション）

1. GravityIndicator 右クリック > UI > Text - TextMeshPro
2. 名前: `GravityInfoText`
3. **Rect Transform**:
   - Pos X: `140`, Pos Y: `0`
   - Width: `150`, Height: `120`
4. **TextMeshPro**:
   - Font Size: `18`
   - Alignment: Left, Middle
   - Color: 白

### 5. スクリプトをアタッチ

1. **GravityIndicator** オブジェクトを選択
2. Add Component > `GravityIndicatorUI`
3. Inspector で各項目を設定:

| フィールド | ドラッグするオブジェクト |
|-----------|------------------------|
| Arrow Primary | ArrowPrimary |
| Arrow Secondary | ArrowSecondary |
| Arrow Combined | ArrowCombined |
| Arrow Primary Image | ArrowPrimary の Image |
| Arrow Secondary Image | ArrowSecondary の Image |
| Arrow Combined Image | ArrowCombined の Image |
| Gravity Info Text | GravityInfoText（オプション） |

### Pivot の設定方法

Pivot は矢印の回転の中心点です。

1. 矢印オブジェクトを選択
2. Inspector の **Rect Transform** を見る
3. **Pivot** の値を変更:
   - X: `0.5`（横方向の中心）
   - Y: `0`（縦方向の下端）

これにより矢印が**下端を中心に回転**し、重力の方向を正しく示します。

### 最終的な階層構造

```
Canvas
└── GravityIndicator (Image + GravityIndicatorUI)
    ├── ArrowPrimary (Image)
    ├── ArrowSecondary (Image)
    ├── ArrowCombined (Image)
    └── GravityInfoText (TextMeshPro) [オプション]
```

---

## ステージシステム

### 概要

ゲームは複数のステージで構成されます。各ステージは `Stage1`, `Stage2`, ... という名前のシーンです。

### ステージシーンの作成

1. `GameScene.unity` を複製
2. 名前を `Stage1.unity`, `Stage2.unity`, ... に変更
3. 各ステージのレベルデザインを変更

### Build Settings

**File > Build Settings** で以下の順序でシーンを追加:

```
0: TitleScene
1: Stage1
2: Stage2
3: Stage3
...
N: ClearScene
N+1: GameOverScene
```

### GameManager 設定

各ステージシーンの `GameManager` で:

| パラメータ | 設定 |
|-----------|------|
| Total Stages | 総ステージ数（例: 5） |

### ステージ進行の仕組み

1. ゴールに到達 → `ClearScene` へ
2. `ClearScene` で「Next Stage」を押す → 次のステージへ
3. 最終ステージクリア → 「Next Stage」ボタンが非表示に
4. 進行状況は `PlayerPrefs` に保存される

### StageManager API

```csharp
// 現在のステージ番号
int stage = StageManager.CurrentStage;

// 次のステージへ
StageManager.GoToNextStage();

// 現在のステージをリトライ
StageManager.RetryCurrentStage();

// 特定のステージへ移動
StageManager.GoToStage(3);

// 進行状況リセット
StageManager.ResetProgress();
```

---

## オーディオセットアップ

BGMとSE（効果音）を管理するシステムです。音量はPlayerPrefsに保存されます。

### 1. AudioManager を作成

どのシーンからでもアクセスできるように、最初に読み込まれるシーン（TitleScene推奨）に配置します。

1. **TitleScene** を開く
2. Hierarchy 右クリック > Create Empty
3. 名前: `AudioManager`
4. **Add Component > AudioManager**
5. Inspector で設定:

| フィールド | 説明 |
|-----------|------|
| BGM Source | 空欄でOK（自動生成） |
| SFX Source | 空欄でOK（自動生成） |
| Jump Sound | ジャンプ時のSE |
| Land Sound | 着地時のSE |
| Switch Sound | 重力スイッチ切替時のSE |
| Goal Sound | ゴール到達時のSE |
| Death Sound | 死亡時のSE |
| Main BGM | ゲーム中のBGM |

### 2. AudioClip の準備

1. `Assets/Audio/` フォルダを作成
2. SE ファイル（.wav, .mp3など）をドラッグ
3. AudioManager の各フィールドにドラッグ

### 3. 音量設定UI（オプション）

ポーズメニューや設定画面に音量調整スライダーを配置する手順です。
TitleScene には既に Canvas（TitleText と StartButton の親）があるので、それを使います。

#### Step 1: 設定パネル作成

1. **TitleScene** を開く
2. Hierarchy で既存の **Canvas** を選択（TitleText の親オブジェクト）
3. Canvas 右クリック > **UI > Panel**
4. 名前を `SettingsPanel` に変更
5. **Rect Transform** を設定:
   - **Anchor Preset**: Middle Center（中央）
   - **Width**: `400`, **Height**: `300`
6. **Image** の Color を `#000000CC`（半透明の黒）に設定

#### Step 3: マスター音量スライダー作成

1. SettingsPanel 右クリック > **UI > Slider**
2. 名前を `MasterSlider` に変更
3. **Rect Transform** を設定:
   - **Pos Y**: `80`
   - **Width**: `300`, **Height**: `30`
4. **Slider** コンポーネントを設定:
   - **Min Value**: `0`
   - **Max Value**: `1`
   - **Value**: `1`
   - **Whole Numbers**: **OFF**（チェックを外す）

#### Step 4: マスター音量ラベル作成

1. SettingsPanel 右クリック > **UI > Text - TextMeshPro**
2. 名前を `MasterLabel` に変更
3. **Rect Transform** を設定:
   - **Pos Y**: `110`
   - **Width**: `300`, **Height**: `30`
4. **TextMeshProUGUI** を設定:
   - **Text**: `マスター: 100%`
   - **Font Size**: `24`
   - **Alignment**: Center

#### Step 5: BGM音量スライダー作成

1. `MasterSlider` を **Ctrl+D** で複製
2. 名前を `BGMSlider` に変更
3. **Rect Transform** の **Pos Y** を `0` に変更

#### Step 6: BGM音量ラベル作成

1. `MasterLabel` を **Ctrl+D** で複製
2. 名前を `BGMLabel` に変更
3. **Rect Transform** の **Pos Y** を `30` に変更
4. **Text** を `BGM: 70%` に変更

#### Step 7: SE音量スライダー作成

1. `MasterSlider` を **Ctrl+D** で複製
2. 名前を `SFXSlider` に変更
3. **Rect Transform** の **Pos Y** を `-80` に変更

#### Step 8: SE音量ラベル作成

1. `MasterLabel` を **Ctrl+D** で複製
2. 名前を `SFXLabel` に変更
3. **Rect Transform** の **Pos Y** を `-50` に変更
4. **Text** を `SE: 100%` に変更

#### Step 9: VolumeSettingsUI をアタッチ

1. `SettingsPanel` を選択
2. **Add Component > Volume Settings UI**
3. Inspector で各フィールドにドラッグ:

| フィールド | ドラッグするオブジェクト |
|-----------|------------------------|
| Master Slider | MasterSlider |
| BGM Slider | BGMSlider |
| SFX Slider | SFXSlider |
| Master Label | MasterLabel |
| BGM Label | BGMLabel |
| SFX Label | SFXLabel |

#### 最終的な階層構造

```
SettingsCanvas
└── SettingsPanel (VolumeSettingsUI.cs)
    ├── MasterSlider (Slider)
    ├── MasterLabel (TextMeshProUGUI)
    ├── BGMSlider (Slider)
    ├── BGMLabel (TextMeshProUGUI)
    ├── SFXSlider (Slider)
    └── SFXLabel (TextMeshProUGUI)
```


### 最終的な階層構造

```
TitleScene
└── AudioManager (AudioManager.cs) ← DontDestroyOnLoad
```

### 音量設定UI の階層

```
Canvas
└── VolumeSettings (VolumeSettingsUI.cs)
    ├── MasterSlider (Slider)
    ├── MasterLabel (TextMeshPro)
    ├── BGMSlider (Slider)
    ├── BGMLabel (TextMeshPro)
    ├── SFXSlider (Slider)
    └── SFXLabel (TextMeshPro)
```

### AudioManager API

```csharp
// SE再生
AudioManager.Instance.PlayJump();
AudioManager.Instance.PlaySwitch();
AudioManager.Instance.PlayGoal();
AudioManager.Instance.PlayDeath();

// カスタムSE再生
AudioManager.Instance.PlaySFX(myClip);

// BGM制御
AudioManager.Instance.PlayBGM(myBGM);
AudioManager.Instance.StopBGM();
AudioManager.Instance.PauseBGM();
AudioManager.Instance.ResumeBGM();

// 音量設定 (0.0 - 1.0)
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetBGMVolume(0.7f);
AudioManager.Instance.SetSFXVolume(1.0f);
```

---

## 切り替え壁とボタン セットアップ

ボタンを踏むと壁がオン/オフ切り替わるギミックです。チャンネル番号で複数組を管理できます。

### 記号とチャンネルの対応

| 記号 | オブジェクト | チャンネルID |
|------|--------------|--------------|
| `0`～`9` | 壁ボタン | 0～9 |
| `a`～`j` | 切り替え壁 | 0～9 |

例: `0` ボタンを踏むと `a` 壁がトグル、`1` ボタンを踏むと `b` 壁がトグル

### 1. 切り替え壁のプレハブ作成（ToggleableWall）

1. **Hierarchy > 右クリック > 2D Object > Sprites > Square**
2. 名前を `ToggleableWall` に変更
3. **Transform**:
   - Scale: `(1, 1, 1)`
4. **Add Component > Box Collider 2D**
   - Size: `(1, 1)`
   - **Is Trigger: オフ**（壁なので実体が必要）
5. **Add Component > Toggleable Wall**（スクリプト）
6. **SpriteRenderer** の設定:
   - Sprite: 壁用のスプライト
   - Color: 任意（スクリプトで制御されます）
7. **Layer を `Ground` に設定**
8. **Project（Assets/Prefabs/）にドラッグ** → プレハブ化
9. Hierarchy のオブジェクトを削除

#### ToggleableWall コンポーネントの設定

| パラメータ | 説明 | デフォルト |
|------------|------|------------|
| Channel Id | このボタンで制御されるチャンネル | 0 |
| Start Enabled | 初期状態でオンか | true |
| Enabled Color | 有効時の色 | 白 |
| Disabled Color | 無効時の色 | 半透明 |

### 2. 壁ボタンのプレハブ作成（WallButton）

1. **Hierarchy > 右クリック > 2D Object > Sprites > Square**
2. 名前を `WallButton` に変更
3. **Transform**:
   - Scale: `(1, 0.3, 1)` ← 薄いボタン形状
4. **Add Component > Box Collider 2D**
   - Size: `(1, 1)`
   - **Is Trigger: オン** ← 重要！
5. **Add Component > Wall Button**（スクリプト）
6. **SpriteRenderer** の設定:
   - Sprite: ボタン用のスプライト
   - Color: 任意
7. **Project（Assets/Prefabs/）にドラッグ** → プレハブ化
8. Hierarchy のオブジェクトを削除

#### WallButton コンポーネントの設定

| パラメータ | 説明 | デフォルト |
|------------|------|------------|
| Channel Id | 制御する壁のチャンネル | 0 |
| Toggle Mode | true=押すたびにトグル / false=押している間のみ | true |
| Cooldown | トグルモード時のクールダウン（秒） | 0.5 |
| Normal Color | 通常時の色 | 白 |
| Pressed Color | 押された時の色 | グレー |

### 3. MapSettings にプレハブを登録

1. **Project** ウィンドウで `Assets/Data/MapSettings.asset` を選択
2. **Inspector** の **Prefabs - Toggleable** セクションで設定:

| 欄 | 設定するプレハブ |
|----|------------------|
| Toggleable Wall Prefab | ToggleableWall |
| Wall Button Prefab | WallButton |

### 4. マップエディタでの使用

マップエディタで以下のキーを使って配置できます:

- **数字キー `0`～`9`**: ボタン（チャンネル0～9）
- **小文字 `a`～`j`**: 切り替え壁（チャンネル0～9）

### 5. テストプレイ

1. マップエディタで `0` キーを押してボタンを配置
2. `a` キーを押して切り替え壁を配置
3. **Play** ボタンでテストプレイ
4. プレイヤーでボタンを踏むと壁がトグルすることを確認

### マップ例

```
####################
#     aaa          #   ← a壁（チャンネル0）
#   0         1    #   ← 0ボタンでa壁、1ボタンでb壁を切り替え
#               bbb#   ← b壁（チャンネル1）
# S            G   #
####################
```

### 最終的な階層構造

```
Assets/
└── Prefabs/
    ├── ToggleableWall.prefab  (ToggleableWall.cs)
    └── WallButton.prefab      (WallButton.cs)
```

