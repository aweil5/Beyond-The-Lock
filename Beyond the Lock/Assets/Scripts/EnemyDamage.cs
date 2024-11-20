using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public float damageAmount = 10f; // Amount of damage dealt to the player on collision

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is the player by tag and has a PlayerHealth component
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get the PlayerHealth component from the player
            PlayerHealth playerHealth = collision.gameObject.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Apply damage to the player
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }
}
