using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    public Transform player; // Assign the player's transform in the Inspector
    public float detectionRange = 10f; // The maximum distance the enemy can detect the player
    public float detectionAngle = 45f; // The angle within which the player can be detected
    public LayerMask obstructionMask; // The layer mask to detect obstructions like walls
    private Vector3 originalScale; // Store the original scale of the enemy

    private bool playerInSight = false;

    void Start()
    {
        // Store the initial scale of the enemy object
        originalScale = transform.localScale;
    }
    void Update()
    {
        DetectPlayer();
    }

    private void DetectPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is within the cone's distance range
        if (distanceToPlayer <= detectionRange)
        {
            // Check if player is within the cone's angle
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer < detectionAngle / 2)
            {
                // Use raycasting to ensure there is no obstruction in the line of sight
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstructionMask))
                {
                    // Player is within cone and in sight
                    if (!playerInSight)
                    {
                        playerInSight = true;
                        OnPlayerDetected();
                    }
                }
                else
                {
                    playerInSight = false;
                    ResetScale();
                }
            }
            else
            {
                playerInSight = false;
                ResetScale();
            }
        }
        else
        {
            playerInSight = false;
            ResetScale();
        }
    }

    private void OnPlayerDetected()
    {
        // Actions to take when the player is detected
        Debug.Log("Player detected by " + gameObject.name);
        // Shrink the object to half size
        transform.localScale = originalScale * 0.5f;
    }

    private void ResetScale()
    {
        // Reset the scale if the player is no longer detected
        if (transform.localScale != originalScale)
        {
            transform.localScale = originalScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the detection range and angle in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 leftBoundary = Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward * detectionRange;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
