using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.Netcode;


public class BossStateManager : NetworkBehaviour
{
    // 델리게이트
    public delegate void BossStateDelegate();
    public delegate void BossStateDelegate2(ulong _index);
    public BossStateDelegate bossDieCallback;
    public BossStateDelegate bossHp10Callback;
    public BossStateDelegate bossHpHalfCallback;
    public BossStateDelegate bossStunCallback;
    public BossStateDelegate bossWallTriggerCallback;
    public BossStateDelegate2 bossRandomTargetCallback;

    // 프로퍼티
    public GameObject Boss { get { return boss; } }
    public GameObject AggroPlayer { get { return aggroPlayer; } }
    public GameObject BossSkin { get { return bossSkin; } }
    public GameObject[] Players { get { return players; } }
    public BoxCollider HitCollider { get { return hitCollider; } }

    // 보스 상태 관련 변수들
    public List<BossChain> activeChain = new List<BossChain>();
    public bool[] hpCheck = new bool[9];
    public GameObject randomTarget;
    public float bestAggro = 0f;
    public bool isPhase2 = false;
    public int maxHp = 100;
    public float chainTime = 0f;
    public GameObject[] players;
    public GameObject aggroPlayer;

    // 네트워크로 동기화 할것들
    public NetworkVariable<int> aggroPlayerIndex = new NetworkVariable<int>(-1);
    public NetworkVariable<int> curHp = new NetworkVariable<int>(-1);
    public NetworkList<float> playerDamage = new NetworkList<float>();
    public NetworkList<float> playerAggro = new NetworkList<float>();

    // 가져와서 넣는거
    public DamageParticle damageParticle;
    public UIBossHpsManager bossHpUI;
    public BgmController bgmController;
    public BossAttackCollider attackCollider;
    public BossBT bossBT;
    public GameObject boss;
    public GameObject bossSkin;
    public BoxCollider hitCollider;

    private void Awake()
    {
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += InitMulti;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
        {
            bossWallTriggerCallback?.Invoke();
            Debug.Log("벽이랑 부딛침");
        }
    }


    // 서버에서만 데미지 받는 함수 실행
    [ServerRpc(RequireOwnership = false)]
    public void BossDamageReceiveServerRpc(ulong _clientId, int _damage, float _aggro)
    {
        Debug.Log("데미지 받음 실행됨");

        if (curHp.Value <= 0) return;

        // 데미지, 어그로 세팅(공유되는 변수에 저장)
        RegisterDamageAndAggro(_clientId, _damage, _aggro);

        // 보스에 상태이상 추가(공유되는 변수에 저장)
        // AddChainList();

        // 데미지를 입힘(공유되는 변수에 저장)
        TakeDamage(_damage);

        // 클라이언트 모두 어그로 플레이어 설정하는 함수
        GetHighestAggroTarget();

        // 클라이언트 모두 보스 피격 파티클 실행
        DamageParticleClientRpc(_damage);

        // 클라이언트 모두 보스 UI설정
        UpdateBossUIClientRpc(_damage);

        // 클라이언트 모두 보스 브금 설정
        ChangeExcitedLevelClientRpc();

        // 서버만 hp콜백(현재 피에 따라 패턴 설정)
        CheckHpCallback();
    }

    #region [ClientRPC]
    // aggroPlayer 변경
    [ClientRpc]
    private void SetAggroPlayerClientRpc(int _num)
    {
        Debug.LogWarning("SetAggroPlayerClientRpc 실행됨");

        Debug.LogWarning("SetAggroPlayerClientRpc의 num : " + _num);

        aggroPlayer = players[_num];
    }

    // 데미지 파티클 실행
    [ClientRpc]
    private void DamageParticleClientRpc(float _damage)
    {
        damageParticle.SetupAndPlayParticles(_damage);
    }

    // 보스 UI 업데이트
    [ClientRpc]
    private void UpdateBossUIClientRpc(int _damage)
    {
        bossHpUI.BossDamage(_damage);
        bossHpUI.HpBarUIUpdate();
    }

    // ExcitedLevel 변경
    [ClientRpc]
    private void ChangeExcitedLevelClientRpc()
    {
        bgmController.ExcitedLevel(ChangeHpToExciteLevel());
    }

    [ClientRpc]
    private void RandomPlayerClientRpc(ulong _num)
    {
        bossRandomTargetCallback?.Invoke(_num);
    }
    #endregion

    #region [BossState]
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

    //보스가 스턴걸렸을때 호출되는 함수
    public void BossStun()
    {
        bossStunCallback?.Invoke();
    }

