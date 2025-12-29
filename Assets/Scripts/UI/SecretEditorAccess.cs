using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 秘密のコマンドでエディタシーンに移動
/// TitleSceneの任意のオブジェクトにアタッチ
/// </summary>
public class SecretEditorAccess : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string editorSceneName = "EditorScene";

    private void Update()
    {
        // Shift + G + ; でエディタシーンに移動
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool g = Input.GetKey(KeyCode.G);
        bool semicolon = Input.GetKey(KeyCode.Semicolon);

        if (shift && g && semicolon)
        {
            GoToEditor();
        }
    }

    private void GoToEditor()
    {
        Debug.Log("Secret command activated! Loading Editor...");
        SceneManager.LoadScene(editorSceneName);
    }
}
