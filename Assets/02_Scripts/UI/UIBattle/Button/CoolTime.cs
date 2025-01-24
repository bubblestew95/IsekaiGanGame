using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoolTime : MonoBehaviour
{
    public float maxCooltime = 0;
    public float curCooltime = 0;
    private Image coolImage = null;
    public TextMeshProUGUI coolSecondUI = null;
    public bool isPressed = false;
    private void Awake()
    {
        coolSecondUI = GetComponentInChildren<TextMeshProUGUI>();
        coolImage = GetComponent<Image>();
    }

    public void SetMaxCooltime(float _cooltime)
    {
        maxCooltime = _cooltime;
        CooltimeDown();
    } // �ʱ� ��Ÿ�� ����

    public void CooltimeDown()
    {
        curCooltime = maxCooltime;
        StartCoroutine(cooldown());
    } // �Լ� �����ϸ� ���� ����

    private IEnumerator cooldown() // UI ǥ�ö� �� ��� ���ÿ� �ϴ� �ڷ�ƾ
    {
        float coolratio = 0;
        coolSecondUI.enabled = true;
        isPressed = true;
        while (curCooltime > 0)
        {
            curCooltime -= Time.deltaTime; //��Ÿ�� ���
            if (curCooltime >= 1)
            {
                coolSecondUI.text = curCooltime.ToString("F0"); //��Ÿ��UI Txtǥ��
            }
            if (curCooltime < 1)
            {
                coolSecondUI.text = curCooltime.ToString("F1"); //��Ÿ��UI Txtǥ��
            }
            coolratio = curCooltime / maxCooltime; // ������ ���� ���
            coolImage.fillAmount = coolratio;//��Ÿ��UI �ð�ǥ��
            yield return null;
        }
        curCooltime = 0; 
        coolSecondUI.enabled = false;
        isPressed = false;
    }
}
