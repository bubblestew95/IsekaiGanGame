using StructTypes;
using UnityEngine;

public class PlayerMovementManager
{
    private PlayerManager playerManager = null;
    private CharacterController characterController = null;

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
}
