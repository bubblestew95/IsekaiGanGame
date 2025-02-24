using UnityEngine;
using UnityEngine.UI;

public class LogSlot : MonoBehaviour
{
    [SerializeField]
    private Image slotImage = null;

    public void SetImage(Sprite sprite)
    {
        slotImage.sprite = sprite;
    }
}
