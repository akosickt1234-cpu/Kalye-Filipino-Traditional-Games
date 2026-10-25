
using Unity.Netcode;
using UnityEngine;

public class Tsinelas : NetworkBehaviour
{
    public ulong ThrowerClientId { get; private set; }

    public void SetThrower(ulong clientId)
    {
        if (!IsServer)
            return;

        ThrowerClientId = clientId;
    }
}
