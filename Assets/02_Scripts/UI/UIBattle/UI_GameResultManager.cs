using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UI_GameResultManager : MonoBehaviour
{
    public Image backgroundImg = null;
    public List<Image> imgs = new List<Image>();
    public List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    public List<Button> buttons = new List<Button>();
    public bool isGood;

    // ������ ������
    private List<Color> initialImgColors = new List<Color>();
    private List<Vector3> initialImgScales = new List<Vector3>();
    private List<Color> initialTextColors = new List<Color>();
    private List<Vector3> initialTextScales = new List<Vector3>();
    private Color initialBackgroundColor;
    private Vector3 initialBackgroundScale;


    private void Awake()
    {
        // �ڽ� �̹����� �� �ؽ�Ʈ���� ����Ʈ�� ����
        imgs = GetComponentsInChildren<Image>().ToList();
        texts = GetComponentsInChildren<TextMeshProUGUI>().ToList();
        backgroundImg = GetComponentInChildren<FillScreenImage>().GetComponent<Image>();
        buttons = GetComponentsInChildren<Button>().ToList();  // ��ư ����Ʈ�� �ʱ�ȭ

        // �� UI ��ҵ��� �ʱ� ���� ���� ũ�⸦ ����
        foreach (var img in imgs)
        {
            initialImgColors.Add(img.color);
            initialImgScales.Add(img.transform.localScale);
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);  // �ʱ� ���İ� 0���� ����
            img.transform.localScale = Vector3.zero;  // �ʱ� ũ�� 0���� ����
        }

        foreach (var text in texts)
        {
            initialTextColors.Add(text.color);
            initialTextScales.Add(text.transform.localScale);
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);  // �ʱ� ���İ� 0���� ����
            text.transform.localScale = Vector3.zero;  // �ʱ� ũ�� 0���� ����
        }

        if (backgroundImg != null)
        {
            initialBackgroundColor = backgroundImg.color;
            initialBackgroundScale = backgroundImg.transform.localScale;
            backgroundImg.color = new Color(backgroundImg.color.r, backgroundImg.color.g, backgroundImg.color.b, 0f);  // �ʱ� ���İ� 0���� ����
            backgroundImg.transform.localScale = Vector3.zero;  // �ʱ� ũ�� 0���� ����
        }

        // ��ư�� �ʱ�ȭ �� �� ��� ��Ȱ��ȭ
        foreach (var button in buttons)
        {
            button.interactable = false;
        }
    }

    // UI ��ҵ� ���̵� �� �� ������ ���� ó���ϴ� �ڷ�ƾ
    public IEnumerator FadeInAndScaleIn()
    {
        // �ִϸ��̼� ���� �ð�
        float duration = 0.3f;

        // ��� �̹��� ���̵� �� ó�� (���� ���� ��Ÿ���� ��)
        if (backgroundImg != null)
        {
            Color currentColor = initialBackgroundColor;

            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                backgroundImg.color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(0f, currentColor.a, t)); // ���� �� ���̵� ��

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            backgroundImg.color = currentColor; // ���� ���İ����� ����
        }

        // �̹����鿡 ���� ���̵� �ΰ� ������ �� ó��
        for (int i = 0; i < imgs.Count; i++)
        {
            var img = imgs[i];
            Color currentColor = initialImgColors[i];
            Vector3 currentScale = initialImgScales[i];

            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                img.color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(0f, currentColor.a, t)); // ���� �� ���̵� ��
                img.transform.localScale = Vector3.Lerp(Vector3.zero, currentScale, t); // ũ�� ����

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            img.color = currentColor; // ���� ���İ����� ����
            img.transform.localScale = currentScale; // ���� ũ��� ����
        }

        // �ؽ�Ʈ�鿡 ���� ���̵� �ΰ� ������ �� ó��
        for (int i = 0; i < texts.Count; i++)
        {
            var text = texts[i];
            Color currentColor = initialTextColors[i];
            Vector3 currentScale = initialTextScales[i];

            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                float t = timeElapsed / duration;
                text.color = new Color(currentColor.r, currentColor.g, currentColor.b, Mathf.Lerp(0f, currentColor.a, t)); // ���� �� ���̵� ��
                text.transform.localScale = Vector3.Lerp(Vector3.zero, currentScale, t); // ũ�� ����

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            text.color = currentColor; // ���� ���İ����� ����
            text.transform.localScale = currentScale; // ���� ũ��� ����
        }

        // ��� UI ��ҵ��� �� ���̵� �� �Ǹ� ��ư�� Ȱ��ȭ
        foreach (var button in buttons)
        {
            button.interactable = true;
        }
    }

    // �ڷ�ƾ�� �����ϴ� ���� (�ʿ��� ������ ȣ��)
    public void StartFadeIn(bool _isGood)
    {
        if (_isGood == isGood)
        {
            StartCoroutine(FadeInAndScaleIn());
        }
    }
}
