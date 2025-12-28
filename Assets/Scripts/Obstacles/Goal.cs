using UnityEngine;

/// <summary>
/// ゴールエリア - プレイヤーがゴールしたか判定
/// </summary>
public class Goal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<CharacterBase>(out var player))
        {
            player.EnterGoal();
            AudioManager.Instance?.PlayGoal();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<CharacterBase>(out var player))
        {
            player.ExitGoal();
        }
    }
}
