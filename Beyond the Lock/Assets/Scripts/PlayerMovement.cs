using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public float speed = 12f;
    public float crouchSpeed = 6f; // Speed while crouching
    public float gravity = -9.81f * 2;
    public float jumpHeight = 4f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public Transform gun; // Reference to the gun object
    private Vector3 gunOriginalScale; // Store original gun scale

    public Vector3 velocity;
    bool isGrounded;

    private Vector3 crouchScale = new Vector3(1, 0.5f, 1); // Scale when crouching
    private Vector3 playerScale = new Vector3(1, 1f, 1);   // Original scale
    private bool isCrouching = false; // Track crouch state

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        gunOriginalScale = gun.localScale; // Store the gun's original scale
    }

    // Update is called once per frame
    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to keep grounded
        }

        // Movement input (x-z plane)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Movement direction without affecting vertical component
        Vector3 move = transform.right * x + transform.forward * z;
        move.y = 0; // Ensure no vertical component from camera tilt
        float currentSpeed = isCrouching ? crouchSpeed : speed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching) // Prevent jumping while crouching
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        // Apply vertical velocity
        controller.Move(velocity * Time.deltaTime);

        // Crouching
        Crouch();
    }

    void Crouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) // Start crouching
        {
            isCrouching = true;
            transform.localScale = crouchScale; // Reduce player scale

            // Scale up the gun inversely
            float scaleFactor = playerScale.y / crouchScale.y; // Calculate scaling factor
            gun.localScale = new Vector3(
                gunOriginalScale.x,
                gunOriginalScale.y * scaleFactor,
                gunOriginalScale.z
            );

            // Adjust position to stay grounded
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y - (playerScale.y - crouchScale.y) / 2,
                transform.position.z
            );
        }

        if (Input.GetKeyUp(KeyCode.LeftShift)) // Stop crouching
        {
            isCrouching = false;
            transform.localScale = playerScale; // Reset player scale

            // Reset the gun scale
            gun.localScale = gunOriginalScale;

            // Adjust position back to original
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + (playerScale.y - crouchScale.y) / 2,
                transform.position.z
            );
        }
    }
}
