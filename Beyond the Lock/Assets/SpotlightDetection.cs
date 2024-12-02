using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpotlightDetection : MonoBehaviour
{
    public Transform spotlight;
    public float detectionRange = 100f;
    public float spotAngle = 40f;
    public LayerMask detectionMask;
    public float visibilityThreshold = 0.3f;

    [Header("Audio Settings")]
    public AudioClip telportDown;
    public AudioClip teleportUp;
    public AudioClip detectionSound;
    public AudioSource audioSource;
    public float audioCooldownTime = 5f;
    private float lastDetectionTime = -5f;

    public bool enemiesSpawned = false;
    public float teleportDisableTime = 15f;

    public List<Vector3> enemySpawnPoints = new List<Vector3>();
    public GameObject enemyPrefab;

    public bool specialCam = false;
    public List<GameObject> roomEnemies = new List<GameObject>();

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

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
        Collider[] hits = Physics.OverlapSphere(spotlight.position, detectionRange, detectionMask);
        if (!enemiesSpawned)
        {
            foreach (Collider hit in hits)
            {
                Vector3 directionToTarget = (hit.transform.position - spotlight.position).normalized;
                float angleToTarget = Vector3.Angle(spotlight.forward, directionToTarget);
                if (angleToTarget <= spotAngle / 2)
                {
                    Collider playerCollider = hit;
                    float visiblePercentage = CheckPlayerVisibility(playerCollider);

                    if (visiblePercentage > visibilityThreshold && !enemiesSpawned)
                    {
                        Debug.Log($"Player detected! Visible percentage: {visiblePercentage * 100}%");
                        Vector3 playerPosition = playerCollider.transform.position;
                        Debug.Log("Player Position: " + playerPosition);
                        PlaySound(detectionSound);
                        DisablePortalsByName();
                        SpawnEnemies();
                        enemiesSpawned = true;
                    }
                }
            }
        }
    }

    private void DisablePortalsByName()
    {
        if (enemiesSpawned)
        {
            return;
        }
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        List<GameObject> portals = new List<GameObject>();
        PlaySound(telportDown);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Portal(Clone)")
            {
                portals.Add(obj);
                obj.SetActive(false);
            }
        }

        Debug.Log($"Disabled {portals.Count} portals.");
        StartCoroutine(ReenablePortalsAfterDelay(portals, teleportDisableTime));
    }

    private IEnumerator ReenablePortalsAfterDelay(List<GameObject> portals, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (specialCam)
        {
            while (roomEnemies.Count > 0)
            {
                StartCoroutine(ReenablePortalsAfterDelay(portals));
                yield return new WaitForSeconds(1);
            }
        }
        else
        {
            foreach (GameObject portal in portals)
            {
                if (portal != null)
                {
                    portal.SetActive(true);
                }
            }

            Debug.Log($"Re-enabled {portals.Count} portals.");
            PlaySound(teleportUp);
        }
    }

    private IEnumerator ReenablePortalsAfterDelay(List<GameObject> portals)
    {
        while (roomEnemies.Count > 0)
        {
            roomEnemies.RemoveAll(enemy => enemy == null);
            yield return new WaitForSeconds(1);
        }

        foreach (GameObject portal in portals)
        {
            if (portal != null)
            {
                portal.SetActive(true);
            }
        }

        Debug.Log($"Re-enabled {portals.Count} portals.");
        PlaySound(teleportUp);
    }

    private void SpawnEnemies()
    {
        if (enemiesSpawned)
        {
            return;
        }
        if (enemySpawnPoints.Count == 0)
        {
            Debug.LogWarning("No enemy spawn points assigned!");
            return;
        }
        foreach (Vector3 spawnPoint in enemySpawnPoints)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);

        }
    }

    void PlaySound(AudioClip sound)
    {
        if (Time.time - lastDetectionTime >= audioCooldownTime)
        {
            if (sound != null && audioSource != null)
            {
                audioSource.PlayOneShot(sound);
                lastDetectionTime = Time.time;
            }
            else
            {
                if (detectionSound == null)
                    Debug.LogWarning("Detection sound is not assigned!");
                if (audioSource == null)
                    Debug.LogWarning("No AudioSource found for detection sound!");
            }
        }
    }

    float CheckPlayerVisibility(Collider playerCollider)
    {
        if (playerCollider is BoxCollider boxCollider)
        {
            Bounds bounds = boxCollider.bounds;

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

            foreach (Vector3 point in samplePoints)
            {
                Vector3 directionToPoint = (point - spotlight.position).normalized;
                Ray ray = new Ray(spotlight.position, directionToPoint);
                float rayDistance = Vector3.Distance(spotlight.position, point) + 0.1f;

                if (Physics.Raycast(ray, out RaycastHit hitInfo1, rayDistance))
                {
                    if (hitInfo1.collider == playerCollider)
                    {
                        visiblePoints++;
                    }
                }
            }

            return (float)visiblePoints / totalPoints;
        }

        Ray centerRay = new Ray(spotlight.position, (playerCollider.bounds.center - spotlight.position).normalized);
        if (Physics.Raycast(centerRay, out RaycastHit hitInfo, detectionRange))
        {
            return hitInfo.collider == playerCollider ? 1f : 0f;
        }

        return 0f;
    }
}
