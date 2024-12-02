using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float damage = 10f; 
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected from enemy shot");
        if (collision.gameObject.CompareTag("Player"))
        {   
            Debug.Log("Player hit");
           
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); 
            }

            Destroy(gameObject);
        }

        
        
    }
}
