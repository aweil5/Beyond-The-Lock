using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour
{
    public GameObject bulletPrefab; // Assign bullet prefab
    public Transform bulletSpawnPoint; // The point where bullets are spawned
    public float bulletSpeed = 20f; // Speed of the bullet
    public float fireRate = 0.1f; // Time between shots
    private float nextFireTime = 0f; // Time control for shooting

    public float clipLength = 1f;

    public GameObject shotSound; // Assign a GameObject with the audio source

    void Start() {
        
            shotSound.SetActive(false);
        
    }

    void Update()
    {
        // Check if the player presses the fire button
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // Visualize the shoot direction in the Scene view
        Debug.DrawRay(bulletSpawnPoint.position, bulletSpawnPoint.forward * 10f, Color.red, 1f);

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = bulletSpawnPoint.forward * bulletSpeed;
        }

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
