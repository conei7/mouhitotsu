using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;

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

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void CopyToClipboard(string text);

    [DllImport("__Internal")]
    private static extern void PasteFromClipboard(string gameObjectName, string methodName);
#endif

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
        placementSystem?.ClearAll();
    }

    private void OnSaveClick()
    {
        if (placementSystem == null) return;

        string mapText = placementSystem.ToText();
        if (string.IsNullOrEmpty(mapText))
        {
            Debug.Log("No tiles to save");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        CopyToClipboard(mapText);
#else
        GUIUtility.systemCopyBuffer = mapText;
#endif
        Debug.Log("Map copied to clipboard!");
    }

    private void OnLoadClick()
    {
        if (placementSystem == null) return;

#if UNITY_WEBGL && !UNITY_EDITOR
        PasteFromClipboard(gameObject.name, "OnClipboardPaste");
#else
        string mapText = GUIUtility.systemCopyBuffer;
        LoadMapFromText(mapText);
#endif
    }

    // WebGLからのコールバック
    public void OnClipboardPaste(string mapText)
    {
        LoadMapFromText(mapText);
    }

    private void LoadMapFromText(string mapText)
    {
        if (string.IsNullOrEmpty(mapText))
        {
            Debug.Log("Clipboard is empty");
            return;
        }

        if (!mapText.Contains("#") && !mapText.Contains("S"))
        {
            Debug.Log("No valid map data in clipboard");
            return;
        }

        placementSystem.LoadFromText(mapText);
        Debug.Log("Map loaded from clipboard!");
    }

    private void OnTestPlayClick()
    {
        if (EditorManager.Instance != null)
        {
            if (EditorManager.Instance.IsPlayMode)
            {
                EditorManager.Instance.StopTestPlay();
            }
            else
            {
                EditorManager.Instance.StartTestPlay();
            }
        }
        else
        {
            Debug.LogWarning("EditorManager not found");
        }
    }

    private void OnTitleClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
    }
}
