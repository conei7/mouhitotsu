using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 結合された切替壁グループを管理
/// チャンネルごとにオン/オフを制御
/// </summary>
public class ToggleableWallGroup : MonoBehaviour
{
    private int channelId = -1;
    private CompositeCollider2D compositeCollider;
    private bool isEnabled = true;
    private bool isInitialized = false;

    /// <summary>
    /// 初期化（エディタのテストプレイ用）
    /// </summary>
    public void Initialize(int channelId, CompositeCollider2D collider, List<Vector2Int> positions, PlacementSystem placement)
    {
        this.channelId = channelId;
        this.compositeCollider = collider;

        // 初期状態を取得（最初の壁の状態を基準にする）
        if (positions.Count > 0 && placement.PlacedTiles.TryGetValue(positions[0], out PlacedTile tile))
        {
            var toggleWall = tile.gameObject?.GetComponent<ToggleableWall>();
            if (toggleWall != null)
            {
                isEnabled = toggleWall.IsEnabled;
            }
        }

        isInitialized = true;
        UpdateState();
    }

    /// <summary>
    /// 初期化（ゲーム本番用 - 子のToggleableWallから自動検出）
    /// </summary>
    public void InitializeFromChildren()
    {
        compositeCollider = GetComponent<CompositeCollider2D>();

        // 子のToggleableWallからチャンネルIDと初期状態を取得
        var toggleWalls = GetComponentsInChildren<ToggleableWall>();
        if (toggleWalls.Length > 0)
        {
            channelId = toggleWalls[0].ChannelId;
            isEnabled = toggleWalls[0].IsEnabled;
        }

        isInitialized = true;
        UpdateState();
    }

    private void Start()
    {
        // 1フレーム待ってから初期化（子のToggleableWallが先に初期化されるように）
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return null; // 1フレーム待つ
        
        if (!isInitialized)
        {
            InitializeFromChildren();
        }
    }

    private void OnEnable()
    {
        // ToggleableWallのトグルイベントを監視
        ToggleableWall.OnChannelToggled += OnChannelToggled;
    }

    private void OnDisable()
    {
        ToggleableWall.OnChannelToggled -= OnChannelToggled;
    }

    /// <summary>
    /// チャンネルがトグルされた時のコールバック
    /// </summary>
    private void OnChannelToggled(int toggledChannelId)
    {
        if (toggledChannelId == channelId)
        {
            isEnabled = !isEnabled;
            UpdateState();
        }
    }

    /// <summary>
    /// 状態を更新
    /// </summary>
    private void UpdateState()
    {
        // コライダーの有効/無効を切り替え
        if (compositeCollider != null)
        {
            compositeCollider.enabled = isEnabled;
        }

        // 子のBoxCollider2Dも切り替え
        foreach (var collider in GetComponentsInChildren<BoxCollider2D>())
        {
            collider.enabled = isEnabled;
        }
    }

    /// <summary>
    /// 状態をリセット
    /// </summary>
    public void ResetState()
    {
        // 子のToggleableWallの状態に合わせる
        var toggleWalls = GetComponentsInChildren<ToggleableWall>();
        if (toggleWalls.Length > 0)
        {
            isEnabled = toggleWalls[0].IsEnabled;
            UpdateState();
        }
    }
}
