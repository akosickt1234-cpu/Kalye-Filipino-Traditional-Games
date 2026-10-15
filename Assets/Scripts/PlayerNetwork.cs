using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Image foreground;
    [SerializeField] private Image background; 
    [SerializeField] private Camera camera; // Player Camera
    [SerializeField] private Transform cameraPivot; // Player Camera's Position
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float sprintSpeedMult = 1.2f;
    private float xRotation;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private float highJumpForce = 4f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private float _MAXSTAMINA = 100f;
    [SerializeField] private float stamina = 100f;
    [SerializeField] private float addOrDeductStamina = 0.5f;
    [SerializeField] private float maxWidth = 200f;

    [SerializeField] private Animator animator;

    private float colorTransitionSpeed = 5f;
    private bool jumpRequested;
    private bool sprintRequested;
    private bool highJumpRequested;
    private bool isGrounded = false;

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
            animator.SetBool("isJump", true);
            jumpRequested = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
           // animator.SetBool("isJump", false);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            sprintRequested = true;
            animator.SetBool("isRun", true);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            sprintRequested = false;
            animator.SetBool("isRun", false);
        }

        if (sprintRequested == true && stamina >= 0 && isGrounded)
        {
            

            stamina -= addOrDeductStamina;
            

            if (Input.GetKeyDown(KeyCode.Space))
            {
                highJumpRequested = true;
            }
        }
        else if (sprintRequested == false && stamina <= 100) 
        {
            stamina += addOrDeductStamina;

        }

        if (stamina < 100.5f)
        {
            background.gameObject.SetActive(true);
        }
        else
        {
            background.gameObject.SetActive(false);
        }

        float percentage = stamina / _MAXSTAMINA;
        foreground.rectTransform.sizeDelta = new Vector2(maxWidth * percentage, 5f);
        Color targetColor = Color.Lerp(
            Color.red,
            Color.green,
            percentage
        );


        foreground.color = Color.Lerp(
            foreground.color,
            targetColor,
            Time.deltaTime * colorTransitionSpeed
        );


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

        

        isGrounded = Physics.CheckSphere(
        groundCheck.position,
        groundCheckRadius,
        groundLayer
    );

        if (jumpRequested && isGrounded)
        {
            //animator.SetBool("isJump", true);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        jumpRequested = false;

        

        

        if (sprintRequested && stamina >= 0.1 && isGrounded)
        {
            speed = moveSpeed * sprintSpeedMult;
            if (highJumpRequested && isGrounded)
            {
                stamina -= 15f;
                rb.AddForce(Vector3.up * highJumpForce, ForceMode.Impulse);
            }
        }
        else
        {
            speed = moveSpeed;
        }

        highJumpRequested = false;
        //animator.SetBool("isJump", false);

        Vector3 movement = (forward * vertical + right * horizontal).normalized;

        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);

        
    }

    
}
