using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 20f;

    public float lifeTime = 2f;
    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }

}
