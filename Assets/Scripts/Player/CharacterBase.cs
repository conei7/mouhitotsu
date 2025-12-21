using UnityEngine;

/// <summary>
/// キャラクターの基底クラス - 移動、ジャンプ、ゴール判定
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public abstract class CharacterBase : MonoBehaviour
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
        // 移動
        float h = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;

        rb.velocity = new Vector2(h * moveSpeed, rb.velocity.y);

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
