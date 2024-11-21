using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    public float rotationSpeed = 20f; // Speed of rotation
    private float currentAngle = 0f; // Track the current angle of rotation
    private int direction = 1; // Track the rotation direction (1 for clockwise, -1 for counterclockwise)
    private const float maxAngle = 60f; // Maximum rotation angle in either direction

    private void Update()
    {
        // Calculate the rotation for this frame
        float angleToRotate = rotationSpeed * Time.deltaTime * direction;
        currentAngle += angleToRotate;

        // Check if the rotation has reached 90 degrees in either direction
        if (Mathf.Abs(currentAngle) >= maxAngle)
        {
            // Clamp the angle and reverse direction
            angleToRotate -= (currentAngle - Mathf.Sign(currentAngle) * maxAngle);
            direction *= -1;
            currentAngle = Mathf.Sign(currentAngle) * maxAngle;
        }

        // Rotate the camera around the X-axis
        transform.Rotate(0, angleToRotate, 0);
    }
}
