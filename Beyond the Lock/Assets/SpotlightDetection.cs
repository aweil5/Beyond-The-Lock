using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightDetection : MonoBehaviour
{
    public Transform spotlight;       // Assign the Spotlight transform in the Inspector
    public float detectionRange = 100f;  // The range of the spotlight
    public float spotAngle = 40f;     // The angle of the spotlight
    public LayerMask detectionMask;   // Set this to detect the "Player" layer

    /*public GameObject npcPrefab;      // Assign the NPC prefab in the Inspector
    public List<Vector3> npcSpawns;   // List of spawn locations for NPCs*/

    void Update()
    {
        DetectPlayerWithRaycast();
    }

    void DetectPlayerWithRaycast()
    {
        // Use Physics.OverlapSphere to find all objects in range
        Collider[] hits = Physics.OverlapSphere(spotlight.position, detectionRange, detectionMask);

        foreach (Collider hit in hits)
        {
            // Calculate direction to the object
            Vector3 directionToTarget = (hit.transform.position - spotlight.position).normalized;

            // Check if the object is within the spotlight's cone
            float angleToTarget = Vector3.Angle(spotlight.forward, directionToTarget);
            if (angleToTarget <= spotAngle / 2)
            {
                // Perform a raycast to verify line of sight
                Ray ray = new Ray(spotlight.position, directionToTarget);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, detectionRange, detectionMask))
                {
                    if (hitInfo.collider.CompareTag("Player"))
                    {
                        Debug.Log("Player detected in spotlight!");
                        
                        // Print the player's coordinates
                        Vector3 playerPosition = hitInfo.collider.transform.position;
                        Debug.Log("Player Position: " + playerPosition);
                        
                        // Add additional custom logic here

                        // Spawn NPCs at designated locations
                        /*SpawnNPCs();*/    
                    }
                }
            }
        }
    }

    /*void SpawnNPCs()
    {
        foreach (Vector3 spawnLocation in npcSpawns)
        {
            Instantiate(npcPrefab, spawnLocation, Quaternion.identity);
        }
        Debug.Log("NPCs spawned at all designated locations.");
    }*/
}