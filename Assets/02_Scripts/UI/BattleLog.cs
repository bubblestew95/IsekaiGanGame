using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleLog : MonoBehaviour
{
    /// <summary>
    /// �ൿ �̹���
    /// </summary>
    [SerializeField]
    private LogSlot image_How = null;
    /// <summary>
    /// ��ü �̹���
    /// </summary>
    [SerializeField]
    private LogSlot image_Who = null;
    /// <summary>
    /// ��� �̹���
    /// </summary>
    [SerializeField]
    private LogSlot image_Whom = null;

    /// <summary>
    /// �α� ��� ���ӽð�
    /// </summary>
    [SerializeField]
    private float showLogDuration = 2f;



    /// <summary>
    /// �α��� �̹����� �����ϴ� �Լ�.
    /// </summary>
    public void SetLogImages()
    {

    }

    /// <summary>
    /// �α׸� ����ϴ� �Լ�
    /// </summary>
    public void ShowLog()
    {
        // �α׸� ����Ѵ�.
        {
            image_How.gameObject.SetActive(true);
            image_Who.gameObject.SetActive(true);
            image_Whom.gameObject.SetActive(true);
        }

        // ���� �ð� �� �α׸� �ٽ� �����Ѵ�.
        StartCoroutine(HideLogCoroutine(showLogDuration));
    }

    //
    private IEnumerator HideLogCoroutine(float _duration)
    {
        yield return new WaitForSeconds(_duration);

        // �α׸� �ٽ� �����.
        {
            image_How.gameObject.SetActive(false);
            image_Who.gameObject.SetActive(false);
            image_Whom.gameObject.SetActive(false);
        }
    }
}
