using UnityEngine;

public class Teleporter2 : MonoBehaviour
{
    public Transform targetTeleporter;
    public AudioClip teleportNoise;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.enabled = false;
                controller.transform.position = targetTeleporter.position + Vector3.up * 0.5f;
                controller.enabled = true;
            }
            else
            {

                other.transform.position = targetTeleporter.position + Vector3.up * 0.5f;
            }
            var playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.velocity = Vector3.zero; 
            }


        }
    }
}
