using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ステージ管理 - ステージの進行状況を管理
/// 動的マップ生成システム対応
/// </summary>
public static class StageManager
{
    private const string CURRENT_STAGE_KEY = "CurrentStage";
    private const string CURRENT_STAGE_ID_KEY = "CurrentStageId";
    private const string MAX_CLEARED_STAGE_KEY = "MaxClearedStage";
    private const string LAST_PLAYED_SCENE_KEY = "LastPlayedScene";

    /// <summary>
    /// 動的生成用のゲームシーン名
    /// </summary>
    public const string GAME_SCENE = "GameScene";

    /// <summary>
    /// 現在のステージID（動的生成用）
    /// </summary>
    public static string CurrentStageId
    {
        get => PlayerPrefs.GetString(CURRENT_STAGE_ID_KEY, "stage_001");
        set
        {
            PlayerPrefs.SetString(CURRENT_STAGE_ID_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 現在のステージデータ（メモリ上にキャッシュ）
    /// </summary>
    public static StageData CurrentStageData { get; set; }

    /// <summary>
    /// 最後にプレイしたシーン名（GameOver/Clear画面からのリトライ用）
    /// </summary>
    public static string LastPlayedScene
    {
        get => PlayerPrefs.GetString(LAST_PLAYED_SCENE_KEY, GAME_SCENE);
        set
        {
            PlayerPrefs.SetString(LAST_PLAYED_SCENE_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 現在のステージ番号（1から開始）- レガシー互換用
    /// </summary>
    public static int CurrentStage
    {
        get => PlayerPrefs.GetInt(CURRENT_STAGE_KEY, 1);
        set => PlayerPrefs.SetInt(CURRENT_STAGE_KEY, value);
    }

    /// <summary>
    /// クリア済みの最大ステージ番号
    /// </summary>
    public static int MaxClearedStage
    {
        get => PlayerPrefs.GetInt(MAX_CLEARED_STAGE_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(MAX_CLEARED_STAGE_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// ステージシーン名のプレフィックス - レガシー互換用
    /// </summary>
    public const string STAGE_PREFIX = "Stage";

    /// <summary>
    /// 総ステージ数（Build Settingsに登録されているステージ数）- レガシー互換用
    /// </summary>
    public static int TotalStages { get; set; } = 5;

    /// <summary>
    /// ステージ番号からシーン名を取得 - レガシー互換用
    /// </summary>
    public static string GetStageSceneName(int stageNumber)
    {
        return $"{STAGE_PREFIX}{stageNumber}";
    }

    /// <summary>
    /// 現在のステージをクリア
    /// </summary>
    public static void ClearCurrentStage()
    {
        if (CurrentStageData != null && CurrentStageData.isBuiltIn)
        {
            if (CurrentStageData.stageNumber > MaxClearedStage)
            {
                MaxClearedStage = CurrentStageData.stageNumber;
            }
        }
        else if (CurrentStage > MaxClearedStage)
        {
            MaxClearedStage = CurrentStage;
        }
    }

    /// <summary>
    /// 次のステージがあるか（動的生成版）
    /// </summary>
    public static bool HasNextStage()
    {
        if (CurrentStageData != null && CurrentStageData.isBuiltIn && StageDatabase.Instance != null)
        {
            var nextStage = StageDatabase.Instance.GetNextBuiltInStage(CurrentStageData.id);
            return nextStage != null;
        }
        return CurrentStage < TotalStages;
    }

    /// <summary>
    /// 次のステージへ進む（動的生成版）
    /// </summary>
    public static void GoToNextStage()
    {
        if (CurrentStageData != null && CurrentStageData.isBuiltIn && StageDatabase.Instance != null)
        {
            var nextStage = StageDatabase.Instance.GetNextBuiltInStage(CurrentStageData.id);
            if (nextStage != null)
            {
                GoToStage(nextStage);
                return;
            }
        }

        // レガシー互換
        if (HasNextStage())
        {
            CurrentStage++;
            SceneManager.LoadScene(GetStageSceneName(CurrentStage));
        }
    }

    /// <summary>
    /// 現在のステージをリトライ
    /// </summary>
    public static void RetryCurrentStage()
    {
        // 動的生成の場合はGameSceneをリロード
        if (CurrentStageData != null)
        {
            SceneManager.LoadScene(GAME_SCENE);
        }
        else
        {
            SceneManager.LoadScene(LastPlayedScene);
        }
    }

    /// <summary>
    /// 現在のステージをリトライ
    /// </summary>
    public static void Retry()
    {
        RetryCurrentStage();
    }

    /// <summary>
    /// ステージIDでステージへ移動（動的生成版）
    /// </summary>
    public static void GoToStage(string stageId)
    {
        if (StageDatabase.Instance != null)
        {
            var stageData = StageDatabase.Instance.GetStage(stageId);
            if (stageData != null)
            {
                GoToStage(stageData);
                return;
            }
        }

        Debug.LogError($"Stage not found: {stageId}");
    }

    /// <summary>
    /// ステージデータでステージへ移動（動的生成版）
    /// </summary>
    public static void GoToStage(StageData stageData)
    {
        CurrentStageId = stageData.id;
        CurrentStageData = stageData;
        CurrentStage = stageData.stageNumber;
        SceneManager.LoadScene(GAME_SCENE);
    }

    /// <summary>
    /// 特定のステージへ移動 - レガシー互換用
    /// </summary>
    public static void GoToStage(int stageNumber)
    {
        // 動的生成版を優先
        if (StageDatabase.Instance != null)
        {
            var stages = StageDatabase.Instance.GetBuiltInStages();
            var stage = stages.Find(s => s.stageNumber == stageNumber);
            if (stage != null)
            {
                GoToStage(stage);
                return;
            }
        }

        // レガシー互換
        if (stageNumber >= 1 && stageNumber <= TotalStages)
        {
            CurrentStage = stageNumber;
            SceneManager.LoadScene(GetStageSceneName(stageNumber));
        }
    }

    /// <summary>
    /// 進行状況をリセット
    /// </summary>
    public static void ResetProgress()
    {
        CurrentStage = 1;
        CurrentStageId = "stage_001";
        CurrentStageData = null;
        MaxClearedStage = 0;
        PlayerPrefs.Save();
    }
}
