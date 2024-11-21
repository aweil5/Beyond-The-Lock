using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDetectPlayer : MonoBehaviour
{
    public Transform player;               // Reference to the player object
    public float detectionAngle = 45f;      // Field of view angle in degrees
    public float detectionDistance = 10f;   // Maximum distance for detection
    private Color originalColor;            // To store the original color of the camera
    private Renderer cameraRenderer;        // Renderer for the camera object


    private void Start()
    {
        // Get the Renderer component and save the original color
        cameraRenderer = GetComponent<Renderer>();
        if (cameraRenderer != null)
        {
            originalColor = cameraRenderer.material.color;
        }





    }

    private void Update()
    {
        if (IsPlayerInView())
        {
            // Change color to red if the player is in view
            cameraRenderer.material.color = Color.red;
        }
        else
        {
            // Revert to the original color if the player is not in view
            cameraRenderer.material.color = originalColor;
        }
    }

    private bool IsPlayerInView()
    {
        // Calculate direction to the player
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Check if the player is within detection distance
        if (distanceToPlayer > detectionDistance)
        {
            return false;
        }

        // Calculate angle between the camera's forward direction and the direction to the player
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Check if the player is within the detection angle
        if (angleToPlayer <= detectionAngle / 2)
        {
            // Perform a raycast to check if there's a clear line of sight
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, detectionDistance))
            {
                // Return true if the raycast hits the player
                return hit.transform == player;
            }
        }

        return false;
    }
}
