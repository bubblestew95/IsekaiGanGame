using TMPro;
using UnityEngine;

public class SetBossHp : MonoBehaviour
{
    public TMP_InputField inputField;
    private int bossHP;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        inputField.onValueChanged.AddListener(ValidateInput);
    }

    private void ValidateInput(string input)
    {
        if (int.TryParse(input, out int result))
        {
            bossHP = result;
        }
        else
        {
            inputField.text = bossHP.ToString(); // ��ȿ���� ������ ���� �� ����
        }
    }
    public int GetBossHP()
    {
        return bossHP;
    }
}