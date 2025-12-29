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
        Bounds bounds = boxCollider.bounds;
        Vector2 center = bounds.center;
        float extent = Mathf.Max(bounds.extents.x, bounds.extents.y);
        float inset = extent * 0.9f; // 起点を少し内側に
        
        // 現在の壁方向（法線の逆）
        Vector2 currentWallDir = -zeroGravityWallNormal;
        
        // まず現在の壁方向をチェック
        Vector2 origin = center + currentWallDir * inset;
        RaycastHit2D hit = Physics2D.Raycast(origin, currentWallDir, groundCheckDistance + extent * 0.2f, groundLayer);
        
        if (hit.collider != null)
        {
            // 現在の壁にまだ接触している
            isGrounded = true;
            return;
        }
        
        // 現在の壁から離れた（角に来た）
        // 移動方向の「先」にある壁を探す（角を曲がる）
        // 移動方向 = 壁に沿った方向
        Vector2 alongWall = new Vector2(zeroGravityWallNormal.y, -zeroGravityWallNormal.x);
        
        // プレイヤーが移動している方向に次の壁があるはず
        // 角では、移動方向そのものが次の壁の法線になる
        if (horizontalInput != 0)
        {
            Vector2 nextWallDir = alongWall * horizontalInput; // 移動している方向
            origin = center + nextWallDir * inset;
            hit = Physics2D.Raycast(origin, nextWallDir, groundCheckDistance + extent * 0.5f, groundLayer);
            
            if (hit.collider != null)
            {
                isGrounded = true;
                zeroGravityWallNormal = -nextWallDir.normalized;
                return;
            }
            
            // 反対方向もチェック（内角の角）
            nextWallDir = -alongWall * horizontalInput;
            origin = center + nextWallDir * inset;
            hit = Physics2D.Raycast(origin, nextWallDir, groundCheckDistance + extent * 0.5f, groundLayer);
            
            if (hit.collider != null)
            {
                isGrounded = true;
                zeroGravityWallNormal = -nextWallDir.normalized;
                return;
            }
        }
        
        // それでも見つからない場合、全方向をチェック
        Vector2[] directions = { Vector2.down, Vector2.up, Vector2.left, Vector2.right };
        isGrounded = false;
        
        foreach (var dir in directions)
        {
            origin = center + dir * inset;
            hit = Physics2D.Raycast(origin, dir, groundCheckDistance + extent * 0.2f, groundLayer);
            
            if (hit.collider != null)
            {
                isGrounded = true;
                zeroGravityWallNormal = -dir;
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 無重力時、接触中は常に法線を更新（角を曲がるため）
        if (isZeroGravity && collision.contacts.Length > 0)
        {
            // 現在の入力方向に最も近い法線を選ぶ
            Vector2 bestNormal = collision.contacts[0].normal;
            
            if (horizontalInput != 0 && collision.contacts.Length > 1)
            {
                Vector2 inputDir = moveDirection * horizontalInput;
                float bestDot = -1f;
                
                foreach (var contact in collision.contacts)
                {
                    // 移動方向に垂直な法線（次の壁）を優先
                    float dot = Mathf.Abs(Vector2.Dot(contact.normal, inputDir));
                    if (dot > bestDot && dot > 0.5f)
                    {
                        bestDot = dot;
                        bestNormal = contact.normal;
                    }
                }
            }
            
            zeroGravityWallNormal = bestNormal;
            isGrounded = true;
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
