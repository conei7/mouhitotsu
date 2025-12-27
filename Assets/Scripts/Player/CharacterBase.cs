using UnityEngine;

/// <summary>
/// プレイヤーキャラクター - 移動、ジャンプ、ゴール判定
/// 重力方向対応: ジャンプは接触面に対して垂直方向
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class CharacterBase : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground/Surface Check")]
    [SerializeField] private float surfaceCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool jumpRequested = false;
    
    // 接触面の情報
    private bool isTouchingSurface = false;
    private Vector2 surfaceNormal = Vector2.up;

    public bool IsGrounded => isTouchingSurface;
    public bool IsInGoal { get; private set; }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        CheckSurfaces();

        // ジャンプ入力を受け付け（何かに触れていればジャンプ可能）
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isTouchingSurface)
            {
                jumpRequested = true;
            }
        }
    }

    private void FixedUpdate()
    {
        // 入力取得
        float h = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;

        // 移動処理
        if (h != 0)
        {
            if (isTouchingSurface)
            {
                // 地上: フル速度で移動
                rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
            }
            else
            {
                // 空中: 弱い制御（入力方向への速度に上限あり）
                float airControlSpeed = moveSpeed * 0.5f;  // 空中での最大制御速度（50%）
                float currentVelX = rb.velocity.x;
                
                // 入力方向への現在の速度を計算
                float velInInputDir = currentVelX * h;  // 正なら入力方向に動いている
                
                // 入力方向への速度が上限未満なら加速
                if (velInInputDir < airControlSpeed)
                {
                    float accel = h * moveSpeed * 0.5f * Time.fixedDeltaTime * 10f;
                    float newVelX = currentVelX + accel;
                    
                    // 入力方向への速度が上限を超えないように制限
                    float newVelInInputDir = newVelX * h;
                    if (newVelInInputDir > airControlSpeed)
                    {
                        newVelX = airControlSpeed * h;
                    }
                    
                    rb.velocity = new Vector2(newVelX, rb.velocity.y);
                }
                // すでに上限以上なら何もしない（重力に任せる）
            }
        }
        else if (isTouchingSurface)
        {
            // 地上で入力なし → 摩擦で減速
            rb.velocity = new Vector2(rb.velocity.x * 0.8f, rb.velocity.y);
        }
        // 空中で入力なし → 重力に任せる（何もしない）

        // 最大速度制限（両軸）
        float maxSpeed = moveSpeed * 2f;
        rb.velocity = new Vector2(
            Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed),
            Mathf.Clamp(rb.velocity.y, -maxSpeed, maxSpeed)
        );

        // ジャンプ実行
        if (jumpRequested)
        {
            PerformJump();
            jumpRequested = false;
        }
    }

    /// <summary>
    /// 接触面に対して垂直方向にジャンプ
    /// </summary>
    private void PerformJump()
    {
        if (!isTouchingSurface) return;

        // 接触面の法線方向にジャンプ
        Vector2 jumpDirection = surfaceNormal;
        
        // ジャンプ方向と逆向きの重力成分を計算してスケール
        Vector2 gravity = GetCurrentGravity();
        float oppositeGravity = Vector2.Dot(-gravity, jumpDirection);
        oppositeGravity = Mathf.Max(oppositeGravity, 0f);
        
        float gravityScale = 1f;
        if (oppositeGravity > 0.1f)
        {
            gravityScale = Mathf.Sqrt(oppositeGravity / 9.81f);
            gravityScale = Mathf.Max(gravityScale, 1f);
        }
        
        float effectiveJumpForce = jumpForce * gravityScale;
        rb.velocity = jumpDirection * effectiveJumpForce;
        
        Debug.Log($"Jump! Normal: {surfaceNormal}, Force: {effectiveJumpForce:F1}");
    }

    /// <summary>
    /// 全方向で接触面をチェック
    /// </summary>
    private void CheckSurfaces()
    {
        isTouchingSurface = false;
        surfaceNormal = Vector2.up;

        Bounds bounds = boxCollider.bounds;
        float checkDist = surfaceCheckDistance;

        // 4方向チェック（優先順位: 重力方向 > 他）
        // 下、左、右、上の順でチェック
        Vector2 gravity = GetCurrentGravity();
        
        // 最も重力が強い方向を優先
        CheckInfo[] checks = new CheckInfo[]
        {
            new CheckInfo(Vector2.down, new Vector2(bounds.center.x, bounds.min.y)),
            new CheckInfo(Vector2.left, new Vector2(bounds.min.x, bounds.center.y)),
            new CheckInfo(Vector2.right, new Vector2(bounds.max.x, bounds.center.y)),
            new CheckInfo(Vector2.up, new Vector2(bounds.center.x, bounds.max.y))
        };

        // 重力方向に近い順にソート
        System.Array.Sort(checks, (a, b) => 
        {
            float dotA = Vector2.Dot(gravity.normalized, a.direction);
            float dotB = Vector2.Dot(gravity.normalized, b.direction);
            return dotB.CompareTo(dotA);  // 重力方向に近いものが先
        });

        foreach (var check in checks)
        {
            RaycastHit2D hit = Physics2D.Raycast(check.origin, check.direction, checkDist, groundLayer);
            
            Debug.DrawRay(check.origin, check.direction * checkDist, 
                hit.collider != null ? Color.green : Color.red);
            
            if (hit.collider != null)
            {
                isTouchingSurface = true;
                surfaceNormal = hit.normal;
                return;  // 最初に見つかった面を使用
            }
        }
    }

    private struct CheckInfo
    {
        public Vector2 direction;
        public Vector2 origin;
        
        public CheckInfo(Vector2 dir, Vector2 orig)
        {
            direction = dir;
            origin = orig;
        }
    }

    /// <summary>
    /// 現在の合成重力を取得
    /// </summary>
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
        
        // 接触面の法線（ジャンプ方向）を表示
        if (isTouchingSurface)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + surfaceNormal * 1.5f);
        }
    }
}
