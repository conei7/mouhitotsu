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
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

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
        }
    }

    private void FixedUpdate()
    {
        // 接地判定
        if (isZeroGravity)
        {
            CheckGroundZeroGravity();
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
            
            if (horizontalInput != 0)
            {
                moveComponent = horizontalInput * moveSpeed;
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
        // 無重力時：全方向をチェックして壁を探す
        Bounds bounds = boxCollider.bounds;
        Vector2 center = bounds.center;
        float extent = Mathf.Max(bounds.extents.x, bounds.extents.y);
        
        Vector2[] directions = { Vector2.down, Vector2.up, Vector2.left, Vector2.right };
        
        isGrounded = false;
        foreach (var dir in directions)
        {
            Vector2 origin = center + dir * extent;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, groundCheckDistance, groundLayer);
            
            if (hit.collider != null)
            {
                isGrounded = true;
                zeroGravityWallNormal = -dir; // 壁の法線は進行方向の逆
                break;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 無重力で壁に衝突したら、その壁に張り付く
        if (isZeroGravity && collision.contacts.Length > 0)
        {
            zeroGravityWallNormal = collision.contacts[0].normal;
            rb.velocity = Vector2.zero; // 停止
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
}
