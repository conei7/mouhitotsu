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

    private void Start()
    {
        CreatePreviewObject();
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
        
        if (action.wasPlace)
        {
            // 配置を取り消す = 削除
            RemoveTileInternal(action.position);
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
        
        if (action.wasPlace)
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
        // 既に同じタイルがある場合はスキップ
        if (placedTiles.TryGetValue(gridPos, out PlacedTile existing))
        {
            if (existing.tileType == tileType) return;
            // 違うタイルなら削除してから配置
            RemoveTileInternal(gridPos);
        }

        PlaceTileInternal(gridPos, tileType);

        // Undo履歴に追加
        undoStack.Push(new UndoAction { position = gridPos, tileType = tileType, wasPlace = true });
        redoStack.Clear(); // 新しいアクションでRedoをクリア

        // 履歴制限
        while (undoStack.Count > MAX_UNDO)
        {
            // 古いものを捨てる（Stackなので難しいが、とりあえず制限）
        }
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
    /// 全タイルを削除
    /// </summary>
    public void ClearAll()
    {
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
}

