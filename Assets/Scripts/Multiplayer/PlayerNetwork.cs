using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera camera;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform defaultCameraPivot;
    [SerializeField] private float mouseSensitivity = 2f;
    private float xRotation;

    [Header("Movement")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float sprintSpeedMult = 1.2f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private float highJumpForce = 4f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Stamina")]
    [SerializeField] private Image foreground;
    [SerializeField] private Image background;
    [SerializeField] private float stamina = 100f;
    [SerializeField] private float addOrDeductStamina = 0.5f;
    [SerializeField] private float maxWidth = 200f;
    [SerializeField] private float colorTransitionSpeed = 5f;

    private const float MaxStamina = 100f;

    [Header("Animation")]
    [SerializeField] private AnimationController animatorController;

    private bool jumpRequested;
    private bool sprintRequested;
    private bool highJumpRequested;
    private bool isGrounded;
    private bool isJumping;

    [Header("Objects")]
    [SerializeField] private GameObject player;

    [Header("Tsinelas")]
    [SerializeField] private Transform tsinelasPrefab;
    private Transform spawnedTsinelasTransform;
    private NetworkObject spawnedTsinelas;
    private bool hadSpawenedTsinelas = false;
    [SerializeField] private float pickupDistance = 2f;
    [SerializeField] private float throwSpeed = 12f;
    [SerializeField] private float spawnDistance = 1.2f;
    private bool canThrow = false;
    private bool canGet = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cameraPivot.gameObject.SetActive(true);

            GameObject sceneCam =
                GameObject.FindWithTag("SceneCamera");

            if (sceneCam != null)
            {
                sceneCam.SetActive(false);
            }

            
        }
        else
        {
            cameraPivot.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (!IsOwner) return;

        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
    }

    IEnumerator disAndAbleChar()
    {
        player.SetActive(false);
        yield return new WaitForSeconds(2f);
        player.SetActive(true);
    }



    [Rpc(SendTo.Server)]
    private void SpawnTsinelasRpc(RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

       
        if (spawnedTsinelas != null && spawnedTsinelas.IsSpawned)
            return;

        SpawnTsinelasWithAimRpc(
            camera.transform.position,
            camera.transform.forward
        );
    }

    [Rpc(SendTo.Server)]
    private void SpawnTsinelasWithAimRpc(
        Vector3 cameraPosition,
        Vector3 cameraForward,
        RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        if (spawnedTsinelas != null && spawnedTsinelas.IsSpawned)
            return;

        
        if (Vector3.Distance(cameraPosition, transform.position) > 5f)
            return;

        cameraForward.Normalize();

        Vector3 spawnPosition =
            cameraPosition + cameraForward * spawnDistance;

        GameObject tsinelas = Instantiate(
            tsinelasPrefab.gameObject,
            spawnPosition,
            Quaternion.LookRotation(cameraForward)
        );

        NetworkObject networkObject =
            tsinelas.GetComponent<NetworkObject>();

        Rigidbody tsinelasRb =
            tsinelas.GetComponent<Rigidbody>();

        if (networkObject == null || tsinelasRb == null)
        {
            Destroy(tsinelas);
            Debug.LogError(
                "Tsinelas prefab needs NetworkObject and Rigidbody!"
            );
            return;
        }

        networkObject.Spawn();

        // Throw
        tsinelasRb.linearVelocity = cameraForward * throwSpeed;

        spawnedTsinelas = networkObject;
        spawnedTsinelasTransform = tsinelas.transform;
    }

    [Rpc(SendTo.Server)]
    private void DespawnTsinelasRpc(RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        if (spawnedTsinelas == null || !spawnedTsinelas.IsSpawned)
            return;

        float distance = Vector3.Distance(
        transform.position,
        spawnedTsinelas.transform.position
        );

        if (distance > pickupDistance)
        {
            Debug.Log("You're too far away from the tsinelas!");
            return;
        }

        spawnedTsinelas.Despawn(true);
        spawnedTsinelas = null;
        hadSpawenedTsinelas = false;
    }



    private void Update()
    {
        if (!IsOwner || !IsClient) return;

        // Taya or Capture animation
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (hadSpawenedTsinelas == false) return;
            if (canGet == false) return;
            //StartCoroutine(disAndAbleChar());
            animatorController.SetPickUp();
            DespawnTsinelasRpc();
            

        }

        // throw animation for slippers
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (hadSpawenedTsinelas == true) return;
            if (canThrow == false) return;
            //StartCoroutine(disAndAbleChar());
            animatorController.SetThrow();
            SpawnTsinelasRpc();
            hadSpawenedTsinelas = true;
        }

        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpRequested = true;
            highJumpRequested = sprintRequested;
        }

        // Sprint input
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            sprintRequested = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            sprintRequested = false;
        }

        // Stamina
        if (sprintRequested && isGrounded)
        {
            stamina -= addOrDeductStamina;
        }
        else if (!sprintRequested)
        {
            stamina += addOrDeductStamina;
        }

        stamina = Mathf.Clamp(stamina, 0f, MaxStamina);

        // Stamina UI
        float percentage = stamina / MaxStamina;

        background.gameObject.SetActive(stamina < MaxStamina);

        foreground.rectTransform.sizeDelta =
            new Vector2(maxWidth * percentage, 5f);

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



        // Camera rotation
        float mouseX =
            Input.GetAxis("Mouse X") * mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -70f, 70f);

        cameraPivot.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);
    }

    

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Ground check
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Jump
        bool doJump = jumpRequested && isGrounded;
        bool doHighJump = doJump
            && highJumpRequested
            && stamina >= 15f;

        if (doJump)
        {
            isJumping = true;

            float force = doHighJump
                ? highJumpForce
                : jumpForce;

            if (doHighJump)
            {
                stamina = Mathf.Clamp(
                    stamina - 15f,
                    0f,
                    MaxStamina
                );
            }

            rb.AddForce(
                Vector3.up * force,
                ForceMode.Impulse
            );
        }

        jumpRequested = false;
        highJumpRequested = false;

        // Reset jump state after landing
        if (isGrounded && !doJump && rb.linearVelocity.y <= 0.05f)
        {
            isJumping = false;
        }

        // Movement
        bool hasMovementInput =
            horizontal != 0f || vertical != 0f;

        bool isSprinting =
            sprintRequested &&
            stamina > 0.1f &&
            isGrounded &&
            !isJumping;

        float speed = isSprinting
            ? moveSpeed * sprintSpeedMult
            : moveSpeed;

        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement =
            (forward * vertical + right * horizontal).normalized;

        rb.MovePosition(
            rb.position + movement * speed * Time.fixedDeltaTime
        );

        // Animation
        animatorController.SetLocomotion(
            hasMovementInput && !isSprinting && !isJumping,
            hasMovementInput && isSprinting && !isJumping,
            isJumping
        );

        if (hasMovementInput == false && isJumping == false)
        {
            canThrow = true;
        }
        else
        {
            canThrow = false;
        }

        if (isJumping == true)
        {
            canGet = false;
        }
        else
        {
            canGet = true;
        }

    }
}
