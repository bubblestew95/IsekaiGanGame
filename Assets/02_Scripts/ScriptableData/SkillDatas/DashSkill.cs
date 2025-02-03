using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DashSkill", menuName = "Scriptable Objects/Player Skill/Dash")]
public class DashSkill : PlayerSkillBase
{
    public float dashDistance = 5f;

    public float dashTime = 1f;

    public override void UseSkill(PlayerManager _player, int _order)
    {
        _player.StartCoroutine(DashCoroutine(_player));
    }

    private IEnumerator DashCoroutine(PlayerManager _player)
    {
        float currentTime = 0f;
        CharacterController characterCont = _player.GetComponent<CharacterController>();
        while (currentTime <= dashTime)
        {
            // characterCont.Move()
            yield return null;
        }
    }
}
