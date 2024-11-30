using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class __movement : MonoBehaviour
{
    public float moveSpeed = 5f;


    public float forwardLimit = 5f;
    public float backLimit = -5f;
    public float rightLimit = 5f;
    public float leftLimit = -5f;


    public Transform player;
    public float viewRange = 5f;
    public float viewAngle = 45f;

    public float resetMovement = 5f;

    private Vector3 direction;
    private float tolerance = 1f;
    private bool hasReachedLimit = false;
    private bool avoidingObstacle = false;
    private bool isChasingPlayer = false;
    private float timeOutOfRange = 0f;

    private followPlayer followScript;

    void Start()
    {
        direction = Vector3.forward;
        followScript = GetComponent<followPlayer>();
    }

    void Update()
    {

        if (CanSeePlayer())
        {
            StopNPC();
            isChasingPlayer = true;
            timeOutOfRange = 0f;
            return;
        }

        // If the player is out of range for too long, reset the movement
        if (isChasingPlayer)
        {
            timeOutOfRange += Time.deltaTime;
            if (timeOutOfRange >= resetMovement)
            {
                viewAngle = 45f;
                ResumeNormalMovement();
            }
        }

        if (moveSpeed == 0f)
        {
            FacePlayer();
            return;
        }

        // Detect obstacles
        if (!avoidingObstacle && Physics.Raycast(transform.position, direction, out RaycastHit hit, 0.5f))
        {
            if (hit.collider is MeshCollider || hit.collider is BoxCollider)
            {
                StartCoroutine(AvoidObstacle());
                return;
            }
        }

        // Move the NPC
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        // Check if reach limit
        if (!hasReachedLimit && !avoidingObstacle)
        {
            if (direction == Vector3.forward && transform.position.z >= forwardLimit - tolerance)
            {
                ChangeDirection(Vector3.right);
            }
            else if (direction == Vector3.right && transform.position.x >= rightLimit - tolerance)
            {
                ChangeDirection(Vector3.back);
            }
            else if (direction == Vector3.back && transform.position.z <= backLimit + tolerance)
            {
                ChangeDirection(Vector3.left);
            }
            else if (direction == Vector3.left && transform.position.x <= leftLimit + tolerance)
            {
                ChangeDirection(Vector3.forward);
            }
        }
    }

    // Changes the direction
    // rotates NPC
    void ChangeDirection(Vector3 newDirection)
    {
        direction = newDirection;
        transform.rotation = Quaternion.LookRotation(direction);
        hasReachedLimit = true;
        StartCoroutine(ResetLimit());
    }

    // Reset the flag so movement can continue
    IEnumerator ResetLimit()
    {
        yield return new WaitForSeconds(0.2f);
        hasReachedLimit = false;
    }

    // Get first turn
    Vector3 firstTurn(Vector3 currentDirection)
    {
        if (currentDirection == Vector3.forward)
            return Vector3.right;
        else if (currentDirection == Vector3.right)
            return Vector3.back;
        else if (currentDirection == Vector3.back)
            return Vector3.left;
        else if (currentDirection == Vector3.left)
            return Vector3.forward;
        else
            return Vector3.zero;
    }

    // Avoid obstacle movement
    IEnumerator AvoidObstacle()
    {
        avoidingObstacle = true;
        Vector3 firstTurnDirection = firstTurn(direction);
        ChangeDirection(firstTurnDirection);

        yield return new WaitForSeconds(0.2f);

        avoidingObstacle = false;
    }

    // Check if the NPC can see the player
    public bool CanSeePlayer()
    {
        if (player == null){
            return false;
        }

        Vector3 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;
        float angle = Vector3.Angle(direction, toPlayer);

        if (distanceToPlayer <= viewRange && angle < viewAngle)
        {
            if (Physics.Raycast(transform.position, toPlayer.normalized, out RaycastHit hit, viewRange))
            {
                if (hit.transform == player)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Makes the NPC face the player
    void FacePlayer()
    {
        if (player != null)
        {
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0;
            if (toPlayer != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(toPlayer);
            }
        }
    }

    // Resumes the NPC's normal movement after the player has been out of chase range for a certain amount of time
    public void ResumeNormalMovement()
    {
        isChasingPlayer = false;
        timeOutOfRange = 0f;
        moveSpeed = 5f;
        hasReachedLimit = false;
        avoidingObstacle = false;
    }

    // Stop the NPC permanently
    void StopNPC()
    {
        moveSpeed = 0f;
        hasReachedLimit = true;
        avoidingObstacle = true;
        FacePlayer();
    }
}