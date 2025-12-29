using UnityEngine;

/// <summary>
/// 箱 - 重力で動く物体、スイッチを押せる
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Box : MonoBehaviour
{
    private Rigidbody2D rb;
    private GravityController gravityController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // 自前で重力を適用
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        gravityController = FindObjectOfType<GravityController>();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        Vector2 gravity = GetCurrentGravity();
        rb.AddForce(gravity * rb.mass, ForceMode2D.Force);
    }

    private Vector2 GetCurrentGravity()
    {
        if (gravityController != null)
        {
            return gravityController.CombinedGravity;
        }
        return Physics2D.gravity;
    }
}
