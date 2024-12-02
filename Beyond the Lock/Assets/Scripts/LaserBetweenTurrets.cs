using UnityEngine;

public class LaserBetweenTurrets : MonoBehaviour
{
    public Transform turret1Origin; 
    public Transform turret2Origin; 
    public Material laserMaterial; 
    public float animationSpeed = 5f; 
    public float damage = 10f; 

    private LineRenderer lineRenderer;
    private Vector2 textureOffset = Vector2.zero;

    void Start()
    {
       
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }


        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        
        if (laserMaterial != null)
        {
            lineRenderer.material = laserMaterial;
        }
    }

    void Update()
    {
        if (turret1Origin != null && turret2Origin != null)
        {
            lineRenderer.SetPosition(0, turret1Origin.position);
            lineRenderer.SetPosition(1, turret2Origin.position);
            RaycastHit hit;
            Vector3 direction = turret2Origin.position - turret1Origin.position;
            float laserLength = direction.magnitude;

            
            if (Physics.Raycast(turret1Origin.position, direction.normalized, out hit, laserLength))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage * Time.deltaTime); 
                    }
                }
            }
        }

        //researched how to make move
        if (laserMaterial != null)
        {
            textureOffset.x += Time.deltaTime * animationSpeed;
            laserMaterial.SetTextureOffset("_MainTex", textureOffset);
        }
    }
}
