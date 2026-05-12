using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ステージセレクト画面のリスト生成
/// </summary>
public class StageSelectUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform contentParent; // ScrollViewのContent
    [SerializeField] private GameObject stageButtonPrefab; // ボタンのプレハブ
    [SerializeField] private string titleSceneName = "TitleScene";

    private void Start()
    {
        GenerateStageList();
    }

    private void GenerateStageList()
    {
        // 既存のボタンをクリア（もしあれば）
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (StageDatabase.Instance == null)
        {
            Debug.LogError("StageDatabase not found!");
            return;
        }

        var stages = StageDatabase.Instance.GetAllStages();

        foreach (var stage in stages)
        {
            // ボタン生成
            GameObject btnObj = Instantiate(stageButtonPrefab, contentParent);
            btnObj.name = $"Btn_{stage.id}";

            // テキスト設定
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                // クリア状況に応じて表示を変える？（今はシンプルに）
                btnText.text = stage.name;
            }

            Button btn = btnObj.GetComponent<Button>();
            string stageId = stage.id; // ローカル変数にキャプチャ

            btn.onClick.AddListener(() => OnStageClicked(stageId));
        }
    }

    private void OnStageClicked(string stageId)
    {
        StageManager.GoToStage(stageId);
    }

    public void OnBackClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(titleSceneName);
    }
}
