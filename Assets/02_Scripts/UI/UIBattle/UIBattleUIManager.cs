using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EnumTypes;
using StructTypes;

public class UIBattleUIManager : MonoBehaviour
{
    [SerializeField]
    private SkillUIManager skillUIManager = null;
    [SerializeField]
    private SkillButtonsManager skillButtonsManager = null;

    public PlayerManager playerManager = null;
    public List<UIHpsManager> hpList = new List<UIHpsManager>();
    public List<UIBossHpsManager> bossHpList = new List<UIBossHpsManager>();
    public List<UIWarningManager> warningList = new List<UIWarningManager>();
    public List <ButtonSetting> buttonList = new List<ButtonSetting>();



    public int bossMaxHp = 1;
    public int playerMaxHp = 1;


    private void Awake()
    {
        hpList = GetComponentsInChildren<UIHpsManager>().ToList();
        bossHpList = GetComponentsInChildren<UIBossHpsManager>().ToList();
        warningList = GetComponentsInChildren<UIWarningManager>().ToList();
        buttonList = GetComponentsInChildren<ButtonSetting>().ToList();

        SetupAllUI();

        // skillButtonsManager = GetComponentInChildren<SkillButtonsManager>();
    }

    /// <summary>
    /// �Է¹��� ��ų ��ư�� �÷��̾� �Ŵ������� �����Ѵ�.
    /// </summary>
    /// <param name="_slot">�Է¹��� ��ų ��ư�� ��ų Ÿ��</param>
    public void OnClickedSkillButton(SkillSlot _slot)
    {
        if (playerManager == null)
        {
            Debug.LogWarning("Player Manager is Null!");
            return;
        }

        // playerManager.OnButtonInput(_type);
    }
    public void OnSkillButtonDown(SkillSlot _slot)
    {
        skillUIManager.SetSkillUIEnabled(_slot, true);
    }
    public void OnSkillButtonUp(SkillSlot _slot)
    {
        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);

        playerManager.OnButtonInput(_slot, pointData);
        // playerManager.TryUseSkill(_type);
        skillUIManager.SetSkillUIEnabled(_slot, false);
    }

    public void SendSkillDirectionToSkillUI(SkillSlot _slot, float _horizontal, float _vertical)
    {
        skillUIManager.SetSkillAimPoint(_slot, _horizontal, _vertical);
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
    /// <param name="_slot">������ ��ų Ÿ��</param>
    /// <param name="_time">��Ÿ�� �ð�</param>
    public void ApplyCooltime(SkillSlot _slot, float _time)
    {
        skillButtonsManager.ApplyCooltime(_slot, _time);
    }
}
