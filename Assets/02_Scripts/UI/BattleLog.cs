using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using StructTypes;
using EnumTypes;

public class BattleLog : MonoBehaviour
{
    [Header("UI")]
    /// <summary>
    /// �ൿ �̹���
    /// </summary>
    [SerializeField]
    private LogSlot image_Behaviour = null;
    /// <summary>
    /// ��ü �̹���
    /// </summary>
    [SerializeField]
    private LogSlot image_Who = null;
    /// <summary>
    /// ��� �̹���
    /// </summary>
    [SerializeField]
    private LogSlot image_Whom = null;
    /// <summary>
    /// �α� ��� ���ӽð�
    /// </summary>
    [SerializeField]
    private float showLogDuration = 2f;

    [Header("Data")]
    [SerializeField]
    private List<BossImageData> bossImageDataList = null;
    [SerializeField]
    private List<PlayerImageData> playerImageDataList = null;
    [SerializeField]
    private List<BehaviourImageData> behaviourImageDataList = null;

    private Dictionary<BossType, Sprite> bossImageDic = null;
    private Dictionary<CharacterClass, Sprite> CharacterImageDic = null;
    private Dictionary<BehaviourLogType, Sprite> behaviourImageDic = null;

    /// <summary>
    /// ��� �α׸� ����ϴ� �Լ�
    /// </summary>
    public void SetKillLog(BossType _boss, CharacterClass _player, bool _isPlayerDied = true)
    {
        if(_isPlayerDied)
        {
            image_Who.SetImage(bossImageDic[_boss]);
            image_Whom.SetImage(CharacterImageDic[_player]);
        }
        else
        {
            image_Who.SetImage(CharacterImageDic[_player]);
            image_Whom.SetImage(bossImageDic[_boss]);
        }

        image_Behaviour.SetImage(behaviourImageDic[BehaviourLogType.Kill]);
    }

    /// <summary>
    /// ��Ȱ �α׸� ����ϴ� �Լ�
    /// </summary>
    /// <param name="_revive"></param>
    /// <param name="_getRevived"></param>
    public void SetReviveLog(CharacterClass _revive, CharacterClass _getRevived)
    {
        image_Who.SetImage(CharacterImageDic[_revive]);
        image_Whom.SetImage(CharacterImageDic[_getRevived]);
        image_Behaviour.SetImage(behaviourImageDic[BehaviourLogType.Revive]);
    }

    /// <summary>
    /// �α׸� ����ϴ� �Լ�
    /// </summary>
    public void ShowLog()
    {
        // �α׸� ����Ѵ�.
        {
            image_Behaviour.gameObject.SetActive(true);
            image_Who.gameObject.SetActive(true);
            image_Whom.gameObject.SetActive(true);
        }

        // ���� �ð� �� �α׸� �ٽ� �����Ѵ�.
        StartCoroutine(HideLogCoroutine(showLogDuration));
    }

    private IEnumerator HideLogCoroutine(float _duration)
    {
        yield return new WaitForSeconds(_duration);

        // �α׸� �ٽ� �����.
        {
            image_Behaviour.gameObject.SetActive(false);
            image_Who.gameObject.SetActive(false);
            image_Whom.gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        bossImageDic = new Dictionary<BossType, Sprite>();
        CharacterImageDic = new Dictionary<CharacterClass, Sprite>();
        behaviourImageDic = new Dictionary<BehaviourLogType, Sprite>();

        foreach (var data in bossImageDataList)
        {
            bossImageDic.Add(data.bossType, data.bossImage);
        }
        foreach (var data in playerImageDataList)
        {
            CharacterImageDic.Add(data.characterClass, data.playerImage);
        }
        foreach (var data in behaviourImageDataList)
        {
            behaviourImageDic.Add(data.behaviourType, data.behaviourImage);
        }
    }
}
