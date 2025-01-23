using UnityEngine;

/// <summary>
/// 플레이어 캐릭터를 관리하는 매니저.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    /// <summary>
    /// 가상 조이스틱 컴포넌트 지정.
    /// </summary>
    [SerializeField]
    private FloatingJoystick joystick = null;

    [SerializeField]
    private PlayerData playerData = null;

    private CharacterController characterCont = null;

    #region Public Functions
    #endregion

    #region Private Functions
    /// <summary>
    /// 조이스틱 입력을 받고 움직임을 처리한다.
    /// </summary>
    private void MoveByJoystick()
    {
        float x = joystick.Horizontal;
        float z = joystick.Vertical;
        float speed = playerData.speed;

        Vector3 moveVector = new Vector3(x, 0f, z) * speed * Time.deltaTime;

        characterCont.Move(moveVector);

        if (moveVector.sqrMagnitude == 0f)
            return;

        transform.rotation = Quaternion.LookRotation(moveVector);
    }

    private void UseSkill()
    {
        Debug.Log("Use Skill!");


    }
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        characterCont = GetComponent<CharacterController>();
    }

    private void Update()
    {
        MoveByJoystick();

        if (Input.GetKeyDown(KeyCode.K))
            UseSkill();
    }

    #endregion
}
