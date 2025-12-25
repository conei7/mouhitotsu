using UnityEngine;

/// <summary>
/// 重力切替スイッチ - 踏むと追加重力の方向を変更
/// 重ねがけ対応: 加算モードと上書きモードを選択可能
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

    public enum SwitchMode
    {
        Add,        // 重ねがけ（加算）
        Set,        // 上書き
        Clear       // リセット
    }

    [Header("Switch Settings")]
    [SerializeField] private GravityDirection gravityDirection = GravityDirection.Up;
    [SerializeField] private SwitchMode switchMode = SwitchMode.Add;  // デフォルトは重ねがけ
    [SerializeField, Range(0.1f, 2f)] private float strength = 1f;    // 加算時の強度
    
    [Header("Custom Direction (when Custom selected)")]
    [Tooltip("任意の重力方向ベクトル（正規化されます）")]
    [SerializeField] private Vector2 customDirection = Vector2.up;
    [Tooltip("角度で指定（度数法、右が0度、反時計回り）")]
    [SerializeField] private float customAngle = 90f;
    [SerializeField] private bool useAngle = false;  // trueならcustomAngleを使用

    [Header("Appearance")]
    [SerializeField] private Color switchColor = Color.cyan;
    [SerializeField] private bool isOneTimeUse = false;
    [SerializeField] private bool autoColorFromDirection = true;  // 方向に応じて自動で色を設定

    private SpriteRenderer spriteRenderer;
    private bool hasBeenUsed = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    private void OnValidate()
    {
        // エディタで値が変更されたとき色を更新
        if (autoColorFromDirection)
        {
            UpdateColorFromDirection();
        }
    }

    private void UpdateColor()
    {
        if (spriteRenderer != null)
        {
            if (autoColorFromDirection)
            {
                UpdateColorFromDirection();
            }
            else
            {
                spriteRenderer.color = switchColor;
            }
        }
    }

    private void UpdateColorFromDirection()
    {
        if (spriteRenderer == null) return;

        // Clearモードの場合はグレー
        if (switchMode == SwitchMode.Clear || gravityDirection == GravityDirection.None)
        {
            spriteRenderer.color = Color.gray;
            switchColor = Color.gray;
            return;
        }

        Vector2 dir = GetDirectionVector();
        
        if (dir == Vector2.zero)
        {
            spriteRenderer.color = Color.gray;
            return;
        }

        // 方向に基づいて色を決定（HSVカラーホイール）
        float angle = Mathf.Atan2(dir.y, dir.x);
        float hue = (angle + Mathf.PI) / (2 * Mathf.PI);  // 0-1の範囲に正規化
        spriteRenderer.color = Color.HSVToRGB(hue, 0.8f, 1f);
        switchColor = spriteRenderer.color;
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

    private Vector2 GetDirectionVector()
    {
        if (gravityDirection == GravityDirection.Custom)
        {
            if (useAngle)
            {
                // 角度からベクトルを計算
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

    private void ActivateSwitch()
    {
        if (GravityController.Instance == null) return;

        switch (switchMode)
        {
            case SwitchMode.Add:
                // 重ねがけ（加算）
                Vector2 direction = GetDirectionVector();
                if (direction != Vector2.zero)
                {
                    GravityController.Instance.AddSecondaryGravity(direction, strength);
                }
                break;

            case SwitchMode.Set:
                // 上書き
                Vector2 setDirection = GetDirectionVector();
                if (setDirection != Vector2.zero)
                {
                    GravityController.Instance.SetSecondaryGravity(setDirection);
                }
                else
                {
                    GravityController.Instance.ClearSecondaryGravity();
                }
                break;

            case SwitchMode.Clear:
                // リセット
                GravityController.Instance.ClearSecondaryGravity();
                break;
        }

        if (isOneTimeUse)
        {
            hasBeenUsed = true;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
        }
    }

    /// <summary>
    /// 外部からカスタム方向を設定
    /// </summary>
    public void SetCustomDirection(Vector2 direction)
    {
        gravityDirection = GravityDirection.Custom;
        useAngle = false;
        customDirection = direction;
        UpdateColor();
    }

    /// <summary>
    /// 外部からカスタム角度を設定
    /// </summary>
    public void SetCustomAngle(float angleDegrees)
    {
        gravityDirection = GravityDirection.Custom;
        useAngle = true;
        customAngle = angleDegrees;
        UpdateColor();
    }
}
