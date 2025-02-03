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

    public GameObject Boss {get {return boss;}}
    public GameObject BossSkin { get { return bossSkin; } }
    public GameObject[] Players {get {return players;}}

    private List<BossChain> activeChain = new List<BossChain>();
    private float maxHp;
    private float curHp;
    private bool[] hpCheck = new bool[5];
    private GameObject randomTarget;

    private void Awake()
    {
        hpCheck[0] = false;
        hpCheck[1] = false;
        hpCheck[2] = false;
        hpCheck[3] = false;
        hpCheck[4] = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            BossStun();
        }
    }

    // 공격이 들어왔을때
    private void OnTriggerEnter(Collider _playerAttack)
    {
        // 추가로 고려해야 하는 상황
        // 1. 네트워크 동기화
        // 2. 플레이어별 데미지 check를 위한 구성
        // 3. 플레이어별 어그로 check

        // 플레이어 어택의 공격력을 가져옴.

        // 해당 공격력만큼 보스피를 하락

        // 만약 체인어택 공격이라면 상태이상 리스트에 추가

    }

    // 데미지만큼 hp에서 하락시킴.
    private void TakeDamage(float _damage)
    {
        if (curHp <= 0) return;

        curHp -= _damage;

        if (curHp <= 0)
        {
            curHp = 0;
            bossDieCallback?.Invoke();
        }

        CheckHpCallback();

        // UI에 보스체력 동기화 시키는 코드 필요
    }

    // 특정 hp이하일때 마다 콜백을 던짐
    private void CheckHpCallback()
    {
        float hp = (curHp / maxHp) * 100f;

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
}
