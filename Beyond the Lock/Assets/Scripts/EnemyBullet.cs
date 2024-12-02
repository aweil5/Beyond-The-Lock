using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float damage = 10f; 
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
           
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); 
            }

            Destroy(gameObject);
        }

        
        Destroy(gameObject);
        
    }
}
