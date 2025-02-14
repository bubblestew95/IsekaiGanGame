using UnityEngine;
using UnityEngine.UI;

public class ButtonInvoker : MonoBehaviour
{
    private Button targetButton;  // ��ư ������Ʈ

    void Awake()
    {
        // ���� ������Ʈ���� ��ư ������Ʈ ��������
        targetButton = GetComponent<Button>();

        // ��ư�� �����ϴ� ��쿡�� �̺�Ʈ ȣ��
        if (targetButton != null)
        {
            // ��ư�� ��ϵ� OnClick �̺�Ʈ�� �� �� ����
            targetButton.onClick.Invoke();
        }
    }
}
