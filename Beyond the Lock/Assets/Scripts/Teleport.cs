using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject reciever;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform parentTransform = other.transform.parent;
        if (parentTransform != null)
        {
            parentTransform.position = reciever.transform.position;
        }
        else
        {
            other.transform.position = reciever.transform.position;
        }
    }
}
