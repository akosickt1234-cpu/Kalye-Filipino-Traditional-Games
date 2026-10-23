
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ThrowerBase : NetworkBehaviour
{
    private readonly HashSet<ulong> playersWhoLeftBase = new();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        PlayerNetwork player =
            other.GetComponentInParent<PlayerNetwork>();

        if (player == null)
            return;

        ulong playerId = player.OwnerClientId;
        player.SetInsideThrowerBase(true);

        TumbangPresoManager manager =
            FindAnyObjectByType<TumbangPresoManager>();

        if (manager == null || !manager.IsMatchActive)
            return;

        // The Guard doesn't receive thrower return points.
        if (manager.GuardId == playerId)
            return;

        // Only reward a thrower who actually left the base.
        if (!playersWhoLeftBase.Contains(playerId))
            return;

        // The thrower must have retrieved their slipper.
        if (player.HasSlipperInWorld)
            return;

        /*PresoCan can = FindAnyObjectByType<PresoCan>();

        // The can must have been knocked down this attempt.
        if (can == null || !can.HasBeenKnockedDown)
            return;
        */
        // AwardSafeReturn should award +6 once and reset the can
        // when the manager confirms the successful return.
        if (manager.AwardSafeReturn(playerId))
        {
            playersWhoLeftBase.Remove(playerId);
            
            Debug.Log($"Thrower {playerId} returned safely: +6 points!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer)
            return;

        PlayerNetwork player =
            other.GetComponentInParent<PlayerNetwork>();

        if (player == null)
            return;

        ulong playerId = player.OwnerClientId;
        player.SetInsideThrowerBase(false);

        TumbangPresoManager manager =
            FindAnyObjectByType<TumbangPresoManager>();

        if (manager != null &&
            manager.IsMatchActive &&
            manager.GuardId != playerId)
        {
            playersWhoLeftBase.Add(playerId);
        }
    }
}
