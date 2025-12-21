using UnityEngine;

/// <summary>
/// 触れると死亡するトゲ
/// </summary>
public class Spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<CharacterBase>(out var character))
        {
            character.Die();
        }
    }
}
