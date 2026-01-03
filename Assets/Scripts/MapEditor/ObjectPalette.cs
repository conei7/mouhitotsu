using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// オブジェクトパレット - 配置するオブジェクトを選択
/// </summary>
public class ObjectPalette : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlacementSystem placementSystem;

    [Header("Button Template")]
    [SerializeField] private GameObject buttonTemplate;
    [SerializeField] private Transform buttonContainer;

    [Header("Channel Selector")]
    [SerializeField] private Transform channelContainer;
    [Tooltip("チャンネル選択ボタンの親パネル（背景含む全体を消すために使用）")]
    [SerializeField] private GameObject channelControlPanel;

    [Header("Visual Settings")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    // パレットアイテム定義
    private readonly List<PaletteItem> items = new List<PaletteItem>
    {
        new PaletteItem { symbol = '#', name = "壁", shortcut = KeyCode.Alpha1 },
        new PaletteItem { symbol = 'S', name = "プレイヤー", shortcut = KeyCode.Alpha2 },
        new PaletteItem { symbol = 'G', name = "ゴール", shortcut = KeyCode.Alpha3 },
        new PaletteItem { symbol = '^', name = "↑スイッチ", shortcut = KeyCode.Alpha4 },
        new PaletteItem { symbol = 'v', name = "↓スイッチ", shortcut = KeyCode.Alpha5 },
        new PaletteItem { symbol = '<', name = "←スイッチ", shortcut = KeyCode.Alpha6 },
        new PaletteItem { symbol = '>', name = "→スイッチ", shortcut = KeyCode.Alpha7 },
        new PaletteItem { symbol = 'B', name = "箱", shortcut = KeyCode.Alpha8 },
        new PaletteItem { symbol = 'X', name = "トゲ", shortcut = KeyCode.Alpha9 },
        new PaletteItem { symbol = '!', name = "ボタン", shortcut = KeyCode.Alpha0, isChannelBased = true },
        new PaletteItem { symbol = '@', name = "切替壁", shortcut = KeyCode.Q, isChannelBased = true },
    };

    private List<Button> buttons = new List<Button>();
    private List<Button> channelButtons = new List<Button>();
    private int selectedIndex = 0;
    private int currentChannel = 0;
    private bool isShiftHeld = false;

    public int CurrentChannel => currentChannel;

    private void Start()
    {
        if (placementSystem == null)
        {
            placementSystem = FindObjectOfType<PlacementSystem>();
        }

        CreateButtons();
        CreateChannelButtons();
        SelectItem(0);
    }

    private void Update()
    {
        HandleShortcuts();
        HandleChannelShortcuts();
        HandleShiftKey();
    }

    private void CreateButtons()
    {
        if (buttonTemplate == null || buttonContainer == null) return;

        buttonTemplate.SetActive(false);

        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            PaletteItem item = items[i];

            GameObject btnObj = Instantiate(buttonTemplate, buttonContainer);
            btnObj.SetActive(true);
            btnObj.name = $"Btn_{item.name}";

            // ボタンテキスト設定
            Text text = btnObj.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = item.name;
            }

            TMPro.TextMeshProUGUI tmpText = btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = item.name;
            }

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SelectItem(index));
                buttons.Add(btn);
            }
        }
    }

    private void CreateChannelButtons()
    {
        if (channelContainer == null || buttonTemplate == null) return;

        for (int i = 0; i < ChannelColors.ColorCount; i++)
        {
            int channel = i;

            GameObject btnObj = Instantiate(buttonTemplate, channelContainer);
            btnObj.SetActive(true);
            btnObj.name = $"Channel_{i}";

            // サイズを40x40に固定（LayoutElementを追加）
            var layoutElement = btnObj.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = btnObj.AddComponent<UnityEngine.UI.LayoutElement>();
            }
            layoutElement.preferredWidth = 40;
            layoutElement.preferredHeight = 40;
            layoutElement.minWidth = 40;
            layoutElement.minHeight = 40;

            // テキストを非表示（色だけで区別）
            Text text = btnObj.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = "";
            }

            TMPro.TextMeshProUGUI tmpText = btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = "";
            }

            // チャンネル色を背景に適用
            var image = btnObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = ChannelColors.GetColor(channel);
            }

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SelectChannel(channel));
                channelButtons.Add(btn);
            }
        }

        UpdateChannelButtonVisuals();
    }

    private void HandleShortcuts()
    {
        // ショートカットは無効化（ユーザーリクエスト）
        // 必要に応じて再有効化可能
    }

    private void HandleChannelShortcuts()
    {
        // [ ] キーでチャンネル変更
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            SelectChannel((currentChannel - 1 + ChannelColors.ColorCount) % ChannelColors.ColorCount);
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            SelectChannel((currentChannel + 1) % ChannelColors.ColorCount);
        }
    }

    private void HandleShiftKey()
    {
        // Shiftキーの状態を監視（切替壁の初期ON/OFF切替用）
        bool shiftNow = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (shiftNow != isShiftHeld)
        {
            isShiftHeld = shiftNow;
            // 切替壁が選択中ならシンボルを更新
            if (selectedIndex >= 0 && selectedIndex < items.Count && items[selectedIndex].symbol == '@')
            {
                char symbol = GetChannelSymbol('@', currentChannel);
                placementSystem?.SetTileType(symbol);
            }
        }
    }

    /// <summary>
    /// アイテムを選択
    /// </summary>
    public void SelectItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        selectedIndex = index;
        PaletteItem item = items[index];

        // チャンネルベースのアイテムの場合、実際のシンボルを計算
        char symbol = item.symbol;
        if (item.isChannelBased)
        {
            symbol = GetChannelSymbol(item.symbol, currentChannel);
        }

        placementSystem?.SetTileType(symbol);

        // ボタンの色を更新
        for (int i = 0; i < buttons.Count; i++)
        {
            ColorBlock colors = buttons[i].colors;
            Color targetColor = (i == selectedIndex) ? selectedColor : normalColor;
            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.selectedColor = targetColor;
            buttons[i].colors = colors;

            var image = buttons[i].GetComponent<Image>();
            if (image != null)
            {
                image.color = targetColor;
            }
        }

        // チャンネルセレクターの表示切替
        UpdateChannelSelectorVisibility();
    }

    /// <summary>
    /// チャンネルを選択
    /// </summary>
    public void SelectChannel(int channel)
    {
        currentChannel = Mathf.Clamp(channel, 0, ChannelColors.ColorCount - 1);
        
        // PlacementSystemにチャンネルを通知
        placementSystem?.SetCurrentChannel(currentChannel);

        // 現在のアイテムがチャンネルベースなら、シンボルを更新
        if (selectedIndex >= 0 && selectedIndex < items.Count && items[selectedIndex].isChannelBased)
        {
            char symbol = GetChannelSymbol(items[selectedIndex].symbol, currentChannel);
            placementSystem?.SetTileType(symbol);
        }

        UpdateChannelButtonVisuals();
    }

    private void UpdateChannelButtonVisuals()
    {
        for (int i = 0; i < channelButtons.Count; i++)
        {
            var image = channelButtons[i].GetComponent<Image>();
            if (image != null)
            {
                Color c = ChannelColors.GetColor(i);
                if (i == currentChannel)
                {
                    // 選択中は明るく＋枠
                    image.color = c;
                    var outline = channelButtons[i].GetComponent<Outline>();
                    if (outline == null)
                    {
                        outline = channelButtons[i].gameObject.AddComponent<Outline>();
                    }
                    outline.effectColor = Color.white;
                    outline.effectDistance = new Vector2(3, 3);
                    outline.enabled = true;
                }
                else
                {
                    // 非選択は少し暗め
                    image.color = new Color(c.r * 0.7f, c.g * 0.7f, c.b * 0.7f, c.a);
                    var outline = channelButtons[i].GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }
                }
            }
        }
    }

    private void UpdateChannelSelectorVisibility()
    {
        // チャンネルベースのアイテムが選択されている時のみ表示
        bool showChannels = selectedIndex >= 0 && selectedIndex < items.Count && items[selectedIndex].isChannelBased;

        // パネル全体が設定されていればそれを制御、なければコンテナのみ制御
        if (channelControlPanel != null)
        {
            channelControlPanel.SetActive(showChannels);
        }
        else if (channelContainer != null)
        {
            channelContainer.gameObject.SetActive(showChannels);
        }
    }

    /// <summary>
    /// 選択中のアイテム名を取得
    /// </summary>
    public string GetSelectedItemName()
    {
        if (selectedIndex >= 0 && selectedIndex < items.Count)
        {
            PaletteItem item = items[selectedIndex];
            if (item.isChannelBased)
            {
                // 切替壁の場合はShift状態も表示
                if (item.symbol == '@')
                {
                    string state = isShiftHeld ? "OFF" : "ON";
                    return $"{item.name} (CH{currentChannel}/{state})";
                }
                return $"{item.name} (CH{currentChannel})";
            }
            return item.name;
        }
        return "";
    }

    /// <summary>
    /// チャンネルベースのシンボルを取得
    /// </summary>
    private char GetChannelSymbol(char baseSymbol, int channel)
    {
        if (baseSymbol == '!')
        {
            // ボタン: 0-9
            return (char)('0' + channel);
        }
        else if (baseSymbol == '@')
        {
            // 切替壁: Shiftで初期OFF (k-t)、通常は初期ON (a-j)
            if (isShiftHeld)
            {
                return (char)('k' + channel); // 初期OFF
            }
            else
            {
                return (char)('a' + channel); // 初期ON
            }
        }
        return baseSymbol;
    }
}

/// <summary>
/// パレットアイテム
/// </summary>
[System.Serializable]
public class PaletteItem
{
    public char symbol;
    public string name;
    public KeyCode shortcut;
    public bool isChannelBased = false;
}
