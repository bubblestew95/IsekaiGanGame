using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBossHpsManager : MonoBehaviour
{
    public int maxHp = 0;
    public int curHp = 0;
    private List<Image> images = new List<Image>();
    private TextMeshProUGUI textMeshPro = null;
    public RectTransform rectTransform = null; // ��鸱 RectTransform
    public float shakeAmount = 3f;     // ���� ����
    public float shakeDuration = 0.5f;  // ���� �ð�
    private void Awake()
    {
        // rectTransform�� �� ������Ʈ�� RectTransform���� ����
        rectTransform = GetComponent<RectTransform>();
        images = GetComponentsInChildren<Image>().ToList();
        textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        foreach (Image img in images)
        {
            img.raycastTarget = false;
        }
        textMeshPro.raycastTarget = false;
    }

    public void SetMaxHp(int _maxHp)
    {
        maxHp = _maxHp;
        curHp = _maxHp;
    } // �ִ�ü�� ���� �׸��� �ִ�ü�� ��ġ�� ����ü�� ��ġ�� ����� �Լ� 

    public void SetCurrentHp(int _curHp)
    {
        curHp = Mathf.Clamp(_curHp, 0, maxHp);
    }
    public void BossDamage(int _Damage)
    {
        if (IsAlive())
        {
            curHp -= _Damage;
            if (IsDead())
            {
                curHp = 0;
            }
        }
    } //������ �ִ� �Լ�

    public void HpBarUIUpdate()
    {
        textMeshPro.text = (curHp + "/" + maxHp); // �ؽ�Ʈ ������Ʈ "����ü��/�ִ�ü��"
        Vector2 newCurHP = images[0].rectTransform.sizeDelta;
        float hpRatio = (float)curHp / (float)maxHp;
        newCurHP.x = hpRatio * newCurHP.x;  // X ���� ����
        images[images.Count - 1].rectTransform.sizeDelta = newCurHP;  // ����� sizeDelta ����
        StartCoroutine(Shake());//����ü�¹� ����
    }//UI ǥ�� �Լ�
    IEnumerator Shake()
    {
        Vector3 originalPos = rectTransform.localPosition;  // ���� ��ġ�� ����

        float elapsed = 0f;  // ��� �ð�
        while (elapsed < shakeDuration)
        {
            float xOffset = Random.Range(-shakeAmount, shakeAmount); // �¿�� ������ ��
            rectTransform.localPosition = originalPos + new Vector3(xOffset, 0f, 0f); // ��ġ ����

            elapsed += Time.deltaTime; // �ð� ����
            yield return null; // �� ������ ��ٸ�
        }

        rectTransform.localPosition = originalPos; // ���� ��ġ�� ����
    }
    #region ���� üũ
    public bool IsAlive()
    {
        if(curHp > 0)
            return true;
        else
            return false;
    } //���� ü�� 0 �ʰ��� true
    public bool IsDead()
    {
        if (curHp <= 0)
            return true;
        else
            return false;
    }//���� ü�� 0 �����̸� true
    #endregion
}
