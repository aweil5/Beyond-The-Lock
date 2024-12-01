using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 20f;
    public float fireRate = 0.1f;
    public float maxShootDistance = 100f;

    private float nextFireTime = 0f;

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            ShootRaycast();
            nextFireTime = Time.time + fireRate;
        }
    }

    void ShootRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(bulletSpawnPoint.position, bulletSpawnPoint.forward, out hit, maxShootDistance))
        {
            Debug.DrawRay(bulletSpawnPoint.position, bulletSpawnPoint.forward * hit.distance, Color.red, 1f);

            // Visualize bullet
            SpawnBullet();

            // Apply damage or effects
            if (hit.collider.gameObject.TryGetComponent(out EnemyHealth enemyHealth))
            {
                enemyHealth.TakeDamage(10);
            }
        }
        else
        {
            // No hit; spawn bullet visual traveling forward
            SpawnBullet();
        }
    }

    void SpawnBullet()
    {
        GameObject bulletVisual = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(bulletSpawnPoint.forward));
        Rigidbody rb = bulletVisual.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = bulletSpawnPoint.forward * bulletSpeed;
        }
        Destroy(bulletVisual, 2f);
    }
}
