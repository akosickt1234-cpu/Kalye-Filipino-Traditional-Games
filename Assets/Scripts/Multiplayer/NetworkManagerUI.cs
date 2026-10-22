using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
public class NetworkManagerUI : MonoBehaviour
{
    
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    [SerializeField] private Button createBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private TMP_InputField code;
    [SerializeField] private TestRelay testRelay;

    private bool isReturningToMenu;

    private void Awake()
    {   

        createBtn.onClick.AddListener(() =>
        {
            testRelay.CreateRelay();

            
        });

        joinBtn.onClick.AddListener(() =>
        {
            string joinCode = code.text.Trim();

            if (string.IsNullOrEmpty(joinCode))
            {
                
                return;
            }

            testRelay.JoinRelay(joinCode);


        });
    }


    private NetworkManager subscribedNetworkManager;

    private void OnEnable()
    {
        SubscribeToDisconnect();
    }

    private void OnDisable()
    {
        if (subscribedNetworkManager != null)
        {
            subscribedNetworkManager.OnClientDisconnectCallback
                -= OnClientDisconnected;

            subscribedNetworkManager = null;
        }
    }

    private void SubscribeToDisconnect()
    {
        NetworkManager nm = NetworkManager.Singleton;

        if (nm == null)
        {
            Debug.LogWarning("NetworkManager.Singleton is not available yet.");
            return;
        }

        if (subscribedNetworkManager == nm)
            return;

        if (subscribedNetworkManager != null)
        {
            subscribedNetworkManager.OnClientDisconnectCallback
                -= OnClientDisconnected;
        }

        subscribedNetworkManager = nm;
        subscribedNetworkManager.OnClientDisconnectCallback
            += OnClientDisconnected;

        Debug.Log("Subscribed to disconnect callback!");
    }



    private void OnClientDisconnected(ulong clientId)
    {
        NetworkManager nm = NetworkManager.Singleton;

        if (nm == null)
        {
            Debug.LogWarning("NetworkManager is null during disconnect!");
            return;
        }

        if (isReturningToMenu)
            return;

        Debug.Log($"Disconnected client ID: {clientId}");
        Debug.Log($"IsServer: {nm.IsServer}");

        // On a non-host client, a disconnect of its own client ID
        // means it has lost its connection to the host.
        if (!nm.IsServer && clientId == nm.LocalClientId)
        {
            Debug.Log("Client disconnected! Returning to menu.");

            isReturningToMenu = true;
            StartCoroutine(ReturnToMenu());
        }
    }


    private IEnumerator ReturnToMenu()
    {
        Debug.Log("COROUTINE STARTED!");
        NetworkManager nm = NetworkManager.Singleton;

        if (nm != null && nm.IsListening)
            nm.Shutdown();

        yield return null;

        // Wait for shutdown to finish.
        while (nm != null && nm.ShutdownInProgress)
            yield return null;

        Debug.Log("LOADING SAMPLE SCENE!");


        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Loading SampleScene...");

        SceneManager.LoadScene("SampleScene");
    }
}
