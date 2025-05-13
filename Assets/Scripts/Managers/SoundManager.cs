using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    public float bgmVolume { get; private set; }
    public float sfxVolume { get; private set; }
    public bool m_sfxSource { get; private set; }

    private AudioSource m_bgmSource, m_bgmChange;
    private bool isChangebgm = false;
    [SerializeField] private AudioSource SFXObject;
    [SerializeField] Transform sfxPosi;

    [SerializeField]
    private AudioMixerGroup m_sfxMixer;
    [SerializeField]
    private AudioMixerGroup m_bgmMixer;

    private void Awake()
    {
        m_bgmSource = GameObject.FindGameObjectWithTag(Constraints.Tag.BGMSource).GetComponent<AudioSource>();
        isChangebgm = false;
        bgmVolume = PlayerPrefs.GetFloat(Constraints.PlayerPref.BGMVolume, 0.5f);
        sfxVolume = PlayerPrefs.GetFloat(Constraints.PlayerPref.SFXVolume, 0.5f);

        //Set initial volume value from Player Prefs
        m_bgmSource.volume = bgmVolume;
        m_sfxMixer.audioMixer.SetFloat("sfxVolume", Mathf.Log10(sfxVolume) * 20 );
        m_bgmMixer.audioMixer.SetFloat("bgmVolume", Mathf.Log10(bgmVolume) * 20 );
    }

    public void SetBGMVolume(float _volume)
    {
        bgmVolume = _volume;
        PlayerPrefs.SetFloat(Constraints.PlayerPref.BGMVolume, bgmVolume);

        m_bgmMixer.audioMixer.SetFloat("bgmVolume", Mathf.Log10(bgmVolume)*20 );
        //m_bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float _volume)
    {
        sfxVolume = _volume;
        PlayerPrefs.SetFloat(Constraints.PlayerPref.SFXVolume, sfxVolume);

        m_sfxMixer.audioMixer.SetFloat("sfxVolume", Mathf.Log10(sfxVolume) * 20);
    }

    public void PlayBGM()
    {
        if(!m_bgmSource)
            m_bgmSource = GameObject.FindGameObjectWithTag(Constraints.Tag.BGMSource).GetComponent<AudioSource>();

        m_bgmSource.Play();
    }

    public void StopBGM()
    {
        if (!m_bgmSource)
            m_bgmSource = GameObject.FindGameObjectWithTag(Constraints.Tag.BGMSource).GetComponent<AudioSource>();
        m_bgmSource.Stop();
        if (m_bgmChange != null)
        {
            m_bgmChange.Stop();
            m_bgmChange.clip = null;
        }
        isChangebgm = false;
        m_bgmSource.clip = null;
    }

    public void PlayBGM(AudioClip audioClip)
    {
        if(!isChangebgm && m_bgmSource.clip == null)
        {
            m_bgmSource.clip = audioClip;
            m_bgmSource.Play();
        }
        else if (!isChangebgm && m_bgmSource.clip.name != audioClip.name)
        {
            if(!m_bgmChange)
                m_bgmChange = GameObject.FindGameObjectWithTag(Constraints.Tag.BGMSource).AddComponent<AudioSource>();
            m_bgmChange.loop = true;
            m_bgmChange.outputAudioMixerGroup = m_bgmSource.outputAudioMixerGroup;
            StartCoroutine(FadeMusic(audioClip, m_bgmSource, m_bgmChange));
        }
        else if (isChangebgm && m_bgmChange.clip.name != audioClip.name) 
        {
            StartCoroutine(FadeMusic(audioClip, m_bgmChange, m_bgmSource));
        }
    }

    public void PlaySFX(AudioClip audioClip, Transform spawnTrans = null)
    {
        Transform sfxPosition = spawnTrans == null? sfxPosi : spawnTrans;

        AudioSource audioSource = Instantiate(SFXObject, sfxPosition.position, Quaternion.identity);
        audioSource.transform.parent = sfxPosi.transform;
        audioSource.clip = audioClip;
        audioSource.volume = sfxVolume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLength);
    }

    private IEnumerator FadeMusic(AudioClip clip, AudioSource source1, AudioSource source2)
    {
        float timeToFade = 2.25f;
        float timeElapsed = 0f;

        source2.clip = clip;
        source2.Play();

        while (timeElapsed < timeToFade)
        {
            source2.volume = Mathf.Lerp(0, 1, timeElapsed / timeToFade);
            source1.volume = Mathf.Lerp(1, 0, timeElapsed / timeToFade);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        source1.Stop();
        isChangebgm = !isChangebgm;
        yield break;
    }
}
