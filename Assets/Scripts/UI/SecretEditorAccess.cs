using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// エディタシーンへ移動するヘルパー
/// ボタンから呼び出して使用
/// </summary>
public class SecretEditorAccess : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string editorSceneName = "EditorScene";

    public void GoToEditor()
    {
        Debug.Log("Loading Editor...");
        SceneManager.LoadScene(editorSceneName);
    }
}
