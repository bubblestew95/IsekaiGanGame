using System.Collections;
using UnityEngine;

using StructTypes;
using TMPro;

public class PlayerMovementManager
{
    private PlayerManager playerManager = null;
    private CharacterController characterController = null;
    private Coroutine moveCoroutine = null;

    public void Init(PlayerManager _playerManager)
    {
        playerManager = _playerManager;
        characterController = playerManager.GetComponent<CharacterController>();
    }

    public void MoveByJoystick(JoystickInputData _inputData)
    {
        float speed = playerManager.PlayerData.walkSpeed;

        Vector3 moveVector = new Vector3(_inputData.x, 0f, _inputData.z) * speed * Time.deltaTime;

        characterController.Move(moveVector);

        float currentSpeed = moveVector.sqrMagnitude;

        playerManager.AnimationManager.SetAnimatorWalkSpeed(currentSpeed);

        if (currentSpeed == 0f)
            return;

        if (moveVector != Vector3.zero)
            playerManager.transform.rotation = Quaternion.LookRotation(moveVector);
    }

    public void MoveToPosition(Vector3 _destination)
    {
        StopMove();

        moveCoroutine = playerManager.StartCoroutine(MoveCoroutine(_destination));
    }

    /// <summary>
    /// 현재 이동중이라면 멈춘다.
    /// </summary>
    public void StopMove()
    {
        if(moveCoroutine != null)
        {
            playerManager.StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
    }

    /// <summary>
    /// 지정한 위치로 이동하는 코루틴.
    /// </summary>
    /// <param name="_destination"></param>
    /// <returns></returns>
    private IEnumerator MoveCoroutine(Vector3 _destination)
    {
        // 달리기 애니메이션 재생 시작
        playerManager.AnimationManager.SetAnimatorWalkSpeed(1f);

        float speed = playerManager.PlayerData.walkSpeed;
        Vector3 direction = _destination - playerManager.transform.position;
        direction.y = 0f;
        direction.Normalize();  
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Vector3.Distance(playerManager.transform.position, _destination) > 0.5f)
        {
            characterController.Move(direction * speed * Time.deltaTime);

            playerManager.transform.rotation = 
                Quaternion.Slerp(playerManager.transform.rotation, targetRotation, Time.deltaTime * 10f);

            yield return null;
        }

        // 달리기 애니메이션 재생 종료
        playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
    }
}
