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
| `Camera/DualCamera.cs` | プレイヤー追従カメラ |
| `Obstacles/Goal.cs` | ゴールエリア判定 |
| `Obstacles/Spike.cs` | トゲ（触れると死亡） |
| `Obstacles/FallZone.cs` | 落下死判定エリア |
| `Obstacles/GravitySwitch.cs` | 重力切替スイッチ（Add/Set/Clearモード対応） |
| `Obstacles/GravityOrb.cs` | 一時的重力オーブ |
| `UI/GravityIndicatorUI.cs` | 重力インジケーターUI |
| `UI/TitleButton.cs` | タイトルのStartボタン |
| `UI/RetryButton.cs` | リトライボタン |
| `UI/ToTitleButton.cs` | タイトルへ戻るボタン |


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
3. **テキスト**: Pos `(0, 50, 0)`, Text: `Clear!`, Font Size: `80`, Color: `#4ade80`
4. **Titleボタン**: Pos `(0, -80, 0)`, Size: `200 x 60`, Text: `Title`
   - Add Component > `ToTitleButton`, OnClick > `ToTitleButton.OnTitleClick`

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

