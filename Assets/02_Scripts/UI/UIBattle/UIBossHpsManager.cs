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

    private void Awake()
    {
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
        float hpRatio = curHp / maxHp;
        newCurHP.x = hpRatio * newCurHP.x;  // X ���� ����
        images[images.Count - 1].rectTransform.sizeDelta = newCurHP;  // ����� sizeDelta ����
    }//UI ǥ�� �Լ�
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
