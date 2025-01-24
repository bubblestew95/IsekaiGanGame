using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoolTime : MonoBehaviour
{
    public float maxCooltime = 0;
    public float curCooltime = 0;
    private Image coolImage = null;
    private TextMeshProUGUI coolSecondUI = null;
    private void Awake()
    {
        coolSecondUI = GetComponentInChildren<TextMeshProUGUI>();
        coolImage = GetComponent<Image>();
    }

    public void SetMaxCooltime(float _cooltime)
    {
        maxCooltime = _cooltime;
    } // �ʱ� ��Ÿ�� ����

    public void CooltimeDown()
    {
        curCooltime = maxCooltime;
        StartCoroutine(cooldown());
    } // �Լ� �����ϸ� ���� ����


    private IEnumerator cooldown() // UI ǥ�ö� �� ��� ���ÿ� �ϴ� �ڷ�ƾ
    {
        float coolratio = 0;
        while (curCooltime<=0)
        {
            curCooltime -= Time.deltaTime; //��Ÿ�� ���
            coolSecondUI.text = curCooltime.ToString(); //��Ÿ��UI Txtǥ��
            coolratio = curCooltime / maxCooltime; // ������ ���� ���
            coolImage.fillAmount = coolratio;//��Ÿ��UI �ð�ǥ��
            yield return null;
        }
    }
}
