using EnumTypes;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager
{
    private List<PlayerSkillData> skillDatas = null;

    private Animator animator = null;

    private Dictionary<SkillType, float> coolTimeMap = null;

    #region Public Functions

    /// <summary>
    /// �÷��̾� ��ų �Ŵ����� �ʱ�ȭ�Ѵ�.
    /// </summary>
    /// <param name="_skilldataList">��ų ������ ����Ʈ</param>
    public void Init(PlayerManager _mng)
    {
        skillDatas = _mng.PlayerData.skills;

        coolTimeMap = new Dictionary<SkillType, float>();

        foreach(var data in skillDatas)
        {
            coolTimeMap.Add(data.skillType, 0f);
        }
    }

    /// <summary>
    /// ��ų ����� �õ��Ѵ�.
    /// </summary>
    /// <param name="_skillIdx">����� ��ų�� ��ų ����Ʈ �� �ε���</param>
    public void TryUseSkill(SkillType _type)
    {
        if(skillDatas == null || coolTimeMap == null)
        {
            Debug.LogWarning("Skill List is not valid!");
            return;
        }

        // ��ų�� ���� ��� �������� üũ��.
        if (!IsSkillUsable(_type))
        {
            Debug.Log("Index {0} Skill is coolTime!");
            return;
        }

        Debug.Log("Use Skill!");

        animator.SetTrigger("Skill01");
    }

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
    /// ���� ������ ��ų �ε����� ��ų�� ��� ������ �� üũ�Ѵ�.
    /// </summary>
    /// <param name="_skillIdx">üũ�� ��ų�� �ε���</param>
    /// <returns></returns>
    private bool IsSkillUsable(SkillType _type)
    {
        // ��Ÿ�� üũ
        if (coolTimeMap[_type] > 0f)
        {
            Debug.Log("Index {0} Skill is coolTime!");
            return false;
        }

        return true;
    }

    #endregion
}
