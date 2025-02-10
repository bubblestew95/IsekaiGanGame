using EnumTypes;
using StructTypes;
using UnityEngine;

public class PlayerAiManager : MonoBehaviour
{//플레이어AI 총괄 스크립트
    public bool isPlayerAiMode = true; //현재 AI 모드인지 아닌지 확인하는 bool
    public PlayerManager playerManager; // 플레이어를 조종하기위해 PlayerManager 클래스 참조


    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }   
    private void AiUseSkillWithAim(SkillSlot _type) // 스킬쓰는 함수
    {
        playerManager.InputManager.lastSkillUsePoint = Vector3.zero;
        Vector3 bossPos = playerManager.InputManager.lastSkillUsePoint;
        playerManager.SkillManager.TryUseSkill(_type, bossPos);
    }
    private void AiMove(float _x, float _z) //움직이는 함수
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
    private void HideBehindRock() // 돌 뒤로 숨는 함수 ( 사유 AI 생존 관련 전멸기 회피 
    {
        //플레이어 위치 받기 playerPos
        //맵 중앙 위치 받기 mapCenterPos
        //모든 돌 위치 list로 받기 List<rocks>
        //모든 돌 위치랑 플레이어 위치 계산후 제일 가까운 돌 위치 확정
        //플레이어 목표 위치 = 가까운 돌 위치 + (돌 위치 벡터 - 맵 중앙위치 벡터 ).노말라이즈() * 돌의 크기
        //navmeshagent로 목표 위치까지 이동 (코루틴 필요)
    }
    private void RunawayFromBossAttackRange() // 보스 공격 범위부터 도망치는 함수 ( 사유 AI 생존 관련  
    {
        //플레이어 위치 받기 playerPos
        //보스 공격 지점 받기 attackCenterPos
        //보스 공격 범위 중앙 = 보스 공격 지점
        //if 보스 공격 범위가 부채꼴이라면 
        //  보스 공격 범위 중앙 = 보스 공격 지점 + (보스가 바라보고 있는 방향.노말라이즈() * 반지름의 1/2)
        //플레이어 위치 - 
        //코루틴 이동 범위 밖으로 가는
    }
    private void GoToBossForAttack() // 공격하기위해 보스에게 가까이 가는 함수 ( 사유 공격 
    {
        //코루틴 이동 보스로 가는
        //{플레이어 위치 받기 playerPos
        //보스 위치 받기 bossPos}
    }
}
