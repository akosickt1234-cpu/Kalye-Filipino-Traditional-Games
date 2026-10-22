
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
    [Header("Player Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private int nextSpawnIndex = 0;
    private readonly HashSet<ulong> queuedClients = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback
            += OnClientConnected;

        // Handle players already connected when this manager spawns.
        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            QueueSpawn(client.ClientId);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback
                -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        QueueSpawn(clientId);
    }

    private void QueueSpawn(ulong clientId)
    {
        if (!IsServer || !queuedClients.Add(clientId))
            return;

        StartCoroutine(SpawnPlayer(clientId));
    }

    private IEnumerator SpawnPlayer(ulong clientId)
    {
        // Wait until NGO has created this player's PlayerObject.
        while (NetworkManager.Singleton != null &&
               NetworkManager.Singleton.ConnectedClients.TryGetValue(
                   clientId, out NetworkClient client) &&
               client.PlayerObject == null)
        {
            yield return null;
        }

        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.ConnectedClients.TryGetValue(
                clientId, out NetworkClient connectedClient))
        {
            yield break;
        }

        if (connectedClient.PlayerObject == null)
            yield break;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No player spawn points assigned!");
            yield break;
        }

        Transform spawnPoint =
            spawnPoints[nextSpawnIndex % spawnPoints.Length];

        nextSpawnIndex++;

        PlayerNetwork player =
            connectedClient.PlayerObject.GetComponent<PlayerNetwork>();

        if (player == null)
        {
            Debug.LogError("Player prefab has no PlayerNetwork component!");
            yield break;
        }

        player.TeleportToAssignedSpawnRpc(
            spawnPoint.position,
            spawnPoint.rotation,
            RpcTarget.Single(
                clientId,
                RpcTargetUse.Temp
            )
        );

        Debug.Log(
            $"Player {clientId} assigned to {spawnPoint.name}"
        );
    }
}
