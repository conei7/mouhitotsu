using UnityEngine;

/// <summary>
/// プレイヤーキャラクター
/// カメラの向きに合わせて移動（重力方向が常に下）
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
    private const float JUMP_COOLDOWN_TIME = 0.15f; // ジャンプ後のクールダウン

    public bool IsGrounded => isGrounded;
    public bool IsInGoal { get; private set; }

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
        // 移動方向を更新
        UpdateDirections();
        
        // 入力
        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1f;

        // ジャンプ（固定の力）
        if (isGrounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            // 現在の移動速度を保持してジャンプ
            float currentMoveSpeed = Vector2.Dot(rb.velocity, moveDirection);
            rb.velocity = moveDirection * currentMoveSpeed + jumpDirection * jumpForce;
            justJumped = true;
            jumpCooldown = JUMP_COOLDOWN_TIME;
        }
    }

    private void FixedUpdate()
    {
        // 接地判定
        CheckGround();

        // 回転
        UpdateRotation();

        // ジャンプ直後は移動処理をスキップ（数フレーム）
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
            
            // 地上では移動方向の速度のみ
            rb.velocity = moveDirection * moveComponent;
        }
        else if (horizontalInput != 0 && rb.velocity.magnitude < 0.5f)
        {
            // 空中で速度がほぼ0の場合（角に挟まっているなど）は微小な力を加える
            rb.AddForce(moveDirection * horizontalInput * moveSpeed * 5f);
        }
    }

    private void UpdateDirections()
    {
        // 重力から直接計算（カメラのLerpの影響を受けない）
        Vector2 gravity = GetCurrentGravity();
        if (gravity.sqrMagnitude > 0.01f)
        {
            jumpDirection = -gravity.normalized;
            // 右方向 = 上方向を90度時計回りに回転
            moveDirection = new Vector2(jumpDirection.y, -jumpDirection.x);
        }
        else
        {
            // 無重力時はワールド座標
            moveDirection = Vector2.right;
            jumpDirection = Vector2.up;
        }
    }

    private void UpdateRotation()
    {
        Vector2 gravity = GetCurrentGravity();
        if (gravity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(gravity.x, -gravity.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void CheckGround()
    {
        Vector2 gravityDir = GetCurrentGravity().normalized;
        if (gravityDir.sqrMagnitude < 0.01f) gravityDir = Vector2.down;
        
        Bounds bounds = boxCollider.bounds;
        Vector2 perpendicular = new Vector2(-gravityDir.y, gravityDir.x);
        
        // 重力方向の足元
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
            // Debug.DrawRay(origin, gravityDir * groundCheckDistance, hit.collider != null ? Color.green : Color.red);
            
            if (hit.collider != null)
            {
                isGrounded = true;
                break;
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
}
