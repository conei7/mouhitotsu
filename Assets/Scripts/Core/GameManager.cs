using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の状態を管理
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Characters")]
    [SerializeField] private CharacterBlue blueCharacter;
    [SerializeField] private CharacterRed redCharacter;

    [Header("Scenes")]
    [SerializeField] private string clearSceneName = "ClearScene";
    [SerializeField] private string gameOverSceneName = "GameOverScene";
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string titleSceneName = "TitleScene";

    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (isGameOver) return;

        // リトライ（いつでも可能）
        if (Input.GetKeyDown(KeyCode.R))
        {
            Retry();
            return;
        }

        // ゴール判定
        if (blueCharacter != null && redCharacter != null)
        {
            if (blueCharacter.IsInGoal && redCharacter.IsInGoal)
            {
                GameClear();
            }
        }
    }

    public void OnCharacterDied()
    {
        if (isGameOver) return;
        isGameOver = true;
        SceneManager.LoadScene(gameOverSceneName);
    }

    public void GameClear()
    {
        if (isGameOver) return;
        isGameOver = true;
        SceneManager.LoadScene(clearSceneName);
    }

    public void Retry()
    {
        isGameOver = false;
        SceneManager.LoadScene(gameSceneName);
    }

    public void GoToTitle()
    {
        isGameOver = false;
        SceneManager.LoadScene(titleSceneName);
    }
}
