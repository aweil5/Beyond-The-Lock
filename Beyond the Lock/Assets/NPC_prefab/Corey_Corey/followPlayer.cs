using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followPlayer : MonoBehaviour
{
    public Transform player;
    public GameObject bulletObject;
    public Transform spawnPoint;

    //shooting stuff
    public float bulletSpeed = 10f;
    public float shootCooldown = 1.5f;
    private float lastShootTime = 0f;


    //chasing stuff
    public float playerDistance;
    public float chaseRange = 10f;
    public float NpcLookaroundSpeed = 10;
    public float attackRange = 5f;
    public float runSpeed = 4;
    

    public static bool isPlayerAlive = true;
    private __movement movementScript;

    // Start is called before the first frame update
    void Start()
    {
        movementScript = GetComponent<__movement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerAlive)
            {
                playerDistance = Vector3.Distance(player.position, transform.position);

                if(playerDistance < chaseRange && isPlayerVisible() && movementScript.CanSeePlayer())
            {
                movementScript.viewAngle = 360f;
                lookAtPlayer();
                chase();

                if(playerDistance <= attackRange)
                {
                    if (Time.time - lastShootTime >= shootCooldown)
                    {
                        ShootPlayer();
                        lastShootTime = Time.time;
                    }
                }
            }
        }
    }


    void lookAtPlayer(){
        Quaternion rotation = Quaternion.LookRotation(player.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * NpcLookaroundSpeed);
    }

    bool isPlayerVisible()
    {
        RaycastHit hit;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, directionToPlayer, out hit, chaseRange))
        {
            return hit.transform == player;
        }
        return false;
    }
    void chase()
    {

    /*
        playerDistance = Vector3.Distance(player.position, transform.position);

    if (playerDistance <= (attackRange / 2))
    {
        transform.Translate(Vector3.zero);
        lookAtPlayer();
    } else {
        
    */


        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1f))
            {
                if (hit.collider)
                {
                    TurnClockwise();
                }
            }
            else
            {
                transform.Translate(Vector3.forward * Time.deltaTime * runSpeed);
            }
        }

    void TurnClockwise()
    {
        transform.Rotate(Vector3.up, 90f);
    }

    void ShootPlayer()
    {
        if (spawnPoint == null || bulletObject == null)
        {
            return;
        }

        GameObject bulletObj = Instantiate(bulletObject, spawnPoint.position, spawnPoint.rotation);
        Rigidbody bulletRb = bulletObj.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            Vector3 directionToPlayer = (player.position - spawnPoint.position).normalized;
            bulletRb.velocity = directionToPlayer * bulletSpeed;
        }
        Destroy(bulletObj, 2f);

        StartCoroutine(DelayChase());
    }

    IEnumerator DelayChase()
    {
        yield return new WaitForSeconds(0.5f);
        chase();
    }

}