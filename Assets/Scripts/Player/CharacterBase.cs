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
    private Vector2 zeroGravityWallNormal = Vector2.up;
    private bool isZeroGravity = false;
    private bool atCorner = false; // 角にいる状態
    private Vector2 cornerWallNormal; // 角の先の壁の法線
    private float lastHorizontalInput = 0f; // 前フレームの横入力

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
        float prevInput = horizontalInput;
        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1f;
        
        // 角での回転チェック：入力を一度離して再度押した場合のみ曲がる
        if (atCorner && isZeroGravity)
        {
            // 前フレームで入力が0で、今フレームで入力がある = 新規押下
            bool newPress = (lastHorizontalInput == 0f && horizontalInput != 0f);
            if (newPress)
            {
                // 角を曲がる
                zeroGravityWallNormal = cornerWallNormal;
                atCorner = false;
            }
        }
        lastHorizontalInput = horizontalInput;

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
            isGrounded = false; // ジャンプしたら非接地
            atCorner = false;   // 角状態もリセット
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
            // 角にいる時は移動を止めて待機
            if (atCorner && isZeroGravity)
            {
                rb.velocity = Vector2.zero;
                return;
            }
            
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
                atCorner = false; // 壁の途中にいる
                return;
            }
            
            // 現在の壁から離れた（角に来た）
            // 隣接する壁を探す
            Vector2 alongWall = new Vector2(zeroGravityWallNormal.y, -zeroGravityWallNormal.x);
            
            // 両方向をチェックして隣接壁を探す
            Vector2 foundWallNormal = Vector2.zero;
            foreach (float dir in new float[] { 1f, -1f })
            {
                Vector2 nextWallDir = alongWall * dir;
                origin = center + nextWallDir * (extent * 0.9f);
                hit = Physics2D.Raycast(origin, nextWallDir, checkDist + extent * 0.5f, groundLayer);
                
                if (hit.collider != null)
                {
                    foundWallNormal = -nextWallDir.normalized;
                    break;
                }
            }
            
            if (foundWallNormal != Vector2.zero)
            {
                // 隣接壁がある→角にいる
                if (!atCorner)
                {
                    // 新たに角に到達：現在の入力を記録
                    lastHorizontalInput = horizontalInput;
                }
                atCorner = true;
                cornerWallNormal = foundWallNormal;
                // zeroGravityWallNormalは変更しない！Update()で新規押下時のみ変更
                return;
            }
            
            // 隣接壁がない→落下
            atCorner = false;
            isGrounded = false;
        }
        
        // 2. 飛行中：速度の方向にだけ壁を探す
        Vector2 velocity = rb.velocity;
        
        // 速度がほぼゼロの場合は着地判定しない
        // （コリジョン接触があればOnCollisionEnter2Dで処理される）
        if (velocity.sqrMagnitude < 0.1f)
        {
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
        // 無重力時：飛行中に壁にぶつかった場合
        if (!isZeroGravity || collision.contacts.Length == 0) return;
        
        // 既に接地している場合は何もしない
        if (isGrounded) return;
        
        Vector2 normal = collision.contacts[0].normal;
        Vector2 velocity = rb.velocity;
        
        // 速度が非常に小さい場合は判定しない（横接触で減速しても着地しない）
        if (velocity.sqrMagnitude < 0.5f) return;
        
        // 速度方向と壁法線をチェック
        // 壁に向かっている場合のみ着地
        float dot = Vector2.Dot(velocity.normalized, normal);
        
        if (dot < -0.3f)
        {
            // 正面衝突→着地
            isGrounded = true;
            zeroGravityWallNormal = normal;
            rb.velocity = Vector2.zero;
        }
        // 横接触は無視
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 無重力時：角の検出（2つ以上の異なる法線がある場合）
        if (!isZeroGravity || !isGrounded || collision.contacts.Length == 0) return;
        
        Vector2 currentNormal = zeroGravityWallNormal;
        Vector2 otherNormal = Vector2.zero;
        
        // 現在の壁と異なる法線を探す
        foreach (var contact in collision.contacts)
        {
            Vector2 normal = contact.normal;
            // 現在の法線と大きく異なる法線を探す（角にいる証拠）
            float dot = Vector2.Dot(currentNormal, normal);
            if (dot < 0.7f && normal.sqrMagnitude > 0.5f)
            {
                otherNormal = normal;
                break;
            }
        }
        
        if (otherNormal != Vector2.zero)
        {
            // 角を検出した
            if (!atCorner)
            {
                // 新たに角に到達：現在の入力を記録
                lastHorizontalInput = horizontalInput;
            }
            atCorner = true;
            cornerWallNormal = otherNormal;
            // zeroGravityWallNormalは変更しない！Update()で新規押下時のみ変更
        }
        // 角が検出されなくてもatCornerはfalseにしない
        // （角から離れたかどうかはCheckGroundZeroGravityで判定）
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
