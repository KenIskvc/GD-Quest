using UnityEngine;

/// <summary>
/// Quest 3 (supporting): A collectible coin. When the player walks into its
/// trigger, it tells the UIManager to increment the coin counter and removes
/// itself. Put this on the coin prefabs (coin-bronze/silver/gold) and make
/// sure their collider has 'Is Trigger' enabled.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.CollectCoin();
        }

        Destroy(this.gameObject);
    }
}
