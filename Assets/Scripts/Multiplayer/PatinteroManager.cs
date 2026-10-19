
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PatinteroManager : NetworkBehaviour
{
    [SerializeField] private string gameMode = "Patintero";
    [SerializeField] private Transform tayaSpawnPoint;
    [SerializeField] private float spawnHeight = 1f;
    public void StartPatinteroRound()
    {
        // Only the server assigns player roles.
        if (!IsServer)
            return;

        if (gameMode != "Patintero")
            return;

        bool assignTaya = true;

        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerNetwork plr =
                client.PlayerObject.GetComponent<PlayerNetwork>();

            if (plr == null)
                continue;

            plr.taya.Value = assignTaya;

            if (assignTaya)
            {
                Vector3 spawnPosition = tayaSpawnPoint.position
                                     + Vector3.up * spawnHeight;

                NetworkTransform networkTransform =
                    plr.GetComponent<NetworkTransform>();

                if (networkTransform != null)
                {
                    networkTransform.Teleport(
                        spawnPosition,
                        plr.transform.rotation,
                        plr.transform.localScale
                    );
                }
            }

            // Alternate between Taya and Runner.
            assignTaya = !assignTaya;
        }
    }
}
