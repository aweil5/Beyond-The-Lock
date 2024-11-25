using System.Collections;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject receiver; // Assign the receiver GameObject in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Triggered by: {other.name}");

        // Check the receiver
        if (receiver == null)
        {
            Debug.LogError("Receiver GameObject is not assigned!");
            return;
        }

        // Get the root transform
        Transform rootTransform = other.transform.root;
        Debug.Log($"Teleporting {rootTransform.name} to {receiver.transform.position}");

        // Check if the object has a Rigidbody
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"Found Rigidbody on {other.name}, disabling physics temporarily.");
            rb.isKinematic = true;
        }

        // Teleport the object
        rootTransform.position = receiver.transform.position;

        // Re-enable physics
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
}
