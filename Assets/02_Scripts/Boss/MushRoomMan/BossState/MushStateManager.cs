using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static BossStateManager;

// 보스의 상태를 관리
public class MushStateManager : NetworkBehaviour
{
    // 델리게이트
    public delegate void MushStateDelegate();
    public delegate void MushStateDelegate2(ulong _index);
    public MushStateDelegate bossDieCallback;
    public MushStateDelegate bossChangeStateCallback;
    public MushStateDelegate bossHp25Callback;
    public MushStateDelegate2 bossRandomTargetCallback;

    // 네트워크로 동기화 할것들
    public NetworkVariable<int> aggroPlayerIndex = new NetworkVariable<int>(-1);
    public NetworkVariable<int> curHp = new NetworkVariable<int>(-1);
    public NetworkVariable<float> bestAggro = new NetworkVariable<float>(-1f);
    public NetworkList<float> playerDamage = new NetworkList<float>();
    public NetworkList<float> playerAggro = new NetworkList<float>();

    // 보스에 저장되는 데이터
    public GameObject[] allPlayers;
    public GameObject[] alivePlayers;
    public GameObject aggroPlayer;
    public GameObject randomPlayer;
    public int maxHp;
    public float reduceAggro;
    public float reduceAggroTime;
    public bool[] hpCheck;

    // 참조 목록
    public MushDamageParticle damageParticle;
    public UIBossHpsManager bossHpUI;
    public BgmController bgmController;
    public MushBT mushBT;
    public GameObject boss;


    // 프로퍼티
    public GameObject Boss { get { return boss; } }
    public GameObject AggroPlayer { get { return aggroPlayer; } }
    public GameObject[] AlivePlayers { get { return alivePlayers; } }
    public GameObject RandomPlayer { get { return randomPlayer; } }

    private void Awake()
    {
        FindAnyObjectByType<NetworkGameManager>().playerDieCallback += PlayerDieReceive;
        FindAnyObjectByType<NetworkGameManager>().loadingFinishCallback += Init;
    }

    #region [Damage]

