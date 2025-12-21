# もうひとつ (Mouhitotsu) - セットアップガイド

2つの世界が重なる横スクロールパズルアクションゲーム。
青と赤の2体のキャラクターを同時に操作し、両方を同時にゴールさせることが目的。

---

## 目次

1. [前提条件](#前提条件)
2. [レイヤー設定](#レイヤー設定)
3. [衝突設定](#衝突設定)
4. [スクリプト一覧](#スクリプト一覧)
5. [シーン作成](#シーン作成)
6. [GameScene セットアップ](#gamescene-セットアップ)
7. [Build Settings](#build-settings)
8. [操作方法](#操作方法)

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
| 8 | `BlueWorld` |
| 9 | `RedWorld` |
| 10 | `Ground` |

---

## 衝突設定

**Edit > Project Settings > Physics 2D > Layer Collision Matrix**

- `BlueWorld` と `RedWorld` のチェックを**外す**（互いにすり抜け）
- 他は全てチェック

---

## スクリプト一覧

全て `Assets/Scripts/` 以下に配置済み

| パス | 役割 |
|------|------|
| `Player/CharacterBase.cs` | 移動・ジャンプ・ゴール判定の基底クラス |
| `Player/CharacterBlue.cs` | 青キャラクター |
| `Player/CharacterRed.cs` | 赤キャラクター |
| `Core/GameManager.cs` | ゲーム状態管理・シーン遷移 |
| `Camera/DualCamera.cs` | 2キャラ追従カメラ |
| `Obstacles/Goal.cs` | ゴールエリア判定 |
| `Obstacles/Spike.cs` | トゲ（触れると死亡） |
| `Obstacles/FallZone.cs` | 落下死判定エリア |
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
4. **Add Component > DualCamera**

### GameManager 作成

1. 右クリック > Create Empty
2. 名前: `GameManager`
3. Position: `(0, 0, 0)`
4. **Add Component > GameManager**

### BlueCharacter 作成

1. 右クリック > 2D Object > Sprites > Square
2. 名前: `BlueCharacter`
3. **Layer**: `BlueWorld`（"No, this object only" を選択）
4. **Transform**:
   - Position: `(-3, 1, 0)`
   - Scale: `(0.8, 0.8, 1)`
5. **Sprite Renderer**: Color `#4a90d9`
6. **BoxCollider2D**: Size `(1, 1)`
7. **Add Component > Rigidbody2D**:
   - Collision Detection: `Continuous`
   - Constraints > Freeze Rotation: ☑️ Z
8. **Add Component > CharacterBlue**
9. **子オブジェクト作成**:
   - BlueCharacter 右クリック > Create Empty
   - 名前: `GroundCheck`
   - Position: `(0, -0.5, 0)`
10. **CharacterBlue 設定**:
    - Ground Check: `GroundCheck` をドラッグ
    - Ground Layer: `Ground` を選択

### RedCharacter 作成

1. BlueCharacter を **Ctrl+D** で複製
2. 名前: `RedCharacter`
3. **Layer**: `RedWorld`
4. **Transform**: Position `(3, 1, 0)`
5. **Sprite Renderer**: Color `#d94a4a`
6. CharacterBlue を削除 → **Add Component > CharacterRed**
7. **CharacterRed 設定**:
   - Ground Check: 子の `GroundCheck` をドラッグ
   - Ground Layer: `Ground`

### 床 (Ground)

1. 右クリック > 2D Object > Sprites > Square
2. 名前: `Ground`
3. **Layer**: `Ground`
4. **Transform**:
   - Position: `(0, -2, 0)`
   - Scale: `(30, 1, 1)`
5. **Sprite Renderer**: Color `#666666`
6. **BoxCollider2D**: デフォルト

### 青い壁 (BlueWall)

1. Ground を **Ctrl+D**
2. 名前: `BlueWall`
3. **Layer**: `BlueWorld`
4. **Transform**:
   - Position: `(-5, 0, 0)`
   - Scale: `(1, 3, 1)`
5. **Sprite Renderer**: Color `#4a90d9`, Alpha `180`

### 赤い壁 (RedWall)

1. BlueWall を **Ctrl+D**
2. 名前: `RedWall`
3. **Layer**: `RedWorld`
4. **Transform**: Position `(5, 0, 0)`
5. **Sprite Renderer**: Color `#d94a4a`, Alpha `180`

### GoalBlue

1. 右クリック > 2D Object > Sprites > Square
2. 名前: `GoalBlue`
3. **Layer**: Default
4. **Transform**:
   - Position: `(-10, -0.5, 0)`
   - Scale: `(2, 3, 1)`
5. **Sprite Renderer**:
   - Color: `#4a90d9`, Alpha `100`
   - Order in Layer: `-1`
6. **BoxCollider2D**: ☑️ Is Trigger
7. **Add Component > Goal**: Goal Type: `Blue`

### GoalRed

1. GoalBlue を **Ctrl+D**
2. 名前: `GoalRed`
3. **Transform**: Position `(10, -0.5, 0)`
4. **Sprite Renderer**: Color `#d94a4a`, Alpha `100`
5. **Goal**: Goal Type: `Red`

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
   - Blue Character: `BlueCharacter` をドラッグ
   - Red Character: `RedCharacter` をドラッグ
2. **Main Camera (DualCamera)** を選択:
   - Blue Character: `BlueCharacter` をドラッグ
   - Red Character: `RedCharacter` をドラッグ

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

- **青キャラ**: 青い壁にのみ衝突、青ゴールに入る
- **赤キャラ**: 赤い壁にのみ衝突、赤ゴールに入る
- **クリア条件**: 両方が同時にゴールに存在
- **ゲームオーバー**: どちらかが落下死・トゲに触れる
