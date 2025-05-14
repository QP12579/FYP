using UnityEngine;
using UnityEngine.UI;

public class AudioPanel : MonoBehaviour
{
    [SerializeField]
    private Slider m_BGMSlider;
    [SerializeField]
    private Slider m_SFXSlider;

    public void Start()
    {
        print(SoundManager.instance.name);
        m_BGMSlider.value = SoundManager.instance.bgmVolume;
        m_SFXSlider.value = SoundManager.instance.sfxVolume;
    }
    public void OnBGMSliderValueChanged()
    {
        SoundManager.instance.SetBGMVolume(m_BGMSlider.value);
    }
    public void OnSFXSliderValueChanged()
    {
        SoundManager.instance.SetSFXVolume(m_SFXSlider.value);
    }
}
