using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    public BossBT bossBT;

    private void Start()
    {
        bossBT.behaviorStartCallback += PerformAttack;
    }

    private void PerformAttack(BossState _state)
    {
        switch(_state)
        {
            case BossState.Attack1:
                Attack1();
                break;
            case BossState.Attack2:
                Attack2();
                break;
            case BossState.Attack3:
                Attack3();
                break;
            case BossState.Attack4:
                Attack4();
                break;
            case BossState.Attack5:
                Attack5();
                break;
            case BossState.Attack6:
                Attack6();
                break;
            default:
                break;
        }
    }

    private void Attack1()
    {

    }

    private void Attack2()
    {

    }

    private void Attack3()
    {

    }

    private void Attack4()
    {

    }

    private void Attack5()
    {

    }

    private void Attack6()
    {

    }
}
