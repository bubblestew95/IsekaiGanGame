using UnityEngine;

/// <summary>
/// 플레이어 캐릭터를 관리하는 매니저.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private FloatingJoystick joystick = null;

    [SerializeField]
    private float speed = 1f;

    private CharacterController characterCont = null;

    private void Awake()
    {
        characterCont = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float x = joystick.Horizontal;
        float z = joystick.Vertical;

        Vector3 moveVector = new Vector3(x, 0f, z) * speed * Time.deltaTime;

        characterCont.Move(moveVector);

        if (moveVector.sqrMagnitude == 0f)
            return;

        transform.rotation = Quaternion.LookRotation(moveVector);
    }
}
