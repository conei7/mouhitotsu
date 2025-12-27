using UnityEngine;

/// <summary>
/// タイトル画面 - Startボタンにアタッチしてください
/// </summary>
public class TitleButton : MonoBehaviour
{
    public void OnStartClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            // GameManagerがない場合はStageManagerを直接使用
            StageManager.CurrentStage = 1;
            StageManager.GoToStage(1);
        }
    }

    /// <summary>
    /// 続きから開始（最後にクリアしたステージの次から）
    /// </summary>
    public void OnContinueClick()
    {
        int nextStage = StageManager.MaxClearedStage + 1;
        if (nextStage > StageManager.TotalStages)
        {
            nextStage = StageManager.TotalStages;
        }
        StageManager.GoToStage(nextStage);
    }
}
