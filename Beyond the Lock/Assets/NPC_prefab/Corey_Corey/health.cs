using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class health : MonoBehaviour
{
    // Start is called before the first frame update
    public float playerHealth;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(playerHealth < 0f){
            followPlayer.isPlayerAlive = false;
            Destroy(gameObject);
        }
    }
}
