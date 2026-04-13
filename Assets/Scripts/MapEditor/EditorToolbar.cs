using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    private static extern void DownloadTextFile(string fileName, string text);

    [DllImport("__Internal")]
    private static extern void OpenTextFilePicker(string gameObjectName, string methodName);
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

            string compressedText = MapTextCodec.EncodeIfSmaller(mapText);
        string fileName = BuildDefaultFileName();

#if UNITY_WEBGL && !UNITY_EDITOR
    DownloadTextFile(fileName, compressedText);
    Debug.Log($"Map download started: {fileName}");
#elif UNITY_EDITOR
    SaveWithDialog(fileName, compressedText);
#else
    SaveTextToFile(fileName, compressedText);
#endif
    }

    private void OnLoadClick()
    {
        if (placementSystem == null) return;

#if UNITY_WEBGL && !UNITY_EDITOR
        OpenTextFilePicker(gameObject.name, "OnFileSelected");
#else
        OpenLoadDialog();
#endif
    }

    // WebGLからのコールバック
    public void OnFileSelected(string mapText)
    {
        LoadMapFromText(mapText);
    }

#if UNITY_EDITOR
    private void OpenLoadDialog()
    {
        string path = EditorUtility.OpenFilePanel("マップファイルを選択", "", "txt,map");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            string mapText = File.ReadAllText(path);
            LoadMapFromText(mapText);
            Debug.Log($"Map loaded from file: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load map file: {e.Message}");
        }
    }
#else
    private void OpenLoadDialog()
    {
        string mapText = GUIUtility.systemCopyBuffer;
        LoadMapFromText(mapText);
    }
#endif

    private void SaveTextToFile(string fileName, string text)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            File.WriteAllText(path, text);
            Debug.Log($"Map saved to: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save map file: {e.Message}");
        }
    }

#if UNITY_EDITOR
    private void SaveWithDialog(string fileName, string text)
    {
        string path = EditorUtility.SaveFilePanel("マップを保存", "", fileName, "txt");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            File.WriteAllText(path, text);
            Debug.Log($"Map saved to: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save map file: {e.Message}");
        }
    }
#endif

    private string BuildDefaultFileName()
    {
        return $"mouhitotsu_map_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
    }

    private void LoadMapFromText(string mapText)
    {
        if (string.IsNullOrEmpty(mapText))
        {
            Debug.Log("No map data provided");
            return;
        }

        string decodedMapText = MapTextCodec.DecodeIfNeeded(mapText);

        if (!decodedMapText.Contains("#") && !decodedMapText.Contains("S"))
        {
            Debug.Log("No valid map data in clipboard");
            return;
        }

        placementSystem.LoadFromText(decodedMapText);
        Debug.Log("Map loaded!");
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
