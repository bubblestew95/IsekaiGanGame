using System.Collections.Generic;
using UnityEngine;

using StructTypes;

public class PlayerParticleController : MonoBehaviour
{
    [SerializeField]
    private List<PlayerParticleData> particleDataList = null;

    private PlayerManager playerManager = null;

    /// <summary>
    /// �÷��̾��� ��ġ�� ��ƼŬ�� ������.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToPlayer(string _particleName)
    {
        var particlePrefab = GetParticleObject(_particleName);

        if(particlePrefab == null)
        {
            Debug.LogWarningFormat("{0} name particle is not exist in list!");
            return;
        }

        SpawnParticle(particlePrefab, transform.position, transform.rotation);
    }

    /// <summary>
    /// �÷��̾ ���������� ������ ��ų ��ġ�� ��ƼŬ�� ������.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToSkillUsedPoint(string _particleName)
    {
        var particlePrefab = GetParticleObject(_particleName);

        if (particlePrefab == null)
        {
            Debug.LogWarningFormat("{0} name particle is not exist in list!");
            return;
        }

        SpawnParticle(particlePrefab, playerManager.InputManager.lastSkillUsePoint, transform.rotation);
    }

    /// <summary>
    /// �÷��̾��� ���� ���� ��ġ�� ��ƼŬ�� ������.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToMeleeWeapon(string _particleName)
    {
        var particlePrefab = GetParticleObject(_particleName);

        if (particlePrefab == null)
        {
            Debug.LogWarningFormat("{0} name particle is not exist in list!");
            return;
        }

        SpawnParticle(
            particlePrefab,
            playerManager.AttackManager.GetMeleeWeaponPostion(),
            transform.rotation);
    }

    /// <summary>
    /// �÷��̾��� ���Ÿ� ���� ���� ��ġ�� ��ƼŬ�� ������.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToRangeWeapon(string _particleName)
    {
        var particlePrefab = GetParticleObject(_particleName);

        if (particlePrefab == null)
        {
            Debug.LogWarningFormat("{0} name particle is not exist in list!");
            return;
        }

        SpawnParticle(particlePrefab,
            playerManager.AttackManager.RangeAttackTransform.position,
            playerManager.transform.rotation);
    }

    private void SpawnParticle(GameObject _prefab, Vector3 _position, Quaternion _rotation)
    {
        ParticleLifetime particleLifetime = Instantiate
            (
            _prefab,
            _position,
            _rotation
            ).GetComponent<ParticleLifetime>();

        particleLifetime.DestroyParticle();
    }

    private GameObject GetParticleObject(string _particleName)
    {
        foreach(var particleData in particleDataList)
        {
            if (particleData.particleName == _particleName)
            {
                return particleData.particlePrefab;
            }
        }

        return null;
    }

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }
}
