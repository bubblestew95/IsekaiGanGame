using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using EnumTypes;
using UnityEditor.VersionControl;

public class PlayerSkillManager
{
    private PlayerManager playerManager = null;

    private List<PlayerSkillData> skillDatas = null;

    private Animator animator = null;
    /// <summary>
    /// �ִϸ������� �� Ʈ���� ���̵� �̸� ĳ���س���, �̸� ��ų Ÿ�Կ� ���� �����س���.
    /// </summary>
    private Dictionary<SkillType, int> animatorIdMap = null;

    private Dictionary<SkillType, float> currentCoolTimeMap = null;
    private Dictionary<SkillType, float> maxCoolTimeMap = null;

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

        animatorIdMap = new Dictionary<SkillType, int>();
        currentCoolTimeMap = new Dictionary<SkillType, float>();
        maxCoolTimeMap = new Dictionary<SkillType, float>();

        foreach (var data in skillDatas)
        {
            currentCoolTimeMap.Add(data.skillType, 0f);
            maxCoolTimeMap.Add(data.skillType, data.coolTime);
        }

        // �켱 �ϵ��ڵ����� �ִϸ����� id�� ĳ����. Ȯ�强 �����ϸ� ���߿� ������ ��!
        {
            animatorIdMap.Add(SkillType.Skill_A, Animator.StringToHash("Skill_A"));
            animatorIdMap.Add(SkillType.Skill_B, Animator.StringToHash("Skill_B"));
            animatorIdMap.Add(SkillType.Skill_C, Animator.StringToHash("Skill_C"));
            animatorIdMap.Add(SkillType.BasicAttack, Animator.StringToHash("BasicAttack"));
            animatorIdMap.Add(SkillType.Dash, Animator.StringToHash("Dash"));
        }

    }

    /// <summary>
    /// ��ų ����� �õ��Ѵ�.
    /// </summary>
    /// <param name="_skillIdx">����� ��ų�� ��ų ����Ʈ �� �ε���</param>
    public bool TryUseSkill(SkillType _type)
    {
        if (skillDatas == null || currentCoolTimeMap == null)
        {
            Debug.LogWarning("Skill List is not valid!");
            return false;
        }

        // ��ų�� ���� ��� �������� üũ��.
        if (!IsSkillUsable(_type))
        {
            Debug.LogFormat("Currnet Skill type {0} is not usable!", _type);
            return false;
        }

        Debug.Log("Use Skill!");

        // ��ų ���
        if(animatorIdMap.TryGetValue(_type, out int animId))
        {
            animator.SetTrigger(animId);
        }

        if(_type == SkillType.Dash)
        {
            playerManager.ChangeState(PlayerStateType.Dash);
        }
        else
        {
            playerManager.ChangeState(PlayerStateType.Action);
        }

        // ��Ÿ�� ����
        currentCoolTimeMap[_type] = maxCoolTimeMap[_type];

        return true;
    }

    public float GetCoolTime(SkillType _type)
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

    // ����� üũ ��������� ����ĳ��Ʈ �Լ�. ���߿� �ǻ���� ���ؼ� ��� ���ܳ���.
    //public void TestRaycast()
    //{
    //    Debug.Log("Raycast!");
    //    Debug.DrawLine(transform.position, transform.forward + transform.position, Color.red, 2f);

    //    Ray ray = new Ray(transform.position, transform.forward);

    //    RaycastHit hit;

    //    if (Physics.Raycast(ray, out hit, 30f))
    //    {
    //        Debug.Log(hit.transform.name);

    //        // ����� üũ.
    //        // ĳ������ ���� �������� ����� �����Ը� �����س���. ���ʿ� ����� üũ ������� ���� ĳ���� �������� ���ϰ�
    //        // ���� ĳ������ forward �� �浹ü�� forward �� ������ ����� ���θ� üũ�ϴ� ��.
    //        if (Vector3.Angle(transform.forward, hit.transform.forward) < 80f)
    //            Debug.Log("BackAttack!");
    //    }
    //}

    #endregion

    #region Private Functions

    /// <summary>
    /// ���� ������ ��ų Ÿ���� ��ų�� ��� ������ �� üũ�Ѵ�.
    /// </summary>
    /// <param name="_skillIdx">üũ�� ��ų�� Ÿ��</param>
    /// <returns></returns>
    private bool IsSkillUsable(SkillType _type)
    {
        // ��Ÿ�� üũ
        if (currentCoolTimeMap[_type] > 0f)
        {
            Debug.LogFormat("{0} Skill is coolTime!", _type);
            return false;
        }

        return true;
    }

    #endregion
}
