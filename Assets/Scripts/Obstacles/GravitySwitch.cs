using UnityEngine;

/// <summary>
/// 重力切替スイッチ - 踏むと追加重力の方向を変更
/// </summary>
public class GravitySwitch : MonoBehaviour
{
    public enum GravityDirection
    {
        Up,
        Down,
        Left,
        Right,
        None
    }

    [Header("Switch Settings")]
    [SerializeField] private GravityDirection gravityDirection = GravityDirection.Up;
    [SerializeField] private Color switchColor = Color.cyan;
    [SerializeField] private bool isOneTimeUse = false;

    private SpriteRenderer spriteRenderer;
    private bool hasBeenUsed = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = switchColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasBeenUsed) return;

        // プレイヤーかどうか確認
        if (collision.GetComponent<CharacterBase>() != null)
        {
            ActivateSwitch();
        }
    }

    private void ActivateSwitch()
    {
        if (GravityController.Instance == null) return;

        Vector2 direction = gravityDirection switch
        {
            GravityDirection.Up => Vector2.up,
            GravityDirection.Down => Vector2.down,
            GravityDirection.Left => Vector2.left,
            GravityDirection.Right => Vector2.right,
            GravityDirection.None => Vector2.zero,
            _ => Vector2.zero
        };

        if (direction == Vector2.zero)
        {
            GravityController.Instance.ClearSecondaryGravity();
        }
        else
        {
            GravityController.Instance.SetSecondaryGravity(direction);
        }

        if (isOneTimeUse)
        {
            hasBeenUsed = true;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.gray;
            }
        }
    }
}
