using System.Collections.Generic;
using EnumTypes;
using StructTypes;
using TMPro;
using UnityEngine;

public class PlayerAiManager : MonoBehaviour
{//플레이어AI 총괄 스크립트
    public bool isPlayerAiMode = true; //현재 AI 모드인지 아닌지 확인하는 bool
    public PlayerManager playerManager; // 플레이어를 조종하기위해 PlayerManager 클래스 참조
    private List<Vector3> rocksPos = new List<Vector3>(); // 돌들의 위치를 저장하는 리스트
    public Vector3 mapCenterPos; // 맵의 중앙 위치 
    public float rockSize = 2.0f; // 돌의 크기 (예시로 설정, 실제 돌 크기에 맞게 조정)
    WaitForSeconds wait = new WaitForSeconds(0.5f);
    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }
    #region 위치파악
    private void UpdateRockPositions()// 돌위치 파악
    {
        rocksPos.Clear(); // 기존 리스트를 초기화
        // 현재 "Rock" 태그를 가진 모든 오브젝트의 위치를 리스트로 받기
        GameObject[] rocks = GameObject.FindGameObjectsWithTag("Rock");

        foreach (GameObject rock in rocks)
        {
            rocksPos.Add(rock.transform.position); // 돌의 위치 추가
        }
    } 
    #endregion
    #region 이동
    #region 기본 이동
    private void AiMove(float _x, float _z) // 플레이어 매니저에 조이스틱 입력이랑 같은 입력을 줘서 움직이는 함수
    {
        JoystickInputData data = new JoystickInputData();
        if (_x>1 || _x<-1 || _z > 1 || _z < -1)
        {
            Debug.Log("Ai move input is out of range, Range is (-1 ~ 1) (X:" + _x + ",Z:" + _z + ")");
        }
        data.x = _x;
        data.z = _z;
        playerManager.MovementManager.MoveByJoystick(data);
    }
    #endregion
    #region 이동 심화
    private void HideBehindRock() // 돌 뒤로 숨는 함수 ( 사유 AI 생존 관련 전멸기 회피 
    {
        if (rocksPos.Count == 0) return; // 돌이 없으면 함수 종료

        Vector3 playerPos = transform.position;//플레이어 위치 받기 playerPos
        //맵 중앙 위치 받기 mapCenterPos        
        Vector3 closestRockPos = Vector3.zero; // 가장 가까운 돌 위치를 넣을 곳 
        float closestRockDistance = Mathf.Infinity; // 가장 가까운 돌과의 거리 초기화
        foreach (Vector3 rockPos in rocksPos)// 가장 가까운 돌 찾기
        {
            float distance = Vector3.Distance(playerPos, rockPos);
            if (distance < closestRockDistance)
            {
                closestRockDistance = distance;
                closestRockPos = rockPos;
            }
        }
        // 가장 가까운 돌을 기준으로 목표 위치 계산
        Vector3 directionFromMapCenter = (closestRockPos - mapCenterPos).normalized; // 맵 중심에서 돌 위치 방향
        Vector3 targetPosition = closestRockPos + directionFromMapCenter * rockSize; // 목표 위치는 돌 뒤로


        // 목표 위치를 계산한 후, 목표로 향할 방향 (AI 이동)
        Vector3 directionToTarget = targetPosition - playerPos;
        float x = directionToTarget.x;
        float z = directionToTarget.z;
        AiMove(x,z);
    }
    private void RunawayFromBossAttackRange() // 보스 공격 범위부터 도망치는 함수 ( 사유 AI 생존 관련  
    {
        Vector3 playerPos = transform.position;//플레이어 위치 받기 playerPos
        //보스 공격 지점 받기 attackCenterPos
        //보스 공격 범위 중앙 = 보스 공격 지점
        //if 보스 공격 범위가 부채꼴이라면 
        //  보스 공격 범위 중앙 = 보스 공격 지점 + (보스가 바라보고 있는 방향.노말라이즈() * 반지름의 1/2)
        //플레이어 위치
        //코루틴 이동 범위 밖으로 가는
    }
    private void GoToBossForAttack() // 공격하기 위해 보스에게 가까이 가는 함수 ( 사유 공격 
    {
        Vector3 bossPos = playerManager.InputManager.lastSkillUsePoint; // 보스 위치
        Vector3 playerPos = transform.position;//플레이어 위치 받기 playerPos
        // 목표 위치를 계산한 후, 목표로 향할 방향 (AI 이동)
        Vector3 directionToTarget = bossPos - playerPos;
        float x = directionToTarget.x;
        float z = directionToTarget.z;
        AiMove(x, z);
    }
    #endregion
    #endregion
    #region 공격
    private void AiUseSkillWithAim(SkillSlot _type) // 스킬 데이터를 받아 사거리내에 보스가 있다면 보스 위치에 스킬쓰고 아니면 보스에게 가는 함수
    {
        Vector3 bossPos = playerManager.InputManager.lastSkillUsePoint; // 보스 위치
        Vector3 playerPos = transform.position;//플레이어 위치 받기 playerPos
        float bossPlayerDistance = Vector3.Distance(playerPos, bossPos); // 보스랑 플레이어 위치 계산

        RangeSkill rangeSkill = playerManager.SkillManager.GetSkillData(_type) as RangeSkill; // 스킬 데이터를 받아옴
        if(rangeSkill == null) // 범위 스킬이 아니면 (근접공격)
        {
            if (bossPlayerDistance < 1f) // 보스와 플레이어 사이의 거리가 1미만이라면
            {
                playerManager.SkillManager.TryUseSkill(_type, bossPos); // 보스위치에 스킬 사용
            }
            else
            {
                Debug.Log("Ai Log Boss is out of Skill range");
                GoToBossForAttack();
            }
        }
        else // 범위 스킬이면
        {
            if (bossPlayerDistance < rangeSkill.attackRange) // 스킬 사정거리가 보스와 플레이어 사이 거리보다 작으면
            {
                playerManager.SkillManager.TryUseSkill(_type, bossPos); // 보스위치에 스킬 사용
            }
            else
            {
                Debug.Log("Ai Log Boss is out of Skill range");
                GoToBossForAttack();
            }
        }
    }
    #endregion

    private void Update()
    {
        AiUseSkillWithAim(SkillSlot.Skill_A);
        AiUseSkillWithAim(SkillSlot.Skill_B);
        AiUseSkillWithAim(SkillSlot.Skill_C);
        AiUseSkillWithAim(SkillSlot.BasicAttack);
    }
}
