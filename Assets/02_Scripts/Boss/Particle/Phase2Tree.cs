using UnityEngine;

public class Phase2Tree : MonoBehaviour
{
    public GameObject fire1;
    public GameObject fire2;
    public GameObject fire3;

    private void SetOn()
    {
        fire1.SetActive(true);
        fire2.SetActive(true);
        fire3.SetActive(true);
    }
}
