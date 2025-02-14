using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using EnumTypes;
using StructTypes;

public class PlayerSkillManager
{
    private PlayerManager playerManager = null;

    private List<PlayerSkillBase> skillDatas = null;

    private Animator animator = null;

    /// <summary>
    /// �ִϸ������� �� Ʈ���� ���̵� �̸� ĳ���س���, �̸� ��ų Ÿ�Կ� ���� �����س���.
    /// </summary>
    private Dictionary<SkillSlot, int> animatorIdMap = null;

    private Dictionary<SkillSlot, PlayerSkillBase> skillDataMap = null;
    private Dictionary<SkillSlot, float> currentCoolTimeMap = null;

    #region Public Functions

    /// <summary>
    /// �÷��̾� ��ų �Ŵ����� �ʱ�ȭ�Ѵ�.
    /// </summary>
    /// <param name="_skilldataList">��ų ������ ����Ʈ</param>
    public void Init(PlayerManager _mng)
    {
        playerManager = _mng;
        skillDatas = _mng.PlayerData.skills;

        animator = _mng.GetComponent<Animator>();

        animatorIdMap = new Dictionary<SkillSlot, int>();
        skillDataMap = new Dictionary<SkillSlot, PlayerSkillBase>();
        currentCoolTimeMap = new Dictionary<SkillSlot, float>();

        foreach (var data in skillDatas)
        {
            skillDataMap.Add(data.skillSlot, data);
            currentCoolTimeMap.Add(data.skillSlot, 0f);
        }

        // �켱 �ϵ��ڵ����� �ִϸ����� id�� ĳ����. Ȯ�强 �����ϸ� ���߿� ������ ��!
        {
            animatorIdMap.Add(SkillSlot.Skill_A, Animator.StringToHash("Skill_A"));
            animatorIdMap.Add(SkillSlot.Skill_B, Animator.StringToHash("Skill_B"));
            animatorIdMap.Add(SkillSlot.Skill_C, Animator.StringToHash("Skill_C"));
            animatorIdMap.Add(SkillSlot.BasicAttack, Animator.StringToHash("BasicAttack"));
            animatorIdMap.Add(SkillSlot.Dash, Animator.StringToHash("Dash"));
            animatorIdMap.Add(SkillSlot.Revive, Animator.StringToHash("ReviveOther"));
        }

    }

    /// <summary>
    /// ������ ��ų�� ����Ѵ�.
    /// </summary>
    /// <param name="_skillIdx">����� ��ų�� ��ų ����Ʈ �� �ε���</param>
    public void UseSkill(SkillSlot _slot)
    {
        // ����Ϸ��� �������� ��ȿ�� üũ.
        if (skillDatas == null || currentCoolTimeMap == null || !skillDataMap.ContainsKey(_slot))
        {
            Debug.LogWarning("Skill is not valid!");
            return;
        }

        playerManager.MovementManager.StopMove();

        // ��ų ���
        if (animatorIdMap.TryGetValue(_slot, out int animId))
        {
            if(GameManager.Instance.IsLocalGame)
            {
                animator.SetTrigger(animId);
            }
            else
            {
                playerManager.PlayerNetworkManager.SetNetworkAnimatorTrigger(animId);
            }
        }

        // ��Ÿ�� ����
        currentCoolTimeMap[_slot] = skillDataMap[_slot].coolTime;
    }

    public float GetCoolTime(SkillSlot _type)
    {
        return currentCoolTimeMap[_type];
    }

    /// <summary>
    /// ��� ��ų���� ��Ÿ���� ��ȭ����ŭ ����.
    /// </summary>
    /// <param name="_deltaTime">��ȭ��</param>
    public void DecreaseCoolTimes(float _delta)
    {
        foreach (var key in currentCoolTimeMap.Keys.ToList())
        {
            if(currentCoolTimeMap[key] > 0f)
                currentCoolTimeMap[key] = Mathf.Clamp(currentCoolTimeMap[key] - _delta, 0f, 10000f);
        }
    }


    /// <summary>
    /// ���� ������ ��ų Ÿ���� ��ų�� ��� ������ �� üũ�Ѵ�.
    /// </summary>
    /// <param name="_skillIdx">üũ�� ��ų�� Ÿ��</param>
    /// <returns></returns>
    public bool IsSkillUsable(SkillSlot _type)
    {
        if(!skillDataMap.ContainsKey(_type))
        {
            Debug.LogFormat("{0} Skill is not usable in this character!", _type);
            return false;
        }

        // ��Ÿ�� üũ
        if (currentCoolTimeMap[_type] > 0f)
        {
            Debug.LogFormat("{0} Skill is coolTime!", _type);
            return false;
        }

        return true;
    }

    public void SkillAction(SkillSlot _type)
    {
        skillDataMap[_type].UseSkill(playerManager);
    }

    public PlayerSkillBase GetSkillData(SkillSlot _type)
    {
        return skillDataMap[_type];
    }

    /// <summary>
    /// ��ų �ߵ��� �õ��Ѵ�.
    /// </summary>
    /// <param name="_skillIdx"></param>
    public void TryUseSkill(SkillSlot _type, SkillPointData _point)
    {
        // ��ų �ߵ��� �����ߴٸ�
        if (IsSkillUsable(_type))
        {
            // ĳ���͸� ����Ʈ�� ������ ������ ������ �Ѵ�.
            if (_point.type == SkillPointType.Position)
            {
                Vector3 tempPos = _point.skillUsedPosition;
                tempPos.y = 0f;
                tempPos.Normalize();
                playerManager.transform.rotation = Quaternion.LookRotation(tempPos);
            }
            else
            {
                playerManager.transform.rotation = _point.skillUsedRotation;
            }

            UseSkill(_type);

            // UI�� ��Ÿ���� �����Ѵ�.
            if (playerManager.BattleUIManager != null)
                playerManager.BattleUIManager.ApplyCooltime(_type, GetCoolTime(_type));
        }
    }

    public void TryUseSkill(SkillSlot _type, Vector3 _position)
    {
        // ��ų �ߵ��� �����ߴٸ�
        if (IsSkillUsable(_type))
        {
            playerManager.transform.LookAt(_position);

            UseSkill(_type);

            // UI�� ��Ÿ���� �����Ѵ�.
            if (playerManager.BattleUIManager != null)
                playerManager.BattleUIManager.ApplyCooltime(_type, GetCoolTime(_type));
        }
    }

    #endregion
}
