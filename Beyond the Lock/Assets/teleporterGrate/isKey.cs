using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class isKey : MonoBehaviour
{

    public GameObject spikes;

    public GameObject key;
    bool isPressed = false;

    private float decreaseFac = -5f / 3f;
   void OnTriggerEnter(Collider col) {
    if(!isPressed) {
    spikes.transform.position += new Vector3(0, decreaseFac, 0);
     isPressed = true;
     Destroy(key);
    }
    
   }
}
