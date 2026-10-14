using UnityEngine;
using Unity.Netcode;
public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Camera camera; // Player Camera
    [SerializeField] private Transform cameraPivot; // Player Camera's Position
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float sprintSpeedMult = 2f;
    private float xRotation;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private bool jumpRequested;
    private bool sprintRequested;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cameraPivot.gameObject.SetActive(true);

            // Disable the separate camera in the scene
            GameObject sceneCam = GameObject.FindWithTag("SceneCamera");

            if (sceneCam != null)
            {
                sceneCam.SetActive(false);
            }
        }
        else
        {
            // Disable cameras belonging to other players
            cameraPivot.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner || !IsClient) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpRequested = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            sprintRequested = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            sprintRequested = false;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }


    private void FixedUpdate()
    {
        if (!IsOwner) return;

        float speed;

        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");     // W/S

        bool jump = Input.GetKeyDown(KeyCode.Space);

        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        if (sprintRequested)
        {
            speed = moveSpeed * sprintSpeedMult;
        }
        else
        {
            speed = moveSpeed;
        }

        Vector3 movement = (forward * vertical + right * horizontal).normalized;

        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);

        bool isGrounded = Physics.CheckSphere(
        groundCheck.position,
        groundCheckRadius,
        groundLayer
    );

        if (jumpRequested && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        

        jumpRequested = false;
    }

    
}
