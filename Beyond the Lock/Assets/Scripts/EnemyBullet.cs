using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float damage = 10f; // Damage dealt by the bullet

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the bullet hits the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get the PlayerHealth component from the player
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); // Apply damage to the player
            }

            // Destroy the bullet after hitting the player
            Destroy(gameObject);
        }

        
        Destroy(gameObject);
        
    }
}
