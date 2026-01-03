using UnityEngine;

/// <summary>
/// 壁を切り替えるボタン
/// 同じチャンネルIDのToggleableWallを制御する
/// チャンネルに応じて自動的に色が設定される
/// トグル式：押すたびにON/OFFが切り替わり、見た目も維持される
/// </summary>
public class WallButton : MonoBehaviour
{
    [Header("Channel Settings")]
    [Tooltip("制御するToggleableWallのチャンネルID (0-9)")]
    [SerializeField] private int channelId = 0;

    [Header("Button Behavior")]
    [Tooltip("トグルのクールダウン（秒）")]
    [SerializeField] private float cooldown = 1f;

    [Header("Sprites")]
    [Tooltip("通常状態のスプライト")]
    [SerializeField] private Sprite normalSprite;
    [Tooltip("押された状態のスプライト")]
    [SerializeField] private Sprite pressedSprite;

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;  // トグル状態（押しっぱなし）
    private float lastPressTime = -999f;

    public int ChannelId => channelId;
    public bool IsActivated => isActivated;

    /// <summary>
    /// チャンネルIDを設定（配置時に呼び出し）
    /// </summary>
    public void SetChannelId(int id)
    {
        channelId = Mathf.Clamp(id, 0, ChannelColors.ColorCount - 1);
        UpdateVisual();
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    private void Start()
    {
        // 開始時に色を適用
        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isPlayer = collision.GetComponent<CharacterBase>() != null;
        bool isBox = collision.GetComponent<Box>() != null;

        if (isPlayer || isBox)
        {
            // クールダウンチェック
            if (Time.time - lastPressTime < cooldown) return;

            // トグル：壁の状態を切り替え
            ToggleableWall.ToggleChannel(channelId);
            
            // ボタン自身の状態もトグル（見た目維持用）
            isActivated = !isActivated;
            
            lastPressTime = Time.time;
            AudioManager.Instance?.PlaySwitch();
            
            UpdateVisual();
        }
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        // スプライトを切り替え（トグル状態に基づく）
        if (normalSprite != null && pressedSprite != null)
        {
            spriteRenderer.sprite = isActivated ? pressedSprite : normalSprite;
        }

        // チャンネル色を適用（トグル状態に基づく）
        if (isActivated)
        {
            spriteRenderer.color = ChannelColors.GetPressedColor(channelId);
        }
        else
        {
            spriteRenderer.color = ChannelColors.GetColor(channelId);
        }
    }

    /// <summary>
    /// ボタンの状態をリセット
    /// </summary>
    public void ResetButton()
    {
        isActivated = false;
        lastPressTime = -999f;
        UpdateVisual();
    }

    /// <summary>
    /// スプライトを設定（エディタから呼び出し用）
    /// </summary>
    public void SetSprites(Sprite normal, Sprite pressed)
    {
        normalSprite = normal;
        pressedSprite = pressed;
        UpdateVisual();
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // エディタでチャンネルIDを変更したら色を更新
        channelId = Mathf.Clamp(channelId, 0, 9);
        UpdateVisual();
    }
}
