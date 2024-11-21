using System.Collections;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject receiver; // Assign the receiver GameObject in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player entered");

        // Get the highest parent in the hierarchy (root transform)
        Transform rootTransform = other.transform.root;

        // Teleport the root GameObject to the receiver's position
        rootTransform.position = receiver.transform.position;
    }
}
