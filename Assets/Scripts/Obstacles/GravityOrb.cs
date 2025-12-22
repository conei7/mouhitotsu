using UnityEngine;

/// <summary>
/// 重力オーブ - 拾うと一時的に追加重力を付与
/// </summary>
public class GravityOrb : MonoBehaviour
{
    [Header("Orb Settings")]
    [SerializeField] private Vector2 gravityDirection = Vector2.up;
    [SerializeField] private float duration = 5f;
    [SerializeField] private Color orbColor = Color.blue;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = orbColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<CharacterBase>() != null)
        {
            CollectOrb();
        }
    }

    private void CollectOrb()
    {
        if (GravityController.Instance == null) return;

        GravityController.Instance.SetSecondaryGravity(gravityDirection);

        // 一定時間後に重力をクリア
        StartCoroutine(ClearGravityAfterDuration());
    }

    private System.Collections.IEnumerator ClearGravityAfterDuration()
    {
        // オーブを非表示にするが、コルーチン完了まで破棄しない
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(duration);

        // 別のオーブやスイッチで上書きされていない場合のみクリア
        if (GravityController.Instance != null && 
            GravityController.Instance.SecondaryGravityDirection == gravityDirection.normalized)
        {
            GravityController.Instance.ClearSecondaryGravity();
        }

        Destroy(gameObject);
    }
}
