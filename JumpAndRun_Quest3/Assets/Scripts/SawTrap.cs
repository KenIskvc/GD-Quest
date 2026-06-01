using UnityEngine;

/// <summary>
/// Quest 3 - Task 3: A dangerous saw trap. Drains the player's health while
/// they are in contact, following the lecture's OnTriggerStay pattern
/// (damagePerSecond * Time.fixedDeltaTime for a smooth, frame-rate independent
/// drain).
///
/// Put this on the saw prefab and give it a trigger collider. If the saw also
/// spins/moves, that animation is independent of this damage logic.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SawTrap : MonoBehaviour
{
    [SerializeField]
    private float damagePerSecond = 25.0f;

    // OnTriggerStay() is called every physics update - so we use Time.fixedDeltaTime!
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var character = other.GetComponentInChildren<Character>();

            if (character != null)
            {
                character.InflictDamage(this.damagePerSecond * Time.fixedDeltaTime);
            }
        }
    }
}
