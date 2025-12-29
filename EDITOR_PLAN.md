# マップエディタ実装計画

## 概要
マリオメーカー風のマップエディタを実装する。

## 機能要件
- グリッドベースの配置/削除
- オブジェクトパレット
- カメラ操作（パン/ズーム）
- 保存/読み込み（WebGL対応: PlayerPrefs使用）
- テストプレイ機能
- 可変マップサイズ（壁で囲む形式）

---

## Phase 1: 基盤システム

### 1.1 EditorScene 作成
- [ ] 新規シーン `EditorScene.unity`
- [ ] EditorManager (空オブジェクト)
- [ ] EditorCamera
- [ ] Canvas (EditorUI)

### 1.2 GridSystem.cs
```csharp
- ワールド座標 ↔ グリッド座標変換
- グリッドサイズ設定
- マウス位置からグリッド座標取得
```

### 1.3 EditorCamera.cs
```csharp
- WASDでパン
- マウスホイールでズーム
- 境界なし（無限キャンバス）
```

### 1.4 PlacementSystem.cs
```csharp
- 左クリック: 配置
- 右クリック: 削除
- 配置済みオブジェクト管理 (Dictionary<Vector2Int, GameObject>)
- プレビュー表示（マウス位置にゴースト）
```

---

## Phase 2: UI システム

### 2.1 ObjectPalette.cs
```csharp
- オブジェクト一覧表示（ボタンUI）
- 選択中オブジェクトハイライト
- オブジェクトタイプ:
  - # 壁
  - S スタート（1つのみ）
  - G ゴール
  - ^ v < > スイッチ
  - B 箱
  - X トゲ
```

### 2.2 EditorToolbar.cs
```csharp
- 現在選択中のオブジェクト表示
- クリア（全削除）ボタン
- 保存/読込ボタン
- テストプレイボタン
- タイトルに戻るボタン
```

### 2.3 GridOverlay.cs
```csharp
- グリッド線表示（LineRenderer or Shader）
- 配置可能エリアの視覚化
```

---

## Phase 3: 保存/読み込み

### 3.1 MapSerializer.cs
```csharp
- Dictionary → テキスト形式変換
- テキスト形式 → Dictionary変換
- マップ範囲自動計算
```

### 3.2 MapStorageManager.cs
```csharp
- PlayerPrefs にマップ保存
- スロット管理（複数マップ保存）
- マップ一覧取得
```

### 3.3 SaveLoadUI.cs
```csharp
- 保存ダイアログ（名前入力）
- 読み込みダイアログ（一覧表示）
- 削除確認
```

---

## Phase 4: テストプレイ

### 4.1 EditorManager.cs 拡張
```csharp
- エディタモード ↔ プレイモード切り替え
- プレイモード: エディタUIを非表示、プレイヤー生成
- 一時的なマップオブジェクト生成
- プレイ終了 → エディタに戻る
```

### 4.2 EditorPlayMode.cs
```csharp
- プレイヤー/GravityController/GameManager生成
- Escでエディタに戻る
- クリア/死亡時の処理
```

---

## スクリプト一覧（予定）

```
Assets/Scripts/Editor/
├── MapEditor/
│   ├── EditorManager.cs       # 全体管理
│   ├── GridSystem.cs          # グリッド管理
│   ├── PlacementSystem.cs     # 配置ロジック
│   ├── EditorCamera.cs        # カメラ操作
│   ├── MapSerializer.cs       # シリアライズ
│   └── MapStorageManager.cs   # 保存管理
├── MapEditorUI/
│   ├── ObjectPalette.cs       # パレットUI
│   ├── EditorToolbar.cs       # ツールバー
│   ├── SaveLoadUI.cs          # 保存読込UI
│   └── GridOverlay.cs         # グリッド表示
```

---

## データ構造

### EditorMapData
```csharp
public class EditorMapData
{
    public Dictionary<Vector2Int, char> tiles;
    public Vector2Int playerSpawn;
    public string mapName;
    public DateTime createdAt;
}
```

### 保存形式（PlayerPrefs）
```
Key: "CustomMap_0", "CustomMap_1", ...
Value: テキスト形式のマップデータ
```

---

## 実装順序

1. **EditorScene + GridSystem + EditorCamera** ← 今日
2. **PlacementSystem（基本配置）**
3. **ObjectPalette（UI）**
4. **MapSerializer + MapStorageManager**
5. **SaveLoadUI**
6. **テストプレイ機能**

---

## 見積もり時間

- Phase 1: 2-3時間
- Phase 2: 2-3時間
- Phase 3: 2時間
- Phase 4: 2時間

合計: 約8-10時間
