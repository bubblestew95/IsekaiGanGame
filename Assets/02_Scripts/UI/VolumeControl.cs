using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;  // AudioMixer ����� ���� ���ӽ����̽�

public class VolumeControl : MonoBehaviour
{
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    public AudioMixer audioMixer;  // �ͼ�

    void Start()
    {
        // �θ� ������Ʈ���� ��� �����̴��� �����ɴϴ�.
        Slider[] sliders = GetComponentsInChildren<Slider>(true);

        // �ε����� ����Ͽ� �����̴��� �����մϴ�.
        masterSlider = sliders[0];  // ù ��° �����̴� (������)
        bgmSlider = sliders[1];     // �� ��° �����̴� (BGM)
        sfxSlider = sliders[2];     // �� ��° �����̴� (SFX)

        // �ʱ� �����̴� �� ���� (�ִ� ����)
        masterSlider.value = 0.5f;
        bgmSlider.value = 0.5f;
        sfxSlider.value = 0.5f;

        // �����̴� ���� ����� ������ ȣ��� �޼��� ���
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    // ������ ���� ����
    public void OnMasterVolumeChanged(float value)
    {
        Debug.Log("Master Volume Changed: " + value);  // �����̴� �� Ȯ��
        if (value == 0f)
        {
            audioMixer.SetFloat("Master", -80f); // 0�� ���� -80dB�� ���� (����� �Ϳ� �鸮�� ����)
        }
        else
        {
            audioMixer.SetFloat("Master", Mathf.Log10(value) * 20); // ������ dB ������ ��ȯ
        }
    }


    // BGM ���� ����
    public void OnBGMVolumeChanged(float value)
    {
        if (value == 0f)
        {
            audioMixer.SetFloat("BGM", -80f); // 0�� ���� -80dB�� ���� (����� �Ϳ� �鸮�� ����)
        }
        else
        {
            audioMixer.SetFloat("BGM", Mathf.Log10(value) * 20); // ������ dB ������ ��ȯ
        }
    }

    // SFX ���� ����
    public void OnSFXVolumeChanged(float value)
    {
        if (value == 0f)
        {
            audioMixer.SetFloat("SFX", -80f); // SFX ������ -80dB�� ����
        }
        else
        {
            audioMixer.SetFloat("SFX", Mathf.Log10(value) * 20); // ������ dB�� ��ȯ
        }
    }


}
