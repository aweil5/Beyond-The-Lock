using UnityEngine;

public class Teleporter : MonoBehaviour
{


    public Transform targetTeleporter; // Reference to the target teleporter

    private void OnTriggerEnter(Collider other)
    {


        if (other.CompareTag("Player"))
        {

            var playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.velocity = Vector3.zero; // Reset velocity if applicable
            }
            
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                // Disable CharacterController to avoid conflicts
                controller.enabled = false;

                // Teleport the player slightly above the ground
                // controller.transform.position = targetTeleporter.position + new Vector3(0, 50f, 0);
                controller.transform.position = targetTeleporter.TransformPoint(new Vector3(0, 100f, 0));

                // Re-enable CharacterController
                controller.enabled = true;
            }
            else
            {
                // Fallback for non-CharacterController objects
                other.transform.position = targetTeleporter.TransformPoint(new Vector3(0, 100f, 0));
            }

            // Optional: Reset velocity to prevent falling
            
        }
    }
}
