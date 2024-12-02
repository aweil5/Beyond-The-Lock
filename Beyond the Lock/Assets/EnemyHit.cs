using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    private EnemyAI enemyAI;
    // Start is called before the first frame update
    void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Bullet bullet = collision.gameObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            // Take damage
            if (enemyAI != null)
            {
                Debug.Log("Enemy hit Dealing damage of " + bullet.damage);
                enemyAI.TakeDamage((int)bullet.damage);
            }

            // Destroy the bullet
            Destroy(collision.gameObject);
        }
        
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
