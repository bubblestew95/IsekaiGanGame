using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using EnumTypes;
using UnityEditor.VersionControl;

public class PlayerSkillManager
{
    private PlayerManager playerManager = null;

    private List<PlayerSkillBase> skillDatas = null;

    private Animator animator = null;

    /// <summary>
    /// 애니메이터의 각 트리거 아이디를 미리 캐싱해놓고, 이를 스킬 타입에 따라 저장해놓음.
    /// </summary>
    private Dictionary<SkillSlot, int> animatorIdMap = null;

    private Dictionary<SkillSlot, PlayerSkillBase> skillDataMap = null;
    private Dictionary<SkillSlot, float> currentCoolTimeMap = null;

    #region Public Functions

    /// <summary>
    /// 플레이어 스킬 매니저를 초기화한다.
    /// </summary>
    /// <param name="_skilldataList">스킬 데이터 리스트</param>
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

        // 우선 하드코딩으로 애니메이터 id를 캐싱함. 확장성 생각하면 나중에 수정할 것!
        {
            animatorIdMap.Add(SkillSlot.Skill_A, Animator.StringToHash("Skill_A"));
            animatorIdMap.Add(SkillSlot.Skill_B, Animator.StringToHash("Skill_B"));
            animatorIdMap.Add(SkillSlot.Skill_C, Animator.StringToHash("Skill_C"));
            animatorIdMap.Add(SkillSlot.BasicAttack, Animator.StringToHash("BasicAttack"));
            animatorIdMap.Add(SkillSlot.Dash, Animator.StringToHash("Dash"));
        }

    }

    /// <summary>
    /// 스킬 사용을 시도한다.
    /// </summary>
    /// <param name="_skillIdx">사용할 스킬의 스킬 리스트 상 인덱스</param>
    public void UseSkill(SkillSlot _type)
    {
        // 사용하려는 변수들의 유효성 체크.
        if (skillDatas == null || currentCoolTimeMap == null)
        {
            Debug.LogWarning("Skill List is not valid!");
            return;
        }

        // 스킬 사용
        if(animatorIdMap.TryGetValue(_type, out int animId))
        {
            animator.SetTrigger(animId);
        }

        if(_type == SkillSlot.Dash)
        {
            playerManager.ChangeState(PlayerStateType.Dash);
        }
        else
        {
            playerManager.ChangeState(PlayerStateType.Action);
        }

        // 쿨타임 적용
        currentCoolTimeMap[_type] = skillDataMap[_type].coolTime;
    }

    public float GetCoolTime(SkillSlot _type)
    {
        return currentCoolTimeMap[_type];
    }

    /// <summary>
    /// 모든 스킬들의 쿨타임을 변화량만큼 뺀다.
    /// </summary>
    /// <param name="_deltaTime">변화량</param>
    public void DecreaseCoolTimes(float _delta)
    {
        foreach (var key in currentCoolTimeMap.Keys.ToList())
        {
            if(currentCoolTimeMap[key] > 0f)
                currentCoolTimeMap[key] = Mathf.Clamp(currentCoolTimeMap[key] - _delta, 0f, 10000f);
        }
    }


    /// <summary>
    /// 현재 지정한 스킬 타입의 스킬이 사용 가능한 지 체크한다.
    /// </summary>
    /// <param name="_skillIdx">체크할 스킬의 타입</param>
    /// <returns></returns>
    public bool IsSkillUsable(SkillSlot _type)
    {
        // 쿨타임 체크
        if (currentCoolTimeMap[_type] > 0f)
        {
            Debug.LogFormat("{0} Skill is coolTime!", _type);
            return false;
        }

        return true;
    }

    public void SkillAction(SkillSlot _type, float _multiply)
    {
        skillDataMap[_type].UseSkill(playerManager, _multiply);
    }

    public PlayerSkillBase GetSkill(SkillSlot _type)
    {
        return skillDataMap[_type];
    }

    // 백어택 체크 기술검증용 레이캐스트 함수. 나중에 실사용을 위해서 잠시 남겨놓음.
    /*
    public void TestRaycast()
    {
        Debug.Log("Raycast!");
        Debug.DrawLine(transform.position, transform.forward + transform.position, Color.red, 2f);

        Ray ray = new Ray(transform.position, transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 30f))
        {
            Debug.Log(hit.transform.name);

            // 백어택 체크.
            // 캐릭터의 전방 방향으로 기술이 나가게만 설정해놨음. 애초에 백어택 체크 기술들은 전부 캐릭터 전방으로 향하고
            // 따라서 캐릭터의 forward 와 충돌체의 forward 의 각도로 백어택 여부를 체크하는 중.
            if (Vector3.Angle(transform.forward, hit.transform.forward) < 80f)
                Debug.Log("BackAttack!");
        }
    }
    */

    #endregion

    #region Private Functions

    #endregion
}
