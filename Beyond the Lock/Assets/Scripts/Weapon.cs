using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    // Shooting
    private bool isShooting = false; // Tracks if the weapon is currently on cooldown
    public float shootingDelay = 2f; // Delay between shots in seconds

    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30;
    public float bulletPrefabLifeTime = 3f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isShooting)
        {
            FireWeapon();
        }
    }

    private void FireWeapon()
    {
        // Instantiate Bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        // Shoot Mechanism
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward.normalized * bulletVelocity, ForceMode.Impulse);

        // Destroy the Bullet After its Lifetime
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        // Start cooldown
        StartCoroutine(ShootingCooldown());
    }

    private IEnumerator ShootingCooldown()
    {
        isShooting = true; // Set shooting flag to true to prevent firing
        yield return new WaitForSeconds(shootingDelay); // Wait for the cooldown duration
        isShooting = false; // Reset shooting flag to allow firing again
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
