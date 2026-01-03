using UnityEngine;

/// <summary>
/// 壁を切り替えるボタン
/// 同じチャンネルIDのToggleableWallを制御する
/// </summary>
public class WallButton : MonoBehaviour
{
    [Header("Channel Settings")]
    [Tooltip("制御するToggleableWallのチャンネルID")]
    [SerializeField] private int channelId = 0;

    [Header("Button Behavior")]
    [Tooltip("trueの場合、押すたびにトグル。falseの場合、押している間のみ有効")]
    [SerializeField] private bool toggleMode = true;
    
    [Tooltip("トグルモード時のクールダウン（秒）")]
    [SerializeField] private float cooldown = 0.5f;

    [Header("Appearance")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color pressedColor = Color.gray;
    
    [Header("Sprites (Optional)")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite pressedSprite;

    private SpriteRenderer spriteRenderer;
    private bool isPressed = false;
    private float lastPressTime = -999f;
    private int objectsOnButton = 0; // ボタン上のオブジェクト数

    public int ChannelId => channelId;

    /// <summary>
    /// チャンネルIDを設定（配置時に呼び出し）
    /// </summary>
    public void SetChannelId(int id)
    {
        channelId = id;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーまたは箱がボタンを押せる
        bool isPlayer = collision.GetComponent<CharacterBase>() != null;
        bool isBox = collision.GetComponent<Box>() != null;

        if (isPlayer || isBox)
        {
            objectsOnButton++;
            
            if (toggleMode)
            {
                // トグルモード: 押すたびに切り替え
                if (Time.time - lastPressTime >= cooldown)
                {
                    ToggleableWall.ToggleChannel(channelId);
                    lastPressTime = Time.time;
                    
                    // ボタン音
                    AudioManager.Instance?.PlaySwitch();
                }
            }
            else
            {
                // ホールドモード: 押している間のみ有効
                if (objectsOnButton == 1)
                {
                    ToggleableWall.ToggleChannel(channelId);
                    AudioManager.Instance?.PlaySwitch();
                }
            }
            
            isPressed = true;
            UpdateVisual();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        bool isPlayer = collision.GetComponent<CharacterBase>() != null;
        bool isBox = collision.GetComponent<Box>() != null;

        if (isPlayer || isBox)
        {
            objectsOnButton = Mathf.Max(0, objectsOnButton - 1);
            
            if (!toggleMode && objectsOnButton == 0)
            {
                // ホールドモード: 離れたら元に戻す
                ToggleableWall.ToggleChannel(channelId);
            }
            
            if (objectsOnButton == 0)
            {
                isPressed = false;
                UpdateVisual();
            }
        }
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        // スプライトが設定されていれば切り替え
        if (normalSprite != null && pressedSprite != null)
        {
            spriteRenderer.sprite = isPressed ? pressedSprite : normalSprite;
            spriteRenderer.color = Color.white;
        }
        else
        {
            // スプライトがなければ色で切り替え
            spriteRenderer.color = isPressed ? pressedColor : normalColor;
        }
    }

    /// <summary>
    /// ボタンの状態をリセット
    /// </summary>
    public void ResetButton()
    {
        isPressed = false;
        objectsOnButton = 0;
        lastPressTime = -999f;
        UpdateVisual();
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }
}
