using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hitsPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public float damage = 10f;

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hit Player");
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); 
            }

        }
        Destroy(gameObject);
    }
}
