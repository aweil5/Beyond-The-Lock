using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public float damageAmount = 10f; // Amount of damage dealt to the player on collision

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponentInParent<PlayerHealth>();

                playerHealth.TakeDamage(damageAmount);
            
        }
    }
}
