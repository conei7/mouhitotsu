# もうひとつ (Mouhitotsu) - 仕様書

## 概要

**「もうひとつ」** は、もう一つの重力を操りゴールを目指す2Dアクションパズルゲームです。

- **ジャンル**: 2Dアクションパズル
- **エンジン**: Unity 2022.3+
- **プラットフォーム**: PC (WebGL対応)

---

## ゲームコンセプト

通常の下向き重力に加えて、**追加の重力（もう一つ）** を獲得・操作してゴールを目指します。
重力スイッチを踏むと追加重力がベクトル合成され、独特な挙動が生まれます。

---

## 操作方法

| キー | アクション |
|------|-----------|
| A / ← | 左移動 |
| D / → | 右移動 |
| W / ↑ / Space | ジャンプ |
| R | リトライ |
| Esc | 設定画面表示/非表示 |

---

## プロジェクト構造

```
Assets/
├── Audio/                    # オーディオファイル
├── Data/
│   ├── Maps/                 # マップテキストファイル
│   │   ├── Stage1.txt
│   │   └── Stage2.txt
│   └── MapSettings.asset     # マップ生成設定
├── Prefabs/                  # プレハブ
│   ├── Player.prefab
│   ├── Wall.prefab
│   ├── Goal.prefab
│   └── Switches/
│       ├── SwitchUp.prefab
│       ├── SwitchDown.prefab
│       ├── SwitchLeft.prefab
│       └── SwitchRight.prefab
├── Scenes/
│   ├── TitleScene.unity      # タイトル画面
│   ├── Stage1.unity          # ステージ1
│   ├── Stage2.unity          # ステージ2
│   ├── ClearScene.unity      # クリア画面
│   └── GameOverScene.unity   # ゲームオーバー画面
├── Scripts/                  # スクリプト（詳細は後述）
└── Sprites/                  # スプライト画像
```

---

## スクリプト一覧

### Core（コアシステム）

| ファイル | 説明 |
|----------|------|
| `GameManager.cs` | ゲーム状態管理、シーン遷移、プレイヤー監視 |
| `GravityController.cs` | 重力システム管理（通常重力 + 追加重力の合成） |
| `StageManager.cs` | ステージ進行管理、リトライ、進捗保存（PlayerPrefs） |

### Player（プレイヤー）

| ファイル | 説明 |
|----------|------|
| `CharacterBase.cs` | プレイヤー移動、ジャンプ、接地判定、ゴール判定、無重力時の壁歩き |

### Camera（カメラ）

| ファイル | 説明 |
|----------|------|
| `GravityCamera.cs` | 重力方向に合わせてカメラ回転、プレイヤー追従 |

### Audio（オーディオ）

| ファイル | 説明 |
|----------|------|
| `AudioManager.cs` | BGM/SE管理、音量設定（PlayerPrefsに保存）、シングルトン |

### Obstacles（障害物/ギミック）

| ファイル | 説明 |
|----------|------|
| `GravitySwitch.cs` | 重力切替スイッチ（ON/OFF画像対応、方向別、クールダウン付き） |
| `GravityOrb.cs` | 一時的重力オーブ |
| `Goal.cs` | ゴールエリア判定 |
| `Spike.cs` | トゲ（触れると死亡） |
| `FallZone.cs` | 落下死判定エリア |

### UI（ユーザーインターフェース）

| ファイル | 説明 |
|----------|------|
| `GravityIndicatorUI.cs` | 重力方向インジケーター（3本の矢印で表示） |
| `ClearScreenUI.cs` | クリア画面（次のステージ/リトライ/タイトル） |
| `VolumeSettingsUI.cs` | 音量設定スライダー |
| `SettingsToggle.cs` | 設定パネル表示/非表示切り替え |
| `TitleButton.cs` | タイトルのStartボタン |
| `RetryButton.cs` | リトライボタン |
| `ToTitleButton.cs` | タイトルへ戻るボタン |

### LevelEditor（レベルエディタ）

| ファイル | 説明 |
|----------|------|
| `MapGenerator.cs` | テキストからマップ自動生成 |
| `MapData.cs` | マップデータ（ScriptableObject） |
| `MapSettings.cs` | マップ生成設定（プレハブ参照） |

### Editor（エディタ拡張）

| ファイル | 説明 |
|----------|------|
| `MapGeneratorEditor.cs` | MapGeneratorのカスタムインスペクター |
| `MapDataEditor.cs` | MapDataのカスタムインスペクター |

---

## 重力システム詳細

### 通常重力
- 常に下向き: `(0, -9.81)`
- `Physics2D.gravity` で設定

### 追加重力（もう一つ）
- 重力スイッチで方向を加算/上書き/リセット
- 重ねがけ可能（最大3回分）
- プレイヤーに追加の力として適用

### ベクトル合成
```
合成重力 = 通常重力 + (追加重力 × 強度倍率)
```

### 無重力モード
- 追加重力の方向が `None` の場合、無重力状態
- 壁に張り付いて歩行可能
- ジャンプで壁から離れ、他の壁にぶつかるまで飛行

---

## マップシステム

### テキストフォーマット

