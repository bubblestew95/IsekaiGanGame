using UnityEngine;

public class BgmExciter : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioLowPassFilter lowPassFilter;
    [SerializeField, Range(0f, 1f)]
    public float excitedLevel = 0; // �� ��ġ�� LowPassFilter�� Pitch ���� ����
    public float previousLevel = 0;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();
        SoundExcitedLevel(0);
    }

    public void SoundExcitedLevel(float _level)
    {
        if(_level < 0 || _level > 1)
        {
            Debug.Log("Only 0 ~ 1 Level is Allowed");
            return;
        }

        if (_level != previousLevel)
        {
            // �ʹݿ� õõ�� �ö󰡰� �Ĺݿ� �ް��� �ö󰡴� ȿ�� (���� ��ȭ)
            float adjustedLevel = Mathf.Pow(_level, 2); // ������ ����Ͽ� �������� ���
            lowPassFilter.cutoffFrequency = Mathf.Lerp(1000,22000, adjustedLevel);
            audioSource.pitch = Mathf.Lerp(0.9f, 1.15f, adjustedLevel);

            // ���� ���� ���� ������ ������Ʈ
            previousLevel = _level;
        }
    }

    private void Update()
    {
        SoundExcitedLevel(excitedLevel);
    }
}
