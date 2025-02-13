using UnityEngine;
using UnityEngine.UI;

public class FillScreenImage : MonoBehaviour
{
    RectTransform rectTransform;
    Vector2 lastScreenSize;

    void Start()
    {
        // RectTransform �ʱ�ȭ
        Image image = GetComponent<Image>();
        rectTransform = image.GetComponent<RectTransform>();

        // ó�� ȭ�� ũ�� ����
        lastScreenSize = new Vector2(Screen.width, Screen.height);

        // �ʱ� ������ ����
        UpdateSize();
    }

    void Update()
    {
        // ȭ�� ũ�� ������ ���� ���� ����
        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);

        if (currentScreenSize != lastScreenSize)
        {
            lastScreenSize = currentScreenSize;
            UpdateSize();
        }
    }

    void UpdateSize()
    {
        // ȭ�� ũ�� ������� �̹��� ũ�� ������Ʈ
        rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
    }
}
