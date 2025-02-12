using UnityEngine;

public class LoadingOff : MonoBehaviour
{
    private void Start()
    {
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += () => Invoke("SetOff", 1f);
    }

    private void SetOff()
    {
        gameObject.SetActive(false);
    }
}
