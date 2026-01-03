using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 配置システム - オブジェクトの配置と削除
/// </summary>
public class PlacementSystem : MonoBehaviour
{
    public static PlacementSystem Instance { get; private set; }

    [Header("References")]
    [SerializeField] private MapSettings mapSettings;

    [Header("Preview")]
    [SerializeField] private Color previewColor = new Color(1, 1, 1, 0.5f);
    [SerializeField] private Color deletePreviewColor = new Color(1, 0, 0, 0.5f);

    [Header("Current Selection")]
    [SerializeField] private char currentTileType = '#';

    // 配置済みタイル管理
    private Dictionary<Vector2Int, PlacedTile> placedTiles = new Dictionary<Vector2Int, PlacedTile>();

    // プレビュー用オブジェクト
    private GameObject previewObject;
    private SpriteRenderer previewRenderer;

    // スタート位置（1つのみ）
    private Vector2Int? startPosition = null;

    // Undo/Redo用
    private Stack<UndoAction> undoStack = new Stack<UndoAction>();
    private Stack<UndoAction> redoStack = new Stack<UndoAction>();
    private const int MAX_UNDO = 100;

    public char CurrentTileType => currentTileType;
    public Dictionary<Vector2Int, PlacedTile> PlacedTiles => placedTiles;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private const string AUTO_SAVE_KEY = "EditorAutoSave";

    private void Start()
    {
        CreatePreviewObject();
        // 一時的: 自動保存データをクリア（フリーズ解消後に削除）
        PlayerPrefs.DeleteKey(AUTO_SAVE_KEY);
        
        // 自動保存データがあれば読み込み（現在無効化）
        // AutoLoad();
    }

    private void OnDestroy()
    {
        // シーン離脱時に自動保存
        AutoSave();
    }

    private void OnApplicationQuit()
    {
        AutoSave();
    }

    private void AutoSave()
    {
        if (placedTiles.Count > 0)
        {
            string mapText = ToText();
            PlayerPrefs.SetString(AUTO_SAVE_KEY, mapText);
            PlayerPrefs.Save();
        }
    }

    private void AutoLoad()
    {
        if (PlayerPrefs.HasKey(AUTO_SAVE_KEY))
        {
            string mapText = PlayerPrefs.GetString(AUTO_SAVE_KEY);
            if (!string.IsNullOrEmpty(mapText) && (mapText.Contains("#") || mapText.Contains("S")))
            {
                LoadFromText(mapText);
                Debug.Log("Auto-loaded previous map data");
            }
        }
    }

    /// <summary>
    /// 自動保存データをクリア
    /// </summary>
    public void ClearAutoSave()
    {
        PlayerPrefs.DeleteKey(AUTO_SAVE_KEY);
    }

    private void Update()
    {
        UpdatePreview();
        HandleInput();
        HandleUndoRedo();
    }

