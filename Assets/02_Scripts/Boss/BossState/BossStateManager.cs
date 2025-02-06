using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class BossStateManager : MonoBehaviour
{
    public delegate void BossStateDelegate();
    public delegate void BossStateDelegate2(GameObject _target);
    public BossStateDelegate bossDieCallback;
    public BossStateDelegate bossHp10Callback;
    public BossStateDelegate bossHpHalfCallback;
    public BossStateDelegate bossStunCallback;
    public BossStateDelegate2 bossRandomTargetCallback;

    [SerializeField] public GameObject aggroPlayer;
    [SerializeField] private GameObject[] players;
    [SerializeField] private GameObject boss;
    [SerializeField] public float chainTime;
    [SerializeField] private GameObject bossSkin;
    [SerializeField] private int maxHp;
    [SerializeField] private int curHp;

    public GameObject Boss {get {return boss;}}
    public GameObject BossSkin { get { return bossSkin; } }
    public GameObject[] Players {get {return players;}}
    public int MaxHp { get { return maxHp; } }
    public int CurHp { get { return curHp; } }

    private List<BossChain> activeChain = new List<BossChain>();
    private bool[] hpCheck = new bool[9];
    private GameObject randomTarget;
    private BossAttackCollider attackCollider;

    // 찾아서 넣는거
    private DamageParticle damageParticle;
    private UIBossHpsManager bossHpUI;

    private void Awake()
    {
        hpCheck[0] = false;
        hpCheck[1] = false;
        hpCheck[2] = false;
        hpCheck[3] = false;
        hpCheck[4] = false; 
        hpCheck[5] = false;
        hpCheck[6] = false;
        hpCheck[7] = false;
        hpCheck[8] = false;
        attackCollider = GetComponent<BossAttackCollider>();

        curHp = maxHp;

        damageParticle = FindFirstObjectByType<DamageParticle>();
        bossHpUI = FindFirstObjectByType<UIBossHpsManager>();
    }

    private void Start()
    {
        attackCollider.rockCollisionCallback += BossStun;

        bossHpUI.SetMaxHp(maxHp);
        bossHpUI.HpBarUIUpdate();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            BossStun();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TakeDamage(null, 10, 10);
        }
    }

    // 데미지만큼 hp에서 하락시킴.
    public void TakeDamage(PlayerManager _player, int _damage, float _aggro)
    {
        if (curHp <= 0) return;

        // 플레이어 어그로, 데미지 수치 등록
        RegisterDamageAndAggro(_player, _damage, _aggro);

        // 상태이상 추가
        // AddChainList();

        // 데미지를 입힘
        curHp -= _damage;

        if (curHp <= 0)
        {
            curHp = 0;
            bossDieCallback?.Invoke();
        }

        // 현재 hp콜백(현재 피에 따라 패턴 설정)
        CheckHpCallback();

        // 어그로 플레이어 설정하는 함수
        GetHighestAggroTarget();

        // 보스 피격 파티클 실행
        damageParticle.SetupAndPlayParticles(_damage);

        // 보스 UI설정
        bossHpUI.BossDamage(_damage);
        bossHpUI.HpBarUIUpdate();
    }

    // 특정 hp이하일때 마다 콜백을 던짐
    private void CheckHpCallback()
    {
        float hp = ((float)curHp / (float)maxHp) * 100f;

        Debug.Log("현재 hp" + hp);

        if (hp <= 90f && !hpCheck[0])
        {
            hpCheck[0] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 80f && !hpCheck[1])
        {
            hpCheck[1] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 70f && !hpCheck[2])
        {
            hpCheck[2] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 60f && !hpCheck[3])
        {
            hpCheck[3] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 50f && !hpCheck[4])
        {
            hpCheck[4] = true;
            bossHpHalfCallback?.Invoke();
        }
        else if (hp <= 40f && !hpCheck[5])
        {
            hpCheck[5] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 30f && !hpCheck[6])
        {
            hpCheck[6] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 20f && !hpCheck[7])
        {
            hpCheck[7] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
        else if (hp <= 10f && !hpCheck[8])
        {
            hpCheck[8] = true;
            bossHp10Callback?.Invoke();

            randomTarget = RandomPlayer();
            bossRandomTargetCallback?.Invoke(randomTarget);
        }
    }

    // chain 상태를 관리하는 List에 저장
    private void AddChainList(BossChain _chainType)
    {
        activeChain.Add(_chainType);
        StartCoroutine(RemoveChainList(_chainType));
    }

    // chain 특정 시간후 제거
    private IEnumerator RemoveChainList(BossChain _chainType)
    {
        yield return new WaitForSeconds(chainTime);

        if (activeChain.Contains(_chainType))
        {
            activeChain.Remove(_chainType);
        }
    }

    // Chain 상태 확인
    public bool HasChain(BossChain _chainType)
    {
        return activeChain.Contains(_chainType);
    }

    // 보스와 어그로 플레이어 사이의 거리 계산
    public float GetDisWithoutY()
    {
        Vector2 bossPos2D = new Vector2(boss.transform.position.x, boss.transform.position.z);
        Vector2 playerPos2D = new Vector2(aggroPlayer.transform.position.x, aggroPlayer.transform.position.z);

        return Vector2.Distance(bossPos2D, playerPos2D);
    }

    // 랜덤한 플레이어를 호출하는 함수
    private GameObject RandomPlayer()
    {
        int randomIndex = Random.Range(0, players.Length);

        return players[randomIndex];
    }

    //보스가 스턴걸렸을때 호출되는 함수
    public void BossStun()
    {
        bossStunCallback?.Invoke();
    }

    // 어그로 수치 초기화 하는 함수
    private void ResetAggro()
    {

    }


    // 플레이어의 데미지, 어그로 수치 관리하는 함수
    private void RegisterDamageAndAggro(PlayerManager _player, int _damage, float _aggro)
    {

    }

    // 현재 어그로 수치 기준으로 어그로 대상 판별하는 함수
    private void GetHighestAggroTarget()
    {

    }
}
