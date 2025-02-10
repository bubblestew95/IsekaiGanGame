using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossSkillManager : MonoBehaviour
{
    [SerializeField] private List<BossSkillData> skillDatas;
    [SerializeField] private List<BossSkillData> randomSkillDatas;

    private List<BossSkill> skills = new List<BossSkill>();
    private List<BossSkill> randomSkills = new List<BossSkill>();

    public List<BossSkill> Skills { get { return skills; } }
    public List<BossSkill> RandomSkills { get { return randomSkills; } }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        foreach (BossSkillData skillData in skillDatas)
        {
            skills.Add(new BossSkill(skillData));
        }

        foreach (BossSkillData skillData in randomSkillDatas)
        {
            randomSkills.Add(new BossSkill(skillData));
        }
    }

    // ��ų ��ٿ����� Ȯ���ϴ� �Լ�
    public List<BossSkill> IsSkillCooldown(List<BossSkill> _skills)
    {
        List<BossSkill> availList = new List<BossSkill>();

        foreach (BossSkill skill in _skills)
        {
            if (skill.CheckCooldown())
            {
                availList.Add(skill);
            }
        }

        return availList;
    }

    // ��ų ��Ÿ� �ȿ� �ִ��� Ȯ���ϴ� �Լ�
    public List<BossSkill> IsSkillInRange(float _range, List<BossSkill> _skills)
    {
        List<BossSkill> availList = new List<BossSkill>();

        foreach (BossSkill skill in _skills)
        {
            if (skill.CheckRange(_range))
            {
                availList.Add(skill);
            }
        }
        return availList;
    }

    // ����� �������� Check
    public List<BossSkill> CheckBackAttack(List<BossSkill> _skills, GameObject[] _players, GameObject _boss)
    {
        int cnt = 0;

        List<GameObject> playerInRange = new List<GameObject>();

        // 1. �����ȿ� 2���̻� �ִ��� check
        foreach (GameObject player in _players)
        {
            if (player == null) continue;

            if (CheckDisXZ(player.transform.position, _boss.transform.position) <= skills.FirstOrDefault(skill => skill.SkillData.SkillName == "Attack9").SkillData.AttackRange)
            {
                cnt++;
                playerInRange.Add(player);
            }
        }

        if (cnt <= 2)
        {
            _skills.RemoveAll(skill => skill.SkillData.SkillName == "Attack9");
            return _skills;
        }

        bool plus = false;
        bool minus = false;
        float value = 0f;

        // 2. �����ȿ� Player�� ���ؼ� Vector3.Dot���� ����ؼ�, +�� -�� �Ѵ� �����ϴ��� check
        foreach(GameObject player in playerInRange)
        {
            value = CheckBehindObject(_boss.transform, player.transform);

            if (value > 0f) plus = true;

            if (value < 0f) minus = true;
        }

        // �����Ѵٸ� �׳� ����
        if (plus && minus)
        {
            return _skills;
        }
        else // ���� ���ϸ� ���� ����
        {
            _skills.RemoveAll(skill => skill.SkillData.SkillName == "Attack9");
            return _skills;
        }

    }

    // �� ��ġ �Ÿ� ���
    private float CheckDisXZ(Vector3 _pos1, Vector3 _pos2)
    {
        Vector2 pos1 = new Vector2(_pos1.x, _pos1.z);
        Vector2 pos2 = new Vector2(_pos2.x, _pos2.z);

        return Vector2.Distance(pos1, pos2);
    }

    // ������ ������ ���
    private float CheckBehindObject(Transform _bossTr, Transform _playerTr)
    {
        Vector3 toPlayer = _playerTr.position - _bossTr.position;

        return Vector3.Dot(toPlayer.normalized, -_bossTr.forward);
    }
}
