using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// リトライボタン
/// </summary>
public class RetryButton : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    public void OnRetryClick()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
