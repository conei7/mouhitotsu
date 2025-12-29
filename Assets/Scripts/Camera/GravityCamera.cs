using UnityEngine;

/// <summary>
/// 重力方向に応じてカメラを回転させる
/// 重力が常に画面下になるようにする
/// </summary>
public class GravityCamera : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("回転のスムーズさ（大きいほど速い）")]
    [SerializeField] private float rotationSpeed = 5f;
    
    [Tooltip("プレイヤーを追従するか")]
    [SerializeField] private bool followPlayer = true;
    
    [Tooltip("追従のスムーズさ")]
    [SerializeField] private float followSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform player;

    private float targetRotation = 0f;

    private void Start()
    {
        FindPlayer();
    }

    private void OnEnable()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            var playerObj = FindObjectOfType<CharacterBase>();
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void LateUpdate()
    {
        UpdateRotation();
        
        if (followPlayer && player != null)
        {
            UpdatePosition();
        }
    }

    private void UpdateRotation()
    {
        Vector2 gravity = Physics2D.gravity;
        
        if (GravityController.Instance != null)
        {
            gravity = GravityController.Instance.CombinedGravity;
        }

        // 無重力時はプレイヤーの足元の壁が画面下になるように
        if (gravity.sqrMagnitude < 0.01f)
        {
            // プレイヤーの壁法線を取得
            var playerChar = player?.GetComponent<CharacterBase>();
            if (playerChar != null && playerChar.IsZeroGravity)
            {
                // 壁法線の逆方向（足元方向）が画面下になるように
                Vector2 feetDir = -playerChar.ZeroGravityWallNormal;
                targetRotation = Mathf.Atan2(feetDir.x, -feetDir.y) * Mathf.Rad2Deg;
            }
        }
        else
        {
            // 通常：重力方向が画面下になるように
            targetRotation = Mathf.Atan2(gravity.x, -gravity.y) * Mathf.Rad2Deg;
        }

        // 現在の回転をスムーズに目標に近づける
        float currentRotation = transform.eulerAngles.z;
        float newRotation = Mathf.LerpAngle(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, newRotation);
    }

    private void UpdatePosition()
    {
        Vector3 targetPos = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 画面の「右方向」をワールド座標で取得
    /// </summary>
    public Vector2 GetScreenRight()
    {
        float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    /// <summary>
    /// 画面の「上方向」をワールド座標で取得
    /// </summary>
    public Vector2 GetScreenUp()
    {
        float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
        return new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
    }
}
