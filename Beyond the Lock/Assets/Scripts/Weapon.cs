using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public GameObject bulletPrefab; 
    public Transform bulletSpawnPoint; 
    public float bulletSpeed = 75f; 
    public float fireRate = 1.5f; 

    private float nextFireTime = 0f;

    public float clipLength = 1f;

    public GameObject shotSound; 
    void Start()
    {

        shotSound.SetActive(false);

    }

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = bullet.AddComponent<Rigidbody>();
        }

        rb.AddForce(bulletSpawnPoint.forward * bulletSpeed, ForceMode.Impulse);

        Destroy(bullet, 5f);

        StartCoroutine(PlayShotSound());
    }

    private IEnumerator PlayShotSound()
    {
        if (shotSound != null)
        {
            shotSound.SetActive(true); 
            yield return new WaitForSeconds(clipLength); 
            shotSound.SetActive(false); 
        }
    }
}
