using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float sliderMulti = 25;

    [Header("SFX Settings")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxSliderText;
    [SerializeField] private string sfxParameter;

    [Header("Music Settings")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicSliderText;
    [SerializeField] private string bgmParameter;

    #region Unity Methods

    private void Start()
    {
        LoadPauseMenuValues();
    }

    #endregion

    #region Audio Settings

    // Update SFX volume and save to PlayerPrefs
    public void SfxSliderValue(float value)
    {
        sfxSliderText.text = Mathf.RoundToInt(value * 100) + "%";
        float newValue = Mathf.Log10(value) * sliderMulti;
        audioMixer.SetFloat(sfxParameter, newValue);
        PlayerPrefs.SetFloat(sfxParameter, value);
        PlayerPrefs.Save();
    }

    // Update music volume and save to PlayerPrefs
    public void MusicSliderValue(float value)
    {
        musicSliderText.text = Mathf.RoundToInt(value * 100) + "%";
        float newValue = Mathf.Log10(value) * sliderMulti;
        audioMixer.SetFloat(bgmParameter, newValue);
        PlayerPrefs.SetFloat(bgmParameter, value);
        PlayerPrefs.Save();
    }

    // Load saved values for sliders
    private void LoadPauseMenuValues()
    {
        float sfxValue = PlayerPrefs.GetFloat(sfxParameter, 0.5f);
        sfxSlider.value = sfxValue;
        sfxSliderText.text = Mathf.RoundToInt(sfxValue * 100) + "%";

        float musicValue = PlayerPrefs.GetFloat(bgmParameter, 0.5f);
        musicSlider.value = musicValue;
        musicSliderText.text = Mathf.RoundToInt(musicValue * 100) + "%";
    }

    #endregion
}

