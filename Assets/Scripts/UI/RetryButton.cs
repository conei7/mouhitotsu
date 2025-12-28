using UnityEngine;

/// <summary>
/// リトライボタン
/// </summary>
public class RetryButton : MonoBehaviour
{
    public void OnRetryClick()
    {
        StageManager.RetryCurrentStage();
    }
}
