using System.Collections;
using UnityEngine;

public class PlayerSkillMove
{
    private float moveDistance = 2f;
    private float moveTime = 1f;

    public Coroutine StartPlayerMove(PlayerManager _player, float _dist, float _time)
    {
        moveDistance = _dist;
        moveTime = _time;
        return _player.StartCoroutine(MoveCoroutine(_player));
    }

    private IEnumerator MoveCoroutine(PlayerManager _player)
    {
        float currentTime = 0f;
        CharacterController characterCont = _player.GetComponent<CharacterController>();

        while (currentTime <= moveTime)
        {
            // characterCont.Move()
            currentTime += Time.deltaTime;
            yield return null;
        }
    }
}
