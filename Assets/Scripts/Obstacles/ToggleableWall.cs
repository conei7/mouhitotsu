using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// オン/オフ切り替え可能な壁
/// 同じチャンネルIDのWallButtonで制御される
/// </summary>
public class ToggleableWall : MonoBehaviour
{
    [Header("Channel Settings")]
    [Tooltip("同じチャンネルIDのボタンで制御される")]
    [SerializeField] private int channelId = 0;

    [Header("Initial State")]
    [Tooltip("ゲーム開始時にオンかオフか")]
    [SerializeField] private bool startEnabled = true;

    [Header("Appearance")]
    [SerializeField] private Color enabledColor = Color.white;
    [SerializeField] private Color disabledColor = new Color(1f, 1f, 1f, 0.3f);

    private SpriteRenderer spriteRenderer;
    private Collider2D wallCollider;
    private bool isEnabled;

    // 全てのToggleableWallを管理（チャンネルID別）
    private static Dictionary<int, List<ToggleableWall>> allWalls = new Dictionary<int, List<ToggleableWall>>();

    public int ChannelId => channelId;
    public bool IsEnabled => isEnabled;

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
        wallCollider = GetComponent<Collider2D>();
        
        // 初期状態を設定
        isEnabled = startEnabled;
        UpdateVisual();
    }

    private void OnEnable()
    {
        // 静的リストに登録
        if (!allWalls.ContainsKey(channelId))
        {
            allWalls[channelId] = new List<ToggleableWall>();
        }
        allWalls[channelId].Add(this);
    }

    private void OnDisable()
    {
        // 静的リストから削除
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

        // 見た目の更新
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isEnabled ? enabledColor : disabledColor;
        }
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

    private void OnValidate()
    {
        // エディタで変更時にビジュアルを更新
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (wallCollider == null)
            wallCollider = GetComponent<Collider2D>();
        
        isEnabled = startEnabled;
        UpdateVisual();
    }
}
