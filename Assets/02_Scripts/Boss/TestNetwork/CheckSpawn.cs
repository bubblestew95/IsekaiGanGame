using Unity.Netcode;
using UnityEngine;

public class CheckSpawn : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        FindAnyObjectByType<NetworkGameManager>().CheckPlayerSpawnServerRpc();
    }

}