    // 서버에서만 데미지 받는 함수 실행
    [ServerRpc(RequireOwnership = false)]
    public void BossDamageReceiveServerRpc(ulong _clientId, int _damage, float _aggro)
    {
        if (curHp.Value <= 0) return;

        // 데미지, 어그로 세팅(공유되는 변수에 저장)
        RegisterDamageAndAggro(_clientId, _damage, _aggro);

        // 데미지를 입힘(공유되는 변수에 저장)
        TakeDamage(_damage);

        if (IsServer)
        {
            GetHighestAggroTarget();
        }

        // 클라이언트 모두 보스 피격 파티클 실행
        DamageParticleClientRpc(_damage, _clientId);

        // 클라이언트 모두 보스 UI설정
        UpdateBossUIClientRpc(_damage);

        // 클라이언트 모두 보스 브금 설정
        ChangeExcitedLevelClientRpc();

        // 서버만 hp콜백(현재 피에 따라 패턴 설정)
        CheckHpCallback();
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

    #region [Aggro]
    // 현재 어그로 수치 기준으로 어그로 대상 판별하는 함수
    private void GetHighestAggroTarget()
    {
        bool allAggroZero = true;

        // 전부 어그로 0일때
        foreach (float aggro in playerAggro)
        {
            if (aggro != 0f)
            {
                allAggroZero = false;
            }
        }

        // 전부 어그로 0일때 -> 랜덤 1명 아무나 aggro 10으로 만들고 aggroPlayer 상태로
        if (allAggroZero)
        {
            // 랜덤 플레이어 설정
            ulong randomIndex = RandomPlayerId();
            GameObject randomPlayer = alivePlayers.FirstOrDefault(p => p != null && p.GetComponent<NetworkObject>().OwnerClientId == randomIndex);

            for (int i = 0; i < 4; i++)
            {
                if (alivePlayers[i] == null) continue;

                if (alivePlayers[i] == randomPlayer)
                {
                    playerAggro[i] = 10f;
                    bestAggro.Value = playerAggro[i];
                    aggroPlayerIndex.Value = i;
                }
            }

            SetAggroPlayerClientRpc(aggroPlayerIndex.Value);
            return;
        }

        // 기존의 어그로 왕보다 어그로가 1.2배 크면 어그로 바뀜
        for (int i = 0; i < alivePlayers.Length; i++)
        {
            if (bestAggro.Value * 1.2f <= playerAggro[i])
            {
                bestAggro.Value = playerAggro[i];
                aggroPlayerIndex.Value = i;
            }
        }

        // 어그로 플레이어 변경(클라 모두)
        SetAggroPlayerClientRpc(aggroPlayerIndex.Value);
    }

    // 플레이어의 데미지, 어그로 수치 관리하는 함수
    private void RegisterDamageAndAggro(ulong _clientId, int _damage, float _aggro)
    {
        for (int i = 0; i < alivePlayers.Length; i++)
        {
            if (alivePlayers[i] == null) continue;

            if (alivePlayers[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
            {
                playerDamage[i] += _damage;
                playerAggro[i] += _aggro;
            }
        }
    }

    // aggroPlayer 변경
    [ClientRpc]
    private void SetAggroPlayerClientRpc(int _num)
    {
        aggroPlayer = alivePlayers[_num];
    }

    // 어그로 수치 감소시키는 코루틴
    private IEnumerator ReduceAggroCoroutine()
    {
        float elapseTime = 0f;

        while (true)
        {
            elapseTime += Time.deltaTime;

            if (elapseTime >= reduceAggroTime)
            {
                for (int i = 0; i < 4; ++i)
                {
                    playerAggro[i] -= reduceAggro;

                    if (playerAggro[i] <= 0)
                    {
                        playerAggro[i] = 0;
                    }
                }

                bestAggro.Value -= reduceAggro;

                if (bestAggro.Value <= 0) bestAggro.Value = 0;

                elapseTime = 0f;
            }
            yield return null;
        }
    }

    #endregion

    #region [Particle]

    // 데미지 파티클 실행
    [ClientRpc]
    private void DamageParticleClientRpc(float _damage, ulong _clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == _clientId)
        {
            damageParticle.SetupAndPlayParticlesMine(_damage);
        }
        else
        {
            damageParticle.SetupAndPlayParticles(_damage);
        }
    }


    #endregion

    #region [BossUI]

    // 보스 UI 업데이트
    [ClientRpc]
    private void UpdateBossUIClientRpc(int _damage)
    {
        bossHpUI.BossDamage(_damage);
        bossHpUI.HpBarUIUpdate();
    }

    #endregion

    #region [BGM]

    // ExcitedLevel 변경
    [ClientRpc]
    private void ChangeExcitedLevelClientRpc()
    {
        bgmController.ExcitedLevel(ChangeHpToExciteLevel());
    }

    // hp 리턴
    private float ChangeHpToExciteLevel()
    {
        float hp = ((float)curHp.Value / (float)maxHp);

        return hp;
    }

    #endregion

    #region [Callback]

    // 플레이어가 죽었을때 호출되는 함수
    private void PlayerDieReceive(ulong _clientId)
    {
        int playerIndex = -1;
        bool IsAggroDie = false;

        // 죽은 플레이어의 Aggro수치 리셋 && Player리스트에서 빼기
        for (int i = 0; i < 4; ++i)
        {
            if (alivePlayers[i] == null) continue;

            if (alivePlayers[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
            {
                // 만약 죽은 플레이어가 bestAggro였다면 베스트 어그로도 초기화
                if (i == aggroPlayerIndex.Value)
                {
                    bestAggro.Value = 0f;
                    IsAggroDie = true;
                }
                playerAggro[i] = 0f;
                alivePlayers[i] = null;
                playerIndex = i;

                // Aggro플레이어가 죽었다면, bestAggro 재설정
                if (IsAggroDie)
                {
                    // bestAggro 재설정
                    for (int j = 0; j < alivePlayers.Length; j++)
                    {
                        if (bestAggro.Value <= playerAggro[j])
                        {
                            bestAggro.Value = playerAggro[j];
                            aggroPlayerIndex.Value = j;
                        }
                    }
                }
            }
        }

        // alivePlayer 동기화
        SetAlivePlayerClientRpc(playerIndex);

        // 어그로 플레이어 변경(클라 모두)
        SetAggroPlayerClientRpc(aggroPlayerIndex.Value);

        // 어그로 재설정을 위한 어그로 세팅 함수 호출
        GetHighestAggroTarget();
    }

    // 플레이어 살아났을때 실행되는 함수
    private void PlayerReviveReceive(ulong _clientId)
    {
        // 죽은 플레이어를 alivePlayers배열에 추가
        for (int i = 0; i < 4; ++i)
        {
            if (allPlayers[i] == null) continue;

            if (allPlayers[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
            {
                alivePlayers[i] = allPlayers[i];
            }
        }
    }

    // 특정 hp이하일때 마다 콜백을 던짐
    private void CheckHpCallback()
    {
        float hp = ((float)curHp.Value / (float)maxHp) * 100f;

        Debug.Log("현재 hp" + hp);

        if (hp <= 75f && !hpCheck[0])
        {
            hpCheck[0] = true;
            bossHp25Callback?.Invoke();
        }
        else if (hp <= 50f && !hpCheck[1])
        {
            hpCheck[1] = true;
            bossHp25Callback?.Invoke();
        }
        else if (hp <= 25f && !hpCheck[2])
        {
            hpCheck[2] = true;
            bossHp25Callback?.Invoke();
        }
    }

    #endregion

    #region [Function]

    // 초기화
    private void Init()
    {
        // 초기 값 설정
        maxHp = 30000;
        reduceAggro = 5f;
        reduceAggroTime = 10f;
        hpCheck = new bool[3];
        hpCheck[0] = false;
        hpCheck[1] = false;
        hpCheck[2] = false;

        // 서버에서 저장할거 설정
        if (IsServer)
        {
            for (int i = 0; i < 4; i++)
            {
                playerDamage.Add(0f);
                playerAggro.Add(0f);
            }

            curHp.Value = maxHp;
            aggroPlayerIndex.Value = 0;
            bestAggro.Value = 0f;
        }

        // 참조 가져오기
        boss = transform.gameObject;
        damageParticle = FindFirstObjectByType<MushDamageParticle>();
        bossHpUI = FindFirstObjectByType<UIBossHpsManager>();
        bgmController = FindFirstObjectByType<BgmController>();
        mushBT = FindAnyObjectByType<MushBT>();

        // ui초기 설정
        bossHpUI.SetMaxHp(maxHp);
        bossHpUI.HpBarUIUpdate();

        // 플레이어 참조 설정
        allPlayers = (GameObject[])FindFirstObjectByType<NetworkGameManager>().Players.Clone();
        alivePlayers = (GameObject[])allPlayers.Clone();

        if (IsServer)
        {
            // 어그로 플레이어 설정
            GetHighestAggroTarget();
        }

        // 설정 끝났으니 보스 상태 바꾸라고 콜백
        bossChangeStateCallback?.Invoke();

        if (IsServer)
        {
            StartCoroutine(ReduceAggroCoroutine());
        }
    }

    // 랜덤한 플레이어를 호출하는 함수
    public ulong RandomPlayerId()
    {
        List<ulong> numList = new List<ulong>();
        ulong randomNum = 0;

        for (int i = 0; i < 4; ++i)
        {
            if (alivePlayers[i] == null) continue;

            numList.Add(alivePlayers[i].GetComponent<NetworkObject>().OwnerClientId);
        }

        if (numList.Count != 0)
        {
            randomNum = numList[Random.Range(0, numList.Count)];
        }
        else
        {
            randomNum = 0;
        }


        return randomNum;
    }

    // 클라 모두가 랜덤 플레이어 세팅
    public void SetRandomPlayer()
    {
        SetRandomPlayerClientRpc(RandomPlayerId());
    }

    [ClientRpc]
    private void SetRandomPlayerClientRpc(ulong _clientId)
    {
        for (int i = 0; i < 4; ++i)
        {
            if (alivePlayers[i] == null) continue;

            if (alivePlayers[i].GetComponent<NetworkObject>().OwnerClientId == _clientId)
            {
                randomPlayer = alivePlayers[i];
            }
        }
    }

    [ClientRpc]
    private void SetAlivePlayerClientRpc(int _index)
    {
        alivePlayers[_index] = null;
    }


    #endregion
}
