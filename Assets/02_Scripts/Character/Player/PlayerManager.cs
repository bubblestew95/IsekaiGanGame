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
    private PlayerSkillManager skillMng = null;

    public PlayerData PlayerData
    {
        get {  return playerData; }
    }

    #region Public Functions
    /// <summary>
    /// ��ų �ߵ��� �õ��Ѵ�.
    /// </summary>
    /// <param name="_skillIdx"></param>
    public void UseSkill(int _skillIdx)
    {
        skillMng.TryUseSkill(_skillIdx);
    }
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
    #endregion

    #region Unity Callback
    private void Awake()
    {
        characterCont = GetComponent<CharacterController>();

        skillMng = GetComponent<PlayerSkillManager>();

        skillMng.Init();
    }

    private void Update()
    {
        MoveByJoystick();

        if(Input.GetKeyDown(KeyCode.L))
        {
            UseSkill(0);
        }
    }

    #endregion
}
