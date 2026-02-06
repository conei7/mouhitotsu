using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 動的マップ読み込み - GameSceneでステージを動的に生成
/// </summary>
public class RuntimeMapLoader : MonoBehaviour
{
    public static RuntimeMapLoader Instance { get; private set; }

    [Header("Required References")]
    [Tooltip("マップ設定（プレハブ等）")]
    public MapSettings mapSettings;

    [Header("Camera Settings")]
    [Tooltip("メインカメラ（自動でサイズ・位置を調整）")]
    public Camera mainCamera;

    [Tooltip("カメラの余白")]
    public float cameraPadding = 1f;

    [Header("Generated Objects Container")]
    private Transform objectsContainer;

    [Header("Runtime State")]
    [SerializeField] private bool isLoaded = false;
    [SerializeField] private string currentStageId;

    public bool IsLoaded => isLoaded;
    public string CurrentStageId => currentStageId;

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
        // シーン開始時に StageManager から現在のステージをロード
        if (StageManager.CurrentStageData != null)
        {
            LoadStage(StageManager.CurrentStageData);
        }
        else if (!string.IsNullOrEmpty(StageManager.CurrentStageId))
        {
            LoadStage(StageManager.CurrentStageId);
        }
        else
        {
            Debug.LogWarning("No stage data set. Loading first built-in stage.");
            // デフォルトでステージ1をロード
            if (StageDatabase.Instance != null)
            {
                var stages = StageDatabase.Instance.GetBuiltInStages();
                if (stages.Count > 0)
                {
                    LoadStage(stages[0]);
                }
            }
        }
    }

    /// <summary>
    /// ステージIDでロード
    /// </summary>
    public void LoadStage(string stageId)
    {
        if (StageDatabase.Instance == null)
        {
            Debug.LogError("StageDatabase not found");
            return;
        }

        StageData stageData = StageDatabase.Instance.GetStage(stageId);
        if (stageData == null)
        {
            Debug.LogError($"Stage not found: {stageId}");
            return;
        }

        LoadStage(stageData);
    }

    /// <summary>
    /// ステージデータでロード
    /// </summary>
    public void LoadStage(StageData stageData)
    {
        if (stageData == null)
        {
            Debug.LogError("StageData is null");
            return;
        }

        if (mapSettings == null)
        {
            Debug.LogError("MapSettings が設定されていません");
            return;
        }

        // バリデーション
        if (!stageData.Validate(out string error))
        {
            Debug.LogError($"マップエラー: {error}");
            return;
        }

        if (!mapSettings.Validate(out var settingsErrors))
        {
            foreach (var err in settingsErrors)
            {
                Debug.LogError($"設定エラー: {err}");
            }
            return;
        }

        // 既存のマップを削除
        UnloadCurrentStage();

        // 現在のステージ情報を保存
        currentStageId = stageData.id;
        StageManager.CurrentStageId = stageData.id;
        StageManager.CurrentStageData = stageData;
        StageManager.LastPlayedScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // コンテナ作成
        GameObject container = new GameObject("GeneratedMap");
        objectsContainer = container.transform;

        // マップ生成
        GenerateMap(stageData);

        // カメラ設定
        SetupCamera(stageData);

        isLoaded = true;
        Debug.Log($"Loaded stage: {stageData.name} ({stageData.id})");
    }

    /// <summary>
    /// マップを生成
    /// </summary>
    private void GenerateMap(StageData stageData)
    {
        char[,] map = stageData.GetMapArray();
        float gridSize = mapSettings.gridSize;
        int width = stageData.Width;
        int height = stageData.Height;

        // 壁用の親オブジェクト（CompositeCollider2Dで継ぎ目をなくす）
        GameObject wallContainer = new GameObject("Walls");
        wallContainer.transform.SetParent(objectsContainer);
        wallContainer.layer = LayerMask.NameToLayer("Ground");

        var wallRb = wallContainer.AddComponent<Rigidbody2D>();
        wallRb.bodyType = RigidbodyType2D.Static;

        var composite = wallContainer.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;

        // 切替壁用のCompositeCollider2Dコンテナ（チャンネル別）
        Dictionary<int, GameObject> toggleWallContainers = new Dictionary<int, GameObject>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                char symbol = map[x, y];
                GameObject prefab = mapSettings.GetPrefab(symbol);

                if (prefab != null)
                {
                    Vector3 position = new Vector3(x * gridSize, y * gridSize, 0);

                    // 壁は専用コンテナに入れる
                    Transform parent = objectsContainer;
                    if (symbol == '#')
                    {
                        parent = wallContainer.transform;
                    }
                    else if (MapSettings.IsToggleableWall(symbol))
                    {
                        int channelId = MapSettings.GetChannelId(symbol);
                        if (!toggleWallContainers.ContainsKey(channelId))
                        {
                            // チャンネル別のコンテナを作成
                            GameObject toggleContainer = new GameObject($"ToggleableWalls_CH{channelId}");
                            toggleContainer.transform.SetParent(objectsContainer);
                            toggleContainer.layer = LayerMask.NameToLayer("Ground");

                            var rb = toggleContainer.AddComponent<Rigidbody2D>();
                            rb.bodyType = RigidbodyType2D.Static;

                            var comp = toggleContainer.AddComponent<CompositeCollider2D>();
                            comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
                            comp.enabled = false; // 初期化まで無効化

                            // ToggleableWallGroupを追加
                            var group = toggleContainer.AddComponent<ToggleableWallGroup>();

                            toggleWallContainers[channelId] = toggleContainer;
                        }
                        parent = toggleWallContainers[channelId].transform;
                    }

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

                    // ボタン（0-9）のチャンネルID設定
                    if (MapSettings.IsWallButton(symbol))
                    {
                        var wallButton = obj.GetComponent<WallButton>();
                        if (wallButton != null)
                        {
                            wallButton.SetChannelId(MapSettings.GetChannelId(symbol));
                        }
                    }

                    // 切替壁（a-j, k-t）のチャンネルID・初期状態設定
                    if (MapSettings.IsToggleableWall(symbol))
                    {
                        var toggleableWall = obj.GetComponent<ToggleableWall>();
                        if (toggleableWall != null)
                        {
                            toggleableWall.SetChannelId(MapSettings.GetChannelId(symbol));

                            // 初期状態を設定（k-t は初期OFF）
                            bool initiallyOff = MapSettings.IsToggleableWallInitiallyOff(symbol);
                            toggleableWall.SetInitialState(!initiallyOff);
                        }

                        // コライダーをCompositeに使用
                        var boxCollider = obj.GetComponent<BoxCollider2D>();
                        if (boxCollider != null)
                        {
                            boxCollider.usedByComposite = true;
                        }

                        obj.layer = LayerMask.NameToLayer("Ground");
                    }
                }
            }
        }

        // 切替壁グループを一括初期化
        foreach (var container in toggleWallContainers.Values)
        {
            var group = container.GetComponent<ToggleableWallGroup>();
            if (group != null)
            {
                group.InitializeFromChildren();
            }
        }
    }

    /// <summary>
    /// カメラをマップ全体が見えるように設定
    /// </summary>
    private void SetupCamera(StageData stageData)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null) return;

        float gridSize = mapSettings.gridSize;
        float mapWidth = stageData.Width * gridSize;
        float mapHeight = stageData.Height * gridSize;

        // カメラサイズ（自動計算または手動設定）
        float cameraSize;
        if (stageData.cameraSize > 0)
        {
            cameraSize = stageData.cameraSize;
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
            (mapWidth / 2f) - (gridSize / 2f) + stageData.cameraOffsetX,
            (mapHeight / 2f) - (gridSize / 2f) + stageData.cameraOffsetY,
            -10f
        );
        mainCamera.transform.position = cameraPos;
    }

    /// <summary>
    /// 現在のステージをアンロード
    /// </summary>
    public void UnloadCurrentStage()
    {
        if (objectsContainer != null)
        {
            Destroy(objectsContainer.gameObject);
            objectsContainer = null;
        }

        isLoaded = false;
        currentStageId = null;
    }

    /// <summary>
    /// 現在のステージをリロード
    /// </summary>
    public void ReloadCurrentStage()
    {
        if (StageManager.CurrentStageData != null)
        {
            LoadStage(StageManager.CurrentStageData);
        }
        else if (!string.IsNullOrEmpty(currentStageId))
        {
            LoadStage(currentStageId);
        }
    }
}
