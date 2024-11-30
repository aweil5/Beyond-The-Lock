using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pressureUp : MonoBehaviour
{

    public GameObject spikes;
    bool isPressed = false;
   void OnTriggerEnter(Collider col) {
    if(!isPressed) {
    spikes.transform.position += new Vector3(0, .75f, 0);
     isPressed = true;
    }
    
   }
}
