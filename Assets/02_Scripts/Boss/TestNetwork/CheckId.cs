using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class CheckId : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        GameManager.Instance.CheckPlayerSpawnServerRpc();
    }

    private void CheckNetworkSpwan()
    {

    }
}
