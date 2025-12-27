using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ステージ管理 - ステージの進行状況を管理
/// </summary>
public static class StageManager
{
    private const string CURRENT_STAGE_KEY = "CurrentStage";
    private const string MAX_CLEARED_STAGE_KEY = "MaxClearedStage";

    /// <summary>
    /// 現在のステージ番号（1から開始）
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
    /// ステージシーン名のプレフィックス
    /// </summary>
    public const string STAGE_PREFIX = "Stage";

    /// <summary>
    /// 総ステージ数（Build Settingsに登録されているステージ数）
    /// </summary>
    public static int TotalStages { get; set; } = 5;  // デフォルト5ステージ

    /// <summary>
    /// ステージ番号からシーン名を取得
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
        if (CurrentStage > MaxClearedStage)
        {
            MaxClearedStage = CurrentStage;
        }
    }

    /// <summary>
    /// 次のステージがあるか
    /// </summary>
    public static bool HasNextStage()
    {
        return CurrentStage < TotalStages;
    }

    /// <summary>
    /// 次のステージへ進む
    /// </summary>
    public static void GoToNextStage()
    {
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
        SceneManager.LoadScene(GetStageSceneName(CurrentStage));
    }

    /// <summary>
    /// 特定のステージへ移動
    /// </summary>
    public static void GoToStage(int stageNumber)
    {
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
        MaxClearedStage = 0;
        PlayerPrefs.Save();
    }
}
