using UnityEngine;

/// <summary>
/// プレイヤーキャラクター
/// カメラの向きに合わせて移動（重力方向が常に下）
/// 無重力時は壁を歩ける
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class CharacterBase : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float jumpForce = 8f;
    [Tooltip("Shift押下時の速度倍率")]
    [SerializeField] private float slowSpeedMultiplier = 0.1f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private GravityCamera gravityCamera;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private bool isGrounded = false;
    private float horizontalInput = 0f;
    private Vector2 moveDirection = Vector2.right;
    private Vector2 jumpDirection = Vector2.up;
    private bool justJumped = false;
    private float jumpCooldown = 0f;
    private const float JUMP_COOLDOWN_TIME = 0.15f;
    
    // 無重力用
    private Vector2 zeroGravityWallNormal = Vector2.up; // 現在接触している壁の法線
    private bool isZeroGravity = false;

    public bool IsGrounded => isGrounded;
    public bool IsInGoal { get; private set; }
    public bool IsZeroGravity => isZeroGravity;
    public Vector2 ZeroGravityWallNormal => zeroGravityWallNormal;
    public bool IsAlive { get; private set; } = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        
        // 摩擦なし
        var mat = new PhysicsMaterial2D("NoFriction");
        mat.friction = 0f;
        mat.bounciness = 0f;
        rb.sharedMaterial = mat;
        boxCollider.sharedMaterial = mat;
    }

    private void Start()
    {
        if (gravityCamera == null)
        {
            gravityCamera = FindObjectOfType<GravityCamera>();
        }
    }

    private void Update()
    {
        // 無重力判定
        isZeroGravity = GetCurrentGravity().sqrMagnitude < 0.01f;
        
        // 移動方向を更新
        UpdateDirections();
        
        // 入力
        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1f;

        // ジャンプ
        if (isGrounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            if (isZeroGravity)
            {
                // 無重力ジャンプ：走行中の慣性を保持して壁から離れる
                float currentMoveSpeed = Vector2.Dot(rb.velocity, moveDirection);
                rb.velocity = moveDirection * currentMoveSpeed + zeroGravityWallNormal * jumpForce;
            }
            else
            {
                // 通常ジャンプ
                float currentMoveSpeed = Vector2.Dot(rb.velocity, moveDirection);
                rb.velocity = moveDirection * currentMoveSpeed + jumpDirection * jumpForce;
            }
            justJumped = true;
            jumpCooldown = JUMP_COOLDOWN_TIME;
            
            // ジャンプ音
            AudioManager.Instance?.PlayJump();
        }
    }

    private void FixedUpdate()
    {
        // 接地判定
        if (isZeroGravity)
        {
            CheckGroundZeroGravity();
            // 壁が切り替わった場合に備えて方向を更新
            UpdateDirections();
        }
        else
        {
            CheckGround();
        }

        // 回転
        UpdateRotation();

        // ジャンプ直後は移動処理をスキップ
        if (justJumped)
        {
            jumpCooldown -= Time.fixedDeltaTime;
            if (jumpCooldown <= 0)
            {
                justJumped = false;
            }
            return;
        }

        // 移動
        if (isGrounded)
        {
            float moveComponent;
            
            // Shift押下時はスロー移動
            float speedMultiplier = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) 
                ? slowSpeedMultiplier 
                : 1f;
            
            if (horizontalInput != 0)
            {
                moveComponent = horizontalInput * moveSpeed * speedMultiplier;
            }
            else
            {
                // 減速
                float currentSpeed = Vector2.Dot(rb.velocity, moveDirection);
                moveComponent = currentSpeed * 0.85f;
                if (Mathf.Abs(moveComponent) < 0.1f) moveComponent = 0f;
            }
            
            rb.velocity = moveDirection * moveComponent;
        }
        else if (horizontalInput != 0 && rb.velocity.magnitude < 0.5f)
        {
            // 角に挟まった時の脱出
            rb.AddForce(moveDirection * horizontalInput * moveSpeed * 5f);
        }
    }

    private void UpdateDirections()
    {
        if (isZeroGravity)
        {
            // 無重力時：接触している壁の法線から方向を決定
            jumpDirection = zeroGravityWallNormal;
            moveDirection = new Vector2(jumpDirection.y, -jumpDirection.x);
        }
        else
        {
            // 通常：重力から計算
            Vector2 gravity = GetCurrentGravity();
            jumpDirection = -gravity.normalized;
            moveDirection = new Vector2(jumpDirection.y, -jumpDirection.x);
        }
    }

    private void UpdateRotation()
    {
        if (isZeroGravity)
        {
            // 無重力時：壁の法線に合わせて回転
            if (isGrounded)
            {
                float angle = Mathf.Atan2(-zeroGravityWallNormal.x, zeroGravityWallNormal.y) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        else
        {
            Vector2 gravity = GetCurrentGravity();
            if (gravity.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(gravity.x, -gravity.y) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    private void CheckGround()
    {
        Vector2 gravityDir = GetCurrentGravity().normalized;
        if (gravityDir.sqrMagnitude < 0.01f) gravityDir = Vector2.down;
        
        Bounds bounds = boxCollider.bounds;
        Vector2 perpendicular = new Vector2(-gravityDir.y, gravityDir.x);
        
        float extent = Mathf.Max(bounds.extents.x, bounds.extents.y);
        Vector2 footCenter = (Vector2)bounds.center + gravityDir * extent;
        float checkWidth = Mathf.Min(bounds.size.x, bounds.size.y) * 0.35f;
        
        Vector2[] origins = {
            footCenter,
            footCenter + perpendicular * checkWidth,
            footCenter - perpendicular * checkWidth
        };
        
        isGrounded = false;
        foreach (var origin in origins)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, gravityDir, groundCheckDistance, groundLayer);
            if (hit.collider != null)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void CheckGroundZeroGravity()
    {
        Bounds bounds = boxCollider.bounds;
        Vector2 center = bounds.center;
        float extent = Mathf.Max(bounds.extents.x, bounds.extents.y);
        float checkDist = groundCheckDistance + extent * 0.3f;
        
        // 1. 既に接地中の場合：現在の壁方向と角のチェック
        if (isGrounded)
        {
            // 現在の壁方向をチェック
            Vector2 wallDir = -zeroGravityWallNormal;
            Vector2 origin = center + wallDir * (extent * 0.9f);
            RaycastHit2D hit = Physics2D.Raycast(origin, wallDir, checkDist, groundLayer);
            
            if (hit.collider != null)
            {
                return; // まだ壁にいる
            }
            
            // 角を曲がるチェック（入力方向に壁があるか）
            if (horizontalInput != 0)
            {
                Vector2 alongWall = new Vector2(zeroGravityWallNormal.y, -zeroGravityWallNormal.x);
                Vector2 nextWallDir = alongWall * horizontalInput;
                origin = center + nextWallDir * (extent * 0.9f);
                hit = Physics2D.Raycast(origin, nextWallDir, checkDist + extent * 0.3f, groundLayer);
                
                if (hit.collider != null)
                {
                    zeroGravityWallNormal = -nextWallDir.normalized;
                    return;
                }
            }
            
            // 壁から離れた
            isGrounded = false;
        }
        
        // 2. 飛行中：速度の方向にだけ壁を探す
        Vector2 velocity = rb.velocity;
        if (velocity.sqrMagnitude < 0.1f)
        {
            // 速度がほぼゼロなら全方向チェック（着地直後など）
            Vector2[] directions = { Vector2.down, Vector2.up, Vector2.left, Vector2.right };
            foreach (var dir in directions)
            {
                Vector2 origin = center + dir * (extent * 0.9f);
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, checkDist, groundLayer);
                if (hit.collider != null)
                {
                    isGrounded = true;
                    zeroGravityWallNormal = hit.normal;
                    rb.velocity = Vector2.zero;
                    return;
                }
            }
            return;
        }
        
        // 速度方向に壁があるかチェック
        Vector2 flyDir = velocity.normalized;
        Vector2 rayOrigin = center + flyDir * (extent * 0.9f);
        RaycastHit2D flyHit = Physics2D.Raycast(rayOrigin, flyDir, checkDist, groundLayer);
        
        if (flyHit.collider != null)
        {
            // 前方に壁がある→着地
            isGrounded = true;
            zeroGravityWallNormal = flyHit.normal;
            rb.velocity = Vector2.zero;
        }
        // 前方に壁がない→飛び続ける（何もしない）
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 無重力時：壁の法線を取得（方向の参考用）
        // 接地判定やvelocity停止はCheckGroundZeroGravityで行う
        if (isZeroGravity && collision.contacts.Length > 0)
        {
            // 今触れた壁の法線を記録（CheckGroundで使う可能性）
            // ただしisGroundedは設定しない
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 無重力時：角を曲がる時に法線を更新（すでにisGroundedの時のみ）
        if (isZeroGravity && isGrounded && collision.contacts.Length > 0)
        {
            Vector2 bestNormal = collision.contacts[0].normal;
            
            if (horizontalInput != 0 && collision.contacts.Length > 1)
            {
                Vector2 inputDir = moveDirection * horizontalInput;
                float bestDot = -1f;
                
                foreach (var contact in collision.contacts)
                {
                    float dot = Mathf.Abs(Vector2.Dot(contact.normal, inputDir));
                    if (dot > bestDot && dot > 0.5f)
                    {
                        bestDot = dot;
                        bestNormal = contact.normal;
                    }
                }
            }
            
            zeroGravityWallNormal = bestNormal;
            // isGroundedはCheckGroundZeroGravityで既に設定されている
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
        if (!IsAlive) return;
        IsAlive = false;
        AudioManager.Instance?.PlayDeath();

        // テストプレイ中はEditorManagerでリトライ
        if (EditorManager.Instance != null && EditorManager.Instance.IsPlayMode)
        {
            // EditorManagerが死亡を検知してリトライする
            return;
        }

        GameManager.Instance?.OnCharacterDied();
    }

    /// <summary>
    /// 復活（エディタテストプレイ用）
    /// </summary>
    public void Revive()
    {
        IsAlive = true;
        IsInGoal = false;
        gameObject.SetActive(true);
        
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
}
