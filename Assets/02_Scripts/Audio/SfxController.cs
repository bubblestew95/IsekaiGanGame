using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SfxController : MonoBehaviour
{
    public List<AudioSource> audioList = null;
    private void Awake()
    {
        audioList = GetComponentsInChildren<AudioSource>().ToList(); // ��ü ����� ����Ʈ
    }
    public void PlayByNum(int _num, float _delayTime)
    {
        if (_delayTime < 0)
        {
            Debug.LogWarning("Delay time cannot be negative. Using 0 instead.");
            _delayTime = 0f; // ������ ��� 0���� ����
        }
        if (audioList == null)
        {
            Debug.Log("No Sfx exists");
            return;
        }

        // ������ �� ����� ����� ���� �ڷ�ƾ ����
        StartCoroutine(PlayWithDelay(_num, _delayTime));
    }

    // ������ ������ �� ������� ����ϴ� �ڷ�ƾ
    private IEnumerator<WaitForSeconds> PlayWithDelay(int _num, float _delayTime)
    {
        yield return new WaitForSeconds(_delayTime); // _delayTime �� ���� ���
        audioList[_num].Play(); // ����� ���
    }
}

