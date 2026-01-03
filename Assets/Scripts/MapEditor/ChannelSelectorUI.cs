using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// チャンネルセレクターUI
/// ボタンと切替壁のチャンネルを選択するためのUI
/// </summary>
public class ChannelSelectorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPalette objectPalette;
    [SerializeField] private Transform channelButtonContainer;
    [SerializeField] private TextMeshProUGUI channelLabel;
    [SerializeField] private GameObject channelButtonPrefab;

    [Header("Navigation")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    private Button[] channelButtons;
    private int currentChannel = 0;

    private void Start()
    {
        if (objectPalette == null)
        {
            objectPalette = FindObjectOfType<ObjectPalette>();
        }

        CreateChannelButtons();
        SetupNavigationButtons();
        UpdateVisuals();
    }

    private void CreateChannelButtons()
    {
        if (channelButtonContainer == null || channelButtonPrefab == null) return;

        channelButtons = new Button[ChannelColors.ColorCount];

        for (int i = 0; i < ChannelColors.ColorCount; i++)
        {
            int channel = i;
            GameObject btnObj = Instantiate(channelButtonPrefab, channelButtonContainer);
            btnObj.SetActive(true);
            btnObj.name = $"ChannelBtn_{i}";

            // テキスト設定
            TextMeshProUGUI tmpText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = i.ToString();
            }

            Text legacyText = btnObj.GetComponentInChildren<Text>();
            if (legacyText != null)
            {
                legacyText.text = i.ToString();
            }

            // ボタン設定
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SelectChannel(channel));
                channelButtons[i] = btn;
            }

            // 背景色設定
            Image image = btnObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = ChannelColors.GetColor(i);
            }
        }
    }

    private void SetupNavigationButtons()
    {
        if (prevButton != null)
        {
            prevButton.onClick.AddListener(() => NavigateChannel(-1));
        }
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(() => NavigateChannel(1));
        }
    }

    public void SelectChannel(int channel)
    {
        currentChannel = Mathf.Clamp(channel, 0, ChannelColors.ColorCount - 1);
        objectPalette?.SelectChannel(currentChannel);
        UpdateVisuals();
    }

    public void NavigateChannel(int delta)
    {
        int newChannel = (currentChannel + delta + ChannelColors.ColorCount) % ChannelColors.ColorCount;
        SelectChannel(newChannel);
    }

    private void UpdateVisuals()
    {
        // ラベル更新
        if (channelLabel != null)
        {
            channelLabel.text = $"CH {currentChannel}: {ChannelColors.GetChannelName(currentChannel)}";
            channelLabel.color = ChannelColors.GetColor(currentChannel);
        }

        // ボタンの選択状態を更新
        if (channelButtons == null) return;

        for (int i = 0; i < channelButtons.Length; i++)
        {
            if (channelButtons[i] == null) continue;

            Image image = channelButtons[i].GetComponent<Image>();
            if (image != null)
            {
                Color c = ChannelColors.GetColor(i);
                if (i == currentChannel)
                {
                    // 選択中: フル表示
                    image.color = c;
                    
                    // 枠線追加
                    Outline outline = channelButtons[i].GetComponent<Outline>();
                    if (outline == null)
                    {
                        outline = channelButtons[i].gameObject.AddComponent<Outline>();
                    }
                    outline.effectColor = Color.white;
                    outline.effectDistance = new Vector2(2, 2);
                    outline.enabled = true;
                }
                else
                {
                    // 非選択: 暗め
                    image.color = new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f, 1f);
                    
                    Outline outline = channelButtons[i].GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }
                }
            }
        }
    }

    private void Update()
    {
        // キーボードショートカット
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            NavigateChannel(-1);
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            NavigateChannel(1);
        }
    }
}
