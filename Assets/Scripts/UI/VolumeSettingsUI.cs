using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 音量設定UI - スライダーで音量調整
/// </summary>
public class VolumeSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Labels (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI masterLabel;
    [SerializeField] private TMPro.TextMeshProUGUI bgmLabel;
    [SerializeField] private TMPro.TextMeshProUGUI sfxLabel;

    private void Start()
    {
        // AudioManager の値でスライダーを初期化
        if (AudioManager.Instance != null)
        {
            if (masterSlider != null)
            {
                masterSlider.value = AudioManager.Instance.MasterVolume;
                masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
            if (bgmSlider != null)
            {
                bgmSlider.value = AudioManager.Instance.BGMVolume;
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }
            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.SFXVolume;
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }

        UpdateLabels();
    }

    private void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance?.SetMasterVolume(value);
        UpdateLabels();
    }

    private void OnBGMVolumeChanged(float value)
    {
        AudioManager.Instance?.SetBGMVolume(value);
        UpdateLabels();
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        if (masterLabel != null && masterSlider != null)
        {
            masterLabel.text = $"マスター: {Mathf.RoundToInt(masterSlider.value * 100)}%";
        }
        if (bgmLabel != null && bgmSlider != null)
        {
            bgmLabel.text = $"BGM: {Mathf.RoundToInt(bgmSlider.value * 100)}%";
        }
        if (sfxLabel != null && sfxSlider != null)
        {
            sfxLabel.text = $"SE: {Mathf.RoundToInt(sfxSlider.value * 100)}%";
        }
    }

    private void OnDestroy()
    {
        // リスナー解除
        if (masterSlider != null) masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }
}
