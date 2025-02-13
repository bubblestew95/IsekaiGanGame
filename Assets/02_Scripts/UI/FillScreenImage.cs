using UnityEngine;
using UnityEngine.UI;

public class FillScreenImage : MonoBehaviour
{
    void Start()
    {
        Image image = GetComponent<Image>();
        RectTransform rectTransform = image.GetComponent<RectTransform>();

        // ȭ�� ũ�� �޾ƿ���
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // RectTransform ũ�� ����
        rectTransform.sizeDelta = new Vector2(screenWidth, screenHeight);
    }
}