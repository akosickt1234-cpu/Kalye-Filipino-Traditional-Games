
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PatinteroManager : NetworkBehaviour
{
    public static PatinteroManager Instance { get; private set; }

    [Header("Game Mode")]
    [SerializeField] private int minimumPlayers = 2;
    [SerializeField] private string gameMode = "Patintero";

    [Header("Game Object Toggle")]
    [SerializeField] private GameObject gameObjectToToggle;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] tayaSpawnPoints;
    [SerializeField] private Transform[] runnerSpawnPoints;
    [Header("Lobby Return Spawn Points")]
    [SerializeField] private Transform[] lobbySpawnPoints;
    [SerializeField] private float spawnHeight = 1f;

    [Header("Scoring")]
    [SerializeField, Range(0.1f, 1f)]
    private float tayaCatchThreshold = 0.6f;

    [SerializeField] private int pointsToWin = 3;
    [SerializeField] private float resetDelay = 2f;

    [Header("UI - Optional")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI teamText;

    [Header("End Game UI")]
    [SerializeField] private GameObject patinteroEnd;
    [SerializeField] private TextMeshProUGUI endScoreText;
    [SerializeField] private TextMeshProUGUI messageNotifText;

    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject patinteroUI;
    [SerializeField] private float endScreenDuration = 5f;
    public NetworkVariable<int> runnerScore = new(
        0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> tayaScore = new(
        0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // -1 = match ongoing, 0 = runners won, 1 = Taya won
    public NetworkVariable<int> winnerTeam = new(
        -1, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private readonly HashSet<ulong> runnersAtCheckpoint = new();
    private readonly HashSet<ulong> caughtRunners = new();

    private bool roundEnded;
    public NetworkVariable<bool> gameStart = new(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);
    private bool endGameUIShown;
    private bool endScreenDismissed;
    private readonly Dictionary<ulong, Vector3> lobbyPositions = new();
    private readonly Dictionary<ulong, Quaternion> lobbyRotations = new();
    private bool isEndingMatch;

    [ClientRpc]
    private void SetGameObjectActiveClientRpc(bool isActive)
    {
        if (gameObjectToToggle != null)
            gameObjectToToggle.SetActive(isActive);
    }

    public override void OnNetworkSpawn()
    {
        Instance = this;

        if (patinteroUI != null)
            patinteroUI.SetActive(false);

        if (patinteroEnd != null)
            patinteroEnd.SetActive(false);

        if (startButton != null)
            startButton.SetActive(NetworkManager.Singleton != null
                && NetworkManager.Singleton.IsHost);

        if (scoreText == null)
        {
            GameObject scoreObject = GameObject.Find("Scores");

            if (scoreObject != null)
                scoreText = scoreObject.GetComponent<TextMeshProUGUI>();
        }


        UpdateUI();
    }

    public override void OnNetworkDespawn()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        UpdateUI();

        if (!IsServer || !gameStart.Value || isEndingMatch)
            return;

        int connectedPlayers = 0;

        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
                connectedPlayers++;
        }

        if (connectedPlayers < minimumPlayers)
        {
            isEndingMatch = true;
            roundEnded = true;

            SetPatinteroUIClientRpc(false, false);

            StartCoroutine(EndForInsufficientPlayers());
        }
    }

    [ClientRpc]
    private void ShowInsufficientPlayersClientRpc()
    {
        if (patinteroEnd != null)
            patinteroEnd.SetActive(true);

        if (endScoreText != null)
        {
            endScoreText.text =
                $"{runnerScore.Value}:{tayaScore.Value}";
        }

        if (messageNotifText != null)
        {
            messageNotifText.text =
                "Round ended!\nNot enough players left.";
        }

        endGameUIShown = true;
        endScreenDismissed = false;
    }

    private IEnumerator EndForInsufficientPlayers()
    {
        // Display the end message to everyone.
        ShowInsufficientPlayersClientRpc();

        // Give the remaining players time to read it.
        yield return new WaitForSeconds(endScreenDuration);

        if (!IsServer)
            yield break;

        int spawnIndex = 0;

        // Teleport every remaining player to a lobby spawn point.
        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerNetwork player =
                client.PlayerObject.GetComponent<PlayerNetwork>();

            if (player == null)
                continue;

            if (lobbySpawnPoints == null ||
                lobbySpawnPoints.Length == 0)
            {
                Debug.LogWarning(
                    "No Lobby Spawn Points assigned in PatinteroManager!"
                );
                break;
            }

            Transform spawnPoint =
                lobbySpawnPoints[spawnIndex % lobbySpawnPoints.Length];

            spawnIndex++;

            if (spawnPoint == null)
            {
                Debug.LogWarning("A lobby spawn point is missing!");
                continue;
            }

            Vector3 position =
                spawnPoint.position + Vector3.up * spawnHeight;

            player.TeleportToAssignedSpawnRpc(
                position,
                spawnPoint.rotation,
                RpcTarget.Single(
                    client.ClientId,
                    RpcTargetUse.Temp
                )
            );

            // Restore the player's normal lobby state.
            player.taya.Value = false;
            player.canJump.Value = false;
        }

        // Reset the match and its UI.
        gameStart.Value = false;

        runnerScore.Value = 0;
        tayaScore.Value = 0;
        winnerTeam.Value = -1;

        runnersAtCheckpoint.Clear();
        caughtRunners.Clear();

        roundEnded = false;
        isEndingMatch = false;

        lobbyPositions.Clear();
        lobbyRotations.Clear();

        ResetLobbyUIClientRpc();
        SetPatinteroUIClientRpc(false, true);

        Debug.Log("Patintero ended because too few players remained.");
    }


    private void UpdateUI()
    {
        if (NetworkManager.Singleton == null)
            return;

        // Display the player's team during the match.
        if (teamText != null)
        {
            if (!gameStart.Value)
            {
                teamText.text = "";
            }
            else
            {
                NetworkClient localClient =
                    NetworkManager.Singleton.LocalClient;

                if (localClient != null &&
                    localClient.PlayerObject != null)
                {
                    PlayerNetwork localPlayer =
                        localClient.PlayerObject.GetComponent<PlayerNetwork>();

                    if (localPlayer != null)
                    {
                        teamText.text = localPlayer.taya.Value
                            ? "Team: Taya"
                            : "Team: Runner";
                    }
                }
            }
        }

        // Display the current score during the match.
        if (scoreText != null && gameStart.Value)
        {
            scoreText.text =
                $"{runnerScore.Value}:{tayaScore.Value}";
        }

        // Don't reopen the end panel after it has been dismissed.
        if (winnerTeam.Value == -1 || endGameUIShown || endScreenDismissed)
            return;

        NetworkClient client = NetworkManager.Singleton.LocalClient;

        if (client == null || client.PlayerObject == null)
            return;

        PlayerNetwork localPlayerComponent =
            client.PlayerObject.GetComponent<PlayerNetwork>();

        if (localPlayerComponent == null)
            return;

        // Determine whether THIS player's team won.
        bool localTeamWon =
            (!localPlayerComponent.taya.Value && winnerTeam.Value == 0) ||
            (localPlayerComponent.taya.Value && winnerTeam.Value == 1);

        // Show the end-game panel.
        if (patinteroEnd != null)
            patinteroEnd.SetActive(true);

        // Display the final score.
        if (endScoreText != null)
        {
            endScoreText.text =
                $"{runnerScore.Value}:{tayaScore.Value}";
        }

        // Display the correct result for this player's team.
        if (messageNotifText != null)
        {
            messageNotifText.text = localTeamWon
                ? "Nanalo kayo!"
                : "Natalo kayo!";
        }

        endGameUIShown = true;
    }

    IEnumerator ResetText()
    {
        yield return new WaitForSeconds(1);
        scoreText.text = "";
    }

    [ClientRpc]
    private void SetPatinteroEndClientRpc(bool show)
    {
        if (patinteroEnd != null)
            patinteroEnd.SetActive(show);

        if (!show)
            endScreenDismissed = true;
    }

    [ClientRpc]
    private void SetPatinteroUIClientRpc(bool showMatchUI, bool showStartButton)
    {
        if (patinteroUI != null)
            patinteroUI.SetActive(showMatchUI);

        if (startButton != null)
            startButton.SetActive(showStartButton && NetworkManager.Singleton != null
                && NetworkManager.Singleton.IsHost);
    }
    public void StartPatinteroRound()
    {
        if (!IsServer || gameMode != "Patintero")
            return;

        int playerCount = 0;

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
                playerCount++;
        }

        if (playerCount < minimumPlayers)
        {
            Debug.LogWarning(
                $"Patintero requires at least {minimumPlayers} players. " +
                $"Current players: {playerCount}"
            );
            //scoreText.text = $"Must be at least {minimumPlayers}  players. Current players: {playerCount}";
            //StartCoroutine(ResetText());
            return;
        }

        lobbyPositions.Clear();
        lobbyRotations.Clear();

        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            lobbyPositions[client.ClientId] =
                client.PlayerObject.transform.position;

            lobbyRotations[client.ClientId] =
                client.PlayerObject.transform.rotation;
        }

        isEndingMatch = false;

        gameStart.Value = true;
        SetGameObjectActiveClientRpc(true);
        SetPatinteroUIClientRpc(true, false);

        endGameUIShown = false;
        endScreenDismissed = false;

        if (patinteroEnd != null)
            patinteroEnd.SetActive(false);

        runnerScore.Value = 0;
        tayaScore.Value = 0;
        winnerTeam.Value = -1;
        roundEnded = false;

        runnersAtCheckpoint.Clear();
        caughtRunners.Clear();

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

            plr.UiGameStartClientRpc();
            plr.canJump.Value = false;
            plr.taya.Value = assignTaya;
            assignTaya = !assignTaya;
        }

        TeleportPlayersToSpawnPoints();
    }

    public void RegisterCheckpoint(ulong runnerId)
    {
        if (!IsServer || roundEnded || !IsRunner(runnerId))
            return;

        runnersAtCheckpoint.Add(runnerId);
    }

    public void TryRunnerFinish(ulong runnerId)
    {
        if (!IsServer || roundEnded || !IsRunner(runnerId))
            return;

        // The runner must visit the checkpoint first.
        if (!runnersAtCheckpoint.Contains(runnerId))
            return;

        ScorePoint(false); // Runners score.
    }

    public void RegisterCaughtRunner(ulong runnerId)
    {
        if (!IsServer || roundEnded || !IsRunner(runnerId))
            return;

        // HashSet prevents the same runner counting twice this round.
        caughtRunners.Add(runnerId);

        int totalRunners = GetRunnerCount();
        if (totalRunners <= 0)
            return;

        int requiredCatches =
            Mathf.CeilToInt(totalRunners * tayaCatchThreshold);

        if (caughtRunners.Count >= requiredCatches)
            ScorePoint(true); // Taya score.
    }

    private bool IsRunner(ulong clientId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(
                clientId, out NetworkClient client))
            return false;

        if (client.PlayerObject == null)
            return false;

        PlayerNetwork plr =
            client.PlayerObject.GetComponent<PlayerNetwork>();

        return plr != null && !plr.taya.Value;
    }

    private int GetRunnerCount()
    {
        int count = 0;

        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerNetwork plr =
                client.PlayerObject.GetComponent<PlayerNetwork>();

            if (plr != null && !plr.taya.Value)
                count++;
        }

        return count;
    }

    private void ScorePoint(bool tayaScored)
    {
        if (!IsServer || roundEnded)
            return;

        roundEnded = true;

        if (tayaScored)
            tayaScore.Value++;
        else
            runnerScore.Value++;

        runnersAtCheckpoint.Clear();

        if (tayaScore.Value >= pointsToWin)
            winnerTeam.Value = 1;
        else if (runnerScore.Value >= pointsToWin)
            winnerTeam.Value = 0;

        if (winnerTeam.Value == -1)
        {
            StartCoroutine(ResetRoundAfterDelay());
        }
        else if (!isEndingMatch)
        {
            isEndingMatch = true;

            SetPatinteroUIClientRpc(false, false);

            StartCoroutine(ReturnPlayersToLobby());
        }
    }
    private IEnumerator ReturnPlayersToLobby()
    {
        // Let everyone see the result for 5 seconds.
        yield return new WaitForSeconds(endScreenDuration);

        if (!IsServer)
            yield break;

        // Hide PatinteroEnd on every client.
        SetPatinteroEndClientRpc(false);

        // Now teleport everyone back to the lobby.
        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            ulong id = client.ClientId;

            if (!lobbyPositions.TryGetValue(id, out Vector3 position))
                continue;

            if (!lobbyRotations.TryGetValue(id, out Quaternion rotation))
                continue;

            PlayerNetwork player =
                client.PlayerObject.GetComponent<PlayerNetwork>();

            if (player == null)
                continue;

            player.TeleportToAssignedSpawnRpc(
                position,
                rotation,
                RpcTarget.Single(id, RpcTargetUse.Temp)
            );

            player.taya.Value = false;
            player.canJump.Value = false;
        }

        // Continue with your existing lobby reset.
        ResetLobbyUIClientRpc();
        SetPatinteroUIClientRpc(false, true);
        SetGameObjectActiveClientRpc(false);
        gameStart.Value = false;
        runnerScore.Value = 0;
        tayaScore.Value = 0;
        winnerTeam.Value = -1;

        runnersAtCheckpoint.Clear();
        caughtRunners.Clear();

        roundEnded = false;
    }

    [ClientRpc]
    private void ResetLobbyUIClientRpc()
    {
        if (patinteroEnd != null)
            patinteroEnd.SetActive(false);

        if (scoreText != null)
            scoreText.text = "";

        if (teamText != null)
            teamText.text = "";

        if (endScoreText != null)
            endScoreText.text = "";

        if (messageNotifText != null)
            messageNotifText.text = "";

        endGameUIShown = false;
        endScreenDismissed = true;

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsHost &&
            startButton != null)
        {
            startButton.SetActive(true);
        }
    }
    private IEnumerator ResetRoundAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        runnersAtCheckpoint.Clear();
        caughtRunners.Clear();

        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerNetwork plr =
                client.PlayerObject.GetComponent<PlayerNetwork>();

            if (plr != null)
                plr.ResetForNewRound();
        }

        TeleportPlayersToSpawnPoints();
        roundEnded = false;
    }


    private void TeleportPlayersToSpawnPoints()
    {
        int tayaIndex = 0;
        int runnerIndex = 0;

        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerNetwork player =
                client.PlayerObject.GetComponent<PlayerNetwork>();

            if (player == null)
                continue;

            Transform[] spawnPoints;

            if (player.taya.Value)
            {
                spawnPoints = tayaSpawnPoints;
            }
            else
            {
                spawnPoints = runnerSpawnPoints;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning(
                    $"No spawn points assigned for " +
                    $"{(player.taya.Value ? "Taya" : "Runner")}!"
                );
                continue;
            }

            int spawnIndex;

            if (player.taya.Value)
            {
                spawnIndex = tayaIndex % spawnPoints.Length;
                tayaIndex++;
            }
            else
            {
                spawnIndex = runnerIndex % spawnPoints.Length;
                runnerIndex++;
            }

            Transform spawnPoint = spawnPoints[spawnIndex];

            if (spawnPoint == null)
            {
                Debug.LogWarning(
                    $"Spawn point at index {spawnIndex} is missing!"
                );
                continue;
            }

            Vector3 position =
                spawnPoint.position + Vector3.up * spawnHeight;

            player.TeleportToSpawnRpc(
                position,
                RpcTarget.Single(
                    player.OwnerClientId,
                    RpcTargetUse.Temp
                )
            );
        }
    }

}
