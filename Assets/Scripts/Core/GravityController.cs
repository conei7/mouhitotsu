using UnityEngine;

/// <summary>
/// 重力管理システム - 通常重力と追加重力のベクトル合成を管理
/// </summary>
public class GravityController : MonoBehaviour
{
    public static GravityController Instance { get; private set; }

    [Header("Primary Gravity (Always Active)")]
    [SerializeField] private Vector2 primaryGravity = new Vector2(0, -9.81f);

    [Header("Secondary Gravity Settings")]
    [SerializeField, Range(0f, 2f)] private float secondaryGravityMultiplier = 1.0f;
    [Tooltip("Current secondary gravity direction (set by switches/orbs)")]
    [SerializeField] private Vector2 secondaryGravityDirection = Vector2.zero;

    /// <summary>
    /// 合成された重力ベクトル
    /// </summary>
    public Vector2 CombinedGravity => primaryGravity + (secondaryGravityDirection.normalized * primaryGravity.magnitude * secondaryGravityMultiplier);

    /// <summary>
    /// 追加重力が有効かどうか
    /// </summary>
    public bool HasSecondaryGravity => secondaryGravityDirection != Vector2.zero;

    /// <summary>
    /// 現在の追加重力方向
    /// </summary>
    public Vector2 SecondaryGravityDirection => secondaryGravityDirection;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void FixedUpdate()
    {
        // Physics2D.gravityを合成重力に設定
        Physics2D.gravity = CombinedGravity;
    }

    /// <summary>
    /// 追加重力の方向を設定（スイッチやオーブから呼び出される）
    /// </summary>
    /// <param name="direction">方向ベクトル（正規化される）</param>
    public void SetSecondaryGravity(Vector2 direction)
    {
        secondaryGravityDirection = direction.normalized;
        Debug.Log($"Secondary Gravity Set: {secondaryGravityDirection}, Combined: {CombinedGravity}");
    }

    /// <summary>
    /// 追加重力をクリア
    /// </summary>
    public void ClearSecondaryGravity()
    {
        secondaryGravityDirection = Vector2.zero;
        Debug.Log("Secondary Gravity Cleared");
    }

    /// <summary>
    /// 追加重力の強度倍率を変更
    /// </summary>
    public void SetMultiplier(float multiplier)
    {
        secondaryGravityMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
    }

    private void OnDestroy()
    {
        // シーン終了時に重力をリセット
        Physics2D.gravity = new Vector2(0, -9.81f);
    }
}
