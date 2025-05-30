using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_Settings : MonoBehaviour
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

    [Header("Fire Settings")]
    [SerializeField] private Toggle friendlyFireToggle;
    [SerializeField] private Toggle preciseAimToggle;

    #region Audio Settings

    // Update SFX volume 
    public void SfxSliderValue(float value)
    {
        sfxSliderText.text = Mathf.RoundToInt(value * 100) + "%";
        float newValue = Mathf.Log10(value) * sliderMulti;
        audioMixer.SetFloat(sfxParameter, newValue);
    }

    // Update music volume 
    public void MusicSliderValue(float value)
    {
        musicSliderText.text = Mathf.RoundToInt(value * 100) + "%";
        float newValue = Mathf.Log10(value) * sliderMulti;
        audioMixer.SetFloat(bgmParameter, newValue);
    }

    #endregion

    #region Settings Logic

    // Toggle friendly fire and save to PlayerPrefs
    public void SetFriendlyFireToggle()
    {
        bool friendlyFire = GameManager.instance.friendlyFire;
        GameManager.instance.friendlyFire = !friendlyFire;
        int friendlyFireValue = GameManager.instance.friendlyFire ? 1 : 0;
        PlayerPrefs.SetInt("FriendlyFire", friendlyFireValue);
        PlayerPrefs.Save();
    }

    // Load saved settings for toggles and sliders
    public void LoadSettingsValues()
    {
        if (PlayerPrefs.HasKey("FriendlyFire"))
        {
            int friendlyFireValue = PlayerPrefs.GetInt("FriendlyFire");
            friendlyFireToggle.isOn = friendlyFireValue == 1;
        }
        else
        {
            friendlyFireToggle.isOn = false;
            GameManager.instance.friendlyFire = false;
        }

        sfxSlider.value = PlayerPrefs.GetFloat(sfxParameter, 0.5f);
        musicSlider.value = PlayerPrefs.GetFloat(bgmParameter, 0.5f);
    }

    // Save settings when UI is closed
    private void OnDisable()
    {
        bool friendlyFire = GameManager.instance.friendlyFire;
        int friendlyFireValue = friendlyFire ? 1 : 0;
        PlayerPrefs.SetInt("FriendlyFire", friendlyFireValue);
        PlayerPrefs.SetFloat(sfxParameter, sfxSlider.value);
        PlayerPrefs.SetFloat(bgmParameter, musicSlider.value);
        PlayerPrefs.Save();
    }

    #endregion
}

