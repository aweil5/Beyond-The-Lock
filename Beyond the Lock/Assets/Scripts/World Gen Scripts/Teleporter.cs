using System;
using UnityEngine;
public class Teleporter : MonoBehaviour
{
    public GameObject sender;
    public GameObject receiver;

    public void SetSender(GameObject sender)
    {
        this.sender = sender;
    }

    public void SetReceiver(GameObject receiver)
    {
        this.receiver = receiver;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject != sender)
        {
            Vector3 receiverPosition = receiver.transform.position;
            other.transform.position = new Vector3(receiverPosition.x, receiverPosition.y + 1, receiverPosition.z);
            Camera.main.transform.position = new Vector3(receiverPosition.x, receiverPosition.y + 1, receiverPosition.z);
            Debug.Log("Teleported to receiver");
        }

        
    }
}