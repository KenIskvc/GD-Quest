using UnityEngine;

/// <summary>
/// Quest 3 - Task 1 (last bullet): A "death zone" placed below the level.
/// Previously this teleported the player back automatically. Now, when the
/// player falls in, we set their health to 0 instead. The UIManager notices
/// the 0 health on its next Update and runs the normal death flow: the
/// GameOver canvas fades in and the HUD fades out, and the player respawns
/// only when they press the Respawn button.
///
/// Put this on a large trigger collider stretched under the whole level.
/// </summary>
public class RespawnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Character character = other.GetComponentInChildren<Character>();

        if (character != null)
        {
            // A big hit guarantees health reaches 0 -> triggers GameOver.
            character.InflictDamage(character.GetMaxHealth());
        }
    }
}
