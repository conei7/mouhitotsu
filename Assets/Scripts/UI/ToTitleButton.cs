using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトルへ戻るボタン
/// </summary>
public class ToTitleButton : MonoBehaviour
{
    [SerializeField] private string titleSceneName = "TitleScene";

    public void OnTitleClick()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}
