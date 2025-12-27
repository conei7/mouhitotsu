using UnityEngine;

/// <summary>
/// マップ生成器 - MapDataからシーンにオブジェクトを配置
/// シーンに配置して使用
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [Header("Required References")]
    [Tooltip("生成するマップデータ")]
    public MapData mapData;
    
    [Tooltip("マップ設定（プレハブ等）")]
    public MapSettings mapSettings;

    [Header("Generated Objects Container")]
    [Tooltip("生成したオブジェクトの親（空なら自動作成）")]
    public Transform objectsContainer;

    [Header("Camera Settings")]
    [Tooltip("メインカメラ（自動でサイズ・位置を調整）")]
    public Camera mainCamera;
    
    [Tooltip("カメラの余白")]
    public float cameraPadding = 1f;

    [Header("Runtime")]
    [SerializeField] private bool hasGenerated = false;

    private void Awake()
    {
        // ランタイムで自動生成
        if (!hasGenerated && mapData != null && mapSettings != null)
        {
            GenerateMap();
        }
    }

    /// <summary>
    /// マップを生成（既存のオブジェクトを削除してから）
    /// </summary>
    public void GenerateMap()
    {
        if (mapData == null)
        {
            Debug.LogError("MapData が設定されていません");
            return;
        }

        if (mapSettings == null)
        {
            Debug.LogError("MapSettings が設定されていません");
            return;
        }

        // バリデーション
        if (!mapData.Validate(out string mapError))
        {
            Debug.LogError($"マップエラー: {mapError}");
            return;
        }

        if (!mapSettings.Validate(out var settingsErrors))
        {
            foreach (var error in settingsErrors)
            {
                Debug.LogError($"設定エラー: {error}");
            }
            return;
        }

        // 既存のオブジェクトを削除
        ClearMap();

        // コンテナ作成
        if (objectsContainer == null)
        {
            GameObject container = new GameObject("GeneratedMap");
            objectsContainer = container.transform;
        }

        // マップ生成
        char[,] map = mapData.GetMapArray();
        float gridSize = mapSettings.gridSize;

        // 壁用の親オブジェクト（CompositeCollider2Dで継ぎ目をなくす）
        GameObject wallContainer = new GameObject("Walls");
        wallContainer.transform.SetParent(objectsContainer);
        wallContainer.layer = LayerMask.NameToLayer("Ground");
        
        var wallRb = wallContainer.AddComponent<Rigidbody2D>();
        wallRb.bodyType = RigidbodyType2D.Static;
        
        var composite = wallContainer.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                char symbol = map[x, y];
                GameObject prefab = mapSettings.GetPrefab(symbol);

                if (prefab != null)
                {
                    Vector3 position = new Vector3(x * gridSize, y * gridSize, 0);
                    
                    // 壁は専用コンテナに入れる
                    Transform parent = (symbol == '#') ? wallContainer.transform : objectsContainer;
                    GameObject obj = Instantiate(prefab, position, Quaternion.identity, parent);
                    obj.name = $"{MapSettings.GetSymbolDescription(symbol)}_{x}_{y}";
                    
                    // 壁のコライダーをCompositeに使用
                    if (symbol == '#')
                    {
                        var boxCollider = obj.GetComponent<BoxCollider2D>();
                        if (boxCollider != null)
                        {
                            boxCollider.usedByComposite = true;
                        }
                    }
                }
            }
        }

        // カメラ設定
        SetupCamera();

        hasGenerated = true;
        Debug.Log($"マップ生成完了: {mapData.Width}x{mapData.Height}");
    }

    /// <summary>
    /// 生成したマップを削除
    /// </summary>
    public void ClearMap()
    {
        if (objectsContainer != null)
        {
            // エディタでもランタイムでも動作
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(objectsContainer.gameObject);
            }
            else
            {
                Destroy(objectsContainer.gameObject);
            }
            #else
            Destroy(objectsContainer.gameObject);
            #endif
            
            objectsContainer = null;
        }

        hasGenerated = false;
    }

    /// <summary>
    /// カメラをマップ全体が見えるように設定
    /// </summary>
    private void SetupCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null) return;

        float gridSize = mapSettings.gridSize;
        float mapWidth = mapData.Width * gridSize;
        float mapHeight = mapData.Height * gridSize;

        // カメラサイズ（自動計算または手動設定）
        float cameraSize;
        if (mapData.cameraSize > 0)
        {
            cameraSize = mapData.cameraSize;
        }
        else
        {
            // アスペクト比を考慮して自動計算
            float aspectRatio = mainCamera.aspect;
            float sizeByHeight = (mapHeight / 2f) + cameraPadding;
            float sizeByWidth = ((mapWidth / 2f) + cameraPadding) / aspectRatio;
            cameraSize = Mathf.Max(sizeByHeight, sizeByWidth);
        }

        mainCamera.orthographicSize = cameraSize;

        // カメラ位置（マップ中央）
        Vector3 cameraPos = new Vector3(
            (mapWidth / 2f) - (gridSize / 2f) + mapData.cameraOffset.x,
            (mapHeight / 2f) - (gridSize / 2f) + mapData.cameraOffset.y,
            -10f
        );
        mainCamera.transform.position = cameraPos;
    }

    /// <summary>
    /// マップ情報を取得
    /// </summary>
    public string GetMapInfo()
    {
        if (mapData == null) return "MapData未設定";

        string info = $"マップ: {mapData.stageName}\n";
        info += $"サイズ: {mapData.Width} x {mapData.Height}\n";
        
        // オブジェクト数カウント
        char[,] map = mapData.GetMapArray();
        int wallCount = 0, switchCount = 0;
        
        for (int x = 0; x < mapData.Width; x++)
        {
            for (int y = 0; y < mapData.Height; y++)
            {
                char c = map[x, y];
                if (c == '#') wallCount++;
                if (c == '^' || c == 'v' || c == '<' || c == '>') switchCount++;
            }
        }
        
        info += $"壁: {wallCount}, スイッチ: {switchCount}";
        return info;
    }
}
