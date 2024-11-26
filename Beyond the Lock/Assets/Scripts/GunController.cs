using UnityEngine;

public class GunController : MonoBehaviour
{
    public GameObject bulletPrefab; // Assign your bullet prefab
    public Transform bulletSpawnPoint; // The point where bullets are spawned
    public float bulletSpeed = 20f; // Speed of the bullet
    public float fireRate = 0.1f; // Time between shots
    private float nextFireTime = 0f; // Time control for shooting

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
        // Instantiate the bullet at the spawn point
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Add velocity to the bullet
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = bulletSpawnPoint.forward * bulletSpeed;
        }

        // Optionally, destroy the bullet after some time
        Destroy(bullet, 5f);
    }
}
