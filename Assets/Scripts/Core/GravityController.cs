using UnityEngine;

/// <summary>
/// 重力管理システム - 通常重力と追加重力のベクトル合成を管理
/// 重ねがけ対応: スイッチを踏むたびに追加重力にベクトル加算
/// </summary>
public class GravityController : MonoBehaviour
{
    public static GravityController Instance { get; private set; }

    [Header("Primary Gravity (Always Active)")]
    [SerializeField] private Vector2 primaryGravity = new Vector2(0, -9.81f);

    [Header("Secondary Gravity Settings")]
    [SerializeField, Range(0.1f, 3f)] private float secondaryGravityStrength = 1.0f;
    [Tooltip("追加重力の最大強度（これ以上は強くならない）")]
    [SerializeField] private float maxSecondaryMagnitude = 3f;
    
    [Header("Current State (Debug)")]
    [Tooltip("現在の追加重力ベクトル（重ねがけの結果）")]
    [SerializeField] private Vector2 secondaryGravityVector = Vector2.zero;

    /// <summary>
    /// 合成された重力ベクトル
    /// </summary>
    public Vector2 CombinedGravity => primaryGravity + (secondaryGravityVector * primaryGravity.magnitude * secondaryGravityStrength);

    /// <summary>
    /// 追加重力が有効かどうか
    /// </summary>
    public bool HasSecondaryGravity => secondaryGravityVector != Vector2.zero;

    /// <summary>
    /// 現在の追加重力方向（正規化）
    /// </summary>
    public Vector2 SecondaryGravityDirection => secondaryGravityVector.normalized;

    /// <summary>
    /// 現在の追加重力ベクトル（重ねがけの結果、正規化なし）
    /// </summary>
    public Vector2 SecondaryGravityVector => secondaryGravityVector;

    /// <summary>
    /// 追加重力の強度（ベクトルの大きさ）
    /// </summary>
    public float SecondaryGravityMagnitude => secondaryGravityVector.magnitude;

    /// <summary>
    /// 通常重力ベクトル
    /// </summary>
    public Vector2 PrimaryGravity => primaryGravity;

    /// <summary>
    /// 追加重力の強度倍率
    /// </summary>
    public float SecondaryGravityStrength => secondaryGravityStrength;

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
    /// 追加重力を加算（重ねがけ）
    /// </summary>
    /// <param name="direction">加算する方向ベクトル（正規化される）</param>
    /// <param name="strength">加算する強度（デフォルト1.0）</param>
    public void AddSecondaryGravity(Vector2 direction, float strength = 1f)
    {
        secondaryGravityVector += direction.normalized * strength;
        
        // 最大強度で制限
        if (secondaryGravityVector.magnitude > maxSecondaryMagnitude)
        {
            secondaryGravityVector = secondaryGravityVector.normalized * maxSecondaryMagnitude;
        }
        
        Debug.Log($"Secondary Gravity Added: +{direction.normalized * strength}, Total: {secondaryGravityVector}, Combined: {CombinedGravity}");
    }

    /// <summary>
    /// 追加重力を設定（上書き、重ねがけなし）
    /// </summary>
    /// <param name="direction">方向ベクトル（正規化される）</param>
    public void SetSecondaryGravity(Vector2 direction)
    {
        secondaryGravityVector = direction.normalized;
        Debug.Log($"Secondary Gravity Set: {secondaryGravityVector}, Combined: {CombinedGravity}");
    }

    /// <summary>
    /// 追加重力をクリア
    /// </summary>
    public void ClearSecondaryGravity()
    {
        secondaryGravityVector = Vector2.zero;
        Debug.Log("Secondary Gravity Cleared");
    }

    /// <summary>
    /// 追加重力を削除（特定のスイッチがオフになったとき）
    /// </summary>
    /// <param name="contribution">削除する重力ベクトル</param>
    public void RemoveSecondaryGravity(Vector2 contribution)
    {
        secondaryGravityVector -= contribution;
        Debug.Log($"Secondary Gravity Removed: -{contribution}, Total: {secondaryGravityVector}, Combined: {CombinedGravity}");
    }

    /// <summary>
    /// 追加重力の強度倍率を変更
    /// </summary>
    public void SetStrength(float strength)
    {
        secondaryGravityStrength = Mathf.Clamp(strength, 0.1f, 3f);
    }

    private void OnDestroy()
    {
        // シーン終了時に重力をリセット
        Physics2D.gravity = new Vector2(0, -9.81f);
    }
}
