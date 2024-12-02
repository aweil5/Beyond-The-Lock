using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 5f; // Destroy after 5 seconds
    public float damage = 10f; // Damage value for the projectile

    void Start()
    {
        Destroy(gameObject, lifetime); // Automatically destroy after a duration
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the projectile hit a player or enemy
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            // Example: Apply damage (requires a health script)
            // collision.gameObject.GetComponent<Health>().TakeDamage(damage);

            if (collision.gameObject.CompareTag("Player"))
            {

                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }else{
                    Debug.Log("PlayerHealth is null");
                    GameObject playerParent = collision.gameObject.transform.parent.gameObject;
                    playerHealth = playerParent.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);
                    }
                }
                Destroy(gameObject);
            }

            
        }
        else if (collision.gameObject.CompareTag("Target"))
        {
            return;
            // Do nothing if the projectile hits another projectile
        }
        else
        {
            Destroy(gameObject);
        }

        // Destroy the projectile after impact
    }
}
