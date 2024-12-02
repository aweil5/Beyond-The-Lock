using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public GameObject bulletPrefab; // Prefab for the bullet
    public Transform bulletSpawnPoint; // Where bullets will spawn
    public float bulletSpeed = 75f; // Speed of the bullet
    public float fireRate = 1.5f; // Time between shots

    private float nextFireTime = 0f;

    public float clipLength = 1f;

    public GameObject shotSound; // Assign a GameObject with the audio source

    void Start()
    {

        shotSound.SetActive(false);

    }

    void Update()
    {
        // Check for firing input and rate limiting
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // Spawn the bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Add Rigidbody component if it doesn't already exist
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = bullet.AddComponent<Rigidbody>();
        }

        // Apply forward force to the bullet
        rb.AddForce(bulletSpawnPoint.forward * bulletSpeed, ForceMode.Impulse);

        // Optional: Destroy the bullet after a short time to avoid clutter
        Destroy(bullet, 5f);

        StartCoroutine(PlayShotSound());
    }

    private IEnumerator PlayShotSound()
    {
        if (shotSound != null)
        {
            shotSound.SetActive(true); // Activate the sound effect GameObject
            yield return new WaitForSeconds(clipLength); // Wait for the duration of the clip
            shotSound.SetActive(false); // Deactivate the sound effect GameObject
        }
    }
}
