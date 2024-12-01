using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pressureUp : MonoBehaviour
{
    public GameObject spikes; // The spikes object to raise
    public float damageAmount = 10f; // Damage dealt by the spikes
    private bool isPressed = false;

    void OnTriggerEnter(Collider col)
    {
        if (!isPressed)
        {
            // Raise the spikes
            spikes.transform.position += new Vector3(0, .75f, 0);
            isPressed = true;

            // Check if the colliding object has the "Player" tag
            if (col.CompareTag("Player")) // Make sure the player is tagged as "Player"
            {
                // Access the PlayerHealth component attached to the Player
                PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Call the TakeDamage method
                    playerHealth.TakeDamage(damageAmount);
                }
                else
                {
                    Debug.LogWarning("No PlayerHealth script found on the player!");
                }
            }
        }
    }
}
