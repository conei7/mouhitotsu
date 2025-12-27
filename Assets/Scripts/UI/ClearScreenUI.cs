using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// クリア画面UI - 次のステージへ進むか、タイトルに戻るか選択
/// </summary>
public class ClearScreenUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject nextStageButton;

    [Header("Scene Names")]
    [SerializeField] private string titleSceneName = "TitleScene";

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        int currentStage = StageManager.CurrentStage;
        bool hasNextStage = StageManager.HasNextStage();

        // ステージ番号表示
        if (stageText != null)
        {
            stageText.text = $"Stage {currentStage}";
        }

        // メッセージ表示
        if (messageText != null)
        {
            if (hasNextStage)
            {
                messageText.text = "CLEAR!";
            }
            else
            {
                messageText.text = "ALL STAGES CLEAR!\nおめでとう！";
            }
        }

        // 次のステージボタンの表示/非表示
        if (nextStageButton != null)
        {
            nextStageButton.SetActive(hasNextStage);
        }
    }

    /// <summary>
    /// 次のステージへ進むボタン
    /// </summary>
    public void OnNextStageClick()
    {
        StageManager.GoToNextStage();
    }

    /// <summary>
    /// 現在のステージをリトライ
    /// </summary>
    public void OnRetryClick()
    {
        StageManager.RetryCurrentStage();
    }

    /// <summary>
    /// タイトルへ戻るボタン
    /// </summary>
    public void OnTitleClick()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}
