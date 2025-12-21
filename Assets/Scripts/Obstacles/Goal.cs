using UnityEngine;

/// <summary>
/// ゴールエリア - 対応するキャラクターがゴール判定
/// </summary>
public class Goal : MonoBehaviour
{
    public enum GoalType { Blue, Red }

    [SerializeField] private GoalType goalType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (goalType == GoalType.Blue && other.TryGetComponent<CharacterBlue>(out var blue))
        {
            blue.EnterGoal();
        }
        else if (goalType == GoalType.Red && other.TryGetComponent<CharacterRed>(out var red))
        {
            red.EnterGoal();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (goalType == GoalType.Blue && other.TryGetComponent<CharacterBlue>(out var blue))
        {
            blue.ExitGoal();
        }
        else if (goalType == GoalType.Red && other.TryGetComponent<CharacterRed>(out var red))
        {
            red.ExitGoal();
        }
    }
}
