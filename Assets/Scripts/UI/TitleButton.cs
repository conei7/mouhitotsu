using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// タイトル画面 - Startボタンにアタッチしてください
/// </summary>
public class TitleButton : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    public void OnStartClick()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
