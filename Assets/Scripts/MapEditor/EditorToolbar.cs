using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// エディタツールバー - 各種ボタンと情報表示
/// </summary>
public class EditorToolbar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField] private ObjectPalette objectPalette;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI selectedItemText;
    [SerializeField] private TextMeshProUGUI tileCountText;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button testPlayButton;
    [SerializeField] private Button titleButton;

    private void Start()
    {
        if (placementSystem == null)
            placementSystem = FindObjectOfType<PlacementSystem>();
        if (objectPalette == null)
            objectPalette = FindObjectOfType<ObjectPalette>();

        SetupButtons();
    }

    private void Update()
    {
        UpdateInfo();
    }

    private void SetupButtons()
    {
        if (clearButton != null)
            clearButton.onClick.AddListener(OnClearClick);
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveClick);
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadClick);
        if (testPlayButton != null)
            testPlayButton.onClick.AddListener(OnTestPlayClick);
        if (titleButton != null)
            titleButton.onClick.AddListener(OnTitleClick);
    }

    private void UpdateInfo()
    {
        if (selectedItemText != null && objectPalette != null)
        {
            selectedItemText.text = $"選択: {objectPalette.GetSelectedItemName()}";
        }

        if (tileCountText != null && placementSystem != null)
        {
            tileCountText.text = $"タイル数: {placementSystem.PlacedTiles.Count}";
        }
    }

    private void OnClearClick()
    {
        // 確認ダイアログなしで削除（後で追加可能）
        placementSystem?.ClearAll();
    }

    private void OnSaveClick()
    {
        if (placementSystem == null) return;

        string mapText = placementSystem.ToText();
        if (string.IsNullOrEmpty(mapText))
        {
            Debug.Log("保存するタイルがありません");
            return;
        }

        // クリップボードにコピー
        GUIUtility.systemCopyBuffer = mapText;
        Debug.Log("マップをクリップボードにコピーしました！\nテキストエディタに貼り付けて保存できます。");
    }

    private void OnLoadClick()
    {
        if (placementSystem == null) return;

        // クリップボードから読み込み
        string mapText = GUIUtility.systemCopyBuffer;
        if (string.IsNullOrEmpty(mapText))
        {
            Debug.Log("クリップボードが空です");
            return;
        }

        // マップデータっぽいかチェック（#か空白を含む）
        if (!mapText.Contains("#") && !mapText.Contains("S"))
        {
            Debug.Log("クリップボードにマップデータがありません");
            return;
        }

        placementSystem.LoadFromText(mapText);
        Debug.Log("クリップボードからマップを読み込みました！");
    }

    private void OnTestPlayClick()
    {
        Debug.Log("テストプレイ機能は未実装です");
        // TODO: Phase 4で実装
    }

    private void OnTitleClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
    }
}
