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

    [Header("Visual Settings")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    // パレットアイテム定義
    private readonly List<PaletteItem> items = new List<PaletteItem>
    {
        new PaletteItem { symbol = '#', name = "壁", shortcut = KeyCode.Alpha1 },
        new PaletteItem { symbol = 'S', name = "スタート", shortcut = KeyCode.Alpha2 },
        new PaletteItem { symbol = 'G', name = "ゴール", shortcut = KeyCode.Alpha3 },
        new PaletteItem { symbol = '^', name = "↑スイッチ", shortcut = KeyCode.Alpha4 },
        new PaletteItem { symbol = 'v', name = "↓スイッチ", shortcut = KeyCode.Alpha5 },
        new PaletteItem { symbol = '<', name = "←スイッチ", shortcut = KeyCode.Alpha6 },
        new PaletteItem { symbol = '>', name = "→スイッチ", shortcut = KeyCode.Alpha7 },
        new PaletteItem { symbol = 'B', name = "箱", shortcut = KeyCode.Alpha8 },
        new PaletteItem { symbol = 'X', name = "トゲ", shortcut = KeyCode.Alpha9 },
    };

    private List<Button> buttons = new List<Button>();
    private int selectedIndex = 0;

    private void Start()
    {
        if (placementSystem == null)
        {
            placementSystem = FindObjectOfType<PlacementSystem>();
        }

        CreateButtons();
        SelectItem(0);
    }

    private void Update()
    {
        HandleShortcuts();
    }

    private void CreateButtons()
    {
        if (buttonTemplate == null || buttonContainer == null) return;

        // テンプレートを非表示
        buttonTemplate.SetActive(false);

        for (int i = 0; i < items.Count; i++)
        {
            int index = i; // クロージャ用
            PaletteItem item = items[i];

            GameObject btnObj = Instantiate(buttonTemplate, buttonContainer);
            btnObj.SetActive(true);
            btnObj.name = $"Btn_{item.name}";

            // ボタンテキスト設定
            Text text = btnObj.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = $"{i + 1}: {item.name}";
            }

            // TMPの場合
            TMPro.TextMeshProUGUI tmpText = btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = $"{i + 1}: {item.name}";
            }

            // クリックイベント
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SelectItem(index));
                buttons.Add(btn);
            }
        }
    }

    private void HandleShortcuts()
    {
        // テンキー対応
        KeyCode[] numpadKeys = {
            KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3,
            KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6,
            KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9
        };

        for (int i = 0; i < items.Count; i++)
        {
            // 通常の数字キー
            if (Input.GetKeyDown(items[i].shortcut))
            {
                SelectItem(i);
                break;
            }
            // テンキー
            if (i < numpadKeys.Length && Input.GetKeyDown(numpadKeys[i]))
            {
                SelectItem(i);
                break;
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
        placementSystem?.SetTileType(items[index].symbol);

        // ボタンの色を更新
        for (int i = 0; i < buttons.Count; i++)
        {
            ColorBlock colors = buttons[i].colors;
            colors.normalColor = (i == selectedIndex) ? selectedColor : normalColor;
            buttons[i].colors = colors;
        }
    }

    /// <summary>
    /// 選択中のアイテム名を取得
    /// </summary>
    public string GetSelectedItemName()
    {
        if (selectedIndex >= 0 && selectedIndex < items.Count)
        {
            return items[selectedIndex].name;
        }
        return "";
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
}
