using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    private PlayerManager playerManager = null;

    private void Awake()
    {
        playerManager = FindAnyObjectByType<PlayerManager>();
    }

    private void Update()
    {
        transform.position = playerManager.transform.position;
    }
}
