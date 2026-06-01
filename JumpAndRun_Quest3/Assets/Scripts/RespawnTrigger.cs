using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    [SerializeField]
    private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        // Try to get the CharacterController from whatever entered the trigger
        CharacterController controller = other.gameObject.GetComponent<CharacterController>();

        if (controller != null)
        {
            Respawn(controller);
        }
    }

    private void Respawn(CharacterController controller)
    {
        // Deactivate the CharacterController to avoid collision issues during teleport
        controller.enabled = false;

        // Move the player to the respawn point
        controller.transform.position = respawnPoint.position;

        // Re-activate the CharacterController
        controller.enabled = true;
    }
}
