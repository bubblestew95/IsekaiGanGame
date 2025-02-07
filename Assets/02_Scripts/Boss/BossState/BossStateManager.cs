using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class BossStateManager : NetworkBehaviour
{
    public delegate void BossStateDelegate();
    public delegate void BossStateDelegate2(GameObject _target);
    public BossStateDelegate bossDieCallback;
    public BossStateDelegate bossHp10Callback;
    public BossStateDelegate bossHpHalfCallback;
    public BossStateDelegate bossStunCallback;
    public BossStateDelegate2 bossRandomTargetCallback;

    [SerializeField] private GameObject aggroPlayer;
    [SerializeField] private GameObject[] players;
    [SerializeField] private GameObject boss;
    [SerializeField] public float chainTime;
    [SerializeField] private GameObject bossSkin;
    [SerializeField] private BoxCollider hitCollider;
    [SerializeField] private int maxHp;
    [SerializeField] private int curHp;

    public GameObject Boss {get {return boss;}}
    public GameObject AggroPlayer { get { return aggroPlayer; } }
    public GameObject BossSkin { get { return bossSkin; } }
    public GameObject[] Players {get {return players;}}
    public int MaxHp { get { return maxHp; } }
    public int CurHp { get { return curHp; } }
    public BoxCollider HitCollider { get { return hitCollider; } }


    private List<BossChain> activeChain = new List<BossChain>();
    private bool[] hpCheck = new bool[9];
    private GameObject randomTarget;
    private float[] playerDamage = new float[4];
    private float[] playerAggro = new float[4];
    private float bestAggro;
    private bool isPhase2 = false;

    // 찾아서 넣는거
    private DamageParticle damageParticle;
    private UIBossHpsManager bossHpUI;
    private BgmController bgmController;
    private BossAttackCollider attackCollider;
    private BossBT bossBT;

    private void Awake()
    {
        for (int i = 0; i < hpCheck.Length; i++)
        {
            hpCheck[i] = false;
        }

        for (int i = 0; i < 4; i++)
        {
            playerDamage[i] = 0;
            playerAggro[i] = 0;
        }

        bestAggro = 0f;

        attackCollider = GetComponent<BossAttackCollider>();

        curHp = maxHp;

        // 
        damageParticle = FindFirstObjectByType<DamageParticle>();
        bossHpUI = FindFirstObjectByType<UIBossHpsManager>();
        bgmController = FindFirstObjectByType<BgmController>();
        bossBT = FindAnyObjectByType<BossBT>();
    }

    private void Start()
    {
        // 임시용 플레이어 가져오는 코루틴
        StartCoroutine(GetPlayer());

        // 공격8 스턴 콜백
        attackCollider.rockCollisionCallback += BossStun;
        bossBT.phase2BehaviorEndCallback += ChangePhase2BGM;

        // ui초기 설정
        bossHpUI.SetMaxHp(maxHp);
        bossHpUI.HpBarUIUpdate();

        // 초반 aggro 0이여서 세팅하는 함수
        GetHighestAggroTarget();
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

        // 보스 브금 설정
        bgmController.ExcitedLevel(ChangeHpToExciteLevel());
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
        for (int i = 0; i < playerAggro.Length; i++)
        {
            playerAggro[i] = 0f;
        }
    }


    // 플레이어의 데미지, 어그로 수치 관리하는 함수
    private void RegisterDamageAndAggro(PlayerManager _player, int _damage, float _aggro)
    {
        if (_player == null) return;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == _player.gameObject)
            {
                playerDamage[i] += _damage;
                playerAggro[i] += _aggro;
            }
        }
    }

    // 현재 어그로 수치 기준으로 어그로 대상 판별하는 함수
    private void GetHighestAggroTarget()
    {
        bool allAggroZero = true;

        // 전부 어그로 0일때 -> 랜덤 1명 아무나 aggro 10으로 만들고 aggroPlayer 상태로
        foreach (float aggro in playerAggro)
        {
            if (aggro != 0f)
            {
                allAggroZero = false;
            }
        }

        if (allAggroZero)
        {
            int num = Random.Range(0, players.Length);
            playerAggro[num] = 10f;
            aggroPlayer = players[num];
            return;
        }

        int aggroPlayerIndex = 0;

        // 기존의 어그로 왕보다 어그로가 1.2배 크면 어그로 바뀜
        for (int i = 0; i < players.Length;i++)
        {
            if (bestAggro * 1.2f <= playerAggro[i])
            {
                bestAggro = playerAggro[i];
                aggroPlayerIndex = i;
            }
        }

        // 어그로 플레이어 변경
        aggroPlayer = players[aggroPlayerIndex];
    }

    // hp 1페이즈일때 0~1, 2페이즈 일때 0~1
    private float ChangeHpToExciteLevel()
    {
        float hp = ((float)curHp / (float)maxHp);

        if (!isPhase2)
        {
            if (((1 - hp) * 2) >= 1f) return 1f; 

            return (1 - hp) * 2;
        }
        else
        {
            return 1 - hp * 2;
        }
    }

    // 페이즈 바뀔때 브금 바꾸고 ExcitedLevel 바꿈.
    private void ChangePhase2BGM()
    {
        bgmController.ExcitedLevel(0);
        bgmController.PlayBossRageBgm();
        isPhase2 = true;
    }

    // 플레이어 생성후 호출되는 함수
    private IEnumerator GetPlayer()
    {
        yield return new WaitForSeconds(3f);

        Debug.Log("플레이어 정보 가져옴 실행됨");

        PlayerManager[] managers = FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        int num = 0;

        foreach (PlayerManager player in managers)
        {
            players[num++] = player.gameObject;
        }

        GetHighestAggroTarget();

        bossBT.curState = BossState.Chase;
    }
}
