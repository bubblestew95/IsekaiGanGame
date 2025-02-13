using UnityEngine;
using UnityEngine.UI;

public class FillScreenImage : MonoBehaviour
{
    void Start()
    {
        Image image = GetComponent<Image>();
        RectTransform rectTransform = image.GetComponent<RectTransform>();

        // 화면 크기 받아오기
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // RectTransform 크기 설정
        rectTransform.sizeDelta = new Vector2(screenWidth, screenHeight);
    }
}