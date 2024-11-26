using UnityEngine;

public class Teleporter2 : MonoBehaviour
{
    public Transform targetTeleporter; // Reference to the target teleporter

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                // Disable CharacterController to avoid conflicts
                controller.enabled = false;

                // Teleport the player slightly above the ground
                controller.transform.position = targetTeleporter.position + Vector3.up * 0.5f;

                // Re-enable CharacterController
                controller.enabled = true;
            }
            else
            {
                // Fallback for non-CharacterController objects
                other.transform.position = targetTeleporter.position + Vector3.up * 0.5f;
            }

            // Optional: Reset velocity to prevent falling
            var playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.velocity = Vector3.zero; // Reset velocity if applicable
            }
        }
    }
}
