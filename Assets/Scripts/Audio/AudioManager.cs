using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// オーディオ管理システム - BGMとSEの再生、音量調整
/// シーン間で持続するシングルトン
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip switchSound;
    [SerializeField] private AudioClip goalSound;
    [SerializeField] private AudioClip deathSound;

    [Header("BGM")]
    [SerializeField] private AudioClip mainBGM;

    // 音量設定 (0.0 - 1.0)
    private float masterVolume = 1f;
    private float bgmVolume = 0.7f;
    private float sfxVolume = 1f;

    // PlayerPrefs keys
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    public float MasterVolume => masterVolume;
    public float BGMVolume => bgmVolume;
    public float SFXVolume => sfxVolume;

    private void Awake()
    {
        // シングルトン
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource がなければ作成
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        // 保存された音量を読み込み
        LoadVolumeSettings();
    }

    private void Start()
    {
        // BGM 再生
        if (mainBGM != null)
        {
            PlayBGM(mainBGM);
        }
    }

    #region Volume Control

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveVolumeSettings();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveVolumeSettings();
    }

    private void UpdateVolumes()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = masterVolume * bgmVolume;
        }
        if (sfxSource != null)
        {
            sfxSource.volume = masterVolume * sfxVolume;
        }
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.7f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        UpdateVolumes();
    }

    #endregion

    #region BGM

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;
        
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.UnPause();
        }
    }

    #endregion

    #region SFX

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        
        sfxSource.PlayOneShot(clip, masterVolume * sfxVolume);
    }

    // 便利メソッド
    public void PlayJump()
    {
        PlaySFX(jumpSound);
    }

    public void PlayLand()
    {
        PlaySFX(landSound);
    }

    public void PlaySwitch()
    {
        PlaySFX(switchSound);
    }

    public void PlayGoal()
    {
        PlaySFX(goalSound);
    }

    public void PlayDeath()
    {
        PlaySFX(deathSound);
    }

    #endregion
}
