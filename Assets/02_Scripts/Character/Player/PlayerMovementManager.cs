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
    /// ���� �̵����̶�� �����.
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
    /// ������ ��ġ�� �̵��ϴ� �ڷ�ƾ.
    /// </summary>
    /// <param name="_destination"></param>
    /// <returns></returns>
    private IEnumerator MoveCoroutine(Vector3 _destination)
    {
        // �޸��� �ִϸ��̼� ��� ����
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

        // �޸��� �ִϸ��̼� ��� ����
        playerManager.AnimationManager.SetAnimatorWalkSpeed(0f);
    }
}
