using System.Collections;
using System.Collections.Generic;
using EnumTypes;
using StructTypes;
using UnityEngine;

public class PlayerAiManager : MonoBehaviour
{
    public bool isPlayerAiMode = true; // 현재 AI 모드인지 아닌지 확인하는 bool
    public bool isMoveBack = false; // 도망 상태 확인하는 bool
    public PlayerManager playerManager; // 플레이어를 조종하기위해 PlayerManager 클래스 참조
    private List<Vector3> rocksPos = new List<Vector3>(); // 돌들의 위치를 저장하는 리스트
    public Vector3 mapCenterPos; // 맵의 중앙 위치 
    public float rockSize = 2.0f; // 돌의 크기 (예시로 설정, 실제 돌 크기에 맞게 조정)
    float safeDistance = 1f;

    // SkillSlot 배열에 사용할 스킬을 나열
    SkillSlot[] skillSlots = new SkillSlot[]
    {
        SkillSlot.Skill_A,
        SkillSlot.Skill_B,
        SkillSlot.Skill_C
    };

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        BossBT.AttackStartCallback += () => StartCoroutine(MoveBackwardsBoss()); // 보스 공격 콜백 코루틴 플레이어 도망 시작
        BossBT.AttackEndCallback += () => isMoveBack = false;
    }

    #region 위치 파악

    private void UpdateRockPositions() // 돌 위치 파악
    {
        rocksPos.Clear();
        GameObject[] rocks = GameObject.FindGameObjectsWithTag("Rock");

        foreach (GameObject rock in rocks)
        {
            rocksPos.Add(rock.transform.position);
        }
    }

    #endregion

    #region 이동 처리

    private void AiMove(float _x, float _z) // 플레이어의 이동 처리 함수
    {
        if (playerManager.StateMachine.CurrentState.StateType == PlayerStateType.Idle)
        {
            JoystickInputData data = new JoystickInputData
            {
                x = Mathf.Clamp(_x, -1f, 1f),
                z = Mathf.Clamp(_z, -1f, 1f)
            };

            Vector3 direction = new Vector3(data.x, 0, data.z);
            if (direction.magnitude > 1)
            {
                direction.Normalize();
                data.x = direction.x;
                data.z = direction.z;
            }

            playerManager.MovementManager.MoveByJoystick(data);
        }
    }

    #endregion

    #region 스킬 사용 및 이동 처리

    private void UseSkillWithApproach(SkillSlot skillType)
    {
        Vector3 bossPos = GameManager.Instance.GetBossTransform().position;
        Vector3 playerPos = transform.position;
        float distanceToBoss = Vector3.Distance(playerPos, bossPos);

        RangeSkill rangeSkill = playerManager.SkillManager.GetSkillData(skillType) as RangeSkill;

        if (rangeSkill == null) // 근접 스킬
        {
            HandleMeleeSkill(skillType, distanceToBoss, bossPos);
        }
        else // 범위 스킬
        {
            HandleRangeSkill(skillType, distanceToBoss, rangeSkill.attackRange, bossPos);
        }
    }

    private void HandleMeleeSkill(SkillSlot skillType, float distanceToBoss, Vector3 bossPos)
    {
        if (distanceToBoss <= safeDistance)
        {
            UseMeleeSkill(skillType, bossPos);
        }
        else
        {
            MoveTowardsBoss(bossPos); // 근접 스킬을 사용하기 위해 보스에게 접근
        }
    }

    private void HandleRangeSkill(SkillSlot skillType, float distanceToBoss, float attackRange, Vector3 bossPos)
    {
        if (distanceToBoss <= attackRange)
        {
            UseRangeSkill(skillType, bossPos);
        }
        else
        {
            MoveTowardsBoss(bossPos); // 범위 스킬을 사용하려면 보스에게 접근
        }
    }

    private void UseMeleeSkill(SkillSlot skillType, Vector3 bossPos)
    {
        playerManager.InputManager.lastSkillUsePoint = new Vector3(bossPos.x, 0, bossPos.z);
        playerManager.SkillManager.TryUseSkill(skillType, bossPos);
        Debug.Log("근접스킬");
    }

    private void UseRangeSkill(SkillSlot skillType, Vector3 bossPos)
    {
        playerManager.InputManager.lastSkillUsePoint = new Vector3(bossPos.x, 0, bossPos.z);
        playerManager.SkillManager.TryUseSkill(skillType, bossPos);
        Debug.Log("범위스킬");
    }

    private void MoveTowardsBoss(Vector3 bossPos)
    {
        Vector3 directionToTarget = bossPos - transform.position;
        AiMove(directionToTarget.x, directionToTarget.z);
    }    
    private IEnumerator MoveBackwardsBoss()
    {
        isMoveBack = true;
        while (isMoveBack == true)
        {
            Vector3 bossPos = GameManager.Instance.GetBossTransform().position;
            Vector3 directionToTarget = transform.position - bossPos;
            AiMove(directionToTarget.x, directionToTarget.z);
            yield return null;
        }
        yield break;
    }

    #endregion

    #region AI 주기적인 행동

    private void Update()
    {
        if (isMoveBack == false)// 도망중이 아니라면
        {
            // 랜덤으로 하나의 스킬 선택
            int randomIndex = Random.Range(0, skillSlots.Length);
            // 선택된 스킬로 스킬을 사용하고, 적절히 이동
            UseSkillWithApproach(skillSlots[randomIndex]);
        }
    }

    #endregion
}
