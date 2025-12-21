using UnityEngine;

/// <summary>
/// 落下死判定エリア
/// </summary>
public class FallZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<CharacterBase>(out var character))
        {
            character.Die();
        }
    }
}