    private void HandleUndoRedo()
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        
        if (ctrl && Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }
        if (ctrl && Input.GetKeyDown(KeyCode.Y))
        {
            Redo();
        }
    }

    public void Undo()
    {
        if (undoStack.Count == 0) return;

        UndoAction action = undoStack.Pop();

        // クリアアクションの場合
        if (action.clearActions != null && action.clearActions.Count > 0)
        {
            // 全タイルを復元
            foreach (var subAction in action.clearActions)
            {
                PlaceTileInternal(subAction.position, subAction.tileType);
            }
        }
        else if (action.wasPlace)
        {
            // 配置を取り消す = 削除
            RemoveTileInternal(action.position);
            
            // 上書きされたタイルがあれば復元
            if (action.replacedTileType.HasValue)
            {
                PlaceTileInternal(action.position, action.replacedTileType.Value);
            }
        }
        else
        {
            // 削除を取り消す = 再配置
            PlaceTileInternal(action.position, action.tileType);
        }

        redoStack.Push(action);
    }

    public void Redo()
    {
        if (redoStack.Count == 0) return;

        UndoAction action = redoStack.Pop();

        // クリアアクションの場合
        if (action.clearActions != null && action.clearActions.Count > 0)
        {
            // 全タイルを削除（再クリア）
            foreach (var subAction in action.clearActions)
            {
                RemoveTileInternal(subAction.position);
            }
        }
        else if (action.wasPlace)
        {
            // 配置をやり直す
            PlaceTileInternal(action.position, action.tileType);
        }
        else
        {
            // 削除をやり直す
            RemoveTileInternal(action.position);
        }

        undoStack.Push(action);
    }

    private void CreatePreviewObject()
    {
        previewObject = new GameObject("PlacementPreview");
        previewRenderer = previewObject.AddComponent<SpriteRenderer>();
        previewRenderer.sortingOrder = 100;
        UpdatePreviewSprite();
    }

    private void UpdatePreviewSprite()
    {
        if (previewRenderer == null) return;

        GameObject prefab = mapSettings?.GetPrefab(currentTileType);
        if (prefab != null)
        {
            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                previewRenderer.sprite = sr.sprite;
                previewRenderer.color = previewColor;
            }
        }
    }

    private void UpdatePreview()
    {
        if (previewObject == null || GridSystem.Instance == null) return;

        Vector3 snappedPos = GridSystem.Instance.GetMouseWorldPositionSnapped();
        previewObject.transform.position = snappedPos;

        // 既存タイルがある場所は削除プレビュー色
        Vector2Int gridPos = GridSystem.Instance.GetMouseGridPosition();
        if (placedTiles.ContainsKey(gridPos))
        {
            previewRenderer.color = deletePreviewColor;
        }
        else
        {
            previewRenderer.color = previewColor;
        }
    }

    private void HandleInput()
    {
        // テストプレイ中は編集無効
        if (EditorManager.Instance != null && EditorManager.Instance.IsPlayMode)
        {
            return;
        }

        // UI上にマウスがある場合は無視
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (GridSystem.Instance == null) return;

        Vector2Int gridPos = GridSystem.Instance.GetMouseGridPosition();

        // 左クリック: 配置
        if (Input.GetMouseButton(0))
        {
            PlaceTile(gridPos, currentTileType);
        }

        // 右クリック: 削除
        if (Input.GetMouseButton(1))
        {
            RemoveTile(gridPos);
        }
    }

    /// <summary>
    /// タイルを配置（Undo履歴あり）
    /// </summary>
    public void PlaceTile(Vector2Int gridPos, char tileType)
    {
        char? replacedTileType = null;
        
        // 既に同じタイルがある場合はスキップ
        if (placedTiles.TryGetValue(gridPos, out PlacedTile existing))
        {
            if (existing.tileType == tileType) return;
            // 違うタイルなら削除してから配置（元のタイルタイプを保存）
            replacedTileType = existing.tileType;
            RemoveTileInternal(gridPos);
        }

        PlaceTileInternal(gridPos, tileType);

        // Undo履歴に追加（上書きされたタイルも記録）
        undoStack.Push(new UndoAction { 
            position = gridPos, 
            tileType = tileType, 
            wasPlace = true,
            replacedTileType = replacedTileType
        });
        redoStack.Clear();

        // 履歴制限（Stackは直接制限できないので一旦コメントアウト）
        // TODO: 履歴が多すぎる場合の処理
    }

    /// <summary>
    /// タイルを配置（Undo履歴なし - 内部用）
    /// </summary>
    private void PlaceTileInternal(Vector2Int gridPos, char tileType)
    {
        // スタート位置の処理（1つのみ）
        if (tileType == 'S')
        {
            if (startPosition.HasValue && startPosition.Value != gridPos)
            {
                RemoveTileInternal(startPosition.Value);
            }
            startPosition = gridPos;
        }

        // プレハブ取得
        GameObject prefab = mapSettings?.GetPrefab(tileType);
        if (prefab == null)
        {
            // 壁の場合はデフォルトで四角を生成
            if (tileType == '#')
            {
                prefab = mapSettings?.wallPrefab;
            }
            if (prefab == null) return;
        }

        // オブジェクト生成
        Vector3 worldPos = GridSystem.Instance.GridToWorld(gridPos);
        GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        obj.name = $"Tile_{gridPos.x}_{gridPos.y}";

        // CharacterBaseがあれば無効化（エディタモードでは動かさない）
        var character = obj.GetComponent<CharacterBase>();
        if (character != null)
        {
            character.enabled = false;
        }

        // Rigidbody2Dがあればシミュレーションを無効化（エディタモードでは物理演算させない）
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }

        // 壁タイルの場合
        if (tileType == '#')
        {
            // Groundレイヤーに設定
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                obj.layer = groundLayer;
            }
            
            // Rigidbody2DがあればStaticにして物理演算から除外
            var wallRb = obj.GetComponent<Rigidbody2D>();
            if (wallRb != null)
            {
                wallRb.bodyType = RigidbodyType2D.Static;
            }
            
            // コライダーサイズを明示的に1x1に（Static なので edgeRadius 不要）
            var boxCollider = obj.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                boxCollider.size = new Vector2(1f, 1f);
                boxCollider.offset = Vector2.zero;
            }
        }

        // 動的オブジェクト（Box）の設定
        if (tileType == 'B')
        {
            var boxCollider = obj.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                // コライダーサイズを1x1に
                boxCollider.size = new Vector2(1f, 1f);
                
                // 反発ゼロのPhysics Materialを設定
                var mat = new PhysicsMaterial2D("BoxMaterial");
                mat.friction = 0.4f;
                mat.bounciness = 0f;
                boxCollider.sharedMaterial = mat;
            }
        }

        // ボタン（0-9）のチャンネルID設定
        if (MapSettings.IsWallButton(tileType))
        {
            var wallButton = obj.GetComponent<WallButton>();
            if (wallButton != null)
            {
                wallButton.SetChannelId(MapSettings.GetChannelId(tileType));
            }
        }

        // 切り替え壁（a-j）のチャンネルID設定
        if (MapSettings.IsToggleableWall(tileType))
        {
            var toggleableWall = obj.GetComponent<ToggleableWall>();
            if (toggleableWall != null)
            {
                toggleableWall.SetChannelId(MapSettings.GetChannelId(tileType));
            }
            
            // Groundレイヤーに設定
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                obj.layer = groundLayer;
            }
        }

        // 記録
        placedTiles[gridPos] = new PlacedTile
        {
            tileType = tileType,
            gameObject = obj
        };
    }

    /// <summary>
    /// タイルを削除（Undo履歴あり）
    /// </summary>
    public void RemoveTile(Vector2Int gridPos)
    {
        if (placedTiles.TryGetValue(gridPos, out PlacedTile tile))
        {
            char removedType = tile.tileType;
            RemoveTileInternal(gridPos);

            // Undo履歴に追加
            undoStack.Push(new UndoAction { position = gridPos, tileType = removedType, wasPlace = false });
            redoStack.Clear();
        }
    }

    /// <summary>
    /// タイルを削除（Undo履歴なし - 内部用）
    /// </summary>
    private void RemoveTileInternal(Vector2Int gridPos)
    {
        if (placedTiles.TryGetValue(gridPos, out PlacedTile tile))
        {
            if (tile.gameObject != null)
            {
                Destroy(tile.gameObject);
            }
            placedTiles.Remove(gridPos);

            // スタート位置だった場合
            if (startPosition.HasValue && startPosition.Value == gridPos)
            {
                startPosition = null;
            }
        }
    }

    /// <summary>
    /// 選択中のタイルタイプを変更
    /// </summary>
    public void SetTileType(char tileType)
    {
        currentTileType = tileType;
        UpdatePreviewSprite();
    }

    /// <summary>
    /// 全タイルを削除（Undo対応）
    /// </summary>
    public void ClearAll()
    {
        // Undo用に全タイルを記録
        List<UndoAction> clearActions = new List<UndoAction>();
        foreach (var kvp in placedTiles)
        {
            clearActions.Add(new UndoAction
            {
                position = kvp.Key,
                tileType = kvp.Value.tileType,
                wasPlace = false // 削除アクション
            });
        }

        // 1つのクリアアクションとして記録（Undoで全復元）
        if (clearActions.Count > 0)
        {
            // 複数アクションを1つにまとめるため、特別なアクションを追加
            undoStack.Push(new UndoAction
            {
                position = Vector2Int.zero,
                tileType = '\0', // 特殊マーカー
                wasPlace = false,
                clearActions = clearActions
            });
            redoStack.Clear();
        }

        // 実際に削除
        foreach (var tile in placedTiles.Values)
        {
            if (tile.gameObject != null)
            {
                Destroy(tile.gameObject);
            }
        }
        placedTiles.Clear();
        startPosition = null;
    }

    /// <summary>
    /// マップデータを読み込み
    /// </summary>
    public void LoadFromText(string mapText)
    {
        ClearAll();

        string[] lines = mapText.Split('\n');
        int height = lines.Length;

        for (int y = 0; y < height; y++)
        {
            string line = lines[height - 1 - y]; // Y軸反転
            for (int x = 0; x < line.Length; x++)
            {
                char c = line[x];
                if (c != ' ' && c != '\r')
                {
                    PlaceTile(new Vector2Int(x, y), c);
                }
            }
        }
    }

    /// <summary>
    /// マップデータをテキストに変換
    /// </summary>
    public string ToText()
    {
        if (placedTiles.Count == 0) return "";

        // 境界を計算
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var pos in placedTiles.Keys)
        {
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);
        }

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        // テキスト生成
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int y = height - 1; y >= 0; y--) // Y軸反転
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int pos = new Vector2Int(x + minX, y + minY);
                if (placedTiles.TryGetValue(pos, out PlacedTile tile))
                {
                    sb.Append(tile.tileType);
                }
                else
                {
                    sb.Append(' ');
                }
            }
            if (y > 0) sb.AppendLine();
        }

        return sb.ToString();
    }
}

/// <summary>
/// 配置済みタイル情報
/// </summary>
[System.Serializable]
public class PlacedTile
{
    public char tileType;
    public GameObject gameObject;
}

/// <summary>
/// Undo/Redoアクション
/// </summary>
public class UndoAction
{
    public Vector2Int position;
    public char tileType;
    public bool wasPlace; // true=配置, false=削除
    public char? replacedTileType; // 上書きされた元のタイル
    public List<UndoAction> clearActions; // クリア時の複数アクション
}
