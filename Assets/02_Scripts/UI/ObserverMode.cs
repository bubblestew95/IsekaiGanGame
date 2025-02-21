using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObserverMode : MonoBehaviour
{
    // �����ϱ� ��ư�� ���۳�Ʈ���� ������
    private List<Button> buttonList = new List<Button>();
    public List<TextMeshProUGUI> textMeshProUGUI;
    public bool isObserverModeUI = false;
    private void Awake()
    {
        buttonList = GetComponentsInChildren<Button>().ToList(); // �ڽ� ��ư ����Ʈ�� �޾ƿ�
        textMeshProUGUI = GetComponentsInChildren<TextMeshProUGUI>().ToList(); //�ڽ� �ؽ�Ʈ �޾ƿ�

        ObserverModeHow(false);
    }

    public void ObserverModeHow(bool _how)
    {
        foreach (Button button in buttonList) 
        {
            button.gameObject.SetActive(_how); // ��ư�� _how ó��
        }
        foreach (TextMeshProUGUI texts in textMeshProUGUI)
        {
            if (texts == textMeshProUGUI[0])
            {
                continue;
            }
            texts.gameObject.SetActive(_how);// �ؽ�Ʈ�� _how ó��
        }
        isObserverModeUI = _how; //�Ұ� _how ó��
    }
    public void ObserverModeButtonOn()
    {
        buttonList[0].gameObject.SetActive(true);
    }
}
