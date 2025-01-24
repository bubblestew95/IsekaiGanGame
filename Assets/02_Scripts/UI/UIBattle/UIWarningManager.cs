using UnityEngine;
using UnityEngine.UI;

public class UIWarningManager : MonoBehaviour
{
    public Image errorImage = null;
    public bool isError = false;

    private void Awake()
    {
        errorImage = GetComponentInChildren<Image>();
        errorImage.enabled = false;
    }
    
    public void ConnectionError() // ���� ǥ�� On
    {
        errorImage.enabled = true;
    }
    public void ReConnection() // ���� ǥ�� OFF
    {
        errorImage.enabled = false;
    }
}
