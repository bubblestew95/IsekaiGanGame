using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EnumTypes;

public class UIBattleUIManager : MonoBehaviour
{
    public PlayerManager playerManager = null;
    public List<UIHpsManager> hpList = new List<UIHpsManager>();
    public List<UIBossHpsManager> bossHpList = new List<UIBossHpsManager>();
    public List<UIWarningManager> warningList = new List<UIWarningManager>();
    public List <ButtonSetting> buttonList = new List<ButtonSetting>();
    public FloatingJoystick joyStick = null;
    // public List<float> cooltimeList = new List<float>(); //��Ÿ,ȸ��,��ų1,��ų2,��ų3 ����

    public int bossMaxHp = 1;
    public int playerMaxHp = 1;

    private SkillButtonsManager skillButtonsManager = null;
    private SkillUIManager skillUIManager = null;

    private void Awake()
    {
        hpList = GetComponentsInChildren<UIHpsManager>().ToList();
        bossHpList = GetComponentsInChildren<UIBossHpsManager>().ToList();
        warningList = GetComponentsInChildren<UIWarningManager>().ToList();
        buttonList = GetComponentsInChildren<ButtonSetting>().ToList();
        joyStick = GetComponentInChildren<FloatingJoystick>();

        SetupAllUI();

        skillButtonsManager = GetComponentInChildren<SkillButtonsManager>();
        skillUIManager = transform.parent.GetComponentInChildren<SkillUIManager>();
    }

    /// <summary>
    /// �Է¹��� ��ų ��ư�� �÷��̾� �Ŵ������� �����Ѵ�.
    /// </summary>
    /// <param name="_type">�Է¹��� ��ų ��ư�� ��ų Ÿ��</param>
    public void OnClickedSkillButton(SkillType _type)
    {
        if (playerManager == null)
        {
            Debug.LogWarning("Player Manager is Null!");
            return;
        }

        // playerManager.OnButtonInput(_type);
    }
    public void OnSkillButtonDown(SkillType _type)
    {
        skillUIManager.SetSkillUIEnabled(_type, true);
    }
    public void OnSkillButtonUp(SkillType _type)
    {
        Vector3 point = skillUIManager.GetSkillAimPoint(_type);

        // playerManager.OnButtonInput(_type);
        playerManager.TryUseSkill(_type, point);
        skillUIManager.SetSkillUIEnabled(_type, false);
    }
    public void OnSkillButtonExit(SkillType _type)
    {

    }

    public void SendSkillDirectionToSkillUI(SkillType _type, float _horizontal, float _vertical)
    {
        skillUIManager.SetSkillAimPoint(_type, _horizontal, _vertical);
    }

    //public void CooltimeListSetting(List<float> _timeList) // ��Ÿ,ȸ��,��ų1,��ų2,��ų3 ����
    //{
    //    for (int i = 0; i < _timeList.Count; i++)
    //    {
    //        cooltimeList[i] = _timeList[i];
    //    }
    //}

    public void SetupAllUI() // ��ü������ �ѹ� �� ���ϰ� ����
    {
        foreach (UIBossHpsManager hp in bossHpList) // ���� �ִ�ü�� ����
        {
            hp.SetMaxHp(bossMaxHp);
            hp.HpBarUIUpdate();
        }        
        foreach (UIHpsManager hp in hpList) // �÷��̾� �ִ�ü�� ����
        {
            hp.SetMaxHp(playerMaxHp);
            hp.HpBarUIUpdate();
        }
        warningList[0].ReConnection(); // ���� ���� UI ��Ȱ��ȭ

        for (int i = 0; i < buttonList.Count; i++) // ��Ÿ�� ����Ʈ�� �ִ� ��Ÿ�ӵ� ���� ��ư�� ����// ��Ÿ,ȸ��,��ų1,��ų2,��ų3 ����
        {
            ButtonSetting buttonSetting = buttonList[i];
        }
    }

    /// <summary>
    /// ������ Ÿ���� ��ų ��ư�� ��Ÿ�� UI�� �����Ѵ�.
    /// </summary>
    /// <param name="_type">������ ��ų Ÿ��</param>
    /// <param name="_time">��Ÿ�� �ð�</param>
    public void ApplyCooltime(SkillType _type, float _time)
    {
        skillButtonsManager.ApplyCooltime(_type, _time);
    }
}
