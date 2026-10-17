using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class TestRelay : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI code;
    private bool isCreatingRelay;
    private bool servicesReady;

    private void Start()
    {
        InitializeServices();
    }

    private async void InitializeServices()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            servicesReady = true;
            Debug.Log("Unity Services ready!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Unity Services initialization failed: {e}");
        }
    }


    public async void CreateRelay()
    {
        if (!servicesReady || isCreatingRelay)
            return;

        NetworkManager nm = NetworkManager.Singleton;

        if (nm == null || nm.IsListening)
        {
            Debug.LogWarning("NetworkManager is already running!");
            return;
        }

        isCreatingRelay = true;

        try
        {
            Allocation allocation =
                await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode =
                await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            code.text = joinCode;

            nm.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            if (!nm.StartHost())
            {
                Debug.LogError("Failed to start host!");
                code.text = "Couldn't start room. Please try again.";
            }
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
            code.text = "Couldn't create room. Please try again.";
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            code.text = e.Message;
        }
        finally
        {
            isCreatingRelay = false;
        }
    }


    public async void JoinRelay(string joinCode)
    {
        if (!servicesReady)
        {
            Debug.LogWarning("Unity Services are not ready yet!");
            return;
        }
        try
        {
            Debug.Log("Joining Relay with: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();

            code.text = joinCode;
        }

        catch (RelayServiceException e)
        {
            Debug.LogError(e);

            if (e.Message.ToLower().Contains("join code"))
            {
                code.text = "Room code can't be found!";
                StartCoroutine(ResetText());
            }
            else
            {
                code.text = e.Message;
                StartCoroutine(ResetText());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            code.text = e.Message;
            StartCoroutine(ResetText());
        }


    }

    IEnumerator ResetText()
    {
        yield return new WaitForSeconds(1f);
        code.text = "";
    }
}
