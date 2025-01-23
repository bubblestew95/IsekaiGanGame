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
    /// 플레이어 스킬 매니저를 초기화한다.
    /// </summary>
    /// <param name="_skilldataList">스킬 데이터 리스트</param>
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
    /// 스킬 사용을 시도한다.
    /// </summary>
    /// <param name="_skillIdx">사용할 스킬의 스킬 리스트 상 인덱스</param>
    public void TryUseSkill(SkillType _type)
    {
        if(skillDatas == null || coolTimeMap == null)
        {
            Debug.LogWarning("Skill List is not valid!");
            return;
        }

        // 스킬이 현재 사용 가능한지 체크함.
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

    //        // 백어택 체크.
    //        // 캐릭터의 전방 방향으로 기술이 나가게만 설정해놨음. 애초에 백어택 체크 기술들은 전부 캐릭터 전방으로 향하고
    //        // 따라서 캐릭터의 forward 와 충돌체의 forward 의 각도로 백어택 여부를 체크하는 중.
    //        if (Vector3.Angle(transform.forward, hit.transform.forward) < 80f)
    //            Debug.Log("BackAttack!");
    //    }
    //}

    #endregion

    #region Private Functions

    /// <summary>
    /// 현재 지정한 스킬 인덱스의 스킬이 사용 가능한 지 체크한다.
    /// </summary>
    /// <param name="_skillIdx">체크할 스킬의 인덱스</param>
    /// <returns></returns>
    private bool IsSkillUsable(SkillType _type)
    {
        // 쿨타임 체크
        if (coolTimeMap[_type] > 0f)
        {
            Debug.Log("Index {0} Skill is coolTime!");
            return false;
        }

        return true;
    }

    #endregion
}
