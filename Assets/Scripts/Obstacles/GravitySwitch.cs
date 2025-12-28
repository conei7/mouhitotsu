using UnityEngine;

/// <summary>
/// 重力切替スイッチ - 踏むとオンオフで追加重力を切り替え
/// 各スイッチで重力の強度を個別に設定可能
/// </summary>
public class GravitySwitch : MonoBehaviour
{
    public enum GravityDirection
    {
        Up,
        Down,
        Left,
        Right,
        None,
        Custom  // 任意の角度を使用
    }

    [Header("Switch Settings")]
    [SerializeField] private GravityDirection gravityDirection = GravityDirection.Up;
    [SerializeField, Range(0.1f, 3f)] private float gravityStrength = 1f;  // この重力の強度（加速度倍率）
    
    [Header("Custom Direction (when Custom selected)")]
    [Tooltip("任意の重力方向ベクトル（正規化されます）")]
    [SerializeField] private Vector2 customDirection = Vector2.up;
    [Tooltip("角度で指定（度数法、右が0度、反時計回り）")]
    [SerializeField] private float customAngle = 90f;
    [SerializeField] private bool useAngle = false;

    [Header("Appearance")]
    [SerializeField] private Color onColor = Color.cyan;
    [SerializeField] private Color offColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);
    [SerializeField] private bool autoColorFromDirection = true;
    
    [Header("Sprites (Optional)")]
    [Tooltip("スプライトを設定すると色ではなく画像で切り替え")]
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    private SpriteRenderer spriteRenderer;
    private bool isActive = false;  // 現在オンかオフか
    private Vector2 myGravityContribution = Vector2.zero;  // このスイッチが追加した重力
    private float lastToggleTime = -999f;  // 最後にトグルした時間
    private const float COOLDOWN = 1f;  // クールダウン時間（秒）

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        UpdateVisual();
    }

    private void UpdateOnColorFromDirection()
    {
        if (gravityDirection == GravityDirection.None)
        {
            onColor = Color.gray;
            return;
        }

        Vector2 dir = GetDirectionVector();
        if (dir == Vector2.zero)
        {
            onColor = Color.gray;
            return;
        }

        float angle = Mathf.Atan2(dir.y, dir.x);
        float hue = (angle + Mathf.PI) / (2 * Mathf.PI);
        onColor = Color.HSVToRGB(hue, 0.8f, 1f);
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        // スプライトが設定されていれば切り替え
        if (onSprite != null && offSprite != null)
        {
            spriteRenderer.sprite = isActive ? onSprite : offSprite;
            spriteRenderer.color = Color.white; // スプライトの色をそのまま使う
        }
        else
        {
            // スプライトがなければ色で切り替え
            if (autoColorFromDirection)
            {
                UpdateOnColorFromDirection();
            }
            spriteRenderer.color = isActive ? onColor : offColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<CharacterBase>() != null)
        {
            // クールダウンチェック
            if (Time.time - lastToggleTime < COOLDOWN) return;
            
            ToggleSwitch();
            lastToggleTime = Time.time;
            
            // スイッチ音
            AudioManager.Instance?.PlaySwitch();
        }
    }

    private Vector2 GetDirectionVector()
    {
        if (gravityDirection == GravityDirection.Custom)
        {
            if (useAngle)
            {
                float rad = customAngle * Mathf.Deg2Rad;
                return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            }
            else
            {
                return customDirection.normalized;
            }
        }

        return gravityDirection switch
        {
            GravityDirection.Up => Vector2.up,
            GravityDirection.Down => Vector2.down,
            GravityDirection.Left => Vector2.left,
            GravityDirection.Right => Vector2.right,
            GravityDirection.None => Vector2.zero,
            _ => Vector2.zero
        };
    }

    private void ToggleSwitch()
    {
        if (GravityController.Instance == null) return;

        if (isActive)
        {
            // オフにする: 追加した重力を削除
            GravityController.Instance.RemoveSecondaryGravity(myGravityContribution);
            myGravityContribution = Vector2.zero;
            isActive = false;
        }
        else
        {
            // オンにする: 重力を追加
            Vector2 direction = GetDirectionVector();
            if (direction != Vector2.zero)
            {
                myGravityContribution = direction.normalized * gravityStrength;
                GravityController.Instance.AddSecondaryGravity(direction, gravityStrength);
                isActive = true;
            }
        }

        UpdateVisual();
    }

    /// <summary>
    /// 外部からカスタム方向を設定
    /// </summary>
    public void SetCustomDirection(Vector2 direction)
    {
        gravityDirection = GravityDirection.Custom;
        useAngle = false;
        customDirection = direction;
        UpdateVisual();
    }

    /// <summary>
    /// 外部からカスタム角度を設定
    /// </summary>
    public void SetCustomAngle(float angleDegrees)
    {
        gravityDirection = GravityDirection.Custom;
        useAngle = true;
        customAngle = angleDegrees;
        UpdateVisual();
    }
}
