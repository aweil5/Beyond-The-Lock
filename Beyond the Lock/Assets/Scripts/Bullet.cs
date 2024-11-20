using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 20f; // Damage dealt by the bullet

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the bullet hits an enemy
        if (collision.gameObject.CompareTag("Target"))
        {
            // Get the enemy's health script
            EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage); // Deal damage to the enemy
            }

            // Destroy the bullet
            Destroy(gameObject);
        }

        // Destroy the bullet if it hits something else (e.g., walls)
        if (!collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
