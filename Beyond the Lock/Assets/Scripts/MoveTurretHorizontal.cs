using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MoveTurretHorizontal : MonoBehaviour
{
    public int displacement = 10;
    public int speed = 1;
    private Vector3 initialPosition;
    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;   
    }

    // Update is called once per frame
    void Update()
    {
        float newZ = Mathf.PingPong(Time.time * speed, displacement);
        transform.position = new Vector3(initialPosition.x, initialPosition.y, initialPosition.z + newZ);

        // float newY = Mathf.Sin(Time.time * speed) * displacement;
        // transform.position = new Vector3(initialPosition.x, initialPosition.y + newY, initialPosition.z);
    }
}
