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
    private UIHpsManager ui_PlayerHp = null;
    private UIBossHpsManager ui_BossHp = null;
    private UIWarningManager ui_disconnect = null;

    public int bossMaxHp = 1;
    public int playerMaxHp = 1;


    private void Awake()
    {
        ui_PlayerHp = GetComponentInChildren<UIHpsManager>();
        ui_BossHp = GetComponentInChildren<UIBossHpsManager>();
        ui_disconnect = GetComponentInChildren<UIWarningManager>();
    }

    private void Start()
    {
        SetupAllUI();
    }

    /// <summary>
    /// �Է¹��� ��ų ��ư�� �÷��̾� �Ŵ������� �����Ѵ�.
    /// </summary>
    /// <param name="_slot">�Է¹��� ��ų ��ư�� ��ų Ÿ��</param>
    //public void OnClickedSkillButton(SkillSlot _slot)
    //{
    //    if (playerManager == null)
    //    {
    //        Debug.LogWarning("Player Manager is Null!");
    //        return;
    //    }

    //    // playerManager.OnButtonInput(_type);
    //}
    public void OnSkillJoystickDown(SkillSlot _slot)
    {
        skillUIManager.SetSkillUIEnabled(_slot, true);
    }
    public void OnSkillJoystickUp(SkillSlot _slot)
    {
        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);

        playerManager.OnButtonInput(_slot, pointData);
        skillUIManager.SetSkillUIEnabled(_slot, false);
    }
    public void OnSkillButtonUp(SkillSlot _slot)
    {
        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);
        pointData.type = SkillPointType.None;
        pointData.point = GameManager.Instance.GetBossTransform().position;

        playerManager.OnButtonInput(_slot, pointData);
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
        ui_BossHp.SetMaxHp(GameManager.Instance.GetBossHp());
        ui_BossHp.HpBarUIUpdate();

        ui_PlayerHp.SetMaxHp(playerManager.StatusManager.MaxHp);
        ui_PlayerHp.HpBarUIUpdate();

        ui_disconnect.ReConnection(); // ���� ���� UI ��Ȱ��ȭ
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
