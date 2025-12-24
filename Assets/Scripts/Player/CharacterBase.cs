using UnityEngine;

/// <summary>
/// プレイヤーキャラクター - 移動、ジャンプ、ゴール判定
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class CharacterBase : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool jumpRequested = false;

    public bool IsGrounded { get; private set; }
    public bool IsInGoal { get; private set; }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        CheckGround();

        // ジャンプ入力を受け付け（Updateで）
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && IsGrounded)
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        // 入力取得
        float h = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;

        // 重力による水平加速を計算
        float gravityAccelX = 0f;
        if (GravityController.Instance != null && GravityController.Instance.HasSecondaryGravity)
        {
            gravityAccelX = GravityController.Instance.CombinedGravity.x * Time.fixedDeltaTime;
        }

        // 移動処理
        if (h != 0)
        {
            // 入力があるときは入力速度を設定
            rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);
        }
        else if (IsGrounded)
        {
            // 地上で入力なし → 急速減速（摩擦）ただし重力による加速は加える
            float newVelX = rb.velocity.x * 0.8f + gravityAccelX;
            rb.velocity = new Vector2(newVelX, rb.velocity.y);
        }
        // 空中で入力なし → 重力でゆっくり流される（velocity.xそのまま）

        // 最大速度制限
        float clampedX = Mathf.Clamp(rb.velocity.x, -moveSpeed * 1.5f, moveSpeed * 1.5f);
        rb.velocity = new Vector2(clampedX, rb.velocity.y);

        // ジャンプ実行（FixedUpdateで）
        if (jumpRequested)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpRequested = false;
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            // groundCheck未設定なら足元で判定
            IsGrounded = Physics2D.Raycast(transform.position + Vector3.down * 0.5f, Vector2.down, 0.1f, groundLayer);
        }
        else
        {
            IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }

    public void EnterGoal() => IsInGoal = true;
    public void ExitGoal() => IsInGoal = false;

    public void Die()
    {
        GameManager.Instance?.OnCharacterDied();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
