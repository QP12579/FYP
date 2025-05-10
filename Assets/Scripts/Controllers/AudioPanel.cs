using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioPanel : BasePanel
{
    [SerializeField]
    private Slider m_BGMSlider;
    [SerializeField]
    private Slider m_SFXSlider;

    protected override void Start()
    {
        base.Start();
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
