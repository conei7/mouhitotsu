using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// エディタマネージャー - エディタモードとプレイモードの切り替え
/// </summary>
public class EditorManager : MonoBehaviour
{
    public static EditorManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField] private GridOverlay gridOverlay;
    [SerializeField] private EditorCamera editorCamera;
    [SerializeField] private Canvas editorCanvas;

    [Header("Prefabs for Test Play")]
    [SerializeField] private GameObject gravityControllerPrefab;
    [SerializeField] private GameObject gravityIndicatorPrefab;
    [SerializeField] private MapSettings mapSettings;

    [Header("Test Play UI")]
    [SerializeField] private GameObject testPlayHintPanel;

    [Header("State")]
    [SerializeField] private bool isPlayMode = false;

    // テストプレイ用に生成したオブジェクト
    private GameObject testPlayer;
    private GameObject testGravityController;
    private GameObject testGravityIndicator;
    private GameObject testPlayHint;
    private Vector3 savedCameraPosition;
    private float savedCameraSize;

    public bool IsPlayMode => isPlayMode;

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
        if (placementSystem == null)
            placementSystem = FindObjectOfType<PlacementSystem>();
        if (gridOverlay == null)
            gridOverlay = FindObjectOfType<GridOverlay>();
        if (editorCamera == null)
            editorCamera = FindObjectOfType<EditorCamera>();
    }

    private void Update()
    {
        // Escでテストプレイ終了
        if (isPlayMode && Input.GetKeyDown(KeyCode.Escape))
        {
            StopTestPlay();
        }
    }

    /// <summary>
    /// テストプレイ開始
    /// </summary>
    public void StartTestPlay()
    {
        if (isPlayMode) return;
        if (placementSystem == null) return;

        // スタート位置を探す
        Vector2Int? startPos = null;
        foreach (var kvp in placementSystem.PlacedTiles)
        {
            if (kvp.Value.tileType == 'S')
            {
                startPos = kvp.Key;
                break;
            }
        }

        if (!startPos.HasValue)
        {
            Debug.LogWarning("スタート位置(S)がありません！");
            return;
        }

        isPlayMode = true;

        // カメラ状態を保存
        if (editorCamera != null)
        {
            savedCameraPosition = editorCamera.transform.position;
            savedCameraSize = Camera.main.orthographicSize;
        }

        // エディタUIを無効化
        if (editorCanvas != null)
            editorCanvas.gameObject.SetActive(false);
        if (gridOverlay != null)
            gridOverlay.enabled = false;
        if (editorCamera != null)
            editorCamera.enabled = false;

        // プレビューを非表示
        var preview = GameObject.Find("PlacementPreview");
        if (preview != null)
            preview.SetActive(false);

        // 配置済みオブジェクトを有効化
        EnablePlacedObjects(true);

        // GravityController生成
        if (gravityControllerPrefab != null)
        {
            testGravityController = Instantiate(gravityControllerPrefab);
        }
        else
        {
            testGravityController = new GameObject("TestGravityController");
            testGravityController.AddComponent<GravityController>();
        }

        // GravityIndicatorUI生成
        CreateTestPlayUI();

        // プレイヤーを探して有効化
        foreach (var kvp in placementSystem.PlacedTiles)
        {
            if (kvp.Value.tileType == 'S')
            {
                var character = kvp.Value.gameObject.GetComponent<CharacterBase>();
                if (character != null)
                {
                    character.enabled = true;
                    testPlayer = kvp.Value.gameObject;

                    // Rigidbody2Dの速度をリセット
                    var rb = testPlayer.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.velocity = Vector2.zero;
                    }

                    // カメラをプレイヤーに追従させる
                    var gravityCamera = Camera.main.GetComponent<GravityCamera>();
                    if (gravityCamera == null)
                    {
                        gravityCamera = Camera.main.gameObject.AddComponent<GravityCamera>();
                    }
                    gravityCamera.enabled = true;
                }
                break;
            }
        }

        Debug.Log("テストプレイ開始！ Escで終了");
    }

    /// <summary>
    /// テストプレイ用UIを生成
    /// </summary>
    private void CreateTestPlayUI()
    {
        // 簡易的なUI生成
        var canvas = new GameObject("TestPlayCanvas");
        var canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComp.sortingOrder = 100;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        // ヒントテキスト（英語、標準Text）
        var hintObj = new GameObject("HintText");
        hintObj.transform.SetParent(canvas.transform);
        var hintText = hintObj.AddComponent<Text>();
        hintText.text = "Press ESC to exit";
        hintText.fontSize = 24;
        hintText.alignment = TextAnchor.UpperRight;
        hintText.color = new Color(1, 1, 1, 0.7f);
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var hintRect = hintObj.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(1, 1);
        hintRect.anchorMax = new Vector2(1, 1);
        hintRect.pivot = new Vector2(1, 1);
        hintRect.anchoredPosition = new Vector2(-20, -20);
        hintRect.sizeDelta = new Vector2(250, 40);

        testPlayHint = canvas;

        // GravityIndicatorUIを生成（Canvas内に）
        if (gravityIndicatorPrefab != null)
        {
            testGravityIndicator = Instantiate(gravityIndicatorPrefab, canvas.transform);
        }
    }

    /// <summary>
    /// テストプレイ終了
    /// </summary>
    public void StopTestPlay()
    {
        if (!isPlayMode) return;

        isPlayMode = false;

        // テストプレイUI削除
        if (testPlayHint != null)
        {
            Destroy(testPlayHint);
            testPlayHint = null;
        }
        if (testGravityIndicator != null)
        {
            Destroy(testGravityIndicator);
            testGravityIndicator = null;
        }

        // GravityController削除
        if (testGravityController != null)
        {
            Destroy(testGravityController);
            testGravityController = null;
        }

        // プレイヤーを無効化
        if (testPlayer != null)
        {
            var character = testPlayer.GetComponent<CharacterBase>();
            if (character != null)
            {
                character.enabled = false;
            }

            // Rigidbody2Dの速度をリセット
            var rb = testPlayer.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            // 位置をリセット
            foreach (var kvp in placementSystem.PlacedTiles)
            {
                if (kvp.Value.tileType == 'S')
                {
                    Vector3 startPos = GridSystem.Instance.GridToWorld(kvp.Key);
                    testPlayer.transform.position = startPos;
                    break;
                }
            }
            testPlayer = null;
        }

        // GravityCameraを無効化
        var gravityCamera = Camera.main.GetComponent<GravityCamera>();
        if (gravityCamera != null)
        {
            gravityCamera.enabled = false;
        }

        // 重力をリセット
        Physics2D.gravity = new Vector2(0, -9.81f);

        // スイッチの状態をリセット
        ResetAllSwitches();

        // カメラを復元
        if (editorCamera != null)
        {
            editorCamera.transform.position = savedCameraPosition;
            Camera.main.orthographicSize = savedCameraSize;
            Camera.main.transform.rotation = Quaternion.identity;
        }

        // エディタUIを有効化
        if (editorCanvas != null)
            editorCanvas.gameObject.SetActive(true);
        if (gridOverlay != null)
            gridOverlay.enabled = true;
        if (editorCamera != null)
            editorCamera.enabled = true;

        // プレビューを表示
        var preview = GameObject.Find("PlacementPreview");
        if (preview != null)
            preview.SetActive(true);

        // 配置済みオブジェクトを無効化
        EnablePlacedObjects(false);

        Debug.Log("テストプレイ終了");
    }

    /// <summary>
    /// 配置済みオブジェクトの有効/無効を切り替え
    /// </summary>
    private void EnablePlacedObjects(bool enable)
    {
        if (placementSystem == null) return;

        foreach (var kvp in placementSystem.PlacedTiles)
        {
            var obj = kvp.Value.gameObject;
            if (obj == null) continue;

            // Rigidbody2Dがあれば有効/無効
            var rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = enable;
            }

            // CharacterBaseは個別に制御（プレイヤーのみ有効化）
            var character = obj.GetComponent<CharacterBase>();
            if (character != null)
            {
                character.enabled = enable && kvp.Value.tileType == 'S';
            }

            // Boxは重力シミュレーション
            var box = obj.GetComponent<Box>();
            if (box != null)
            {
                box.enabled = enable;
            }

            // GravitySwitchを有効化
            var gravitySwitch = obj.GetComponent<GravitySwitch>();
            if (gravitySwitch != null)
            {
                gravitySwitch.enabled = enable;
            }

            // Goalを有効化
            var goal = obj.GetComponent<Goal>();
            if (goal != null)
            {
                goal.enabled = enable;
            }
        }
    }

    /// <summary>
    /// 全スイッチの状態をリセット
    /// </summary>
    private void ResetAllSwitches()
    {
        if (placementSystem == null) return;

        foreach (var kvp in placementSystem.PlacedTiles)
        {
            var obj = kvp.Value.gameObject;
            if (obj == null) continue;

            var gravitySwitch = obj.GetComponent<GravitySwitch>();
            if (gravitySwitch != null)
            {
                // スイッチをOFFに戻す
                gravitySwitch.ResetSwitch();
            }
        }
    }
}

