using System;
using UnityEngine;

public class Teleporter : MonoBehaviour
{


    public Transform targetTeleporter; 
    public Boolean canTeleport = true;
    public AudioClip teleportSound;
    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return; 


        if (other.CompareTag("Player") && canTeleport)
        {
            AudioSource.PlayClipAtPoint(teleportSound, transform.position);

            var playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.velocity = Vector3.zero; 
            }
            
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                
                controller.enabled = false;

                controller.transform.position = targetTeleporter.TransformPoint(new Vector3(0, 100f, 0));

                controller.enabled = true;
            }
            else
            {
                other.transform.position = targetTeleporter.TransformPoint(new Vector3(0, 100f, 0));
            }
            
        }
    }
}