    // 특정 hp이하일때 마다 콜백을 던짐
    private void CheckHpCallback()
    {
        float hp = ((float)curHp.Value / (float)maxHp) * 100f;

        Debug.Log("현재 hp" + hp);

        if (hp <= 90f && !hpCheck[0])
        {
            hpCheck[0] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 80f && !hpCheck[1])
        {
            hpCheck[1] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 70f && !hpCheck[2])
        {
            hpCheck[2] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 60f && !hpCheck[3])
        {
            hpCheck[3] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
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

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 30f && !hpCheck[6])
        {
            hpCheck[6] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 20f && !hpCheck[7])
        {
            hpCheck[7] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
        else if (hp <= 10f && !hpCheck[8])
        {
            hpCheck[8] = true;
            bossHp10Callback?.Invoke();

            RandomPlayerClientRpc(RandomPlayer());
        }
    }
    #endregion

    #region [Damage && Aggro]
    // 어그로 수치 초기화 하는 함수
    private void ResetAggro()
    {
        for (int i = 0; i < playerAggro.Count; i++)
        {
            playerAggro[i] = 0f;
        }
    }

    // 플레이어의 데미지, 어그로 수치 관리하는 함수
    private void RegisterDamageAndAggro(ulong _clientId, int _damage, float _aggro)
    {
        if (_clientId == 100) return;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null) continue;

            if (players[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
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
            int len = 0;

            foreach (GameObject player in players)
            {
                if (player != null) len++;
            }

            int num = UnityEngine.Random.Range(0, len);
            playerAggro[num] = 10f;
            aggroPlayerIndex.Value = num;
            SetAggroPlayerClientRpc(aggroPlayerIndex.Value);
            return;
        }

        // 기존의 어그로 왕보다 어그로가 1.2배 크면 어그로 바뀜
        for (int i = 0; i < players.Length; i++)
        {
            if (bestAggro * 1.2f <= playerAggro[i])
            {
                bestAggro = playerAggro[i];
                aggroPlayerIndex.Value = i;
            }
        }

        Debug.Log("서버에서 aggroPlayerIndex.Value :" + aggroPlayerIndex.Value);

        // 어그로 플레이어 변경(클라 모두)
        SetAggroPlayerClientRpc(aggroPlayerIndex.Value);
    }

    // 데미지 받는 함수
    private void TakeDamage(int _damage)
    {
        curHp.Value -= _damage;

        if (curHp.Value <= 0)
        {
            curHp.Value = 0;
            bossDieCallback?.Invoke();
        }
    }
    #endregion

    #region [BGM]
    // hp 1페이즈일때 0~1, 2페이즈 일때 0~1
    private float ChangeHpToExciteLevel()
    {
        float hp = ((float)curHp.Value / (float)maxHp);

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
    #endregion

    #region [Init]

    // 초기화 해야할것들
    private void Init()
    {
        for (int i = 0; i < hpCheck.Length; i++)
        {
            hpCheck[i] = false;
        }
        bestAggro = 0f;

        // 참조 가져오기
        boss = transform.gameObject;
        bossSkin = boss.transform.GetChild(0).gameObject;
        hitCollider = boss.transform.GetChild(1).GetComponent<BoxCollider>();
        attackCollider = GetComponent<BossAttackCollider>();
        damageParticle = FindFirstObjectByType<DamageParticle>();
        bossHpUI = FindFirstObjectByType<UIBossHpsManager>();
        bgmController = FindFirstObjectByType<BgmController>();
        bossBT = FindAnyObjectByType<BossBT>();

        // ui초기 설정
        bossHpUI.SetMaxHp(maxHp);
        bossHpUI.HpBarUIUpdate();
    }

    // 멀티에서 초기화 해야할것들
    private void InitMulti()
    {
        if (IsServer)
        {
            for (int i = 0; i < 4; i++)
            {
                playerDamage.Add(0f);
                playerAggro.Add(0f);
            }

            curHp.Value = maxHp;
            aggroPlayerIndex.Value = 0;
        }

        Init();

        SetCallback();

        SetPlayerMulti();

        bossBT.curState = BossState.Chase;
    }

    // 콜백 설정
    private void SetCallback()
    {
        attackCollider.rockCollisionCallback += BossStun;
        bossBT.phase2BehaviorStartCallback += ChangePhase2BGM;
    }

    // 멀티 플레이어 설정
    private void SetPlayerMulti()
    {
        players = FindFirstObjectByType<NetworkGameManager>().Players;

        // 초반 aggro 0이여서 세팅하는 함수
        GetHighestAggroTarget();
    }
    #endregion

    #region [Etc.]
    // 보스와 어그로 플레이어 사이의 거리 계산
    public float GetDisWithoutY()
    {
        Vector2 bossPos2D = new Vector2(boss.transform.position.x, boss.transform.position.z);
        Vector2 playerPos2D = new Vector2(players[aggroPlayerIndex.Value].transform.position.x, players[aggroPlayerIndex.Value].transform.position.z);

        return Vector2.Distance(bossPos2D, playerPos2D);
    }

    // 랜덤한 플레이어를 호출하는 함수
    public ulong RandomPlayer()
    {
        List<ulong> numList = new List<ulong>();

        for (int i = 0; i < 4; ++i)
        {
            if (Players[i] == null) continue;

            numList.Add(Players[i].GetComponent<NetworkObject>().OwnerClientId);
        }

        ulong randomNum = numList[Random.Range(0, numList.Count)];

        return randomNum;
    }
    #endregion


}