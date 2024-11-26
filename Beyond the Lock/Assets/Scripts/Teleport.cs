using System.Collections;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public GameObject receiver; // Assign the receiver GameObject in the Inspector
    public GameObject player;

    public void Start()
    {
        player = GameObject.Find("Player");
    }   

    void OnTriggerEnter(Collider other)

    {

        Debug.Log("Teleporting player");
        Transform parent = other.transform.parent;
        if (parent != null)
        {
            parent.position = receiver.transform.position;
        }
        else
        {
            other.transform.position = receiver.transform.position;
        }
        

    }

    // private IEnumerator TeleportPlayer(Collider other)
    // {
    //     // Add a delay if needed

    //     Transform parentTransform = other.transform.parent;
    //     if (parentTransform != null)
    //     {
    //         GameObject parentObject = parentTransform.gameObject;
    //         foreach (Transform child in parentObject.transform)
    //         {
    //             child.position = receiver.transform.position;
    //         }
    //         parentObject.transform.position = receiver.transform.position;
            

    //     }
    //     else
    //     {
    //         foreach (Transform child in other.transform)
    //         {
    //             child.position = receiver.transform.position;
    //         }
    //     }
        
    //     yield return new WaitForSeconds(0.1f); // Add a small delay
    // }

}
