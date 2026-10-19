using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera camera;
    private GameObject cam;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform defaultCameraPivot;
    [SerializeField] private float mouseSensitivity = 2f;
    private float xRotation;

    [Header("Movement")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float sprintSpeedMult = 1.2f;
    private bool wasGrounded;

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
    private NetworkVariable<bool> hasTsinelas = new NetworkVariable<bool>(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
    );

    [SerializeField] private float pickupDistance = 2f;
    [SerializeField] private float throwSpeed = 12f;
    [SerializeField] private float spawnDistance = 1.2f;
    private bool canThrow = false;
    private bool canGet = false;


    [Header("Catch")]
    [SerializeField] private BoxCollider hitbox;

    private NetworkVariable<bool> hasBeenHit = new NetworkVariable<bool>(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> taya = new NetworkVariable<bool>(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
    );

    
    bool catchPlayer = false;
    [SerializeField] private bool tayaDisplay;
    [SerializeField] private bool gotHitDisplay;

    [Header("Error Handler")]
    [SerializeField] private TextMeshProUGUI textLabel;

    [Header("PatinteroLineDetector")]
    [SerializeField] private PatinteroLineDetector lineDetector;

    [Header("SoundHandler")]
    [SerializeField] private SoundHandler soundHandler;
    IEnumerator TextChange(string message)
    {
        textLabel.text = message;
        yield return new WaitForSeconds(1f);
        textLabel.text = "";
    }

    [Rpc(SendTo.Server)]
    private void BecomeTayaRpc(RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        if (!IsSpawned) return;

        taya.Value = true;

        Debug.Log($"Player {OwnerClientId} is now Taya!");
    }

    // Detect a possible catch on the catching player's client.
    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner || !IsClient) return;
        if (taya.Value == false) return;
        if (catchPlayer == false) return;
        //Debug.Log($"[{OwnerClientId}] Trigger entered: {other.name}");

        if (hitbox == null)
        {
            Debug.LogWarning("Hitbox is missing!");
            return;
        }

        PlayerNetwork target = other.GetComponentInParent<PlayerNetwork>();

        if (target == null)
        {
            //Debug.Log("Touched object isn't a player.");
            return;
        }

        if (target == this) return;

        Debug.Log($"[{OwnerClientId}] Sending catch request to {target.OwnerClientId}");

        RequestCatchRpc(target.OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    private void RequestCatchRpc(ulong targetClientId, RpcParams rpcParams = default)
    {
        Debug.Log("Receive request");
        // Make sure the request came from this player.
        if (rpcParams.Receive.SenderClientId != OwnerClientId) return;
        Debug.Log("not owner");

        if (!IsSpawned) return;

        // Find the target on the server.
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out NetworkClient targetClient)) return;
        Debug.Log("found target");
        PlayerNetwork target = targetClient.PlayerObject.GetComponent<PlayerNetwork>();

        if (target == null || target == this) return;
        Debug.Log("this is not the target and it isnt null");
        if (target.hasBeenHit.Value) return;
        Debug.Log("target has value");

        Collider targetCollider = target.GetComponent<Collider>();

        if (hitbox == null || targetCollider == null)
        {
            Debug.LogWarning("Catch failed: missing collider.");
            return;
        }

        Debug.Log(
            $"SERVER overlap check | " +
            $"Taya hitbox: {hitbox.bounds} | " +
            $"Target collider: {targetCollider.bounds} | " +
            $"Intersects: {hitbox.bounds.Intersects(targetCollider.bounds)}");

        // [ I DISABLED THIS CHECK TO REDUCE TIME CONSTRAINT, WILL FIX IF THE GAME BECAME SUCCESSFUL ]
        /*if (!hitbox.bounds.Intersects(targetCollider.bounds))
        {
            Debug.LogWarning("Catch rejected: server colliders don't overlap.");
            return;
        }*/

        //Debug.Log("hitboxes intersected");
        // Server marks the target as hit.


        target.hasBeenHit.Value = true;

        Debug.Log("Server validated catch! Player " + targetClientId + " got caught!");

        // Reset hit immunity after 3 seconds.
        target.StartCoroutine(target.ResetHitAfterDelay());
    }
    private void OnTayaChanged(bool previousValue, bool newValue)
    {
        tayaDisplay = newValue;
    }

    private void OnHitStateChanged(bool previousValue, bool newValue)
    {
        gotHitDisplay = newValue;
        if (newValue)
        {
            Debug.Log($"Player {OwnerClientId} got hit!");
        }
        else
        {
            Debug.Log($"Player {OwnerClientId} can be hit again!");
        }
    }
    public override void OnNetworkSpawn()
    {
        gotHitDisplay = hasBeenHit.Value;
        hasBeenHit.OnValueChanged += OnHitStateChanged;

        tayaDisplay = taya.Value;
        taya.OnValueChanged += OnTayaChanged;

        if (IsOwner)
        {
            cameraPivot.gameObject.SetActive(true);

            GameObject sceneCam = GameObject.FindWithTag("SceneCamera");

            if (sceneCam != null)
                sceneCam.SetActive(false);
        }
        else
        {
            cameraPivot.gameObject.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        hasBeenHit.OnValueChanged -= OnHitStateChanged;
        taya.OnValueChanged -= OnTayaChanged;
    }



    private IEnumerator ResetHitAfterDelay()
    {
        Debug.Log($"Reset coroutine started for {OwnerClientId}");
        yield return new WaitForSeconds(3f);

        if (IsServer && IsSpawned)
        {
            hasBeenHit.Value = false;
            Debug.Log($"Player {OwnerClientId} can be hit again!");
            Debug.Log(
            $"Reset complete for {OwnerClientId}. " +
            $"hasBeenHit = {hasBeenHit.Value}"
            );
        }
    }

    private void Start()
    {
        if (!IsOwner) return;
        cam = camera.gameObject;
        //Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
    }

    IEnumerator disAndAbleChar()
    {
        player.SetActive(false);
        yield return new WaitForSeconds(2f);
        player.SetActive(true);
    }




    [Rpc(SendTo.Server)]
    private void SpawnTsinelasRpc(
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
            Debug.LogError("Tsinelas needs a NetworkObject and Rigidbody!");
            return;
        }

        networkObject.Spawn();

        tsinelasRb.linearVelocity = cameraForward * throwSpeed;

        spawnedTsinelas = networkObject;
        spawnedTsinelasTransform = tsinelas.transform;
        hasTsinelas.Value = true;
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
            StartCoroutine(TextChange("anlayo ko sa tsinelas"));
            return;
        }

        spawnedTsinelas.Despawn(true);
        spawnedTsinelas = null;
        hasTsinelas.Value = false;
    }

    IEnumerator Catch()
    {
        catchPlayer = true;
        yield return new WaitForSeconds(1f);
        catchPlayer = false;
    }

    private void Update()
    {
        if (!IsOwner || !IsClient) return;
        if (hasBeenHit.Value) return;
        // Taya or Pick up animation
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (canGet == false) return;
            if (hasTsinelas.Value)
            {
                
                StartCoroutine(disAndAbleChar());
                animatorController.SetPickUp();
                DespawnTsinelasRpc();
            }
            else
            {
                StartCoroutine(disAndAbleChar());
                animatorController.SetPickUp();
                StartCoroutine(Catch());
            }
            
        }

        // throw animation for slippers
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (hasTsinelas.Value) return;
            if (canThrow == false) return;
            StartCoroutine(disAndAbleChar());
            animatorController.SetThrow();
            SpawnTsinelasRpc(camera.transform.position, camera.transform.forward);
            
        }

        // become taya
        if (Input.GetKeyDown(KeyCode.R))
        {
            BecomeTayaRpc();
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
            stamina -= addOrDeductStamina * Time.deltaTime;
        }
        else if (!sprintRequested)
        {
            stamina += addOrDeductStamina * Time.deltaTime;
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
        if (hasBeenHit.Value) return;
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

        /*rb.MovePosition(
            rb.position + movement * speed * Time.fixedDeltaTime
        );*/

        Vector3 nextPosition =
    rb.position + movement * speed * Time.fixedDeltaTime;

        // Replace IsTaya with your existing role check.
        if (!taya.Value || lineDetector.IsOnLine(nextPosition))
        {
            rb.MovePosition(nextPosition);
        }

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

        if (isGrounded && !wasGrounded)
        {
            soundHandler.Thud();
        }

        wasGrounded = isGrounded;

        if (isSprinting == true && hasMovementInput == true)
        {
            soundHandler.Run();
        }
        else if (isSprinting == false && hasMovementInput == true)
        {
            soundHandler.Walk();
        }
        else if (isSprinting == false && hasMovementInput == false)
        {
            soundHandler.Idle();
        }
        

        float targetZ = isSprinting ? 1f : 0.35f;

        Vector3 targetPosition = new Vector3(
            cam.transform.localPosition.x,
            cam.transform.localPosition.y,
            targetZ
        );

        cam.transform.localPosition = Vector3.Lerp(
            cam.transform.localPosition,
            targetPosition,
            Time.fixedDeltaTime * 5f
        );

    }
}
