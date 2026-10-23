
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TumbangPresoManager : NetworkBehaviour
{
    [Header("Match Settings")]
    [SerializeField] private int minimumPlayers = 5;
    [SerializeField] private int maximumPlayers = 7;
    [SerializeField] private int matchDuration = 200;

    [Header("Points")]
    [SerializeField] private int knockCanPoints = 15;
    [SerializeField] private int safeReturnPoints = 6;
    [SerializeField] private int guardEliminationPoints = 15;

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private GameObject tumbangPresoUI;
    [SerializeField] private GameObject startButtons;


    [Header("Role Spawns")]
    [SerializeField] private Transform guardSpawnPoint;
    [SerializeField] private Transform[] throwerSpawnPoints;


    private NetworkVariable<bool> matchActive = new(false);
    private NetworkVariable<ulong> guardClientId =
        new(ulong.MaxValue);

    private NetworkVariable<int> timeRemaining = new(200);

    private readonly Dictionary<ulong, int> playerScores = new();
    private readonly HashSet<ulong> throwers = new();
    private readonly HashSet<ulong> eliminatedPlayers = new();

    // Tracks which rewards have already been earned
    // during each player's current attempt.
    private readonly Dictionary<ulong, bool> canKnocked = new();
    private readonly Dictionary<ulong, bool> safelyReturned = new();

    private readonly Dictionary<ulong, Vector3> throwerSpawnPositions = new();
    private readonly Dictionary<ulong, Quaternion> throwerSpawnRotations = new();

    private float timerAccumulator;
    private bool matchFinished;

    private bool isReturningToLobby;

    private readonly Dictionary<ulong, Vector3> lobbyPositions = new();
    private readonly Dictionary<ulong, Quaternion> lobbyRotations = new();

    public bool IsMatchActive => matchActive.Value;
    public ulong GuardId => guardClientId.Value;


    private void TeleportPlayersToRoleSpawns(List<ulong> playerIds)
    {
        int throwerIndex = 0;

        foreach (ulong id in playerIds)
        {
            if (!NetworkManager.Singleton.ConnectedClients
                .TryGetValue(id, out NetworkClient client))
                continue;

            if (client.PlayerObject == null)
                continue;

            PlayerNetwork player =
                client.PlayerObject.GetComponent<PlayerNetwork>();

            if (player == null)
                continue;

            Transform spawnPoint;

            if (id == guardClientId.Value)
            {
                spawnPoint = guardSpawnPoint;
            }
            else
            {
                if (throwerSpawnPoints == null ||
                    throwerSpawnPoints.Length == 0)
                {
                    Debug.LogWarning(
                        "No Thrower Spawn Points assigned!"
                    );
                    continue;
                }

                spawnPoint = throwerSpawnPoints[
                    throwerIndex % throwerSpawnPoints.Length
                ];

                throwerSpawnPositions[id] = spawnPoint.position;
                throwerSpawnRotations[id] = spawnPoint.rotation;

                throwerIndex++;
            }

            if (spawnPoint == null)
            {
                Debug.LogWarning($"Missing spawn point for player {id}.");
                continue;
            }

            player.TeleportToAssignedSpawnRpc(
                spawnPoint.position,
                spawnPoint.rotation,
                RpcTarget.Single(id, RpcTargetUse.Temp)
            );
        }
    }


    public override void OnNetworkSpawn()
    {
        matchActive.OnValueChanged += OnMatchActiveChanged;
        timeRemaining.OnValueChanged += OnTimeChanged;
        guardClientId.OnValueChanged += OnGuardChanged;

        if (startButton != null)
            startButton.onClick.AddListener(StartMatch);



        // Hide all match texts before the game starts.
        if (!matchActive.Value)
        {
            if (timerText != null)
                timerText.text = "";

            if (scoreText != null)
                scoreText.text = "";

            if (resultText != null)
                resultText.text = "";

            if (roleText != null)
                roleText.text = "";
        }
        else
        {
            UpdateTimerUI(timeRemaining.Value);
            UpdateRoleUI();
            RefreshScoreboard();
        }

        if (startButton != null)
            startButton.gameObject.SetActive(IsServer);
        if (tumbangPresoUI != null)
            tumbangPresoUI.SetActive(false);
        if (startButtons != null)
            startButtons.SetActive(IsHost);
    }

    public override void OnNetworkDespawn()
    {
        matchActive.OnValueChanged -= OnMatchActiveChanged;
        timeRemaining.OnValueChanged -= OnTimeChanged;
        guardClientId.OnValueChanged -= OnGuardChanged;

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartMatch);
        }
    }

    private void Update()
    {
        if (!IsServer || !matchActive.Value || matchFinished)
            return;

        timerAccumulator += Time.deltaTime;

        while (timerAccumulator >= 1f)
        {
            timerAccumulator -= 1f;

            if (timeRemaining.Value > 0)
            {
                timeRemaining.Value--;
            }

            if (timeRemaining.Value <= 0)
            {
                FinishByTime();
                break;
            }
        }
    }

    // Call this from the host's Start button.
    public void StartMatch()
    {
        if (!IsServer || matchActive.Value || isReturningToLobby)
            return;

        int connectedPlayers = NetworkManager.Singleton
            .ConnectedClientsList
            .Count(client => client.PlayerObject != null);

        if (connectedPlayers < minimumPlayers ||
            connectedPlayers > maximumPlayers)
        {
            ShowMessageClientRpc(
                $"Tumbang Preso requires {minimumPlayers}-{maximumPlayers} players."
            );
            return;
        }

        SetTumbangPresoUIClientRpc(true, false);

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
        throwerSpawnPositions.Clear();
        throwerSpawnRotations.Clear();
        playerScores.Clear();
        throwers.Clear();
        eliminatedPlayers.Clear();
        canKnocked.Clear();
        safelyReturned.Clear();

        List<ulong> playerIds = NetworkManager.Singleton
            .ConnectedClientsList
            .Where(client => client.PlayerObject != null)
            .Select(client => client.ClientId)
            .ToList();

        // Randomly select exactly one Guard for this match.
        guardClientId.Value =
            playerIds[Random.Range(0, playerIds.Count)];

        foreach (ulong id in playerIds)
        {
            playerScores[id] = 0;

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(
        id, out NetworkClient client) &&
    client.PlayerObject != null)
            {
                PlayerNetwork player =
                    client.PlayerObject.GetComponent<PlayerNetwork>();

                if (player != null)
                    player.SetSpectatingServer(false);
            }

            if (id != guardClientId.Value)
            {
                throwers.Add(id);
                canKnocked[id] = false;
                safelyReturned[id] = false;
            }
        }

        TeleportPlayersToRoleSpawns(playerIds);

        timerAccumulator = 0f;
        matchFinished = false;
        timeRemaining.Value = matchDuration;
        matchActive.Value = true;

        if (startButton != null)
            startButton.gameObject.SetActive(false);

        ShowMessageClientRpc("");
        UpdateMatchUIClientRpc();

        Debug.Log(
            $"Tumbang Preso started! Guard: {guardClientId.Value}"
        );
    }

    // Call when a thrower starts a new attempt.
    // This resets both one-time rewards for that attempt.
    public void BeginThrowerAttempt(ulong playerId)
    {
        if (!IsServer || !matchActive.Value)
            return;

        if (!throwers.Contains(playerId) ||
            eliminatedPlayers.Contains(playerId))
            return;

        canKnocked[playerId] = false;
        safelyReturned[playerId] = false;
    }

    // Award +15 when the thrower successfully knocks down the can.
    public void AwardCanKnocked(ulong playerId)
    {
        if (!IsServer || !matchActive.Value || matchFinished)
            return;

        if (!throwers.Contains(playerId) ||
            eliminatedPlayers.Contains(playerId))
            return;

        if (canKnocked.TryGetValue(playerId, out bool alreadyKnocked)
            && alreadyKnocked)
            return;

        canKnocked[playerId] = true;
        AddPoints(playerId, knockCanPoints);
    }

    // Award +6 when the thrower safely returns to base.


    public bool AwardSafeReturn(ulong playerId)
    {
        if (!IsServer || !matchActive.Value || matchFinished)
            return false;

        if (!throwers.Contains(playerId) ||
            eliminatedPlayers.Contains(playerId))
            return false;

        // The player must have knocked the can down during this attempt.
        if (!canKnocked.TryGetValue(playerId, out bool knocked) || !knocked)
            return false;

        // Prevent duplicate safe-return rewards.
        if (safelyReturned.TryGetValue(playerId, out bool alreadyReturned)
            && alreadyReturned)
            return false;

        safelyReturned[playerId] = true;
        AddPoints(playerId, safeReturnPoints);

        // Reset the physical can if the Guard hasn't already done so.
        PresoCan can = FindAnyObjectByType<PresoCan>();
        if (can != null && can.HasBeenKnockedDown)
            can.ResetCan();

        // Reset this player's scoring flags for their next attempt.
        BeginThrowerAttempt(playerId);

        return true;
    }



    // Call when the Guard successfully eliminates a thrower.

    public void EliminateThrower(ulong throwerId)
    {
        if (!IsServer || !matchActive.Value || matchFinished)
            return;

        if (!throwers.Contains(throwerId) ||
            eliminatedPlayers.Contains(throwerId))
            return;

        eliminatedPlayers.Add(throwerId);

        AddPoints(guardClientId.Value, guardEliminationPoints);

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(
                throwerId, out NetworkClient client) &&
            client.PlayerObject != null)
        {
            PlayerNetwork player =
                client.PlayerObject.GetComponent<PlayerNetwork>();

            if (player != null)
            {
                // Disable gameplay controls.
                player.SetSpectatingServer(true);

                // Return the eliminated player to their assigned
                // thrower spawn so they can watch the remaining players.
                if (throwerSpawnPositions.TryGetValue(
                        throwerId, out Vector3 position) &&
                    throwerSpawnRotations.TryGetValue(
                        throwerId, out Quaternion rotation))
                {
                    player.TeleportToAssignedSpawnRpc(
                        position,
                        rotation,
                        RpcTarget.Single(
                            throwerId,
                            RpcTargetUse.Temp
                        )
                    );
                }
            }
        }

        SetEliminatedClientRpc(
            throwerId,
            RpcTarget.Single(throwerId, RpcTargetUse.Temp)
        );

        RefreshScoreboard();

        if (eliminatedPlayers.Count >= throwers.Count)
        {
            FinishMatch(
                "THE GUARD WINS!\nAll throwers were eliminated."
            );
        }
    }


    private void AddPoints(ulong playerId, int points)
    {
        if (!playerScores.ContainsKey(playerId))
            playerScores[playerId] = 0;

        playerScores[playerId] += points;

        RefreshScoreboard();
    }

    private void FinishByTime()
    {
        if (!IsServer || matchFinished)
            return;

        // Highest-scoring player wins when time expires.
        int highestScore = playerScores.Count > 0
            ? playerScores.Values.Max()
            : 0;

        List<ulong> winners = playerScores
            .Where(entry => entry.Value == highestScore)
            .Select(entry => entry.Key)
            .ToList();

        if (winners.Count == 1)
        {
            string role = winners[0] == guardClientId.Value
                ? "THE GUARD"
                : "A THROWER";

            FinishMatch(
                $"{role} WINS!\n" +
                $"Player ID: {winners[0]}\n" +
                $"Score: {highestScore}"
            );
        }
        else
        {
            string winnerIds = string.Join(", ", winners);

            FinishMatch(
                $"IT'S A TIE!\n" +
                $"Players: {winnerIds}\n" +
                $"Score: {highestScore}"
            );
        }
    }

    private void FinishMatch(string message)
    {
        if (!IsServer || matchFinished)
            return;

        matchFinished = true;
        matchActive.Value = false;

        ShowResultClientRpc(message);
        UpdateMatchUIClientRpc();

        Debug.Log(message);

        StartCoroutine(ReturnPlayersToLobby());

    }
    private IEnumerator ReturnPlayersToLobby()
    {
        isReturningToLobby = true;

        // Give everyone time to read the result.
        yield return new WaitForSeconds(3f);

        if (!IsServer)
            yield break;

        foreach (NetworkClient client in
                 NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            ulong id = client.ClientId;

            if (!lobbyPositions.TryGetValue(
                    id, out Vector3 position))
                continue;

            if (!lobbyRotations.TryGetValue(
                    id, out Quaternion rotation))
                continue;

            PlayerNetwork player =
                client.PlayerObject.GetComponent<PlayerNetwork>();

            if (player == null)
                continue;

            player.SetSpectatingServer(false);

            player.TeleportToAssignedSpawnRpc(
                position,
                rotation,
                RpcTarget.Single(id, RpcTargetUse.Temp)
            );
        }

        // Clear the completed match.
        playerScores.Clear();
        throwers.Clear();
        eliminatedPlayers.Clear();
        canKnocked.Clear();
        safelyReturned.Clear();

        guardClientId.Value = ulong.MaxValue;
        timeRemaining.Value = matchDuration;
        timerAccumulator = 0f;
        matchFinished = false;


        SetTumbangPresoUIClientRpc(false, true);

        ResetLobbyUIClientRpc();

        isReturningToLobby = false;

        Debug.Log("Tumbang Preso ended. Players returned to lobby.");
    }

    [ClientRpc]
    private void ResetLobbyUIClientRpc()
    {
        if (timerText != null)
            timerText.text = "";

        if (scoreText != null)
            scoreText.text = "";

        if (resultText != null)
            resultText.text = "";

        if (roleText != null)
            roleText.text = "";

        if (startButton != null)
            startButton.gameObject.SetActive(IsServer);
    }
    private void RefreshScoreboard()
    {
        if (!IsServer)
            return;

        string board = "";

        foreach (var entry in playerScores.OrderByDescending(x => x.Value))
        {
            string role = entry.Key == guardClientId.Value
                ? "Guard"
                : "Thrower";

            string status = eliminatedPlayers.Contains(entry.Key)
                ? " (Spectating)"
                : "";

            board += $"{role} [{entry.Key}]: {entry.Value}{status}\n";
        }

        UpdateScoreboardClientRpc(board);
    }

    [ClientRpc]
    private void UpdateScoreboardClientRpc(string board)
    {
        if (scoreText != null)
            scoreText.text = board;
    }

    [ClientRpc]
    private void UpdateMatchUIClientRpc()
    {
        UpdateRoleUI();

        if (startButton != null)
            startButton.gameObject.SetActive(
                IsServer && !matchActive.Value
            );
    }

    [ClientRpc]
    private void ShowMessageClientRpc(string message)
    {
        if (resultText != null)
            resultText.text = message;
    }

    [ClientRpc]
    private void ShowResultClientRpc(string message)
    {
        if (resultText != null)
            resultText.text = message;
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SetEliminatedClientRpc(ulong playerId, RpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId != playerId)
            return;

        if (roleText != null)
            roleText.text = "Role: Spectator";

        // Connect your spectator camera and movement logic here.
    }

    private void OnMatchActiveChanged(bool previous, bool current)
    {
        if (startButton != null)
            startButton.gameObject.SetActive(IsServer && !current);

        if (!current)
        {
            if (timerText != null)
                timerText.text = "";

            if (scoreText != null)
                scoreText.text = "";

            if (roleText != null)
                roleText.text = "";
        }
        else
        {
            UpdateTimerUI(timeRemaining.Value);
            UpdateRoleUI();
            RefreshScoreboard();

            if (resultText != null)
                resultText.text = "";
        }
    }

    private void OnTimeChanged(int previous, int current)
    {
        UpdateTimerUI(current);
    }

    private void OnGuardChanged(ulong previous, ulong current)
    {
        UpdateRoleUI();
    }

    private void UpdateTimerUI(int seconds)
    {
        if (timerText == null)
            return;

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        timerText.text = $"{seconds}";
    }
    [ClientRpc]
    private void SetTumbangPresoUIClientRpc(bool showMatchUI, bool showStartButtons)
    {
        if (tumbangPresoUI != null)
            tumbangPresoUI.SetActive(showMatchUI);

        if (startButtons != null)
            startButtons.SetActive(showStartButtons && IsHost);

        if (startButton != null)
            startButton.gameObject.SetActive(showStartButtons && IsHost);
    }
    private void UpdateRoleUI()
    {
        if (roleText == null || !IsSpawned)
            return;

        if (!matchActive.Value)
        {
            roleText.text = "";
            return;
        }

        ulong localId = NetworkManager.Singleton.LocalClientId;

        if (localId == guardClientId.Value)
            roleText.text = "Role: Guard";
        else
            roleText.text = "Role: Thrower";
    }
}
