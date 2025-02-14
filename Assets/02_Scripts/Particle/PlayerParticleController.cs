using System.Collections.Generic;
using UnityEngine;

using System;

public class PlayerParticleController : MonoBehaviour
{
    [Serializable]
    private class PlayerParticleData
    {
        public string particleName;
        public GameObject particlePrefab;
        public bool forceDestroyable;
    }

    [SerializeField]
    private List<PlayerParticleData> particleDataList = null;

    private List<GameObject> forceDestroyParticles = null;
    private PlayerManager playerManager = null;

    /// <summary>
    /// �÷��̾��� ��ġ�� ��ƼŬ�� ������.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToPlayer(string _particleName)
    {
        var particleData = GetParticleData(_particleName);

        if(particleData == null)
        {
            Debug.LogWarningFormat("{0} name particle is not exist in list!");
            return;
        }

        SpawnParticle(particleData, transform.position, transform.rotation);
    }

    /// <summary>
    /// �÷��̾ ���������� ������ ��ų ��ġ�� ��ƼŬ�� ������.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToSkillUsedPoint(string _particleName)
    {
        var particleData = GetParticleData(_particleName);

        if (particleData == null)
        {
            Debug.LogWarningFormat("{0} name particle is not exist in list!");
            return;
        }

        SpawnParticle(particleData, playerManager.InputManager.lastSkillUsePoint, transform.rotation);
    }

    /// <summary>
    /// �÷��̾��� ���� ���� ��ġ�� ��ƼŬ�� ������.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToMeleeWeapon(string _particleName)
    {
        var particleData = GetParticleData(_particleName);

        if (particleData == null)
        {
            Debug.LogWarningFormat("{0} name particle is not exist in list!");
            return;
        }

        SpawnParticle(
            particleData,
            playerManager.AttackManager.GetMeleeWeaponPostion(),
            transform.rotation);
    }

    /// <summary>
    /// �÷��̾��� ���Ÿ� ���� ���� ��ġ�� ��ƼŬ�� ������.
    /// </summary>
    /// <param name="_particleName"></param>
    public void SpawnParticleToRangeWeapon(string _particleName)
    {
        var particleData = GetParticleData(_particleName);

        if (particleData == null)
        {
            Debug.LogWarningFormat("{0} name particle is not exist in list!", _particleName);
            return;
        }

        SpawnParticle(particleData,
            playerManager.AttackManager.RangeAttackTransform.position,
            playerManager.transform.rotation);
    } 

    public void ForceDestroyParticles()
    {
        foreach(var particle in forceDestroyParticles)
        {
            Destroy(particle);
        }

        forceDestroyParticles.Clear();
    }

    private void SpawnParticle(PlayerParticleData _particleData, Vector3 _position, Quaternion _rotation)
    {
        if (_particleData == null)
        {
            Debug.LogWarning("Particle Data is null!");
            return;
        }

        var spawnedParticleObj = Instantiate
            (
            _particleData.particlePrefab,
            _position,
            _rotation
            );

        if(_particleData.forceDestroyable)
            forceDestroyParticles.Add(spawnedParticleObj);
    }

    private PlayerParticleData GetParticleData(string _particleName)
    {
        foreach(var particleData in particleDataList)
        {
            if (particleData.particleName == _particleName)
            {
                return particleData;
            }
        }

        return null;
    }

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();

        forceDestroyParticles = new List<GameObject>();
    }
}
