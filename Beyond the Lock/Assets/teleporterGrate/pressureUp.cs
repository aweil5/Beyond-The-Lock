using System.Collections;
using UnityEngine;

public class PressureUp : MonoBehaviour
{
    public GameObject spikes; // The spikes object to raise
    public float damageAmount = 10f; // Damage dealt by the spikes
    private bool isPressed = false;

    private Vector3 originalPosition; // Store the initial position of the spikes

    private void Start()
    {
        // Save the original position of the spikes
        if (spikes != null)
        {
            originalPosition = spikes.transform.position;
        }

    }

    private void OnTriggerEnter(Collider col)
    {
        if (!isPressed)
        {
            // Raise the spikes
            spikes.transform.position += new Vector3(0, 2f, 0);
            isPressed = true;
            Debug.Log("Spikes raised!");
            // Check if the colliding object has the "Player" tag
            
            Debug.Log("Player detected from the SPIKES!");
            // Access the PlayerHealth component attached to the Player
            PlayerHealth playerHealth = col.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Call the TakeDamage method
                playerHealth.TakeDamage(damageAmount);
            }else{
                Debug.Log("PlayerHealth is null");
            }
            

            // Start coroutine to retract spikes after 1 second
            StartCoroutine(RetractSpikes());
        }
    }

    private IEnumerator RetractSpikes()
    {
        // Wait for 1 second
        yield return new WaitForSeconds(2f);

        // Retract the spikes to their original position
        if (spikes != null)
        {
            spikes.transform.position = originalPosition;
        }

        // Allow the pressure plate to be pressed again
        isPressed = false;
    }
}
