using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public float mouseSensivity = 500f;

    float xRotation = 0f;
    float yRotation = 0f;
    // Start is called before the first frame update

    public float topClamp = -90f;
    public float bottomClamp = 90f;
    void Start()
    {
        //lock cursor to middle
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, topClamp, bottomClamp);
        yRotation += mouseX;

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
