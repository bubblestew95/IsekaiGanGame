using Unity.Netcode;
using UnityEngine;

public class CheckId : NetworkBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log(NetworkObjectId);
        }
    }
}
