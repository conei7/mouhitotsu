using UnityEngine;

/// <summary>
/// プレイヤーキャラクター - Jump King スタイル
/// 地上でのみ移動可能、空中制御なし
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class CharacterBase : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Jump Settings")]
    [Tooltip("ジャンプで乗れる足場の高さ（ブロック数）")]
    [SerializeField, Range(1, 10)] private float jumpHeightBlocks = 3f;
    [Tooltip("足場に乗るための余裕（ブロック数）")]
    [SerializeField] private float jumpHeightMargin = 0.3f;
    [Tooltip("1ブロックのサイズ（ユニット）")]
    [SerializeField] private float blockSize = 1f;
    [Tooltip("基準重力（通常9.81）")]
    [SerializeField] private float baseGravity = 9.81f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Physics")]
    [Tooltip("壁との摩擦を減らす")]
    [SerializeField] private PhysicsMaterial2D slipperyMaterial;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool isGrounded = false;
    private Vector2 groundNormal = Vector2.up;

    /// <summary>
    /// ジャンプ力（ブロック数 + 余裕から自動計算）
    /// </summary>
    private float JumpForce => Mathf.Sqrt(2f * baseGravity * (jumpHeightBlocks + jumpHeightMargin) * blockSize);

    public bool IsGrounded => isGrounded;
    public bool IsInGoal { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
        
        // 摩擦なしマテリアルを設定（壁への吸着防止）
        if (slipperyMaterial == null)
        {
            slipperyMaterial = new PhysicsMaterial2D("Slippery");
            slipperyMaterial.friction = 0f;
            slipperyMaterial.bounciness = 0f;
        }
        rb.sharedMaterial = slipperyMaterial;
        boxCollider.sharedMaterial = slipperyMaterial;
    }

    private void Update()
    {
        CheckGround();

        // ジャンプ（地上でのみ）
        if (isGrounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            PerformJump();
        }
    }

    private void FixedUpdate()
    {
        // 地上でのみ移動可能（Jump King スタイル）
        if (isGrounded)
        {
            float h = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;

            if (h != 0)
            {
                rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
            }
            else
            {
                // 入力なし → 即停止
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
        // 空中では何もしない（重力に任せる）
    }

    private void PerformJump()
    {
        // 地面の法線方向にジャンプ
        Vector2 jumpDirection = groundNormal;
        
        // 重力に応じてジャンプ力をスケール
        Vector2 gravity = GetCurrentGravity();
        float oppositeGravity = Vector2.Dot(-gravity, jumpDirection);
        oppositeGravity = Mathf.Max(oppositeGravity, 0f);
        
        float gravityScale = 1f;
        if (oppositeGravity > 0.1f)
        {
            gravityScale = Mathf.Sqrt(oppositeGravity / baseGravity);
            gravityScale = Mathf.Max(gravityScale, 1f);
        }
        
        float effectiveJumpForce = JumpForce * gravityScale;
        
        // ジャンプ時は水平速度をリセット
        rb.velocity = jumpDirection * effectiveJumpForce;
    }

    private void CheckGround()
    {
        isGrounded = false;
        groundNormal = Vector2.up;

        Bounds bounds = boxCollider.bounds;
        
        // 下方向にレイキャスト（3点）
        Vector2[] checkPoints = {
            new Vector2(bounds.center.x, bounds.min.y),
            new Vector2(bounds.min.x + 0.1f, bounds.min.y),
            new Vector2(bounds.max.x - 0.1f, bounds.min.y)
        };

        foreach (var point in checkPoints)
        {
            RaycastHit2D hit = Physics2D.Raycast(point, Vector2.down, groundCheckDistance, groundLayer);
            if (hit.collider != null)
            {
                isGrounded = true;
                groundNormal = hit.normal;
                return;
            }
        }
    }

    private Vector2 GetCurrentGravity()
    {
        if (GravityController.Instance != null)
        {
            return GravityController.Instance.CombinedGravity;
        }
        return Physics2D.gravity;
    }

    public void EnterGoal() => IsInGoal = true;
    public void ExitGoal() => IsInGoal = false;

    public void Die()
    {
        GameManager.Instance?.OnCharacterDied();
    }

    private void OnDrawGizmosSelected()
    {
        if (boxCollider == null) return;
        
        Bounds bounds = boxCollider.bounds;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(
            new Vector3(bounds.center.x, bounds.min.y, 0),
            new Vector3(bounds.center.x, bounds.min.y - groundCheckDistance, 0)
        );
    }
}
