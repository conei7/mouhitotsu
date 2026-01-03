using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// オン/オフ切り替え可能な壁
/// 同じチャンネルIDのWallButtonで制御される
/// チャンネルに応じて自動的に色が設定される
/// </summary>
public class ToggleableWall : MonoBehaviour
{
    [Header("Channel Settings")]
    [Tooltip("同じチャンネルIDのボタンで制御される (0-9)")]
    [SerializeField] private int channelId = 0;

    [Header("Initial State")]
    [Tooltip("ゲーム開始時にオンかオフか")]
    [SerializeField] private bool startEnabled = true;

    [Header("Sprites")]
    [Tooltip("有効状態のスプライト")]
    [SerializeField] private Sprite enabledSprite;
    [Tooltip("無効状態のスプライト")]
    [SerializeField] private Sprite disabledSprite;

    private SpriteRenderer spriteRenderer;
    private Collider2D wallCollider;
    private bool isEnabled;

    // 全てのToggleableWallを管理（チャンネルID別）
    private static Dictionary<int, List<ToggleableWall>> allWalls = new Dictionary<int, List<ToggleableWall>>();

    // チャンネルトグルイベント（結合壁用）
    public static event System.Action<int> OnChannelToggled;

    public int ChannelId => channelId;
    public bool IsEnabled => isEnabled;

    /// <summary>
    /// チャンネルIDを設定（配置時に呼び出し）
    /// </summary>
    public void SetChannelId(int id)
    {
        // 既存のリストから削除
        if (allWalls.ContainsKey(channelId))
        {
            allWalls[channelId].Remove(this);
            if (allWalls[channelId].Count == 0)
            {
                allWalls.Remove(channelId);
            }
        }

        channelId = Mathf.Clamp(id, 0, ChannelColors.ColorCount - 1);

        // 新しいリストに追加
        if (!allWalls.ContainsKey(channelId))
        {
            allWalls[channelId] = new List<ToggleableWall>();
        }
        if (!allWalls[channelId].Contains(this))
        {
            allWalls[channelId].Add(this);
        }

        UpdateVisual();
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        wallCollider = GetComponent<Collider2D>();
        
        isEnabled = startEnabled;
        UpdateVisual();
    }

    private void Start()
    {
        // 開始時に色を適用
        UpdateVisual();
    }

    private void OnEnable()
    {
        if (!allWalls.ContainsKey(channelId))
        {
            allWalls[channelId] = new List<ToggleableWall>();
        }
        if (!allWalls[channelId].Contains(this))
        {
            allWalls[channelId].Add(this);
        }
    }

    private void OnDisable()
    {
        if (allWalls.ContainsKey(channelId))
        {
            allWalls[channelId].Remove(this);
            if (allWalls[channelId].Count == 0)
            {
                allWalls.Remove(channelId);
            }
        }
    }

    /// <summary>
    /// 壁の状態を切り替え
    /// </summary>
    public void Toggle()
    {
        isEnabled = !isEnabled;
        UpdateVisual();
    }

    /// <summary>
    /// 壁の状態を設定
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
        UpdateVisual();
    }

    /// <summary>
    /// 初期状態を設定（配置時に呼び出し）
    /// </summary>
    public void SetInitialState(bool enabled)
    {
        startEnabled = enabled;
        isEnabled = enabled;
        UpdateVisual();
    }

    /// <summary>
    /// 初期状態にリセット
    /// </summary>
    public void ResetToInitial()
    {
        isEnabled = startEnabled;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // コライダーの有効/無効
        if (wallCollider != null)
        {
            wallCollider.enabled = isEnabled;
        }

        if (spriteRenderer == null) return;

        // スプライトを切り替え
        if (enabledSprite != null && disabledSprite != null)
        {
            spriteRenderer.sprite = isEnabled ? enabledSprite : disabledSprite;
        }

        // チャンネル色を適用
        if (isEnabled)
        {
            spriteRenderer.color = ChannelColors.GetColor(channelId);
        }
        else
        {
            spriteRenderer.color = ChannelColors.GetDisabledColor(channelId);
        }
    }

    /// <summary>
    /// スプライトを設定（エディタから呼び出し用）
    /// </summary>
    public void SetSprites(Sprite enabled, Sprite disabled)
    {
        enabledSprite = enabled;
        disabledSprite = disabled;
        UpdateVisual();
    }

    /// <summary>
    /// 指定チャンネルの全ての壁を切り替え
    /// </summary>
    public static void ToggleChannel(int channelId)
    {
        if (allWalls.TryGetValue(channelId, out List<ToggleableWall> walls))
        {
            foreach (var wall in walls)
            {
                wall.Toggle();
            }
        }

        // イベントを発火（結合壁グループに通知）
        OnChannelToggled?.Invoke(channelId);
    }

    /// <summary>
    /// 指定チャンネルの全ての壁の状態を設定
    /// </summary>
    public static void SetChannelEnabled(int channelId, bool enabled)
    {
        if (allWalls.TryGetValue(channelId, out List<ToggleableWall> walls))
        {
            foreach (var wall in walls)
            {
                wall.SetEnabled(enabled);
            }
        }
    }

    /// <summary>
    /// 全ての壁を初期状態にリセット
    /// </summary>
    public static void ResetAllWalls()
    {
        foreach (var channel in allWalls.Values)
        {
            foreach (var wall in channel)
            {
                wall.ResetToInitial();
            }
        }
    }

    /// <summary>
    /// 静的リストをクリア（シーン遷移時用）
    /// </summary>
    public static void ClearAllWallsRegistry()
    {
        allWalls.Clear();
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (wallCollider == null)
            wallCollider = GetComponent<Collider2D>();
        
        // エディタでチャンネルIDを変更したら色を更新
        channelId = Mathf.Clamp(channelId, 0, 9);
        isEnabled = startEnabled;
        UpdateVisual();
    }
}
