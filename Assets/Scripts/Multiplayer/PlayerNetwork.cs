using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera camera;
    private GameObject cam;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform defaultCameraPivot;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private RectTransform cursorImage;


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
    private NetworkVariable<bool> hasThrownTsinelas =
    new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public bool HasSlipperInWorld => hasTsinelas.Value;
    public ulong TsinelasOwnerClientId { get; private set; }
    private NetworkVariable<bool> isInsideThrowerBase =
    new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public bool IsInsideThrowerBase => isInsideThrowerBase.Value;

    public void SetInsideThrowerBase(bool inside)
    {
        if (!IsServer)
            return;

        isInsideThrowerBase.Value = inside;
    }

    [SerializeField] private float pickupDistance = 2f;
    [SerializeField] private float throwSpeed = 12f;
    [SerializeField] private float spawnDistance = 1.2f;
    private bool canThrow = false;
    private bool canGet = false;


    [Header("Catch")]
    [SerializeField] private BoxCollider hitbox;
    private bool catchRequestSent;
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
    public NetworkVariable<bool> canJump = new NetworkVariable<bool>(
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

    [Header("UI")]
    [SerializeField] private GameObject inGamePanel;
    [SerializeField] private GameObject managerPanel;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject createButton;
    [SerializeField] private GameObject joinButton;
    [SerializeField] private GameObject codeField;


    IEnumerator TextChange(string message)
    {
        textLabel.text = message;
        yield return new WaitForSeconds(1f);
        textLabel.text = "";
    }


    [Rpc(SendTo.SpecifiedInParams)]
    public void TeleportToSpawnRpc(
        Vector3 position,
        RpcParams rpcParams = default)
    {
        // Only the owning client moves its player.
        if (!IsOwner || !IsClient)
            return;

        Rigidbody body = GetComponent<Rigidbody>();
        NetworkTransform networkTransform =
            GetComponent<NetworkTransform>();

        if (body != null)
        {
            body.position = position;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        if (networkTransform != null)
        {
            networkTransform.Teleport(
                position,
                transform.rotation,
                transform.localScale
            );
        }
    }



    public void ShowStartButtonAfterGame()
    {
        if (!IsOwner || !IsHost) return;

        GameObject start = GameObject.Find("StartButton");

        if (start != null)
            start.SetActive(true);
    }

    [ClientRpc]
    public void UiGameStartClientRpc()
    {
        if (!IsOwner || !IsHost) return;

        GameObject start = GameObject.Find("StartButton");

        if (start != null)
            start.SetActive(false);
    }
   

    [Rpc(SendTo.SpecifiedInParams)]
    public void TeleportToAssignedSpawnRpc(
        Vector3 position,
        Quaternion rotation,
        RpcParams rpcParams = default)
    {
        if (!IsOwner || !IsClient)
            return;

        Rigidbody body = GetComponent<Rigidbody>();
        NetworkTransform networkTransform =
            GetComponent<NetworkTransform>();

        if (body != null)
        {
            body.position = position;
            body.rotation = rotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        transform.rotation = rotation;

        if (networkTransform != null)
        {
            networkTransform.Teleport(
                position,
                rotation,
                transform.localScale
            );
        }
    }


    public void ResetForNewRound()
    {
        if (!IsServer)
            return;

        hasBeenHit.Value = false;
        hasThrownTsinelas.Value = false;

        Rigidbody body = GetComponent<Rigidbody>();

        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    private bool IsTumbangPresoGuard()
    {
        TumbangPresoManager manager =
            FindAnyObjectByType<TumbangPresoManager>();

        return manager != null
            && manager.IsMatchActive
            && manager.GuardId == OwnerClientId;
    }
    private bool CanUseTsinelas()
    {
        // Tsinelas throwing is only allowed during Tumbang Preso.
        TumbangPresoManager manager =
            FindAnyObjectByType<TumbangPresoManager>();

        return manager != null && manager.IsMatchActive;
    }

    private void TryTumbangPresoTag(Collider other)
    {
        // Elimination must be handled by the server.
        if (!IsServer)
            return;

        TumbangPresoManager manager =
            FindAnyObjectByType<TumbangPresoManager>();

        if (manager == null || !manager.IsMatchActive)
            return;

        // Only the selected Guard can eliminate players.
        if (manager.GuardId != OwnerClientId)
            return;

        PlayerNetwork target =
            other.GetComponentInParent<PlayerNetwork>();

        if (target == null || target == this)
            return;

        if (target.IsInsideThrowerBase)
            return;

        // Only eliminate a player who is within tagging distance.
        if (Vector3.Distance(
                transform.position,
                target.transform.position) > 2.5f)
            return;

        manager.EliminateThrower(target.OwnerClientId);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCatchPlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryCatchPlayer(other);
    }

    private void TryCatchPlayer(Collider other)
    {
        if (!IsOwner || !IsClient) return;
        if (!taya.Value || !catchPlayer) return;
        if (catchRequestSent) return;

        PlayerNetwork target =
            other.GetComponentInParent<PlayerNetwork>();

        if (target == null || target == this) return;

        // Don't try to catch another Taya.
        if (target.taya.Value) return;

        catchRequestSent = true;
        RequestCatchRpc(target.OwnerClientId);
    }


    [Rpc(SendTo.Server)]
    private void RequestCatchRpc(ulong targetClientId, RpcParams rpcParams = default)
    {
        if (!taya.Value) return;
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

        TumbangPresoManager manager = FindAnyObjectByType<TumbangPresoManager>();

        if (manager != null &&
            manager.IsMatchActive &&
            target.IsInsideThrowerBase)
        {
            Debug.Log("Catch rejected: target is safe inside their base!");
            
            return;
        }
        

        Debug.Log("this is not the target and it isnt null");
        if (target.taya.Value == true) return;
        if (target.hasBeenHit.Value) return;
        Debug.Log("target has value");

        Collider targetCollider = target.GetComponent<Collider>();

        if (hitbox == null || targetCollider == null)
        {
            Debug.LogWarning("Catch failed: missing collider.");
            return;
        }

        if (!hitbox.bounds.Intersects(targetCollider.bounds))
        {
            Debug.Log("Catch rejected: target is outside hitbox.");

            return;
        }

        target.hasBeenHit.Value = true;

        PatinteroManager.Instance?.RegisterCaughtRunner(
        target.OwnerClientId
        );

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

            Rigidbody body = GetComponent<Rigidbody>();

            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
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
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
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

        TumbangPresoManager manager =
    FindAnyObjectByType<TumbangPresoManager>();

        // Reject slipper spawning outside an active Tumbang Preso match.
        if (manager == null || !manager.IsMatchActive)
            return;

      

        if (manager != null && manager.IsMatchActive)
        {
            if (manager.GuardId == OwnerClientId)
                return;

            if (!IsInsideThrowerBase)
            {
                Debug.Log("You can only throw from your base!");
                
                    
                
                return;
            }
        }

        if (spawnedTsinelas != null && spawnedTsinelas.IsSpawned)
            return;

        if (Vector3.Distance(cameraPosition, transform.position) > 5f)
            return;

        if (IsTumbangPresoGuard())
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

        Tsinelas tsinelasScript = tsinelas.GetComponent<Tsinelas>();

        if (tsinelasScript != null)
        {
            tsinelasScript.SetThrower(OwnerClientId);
        }

        tsinelasRb.linearVelocity = cameraForward * throwSpeed;

        spawnedTsinelas = networkObject;

        spawnedTsinelasTransform = tsinelas.transform;
        hasTsinelas.Value = true;
        hasThrownTsinelas.Value = true;
    }


    [Rpc(SendTo.Server)]
    private void DespawnTsinelasRpc(RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        if (spawnedTsinelas == null || !spawnedTsinelas.IsSpawned)
            return;

        if (IsTumbangPresoGuard())
            return;

        float distance = Vector3.Distance(
        transform.position,
        spawnedTsinelas.transform.position
        );

        if (distance > pickupDistance)
        {
            Debug.Log("You're too far away from the tsinelas!");
            StartCoroutine(TextChange("anlayo ko sa tsinelas / too far away from the slippers"));
            return;
        }

        spawnedTsinelas.Despawn(true);
        spawnedTsinelas = null;
        hasTsinelas.Value = false;
    }

    IEnumerator Catch()
    {
        catchRequestSent = false;
        catchPlayer = true;

        yield return new WaitForSeconds(1f);

        catchPlayer = false;
    }


    [Rpc(SendTo.Server)]
    private void RequestResetCanRpc(RpcParams rpcParams = default)
    {
        // Validate who sent the request.
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        TumbangPresoManager manager =
            FindAnyObjectByType<TumbangPresoManager>();

        if (manager == null || !manager.IsMatchActive)
            return;

        // Only the selected Guard can reset the can.
        if (manager.GuardId != OwnerClientId)
            return;

        PresoCan can = FindAnyObjectByType<PresoCan>();

        if (can == null || !can.IsSpawned)
            return;

        

        // The Guard must be close to the can.
        float distance = Vector3.Distance(
            transform.position,
            can.transform.position
        );

        if (distance > 2.5f)
        {
            
                StartCoroutine(TextChange("anlayo ko sa lata / too far away from can"));

                return;
            
        }
            

        can.ResetCan();

        Debug.Log("Guard reset the can!");
    }


    [Rpc(SendTo.Server)]
    private void RequestTagThrowerRpc(RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId)
            return;

        TumbangPresoManager manager =
            FindAnyObjectByType<TumbangPresoManager>();

        if (manager == null || !manager.IsMatchActive)
            return;

        if (manager.GuardId != OwnerClientId)
            return;

        PlayerNetwork closestThrower = null;
        float closestDistance = 2.5f;

        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerNetwork target =
                client.PlayerObject.GetComponent<PlayerNetwork>();

            if (target == null || target == this)
                continue;

            if (target.OwnerClientId == manager.GuardId)
                continue;

                    // Throwers inside their base cannot be tagged.
            if (target.IsInsideThrowerBase)
                continue;
            

            // Throwers must throw their tsinelas before they can be tagged.
            if (!target.hasThrownTsinelas.Value)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                target.transform.position
            );

            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closestThrower = target;
            }
        }

        if (closestThrower == null)
            return;

        manager.EliminateThrower(closestThrower.OwnerClientId);
    }


    private void Update()
    {
        if (!IsOwner || !IsClient) return;

        if (!IsOwner || !IsClient) return;

        // Tumbang Preso Guard: press F to reset the can.
        TumbangPresoManager tpManager =
            FindAnyObjectByType<TumbangPresoManager>();

        if (tpManager != null &&
            tpManager.IsMatchActive &&
            tpManager.GuardId == OwnerClientId &&
            Input.GetKeyDown(KeyCode.F))
        {
            RequestResetCanRpc();
        }

        if (hasBeenHit.Value) return;

        if (hasBeenHit.Value) return;

        // Taya or Pick up animation

        if (Input.GetKeyDown(KeyCode.Q))
        {
            // In Tumbang Preso, Q lets the Guard attempt a tag.
            if (IsTumbangPresoGuard())
            {
                RequestTagThrowerRpc();
                return;
            }

            if (canGet == false)
                return;

            if (taya.Value)
            {
                animatorController.SetPickUp();
                StartCoroutine(Catch());
                return;
            }

            if (hasTsinelas.Value)
            {
                StartCoroutine(disAndAbleChar());
                animatorController.SetPickUp();
                DespawnTsinelasRpc();
            }
        }

        // Lock cam or not
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = true;
            }
        }

        // throw animation for slippers
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!CanUseTsinelas()) return;
            if (IsTumbangPresoGuard()) return;
            if (hasTsinelas.Value) return;
            if (canThrow == false) return;
            StartCoroutine(disAndAbleChar());
            animatorController.SetThrow();
            SpawnTsinelasRpc(camera.transform.position, camera.transform.forward);
            
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
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            cursorImage.position = Input.mousePosition;
        }
        else
        {
            cursorImage.position = new Vector2(9999, 9999);
        }
        
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
        bool doJump = jumpRequested && isGrounded && canJump.Value;
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
