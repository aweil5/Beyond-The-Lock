using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float health = 100f; // Set initial health

    public void TakeDamage(float damage)
    {
        health -= damage; // Reduce health

        if (health <= 0)
        {
            Die(); // Handle enemy death
        }
    }

    private void Die()
    {
        // Destroy the enemy GameObject
        Destroy(gameObject);
    }
}
