
using Unity.Netcode;
using UnityEngine;

public class PatinteroZone : MonoBehaviour
{
    public enum ZoneType
    {
        Checkpoint,
        StartFinish
    }

    [SerializeField] private PatinteroManager manager;
    [SerializeField] private ZoneType zoneType;

    private void OnTriggerEnter(Collider other)
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
            return;

        PlayerNetwork player =
            other.GetComponentInParent<PlayerNetwork>();

        if (player == null || player.taya.Value)
            return;

        if (zoneType == ZoneType.Checkpoint)
        {
            manager.RegisterCheckpoint(player.OwnerClientId);
        }
        else if (zoneType == ZoneType.StartFinish)
        {
            manager.TryRunnerFinish(player.OwnerClientId);
        }
    }
}
