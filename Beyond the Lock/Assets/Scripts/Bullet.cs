using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 20f; // Damage dealt by the bullet

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the bullet hits an enemy
        if (collision.gameObject.CompareTag("Target"))
        {
            Debug.Log("Hit target");
            // Get the enemy's health script
            EnemyAI enemyHealth = collision.gameObject.GetComponent<EnemyAI>();

            if (enemyHealth != null)
            {
                Debug.Log("Hit enemy");
                enemyHealth.TakeDamage(100); // Deal damage to the enemy
            }

            // Destroy the bullet
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
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