```
####################
#                  #
#                 G#
#                ###
#       ###        #
# S   ^ ###    >   #
####################
```

| 記号 | 意味 |
|------|------|
| `#` | 壁/床 |
| `S` | スタート位置 |
| `G` | ゴール |
| `^` | 上重力スイッチ |
| `v` | 下重力スイッチ |
| `<` | 左重力スイッチ |
| `>` | 右重力スイッチ |
| `o` | 無重力スイッチ |
| `X` | トゲ |
| `0`～`9` | 壁ボタン（チャンネル0～9） |
| `a`～`j` | 切り替え壁（チャンネル0～9） |
| ` ` | 空間 |

### ボタンと切り替え壁の連動

同じチャンネル番号のボタンと壁がリンクします：
- `0` を踏むと `a` の壁がトグル
- `1` を踏むと `b` の壁がトグル
- ...以下同様

```
例:
####################
#     aaa          #
#   0         1    #   ← 0でa壁、1でb壁を切り替え
#               bbb#
# S                #
####################
```

### マップ生成フロー
1. `MapData` (ScriptableObject) にテキストを設定
2. `MapGenerator` がテキストを解析
3. 各記号に対応するプレハブを生成
4. 壁は `CompositeCollider2D` で結合

---

## スプライト一覧

| ファイル名 | 用途 | 推奨サイズ |
|------------|------|-----------|
| `up_on.png` / `up_off.png` | 上スイッチ ON/OFF | 200x200 |
| `down_on.png` / `down_off.png` | 下スイッチ ON/OFF | 200x200 |
| `left_on.png` / `left_off.png` | 左スイッチ ON/OFF | 200x200 |
| `right_on.png` / `right_off.png` | 右スイッチ ON/OFF | 200x200 |
| `ground_block.png` | 床/壁タイル | 1024x1024 |
| `background_space_*.png` | 背景 | 1024x1024 |
| `idle.png` | プレイヤー待機 | - |

### Pixels Per Unit 設定
- スイッチ画像: `200`
- 地面ブロック: `1024`
- 1ユニット = 1ブロック

---

## シーン構成

### TitleScene
- タイトルテキスト
- Startボタン
- 設定パネル（音量調整）
- AudioManager（DontDestroyOnLoad）
- GameManager

### Stage1 / Stage2
- MapGenerator による自動生成マップ
- Player
- GravityController
- GravityIndicatorUI（Canvas）
- 背景画像

### ClearScene
- クリアメッセージ
- Next Stageボタン（次ステージがある場合）
- Retryボタン
- Titleボタン

### GameOverScene
- Game Overメッセージ
- Retryボタン
- Titleボタン

---

## 保存データ（PlayerPrefs）

| キー | 型 | 説明 |
|------|-----|------|
| `CurrentStage` | int | 現在のステージ番号 |
| `MaxClearedStage` | int | クリア済み最大ステージ |
| `LastPlayedScene` | string | 最後にプレイしたシーン名（リトライ用） |
| `MasterVolume` | float | マスター音量 (0-1) |
| `BGMVolume` | float | BGM音量 (0-1) |
| `SFXVolume` | float | SE音量 (0-1) |

---

## 主要パラメータ

### CharacterBase
| パラメータ | 値 | 説明 |
|------------|-----|------|
| `moveSpeed` | 4f | 移動速度 |
| `jumpHeightBlocks` | 3 | ジャンプ高さ（ブロック数） |
| `groundCheckDistance` | 0.1f | 接地判定距離 |

### GravityController
| パラメータ | 値 | 説明 |
|------------|-----|------|
| `primaryGravity` | (0, -9.81) | 通常重力 |
| `secondaryGravityStrength` | 1.0 | 追加重力の強度倍率 |
| `maxSecondaryMagnitude` | 3.0 | 追加重力の最大重ねがけ数 |

### GravitySwitch
| パラメータ | 値 | 説明 |
|------------|-----|------|
| `gravityStrength` | 1.0 | この重力の強度 |
| `COOLDOWN` | 1.0秒 | 連続トグル防止 |

### GravityCamera
| パラメータ | 値 | 説明 |
|------------|-----|------|
| `rotationSpeed` | 5f | 回転スムージング速度 |
| `followSpeed` | 5f | プレイヤー追従速度 |

---

## 依存パッケージ

- **DOTween** - アニメーション
- **TextMeshPro** - テキスト表示

---

## WebGL ビルド

1. **File > Build Settings**
2. **WebGL** を選択 → **Switch Platform**
3. シーンを追加（TitleScene を Index 0 に）
4. **Player Settings**:
   - Default Canvas: 960 x 540
   - Compression: Gzip
5. **Build**

---

## 今後の課題

- [ ] ステージ追加
- [ ] ゴール画像の作成
- [ ] キャラクターアニメーション
- [ ] ポーズメニュー
- [ ] タイマー/スコアシステム
- [ ] チェックポイント
- [ ] チュートリアル

---

## 更新履歴

### 2024-12-28
- 初期バージョン完成
- オーディオシステム追加
- 設定UI追加
- スイッチ画像対応
- リトライ修正
- 無重力壁歩き実装
- 重力インジケーターUI実装
