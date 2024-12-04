using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // NavMesh and Movement
    public NavMeshAgent agent;
    public Transform player;

    // Combat Variables
    public GameObject projectile;
    public float health = 100f;
    public float timeBetweenAttacks;
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    // Patrol Variables
    public Vector3 walkPoint;
    public float walkPointRange = 10f;
    public float minWalkPointDistance = 5f;
    public float maxPatrolWaitTime = 3f;
    public LayerMask whatIsGround, whatIsPlayer;

    // Internal State Trackers
    private bool walkPointSet;
    private bool alreadyAttacked;
    private float waitCounter;
    public float hitboxRadius = 2f;

    // Audio
    public AudioSource audioSource;
    public AudioClip finalDeathSound;
    public AudioClip deathSound;

    public AudioClip enemyShot;
    private void Start()
    {
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
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        player = GameObject.Find("Main Camera").transform;
        if (player == null)
        {
            player = GameObject.Find("Player").transform;
        }
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Check player proximity
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer) &&
    IsDirectLineOfSight(transform.position, player.position, whatIsGround, sightRange);

        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer) &&
            IsDirectLineOfSight(transform.position, player.position, whatIsGround, attackRange);

        // State Machine
        if (!playerInSightRange && !playerInAttackRange) Patrol();
        if (playerInSightRange && !playerInAttackRange) Chase();
        if (playerInSightRange && playerInAttackRange) Attack();
    }
    private bool IsDirectLineOfSight(Vector3 origin, Vector3 target, LayerMask obstacleLayer, float maxDistance)
    {
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        // Only check if within max distance
        if (distance > maxDistance)
            return false;

        // Normalize the direction
        direction.Normalize();

        // Perform raycast
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, distance, obstacleLayer))
        {
            // If raycast hits something before reaching the target, there's no direct line of sight
            return false;
        }

        // No obstacles found
        return true;
    }

    public float walkPointResetTime = 5f;
    private float walkPointTimer;

    private void Patrol()
    {
        // Increment the walk point timer
        walkPointTimer += Time.deltaTime;

        // If no walk point is set, or timer exceeds reset time, search for a new walk point
        if (!walkPointSet || walkPointTimer >= walkPointResetTime)
        {
            SearchWalkPoint();
            walkPointTimer = 0f;
            waitCounter = Random.Range(0f, maxPatrolWaitTime);
        }

        // If waiting, count down the wait time
        if (waitCounter > 0)
        {
            waitCounter -= Time.deltaTime;
            agent.SetDestination(transform.position);
            return;
        }

        // If a walk point is set, move towards it
        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);

            // Calculate distance to walk point
            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            // Check if enemy has reached the walk point
            if (distanceToWalkPoint.magnitude < 1.5f)
            {
                walkPointSet = false;
            }
        }
    }

    private void SearchWalkPoint()
    {
        // Try multiple times to find a valid walk point
        for (int i = 0; i < 10; i++)
        {
            // Generate a random point within the walk point range
            // Use the CURRENT position as the center, not an original spawn point
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            // Calculate the potential walk point
            walkPoint = new Vector3(
                transform.position.x + randomX,
                transform.position.y,
                transform.position.z + randomZ
            );

            // Use NavMesh to sample a valid point
            if (NavMesh.SamplePosition(walkPoint, out NavMeshHit hit, walkPointRange, NavMesh.AllAreas))
            {
                walkPoint = hit.position;
                walkPointSet = true;
                return;
            }
        }

        // If no valid point found after 10 attempts, reset
        walkPointSet = false;
    }

    private void Chase()
    {
        agent.SetDestination(player.position);
    }

    private void Attack()
    {
        // Stop moving
        agent.SetDestination(transform.position);

        // Face the player
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 20f);


        if (!alreadyAttacked)
        {
            Debug.Log("Attacking player!");
            // Shooting
            GameObject bullet = Instantiate(projectile, transform.position + transform.forward, Quaternion.identity);
            bullet.transform.LookAt(new Vector3(player.position.x, bullet.transform.position.y, player.position.z));

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = bullet.AddComponent<Rigidbody>();
            }

            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up, ForceMode.Impulse);

            alreadyAttacked = true;
            audioSource.PlayOneShot(enemyShot);
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            // Play the detection sound
            audioSource.PlayOneShot(deathSound);
        }

        // Destroy the enemy after the sound has played
        if (health <= 0)
        {
            audioSource.PlayOneShot(finalDeathSound);
            DestroyEnemy();
        }

    }

    public GameObject deathParticles;

    private void DestroyEnemy()
    {
        // Instantiate death particles
        GameObject particles = null;
        if (deathParticles != null)
        {
            particles = Instantiate(deathParticles, transform.position, Quaternion.identity);
        }

        if (particles != null)
        {
            Destroy(particles, 3);

        }
        Destroy(gameObject, finalDeathSound.length);

    }

    private void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Walk point
        if (walkPointSet)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(walkPoint, 1f);
        }

        // Patrol range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);
    }
}