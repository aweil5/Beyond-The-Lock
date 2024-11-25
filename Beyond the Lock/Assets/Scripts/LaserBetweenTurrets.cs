using UnityEngine;

public class LaserBetweenTurrets : MonoBehaviour
{
    public Transform turret1Origin; // Start of the laser
    public Transform turret2Origin; // End of the laser
    public Material laserMaterial; // Laser material for animation
    public float animationSpeed = 5f; // Speed of laser texture animation
    public float damage = 10f; // Damage the laser deals per second

    private LineRenderer lineRenderer;
    private Vector2 textureOffset = Vector2.zero;

    void Start()
    {
        // Get or Add a LineRenderer component
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Set LineRenderer properties
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // Assign the laser material
        if (laserMaterial != null)
        {
            lineRenderer.material = laserMaterial;
        }
        else
        {
            Debug.LogWarning("Laser material not assigned!");
        }
    }

    void Update()
    {
        // Update the laser's positions
        if (turret1Origin != null && turret2Origin != null)
        {
            lineRenderer.SetPosition(0, turret1Origin.position);
            lineRenderer.SetPosition(1, turret2Origin.position);

            // Perform raycast to detect if the player is in the laser
            RaycastHit hit;
            Vector3 direction = turret2Origin.position - turret1Origin.position;
            float laserLength = direction.magnitude;

            // Perform a raycast along the laser
            if (Physics.Raycast(turret1Origin.position, direction.normalized, out hit, laserLength))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    // Get the PlayerHealth component and apply damage
                    PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage * Time.deltaTime); // Apply damage per second
                    }
                }
            }
        }

        // Animate the laser texture
        if (laserMaterial != null)
        {
            textureOffset.x += Time.deltaTime * animationSpeed;
            laserMaterial.SetTextureOffset("_MainTex", textureOffset);
        }
    }
}
