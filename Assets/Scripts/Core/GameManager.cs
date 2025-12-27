using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の状態を管理（シーン間で持続するシングルトン）
/// TitleSceneに配置するだけでOK
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Stage Settings")]
    [SerializeField] private int totalStages = 5;

    [Header("Scene Names")]
    [SerializeField] private string clearSceneName = "ClearScene";
    [SerializeField] private string gameOverSceneName = "GameOverScene";
    [SerializeField] private string titleSceneName = "TitleScene";

    private bool isGameOver = false;
    private CharacterBase currentPlayer;

    /// <summary>
    /// 現在のステージ番号
    /// </summary>
    public int CurrentStage => StageManager.CurrentStage;

    /// <summary>
    /// 次のステージがあるか
    /// </summary>
    public bool HasNextStage => StageManager.HasNextStage();

    private void Awake()
    {
        // シングルトン（シーン間で持続）
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 総ステージ数を設定
        StageManager.TotalStages = totalStages;
        
        // シーン読み込み時のイベント登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 新しいシーンでプレイヤーを探す
        currentPlayer = FindObjectOfType<CharacterBase>();
        isGameOver = false;
    }

    private void Update()
    {
        if (isGameOver) return;
        if (currentPlayer == null) return;

        // リトライ（いつでも可能）
        if (Input.GetKeyDown(KeyCode.R))
        {
            Retry();
            return;
        }

        // ゴール判定
        if (currentPlayer.IsInGoal)
        {
            GameClear();
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
        
        // ステージクリア記録
        StageManager.ClearCurrentStage();
        
        SceneManager.LoadScene(clearSceneName);
    }

    public void Retry()
    {
        isGameOver = false;
        StageManager.RetryCurrentStage();
    }

    public void GoToNextStage()
    {
        isGameOver = false;
        StageManager.GoToNextStage();
    }

    public void GoToTitle()
    {
        isGameOver = false;
        SceneManager.LoadScene(titleSceneName);
    }

    /// <summary>
    /// ステージを選択して開始
    /// </summary>
    public void StartStage(int stageNumber)
    {
        isGameOver = false;
        StageManager.GoToStage(stageNumber);
    }

    /// <summary>
    /// ステージ1から開始
    /// </summary>
    public void StartGame()
    {
        StageManager.CurrentStage = 1;
        StageManager.GoToStage(1);
    }
}
