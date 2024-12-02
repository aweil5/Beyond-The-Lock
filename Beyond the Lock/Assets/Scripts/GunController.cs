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

    bool canShoot = true;

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
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(1000f);
        }

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = (targetPoint - bulletSpawnPoint.position).normalized * bulletSpeed;
        }

        Destroy(bullet, 5f);

        StartCoroutine(PlayShotSound());
    }

    private IEnumerator PlayShotSound()
    {
        if (shotSound != null)
        {
            shotSound.SetActive(true);
            canShoot = false;
             // Activate the sound effect GameObject
            yield return new WaitForSeconds(clipLength); // Wait for the duration of the clip
            canShoot = true;
            shotSound.SetActive(false); // Deactivate the sound effect GameObject
        }
    }
}
