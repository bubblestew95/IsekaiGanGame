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
    [SerializeField]
    private FloatingJoystick moveJoystick = null;
    [SerializeField]
    private PlayerManager playerManager = null;

    private UIHpsManager ui_PlayerHp = null;
    private UIBossHpsManager ui_BossHp = null;
    private UIWarningManager ui_disconnect = null;
    public List<UI_GameResultManager> ui_GameResultManager = new List<UI_GameResultManager>();
    public FloatingJoystick MoveJoystick
    {
        get { return moveJoystick; }
    }

    private void Awake()
    {
        ui_PlayerHp = GetComponentInChildren<UIHpsManager>();
        ui_BossHp = GetComponentInChildren<UIBossHpsManager>();
        ui_disconnect = GetComponentInChildren<UIWarningManager>();
        ui_GameResultManager = GetComponentsInChildren<UI_GameResultManager>().ToList();

    }

    private void Start()
    {
        SetupAllUI();
    }

    public void FadeInResult(bool _isWin)
    {
        foreach (UI_GameResultManager result in ui_GameResultManager)
        {
            result.StartFadeIn(_isWin);
        }
    }

    #region Skill Button, Joysticks

    public void OnSkillJoystickDown(SkillSlot _slot)
    {
        skillUIManager.SetSkillUIEnabled(_slot, true);
    }
    public void OnSkillJoystickUp(SkillSlot _slot)
    {
        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);

        playerManager.InputManager.OnButtonInput(_slot, pointData);
        skillUIManager.SetSkillUIEnabled(_slot, false);
    }
    public void OnSkillButtonUp(SkillSlot _slot)
    {
        SkillPointData pointData = skillUIManager.GetSkillAimPoint(_slot);
        pointData.type = SkillPointType.None;
        pointData.skillUsedPosition = GameManager.Instance.GetBossTransform().position;

        Vector3 direction = GameManager.Instance.GetBossTransform().position - playerManager.transform.position;
        direction.y = playerManager.transform.position.y;
        pointData.skillUsedRotation = Quaternion.LookRotation(direction);

        playerManager.InputManager.OnButtonInput(_slot, pointData);
    }

    public void SendSkillDirectionToSkillUI(SkillSlot _slot, float _horizontal, float _vertical)
    {
        skillUIManager.SetSkillAimPoint(_slot, _horizontal, _vertical);
    }

    public void SetSkillButtonEnabled(SkillSlot _slot, bool _enabled)
    {
        skillButtonsManager.SetSkillButtonEnabled(_slot, _enabled);
    }

    #endregion

    public void SetupAllUI() // ��ü������ �ѹ� �� ���ϰ� ����
    {
        ui_BossHp.SetMaxHp(GameManager.Instance.GetBossHp());
        ui_BossHp.HpBarUIUpdate();

        ui_PlayerHp.SetMaxHp(playerManager.StatusManager.MaxHp);
        ui_PlayerHp.HpBarUIUpdate();

        ui_disconnect.ReConnection(); // ���� ���� UI ��Ȱ��ȭ
    }

    public void UpdatePlayerHp()
    {
        ui_PlayerHp.SetCurrentHp(playerManager.StatusManager.CurrentHp);
        ui_PlayerHp.HpBarUIUpdate();
    }

    public void UpdateBossHp()
    {
        ui_BossHp.SetCurrentHp(GameManager.Instance.GetBossHp());
        ui_BossHp.HpBarUIUpdate();
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
