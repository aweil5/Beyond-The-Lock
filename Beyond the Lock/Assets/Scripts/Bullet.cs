using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 20f;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet collided with: " + collision.gameObject.name);
      
        if (collision.gameObject.CompareTag("Target"))
        {
           
            Debug.Log("Hit target");
           
            EnemyAI enemyHealth = collision.gameObject.GetComponent<EnemyAI>();

            if (enemyHealth != null)
            {
                Debug.Log("Hit enemy");
                enemyHealth.TakeDamage(100); 
            }

            
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            
            Destroy(gameObject);
        }
        if (!collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
