using UnityEngine;

/// <summary>
/// �÷��̾� ĳ���͸� �����ϴ� �Ŵ���.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    /// <summary>
    /// ���� ���̽�ƽ ������Ʈ ����.
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
    /// ���̽�ƽ �Է��� �ް� �������� ó���Ѵ�.
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
