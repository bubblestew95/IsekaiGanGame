using UnityEngine;
using UnityEngine.UI;

public class PlayerHitImpact : MonoBehaviour
{
    private Image Image;

    private void Awake()
    {
        Image = GetComponent<Image>();
        PlayerDamagedImpactOff();
    }

    public void PlayerDamagedImpactOn()
    {
        Image.enabled = true;
        Invoke("PlayerDamagedImpactOff", 0.2f);
    }
    public void PlayerDamagedImpactOff()
    {
        Image.enabled = false;
    }
}
