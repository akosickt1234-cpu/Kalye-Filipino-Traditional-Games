
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ThrowerBase : NetworkBehaviour
{
    [SerializeField] private Collider baseCollider;

    private readonly HashSet<ulong> playersWhoLeftBase = new();

    private void Awake()
    {
        if (baseCollider == null)
            baseCollider = GetComponent<Collider>();
    }

    public bool ShouldBlockLeaving(
        PlayerNetwork player,
        Vector3 currentPosition,
        Vector3 nextPosition)
    {
        if (player == null || baseCollider == null)
            return false;

        TumbangPresoManager manager =
            FindAnyObjectByType<TumbangPresoManager>();

        if (manager == null || !manager.IsMatchActive)
            return false;

        // The Guard can leave freely.
        if (manager.GuardId == player.OwnerClientId)
            return false;

        // They must throw before leaving the base.
        if (player.CanLeaveThrowerBase)
            return false;

        Bounds bounds = baseCollider.bounds;

        // Check the horizontal footprint so the player's
        // height or jumping does not affect the boundary.
        bool currentlyInside =
            currentPosition.x >= bounds.min.x &&
            currentPosition.x <= bounds.max.x &&
            currentPosition.z >= bounds.min.z &&
            currentPosition.z <= bounds.max.z;

        bool nextPositionInside =
            nextPosition.x >= bounds.min.x &&
            nextPosition.x <= bounds.max.x &&
            nextPosition.z >= bounds.min.z &&
            nextPosition.z <= bounds.max.z;

        return currentlyInside && !nextPositionInside;
    }

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

        // The Guard can enter and leave freely.
        if (manager.GuardId == playerId)
            return;

        // Returning to the base locks the thrower in again.
        if (player.CanLeaveThrowerBase)
            player.SetCanLeaveThrowerBase(false);

        if (!playersWhoLeftBase.Contains(playerId))
            return;

        // Award safe-return points only if the slipper
        // is no longer in the world.
        if (player.HasSlipperInWorld)
            return;

        if (manager.AwardSafeReturn(playerId))
        {
            playersWhoLeftBase.Remove(playerId);

            Debug.Log(
                $"Thrower {playerId} returned safely: +6 points!"
            );
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

        TumbangPresoManager manager =
            FindAnyObjectByType<TumbangPresoManager>();

        if (manager == null || !manager.IsMatchActive)
        {
            player.SetInsideThrowerBase(false);
            return;
        }

        // The Guard can leave freely.
        if (manager.GuardId == playerId)
        {
            player.SetInsideThrowerBase(false);
            return;
        }

        // Do not allow an unapproved exit.
        // The movement check should stop the player
        // before they cross the boundary.
        if (!player.CanLeaveThrowerBase)
        {
            Debug.Log(
                $"Thrower {playerId} must throw their tsinelas first!"
            );

            return;
        }

        player.SetInsideThrowerBase(false);
        playersWhoLeftBase.Add(playerId);
    }
}
