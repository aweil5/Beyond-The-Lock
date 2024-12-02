using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpotlightDetection : MonoBehaviour
{
    public Transform spotlight;       // Assign the Spotlight transform in the Inspector
    public float detectionRange = 100f;  // The range of the spotlight
    public float spotAngle = 40f;     // The angle of the spotlight
    public LayerMask detectionMask;   // Set this to detect the "Player" layer
    public float visibilityThreshold = 0.3f;  // 30% visibility threshold

    [Header("Audio Settings")]
    public AudioClip detectionSound;  // Drag and drop the detection audio clip in the Inspector
    public AudioSource audioSource;   // Drag and drop an AudioSource component in the Inspector
    public float audioCooldownTime = 5f;  // Prevent sound from playing too frequently
    private float lastDetectionTime = -5f;

    private Boolean enemiesSpawned = false;
    private float teleportDisableTime = 30f;

    public List<Vector3> enemySpawnPoints = new List<Vector3>();
    public GameObject enemyPrefab;

    void Start()
    {
        // If no AudioSource is assigned, try to get one on this GameObject
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // If still no AudioSource, add one
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ensure audio settings are appropriate
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

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
                // Get the player's collider
                Collider playerCollider = hit;

                // Perform multiple raycasts to check visibility percentage
                float visiblePercentage = CheckPlayerVisibility(playerCollider);

                if (visiblePercentage > visibilityThreshold && !enemiesSpawned)
                {
                    enemiesSpawned = true;
                    Debug.Log($"Player detected! Visible percentage: {visiblePercentage * 100}%");

                    // Print the player's coordinates
                    Vector3 playerPosition = playerCollider.transform.position;
                    Debug.Log("Player Position: " + playerPosition);

                    // Play detection sound
                    DisablePortalsByName();
                    PlayDetectionSound();
                    SpawnEnemies();
                }
            }
        }
    }

    private void DisablePortalsByName()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        List<GameObject> portals = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Portal(Clone)")
            {
                portals.Add(obj);
                obj.SetActive(false); // Disable the portal
            }
        }

        Debug.Log($"Disabled {portals.Count} portals.");

        StartCoroutine(ReenablePortalsAfterDelay(portals, teleportDisableTime));
    }

    private IEnumerator ReenablePortalsAfterDelay(List<GameObject> portals, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (GameObject portal in portals)
        {
            if (portal != null) // Ensure the portal still exists in case it's destroyed
            {
                portal.SetActive(true); // Re-enable the portal
            }
        }

        Debug.Log($"Re-enabled {portals.Count} portals.");
    }


    private void SpawnEnemies()
    {
        if (enemySpawnPoints.Count == 0)
        {
            Debug.LogWarning("No enemy spawn points assigned!");
            return;
        }
        foreach (Vector3 spawnPoint in enemySpawnPoints)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
            followPlayer followPlayer = enemy.GetComponent<followPlayer>();
            if (followPlayer != null)
            {
                followPlayer.player = GameObject.FindGameObjectWithTag("Body To Follow").transform;
            }
        }
    }

    void PlayDetectionSound()
    {
        // Check if enough time has passed since last detection
        if (Time.time - lastDetectionTime >= audioCooldownTime)
        {
            // Check if we have a sound and an audio source
            if (detectionSound != null && audioSource != null)
            {
                // Play the detection sound
                audioSource.PlayOneShot(detectionSound);

                // Update last detection time
                lastDetectionTime = Time.time;
            }
            else
            {
                // Log a warning if audio is not set up correctly
                if (detectionSound == null)
                    Debug.LogWarning("Detection sound is not assigned!");
                if (audioSource == null)
                    Debug.LogWarning("No AudioSource found for detection sound!");
            }
        }
    }

    float CheckPlayerVisibility(Collider playerCollider)
    {
        // If it's a BoxCollider, sample multiple points
        if (playerCollider is BoxCollider boxCollider)
        {
            // Get the bounds of the box collider
            Bounds bounds = boxCollider.bounds;

            // Define sample points across the player's collider
            Vector3[] samplePoints = new Vector3[]
            {
                bounds.center,
                bounds.min,
                bounds.max,
                new Vector3(bounds.min.x, bounds.center.y, bounds.center.z),
                new Vector3(bounds.max.x, bounds.center.y, bounds.center.z),
                new Vector3(bounds.center.x, bounds.min.y, bounds.center.z),
                new Vector3(bounds.center.x, bounds.max.y, bounds.center.z),
                new Vector3(bounds.center.x, bounds.center.y, bounds.min.z),
                new Vector3(bounds.center.x, bounds.center.y, bounds.max.z)
            };

            int visiblePoints = 0;
            int totalPoints = samplePoints.Length;

            // Check visibility for each sample point
            foreach (Vector3 point in samplePoints)
            {
                Vector3 directionToPoint = (point - spotlight.position).normalized;
                Ray ray = new Ray(spotlight.position, directionToPoint);

                // Raycast with a slightly extended distance to ensure we hit the player
                float rayDistance = Vector3.Distance(spotlight.position, point) + 0.1f;

                if (Physics.Raycast(ray, out RaycastHit hitInfo1, rayDistance))
                {
                    // Check if this raycast hit the player collider
                    if (hitInfo1.collider == playerCollider)
                    {
                        visiblePoints++;
                    }
                }
            }

            // Calculate visibility percentage
            return (float)visiblePoints / totalPoints;
        }

        // Fallback for other collider types (less accurate)
        Ray centerRay = new Ray(spotlight.position, (playerCollider.bounds.center - spotlight.position).normalized);
        if (Physics.Raycast(centerRay, out RaycastHit hitInfo, detectionRange))
        {
            return hitInfo.collider == playerCollider ? 1f : 0f;
        }

        return 0f;
    }
}