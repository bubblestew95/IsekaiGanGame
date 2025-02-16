using UnityEngine;

public class GolemSceneUi : MonoBehaviour
{
    [SerializeField] private GameObject loadingImg;

    private void Start()
    {
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += () => Invoke("LoadingSetOff", 1f);
    }

    // �ε� �� ����
    private void LoadingSetOff()
    {
        loadingImg.SetActive(false);
    }
}
