using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("BGM Sources")]
    [SerializeField] private AudioSource mainMenuBGM;
    [SerializeField] private AudioSource[] missionBGMs;
    [SerializeField] private AudioSource gameOverBGM;
    [SerializeField] private AudioSource missionCompleteBGM;

    [Header("BGM Settings")]
    [SerializeField] private bool[] loopTracks;
    [SerializeField] private bool playBgm;
    [SerializeField] private float fadeDuration = 1f;

    private int currentMissionBGMIndex;

    #region Unity Methods

    private void Awake()
    {
        instance = this;

        // Validate BGM assignments
        if (mainMenuBGM == null || missionBGMs == null || gameOverBGM == null || missionCompleteBGM == null)
            Debug.LogWarning("AudioManager: One or more BGM sources are not assigned!");

        // Log main menu BGM details
        if (mainMenuBGM != null)
        {
            if (mainMenuBGM.clip == null)
                Debug.LogWarning("AudioManager: MainMenuBGM AudioSource is assigned, but no audio clip is set!");
            else
                Debug.Log($"AudioManager: MainMenuBGM is assigned with clip {mainMenuBGM.clip.name}, volume: {mainMenuBGM.volume}, playOnAwake: {mainMenuBGM.playOnAwake}");
        }

        // Ensure loopTracks matches missionBGMs length
        if (loopTracks == null || loopTracks.Length != missionBGMs.Length)
            loopTracks = new bool[missionBGMs.Length];
    }

    private void Start()
    {
        // Play main menu BGM at start for testing
        if (mainMenuBGM != null && mainMenuBGM.clip != null)
        {
            Debug.Log("AudioManager: Attempting to play MainMenuBGM manually in Start...");
            mainMenuBGM.Play();
        }
    }

    #endregion

    #region SFX

    // Play SFX with optional random pitch
    public void PlaySFX(AudioSource sfx, bool randomPitch = false, float minPitch = 0.85f, float maxPitch = 1.1f)
    {
        if (sfx == null)
            return;

        sfx.pitch = randomPitch ? Random.Range(minPitch, maxPitch) : 1f;
        sfx.Play();
    }

    // Fade and delay SFX playback or stop
    public void ControlSFX_FadeAndDelay(AudioSource source, bool play, float targetVolume, float delay = 0, float fadeDuration = 1)
    {
        if (source == null)
            return;
        StartCoroutine(ProcessSFX_FadeAndDelay(source, play, targetVolume, delay, fadeDuration));
    }

    private IEnumerator ProcessSFX_FadeAndDelay(AudioSource source, bool play, float targetVolume, float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay);

        float startVolume = play ? 0 : source.volume;
        float endVolume = play ? targetVolume : 0;
        float elapsedTime = 0;

        if (play)
        {
            source.volume = 0;
            source.Play();
        }

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, endVolume, elapsedTime / fadeDuration);
            yield return null;
        }

        source.volume = endVolume;

        if (!play)
            source.Stop();
    }

    #endregion

    #region BGM

    // Play BGM by type with crossfade
    public void PlayBGM(int bgmType, float fadeDuration = 0f)
    {
        AudioSource newSource = GetBGMSourceByType(bgmType, out string bgmTypeName);
        if (newSource == null)
            return;

        Debug.Log($"AudioManager: Playing BGM type {bgmTypeName}");
        StartCoroutine(CrossfadeBGM(newSource, fadeDuration > 0 ? fadeDuration : this.fadeDuration));
    }

    // Stop currently playing BGM with fade out
    public void StopAllBGM(float fadeDuration = 0f)
    {
        AudioSource currentSource = GetCurrentPlayingBGM();
        if (currentSource != null)
        {
            Debug.Log($"AudioManager: Stopping BGM {currentSource.name} with fade duration {fadeDuration}");
            StartCoroutine(FadeOutBGM(currentSource, fadeDuration > 0 ? fadeDuration : this.fadeDuration));
        }
    }

    // Get BGM AudioSource by type
    private AudioSource GetBGMSourceByType(int bgmType, out string bgmTypeName)
    {
        bgmTypeName = "";
        switch (bgmType)
        {
            case 0:
                bgmTypeName = "Main Menu";
                return mainMenuBGM;
            case 1:
                bgmTypeName = "Mission";
                if (missionBGMs == null || missionBGMs.Length == 0)
                {
                    Debug.LogWarning("AudioManager: No mission BGM tracks assigned!");
                    return null;
                }
                int newIndex = Random.Range(0, missionBGMs.Length);
                while (newIndex == currentMissionBGMIndex && missionBGMs.Length > 1)
                    newIndex = Random.Range(0, missionBGMs.Length);
                currentMissionBGMIndex = newIndex;
                return missionBGMs[currentMissionBGMIndex];
            case 2:
                bgmTypeName = "Game Over";
                return gameOverBGM;
            case 3:
                bgmTypeName = "Mission Complete";
                return missionCompleteBGM;
            default:
                Debug.LogWarning("AudioManager: Invalid BGM type!");
                return null;
        }
    }

    // Get currently playing BGM AudioSource
    private AudioSource GetCurrentPlayingBGM()
    {
        if (mainMenuBGM != null && mainMenuBGM.isPlaying) return mainMenuBGM;
        foreach (var bgm in missionBGMs)
            if (bgm != null && bgm.isPlaying) return bgm;
        if (gameOverBGM != null && gameOverBGM.isPlaying) return gameOverBGM;
        if (missionCompleteBGM != null && missionCompleteBGM.isPlaying) return missionCompleteBGM;
        return null;
    }

    // Fade out all tracks except the new one
    private void StopAllTracksExcept(AudioSource excludeSource, float fadeDuration)
    {
        if (mainMenuBGM != null && mainMenuBGM != excludeSource && mainMenuBGM.isPlaying)
            StartCoroutine(FadeOutBGM(mainMenuBGM, fadeDuration));

        foreach (var bgm in missionBGMs)
            if (bgm != null && bgm != excludeSource && bgm.isPlaying)
                StartCoroutine(FadeOutBGM(bgm, fadeDuration));

        if (gameOverBGM != null && gameOverBGM != excludeSource && gameOverBGM.isPlaying)
            StartCoroutine(FadeOutBGM(gameOverBGM, fadeDuration));

        if (missionCompleteBGM != null && missionCompleteBGM != excludeSource && missionCompleteBGM.isPlaying)
            StartCoroutine(FadeOutBGM(missionCompleteBGM, fadeDuration));
    }

    // Crossfade to new BGM
    private IEnumerator CrossfadeBGM(AudioSource newSource, float fadeDuration)
    {
        if (newSource == null)
        {
            Debug.LogWarning("AudioManager: CrossfadeBGM received a null AudioSource!");
            yield break;
        }

        StopAllTracksExcept(newSource, fadeDuration);

        newSource.volume = 0;
        newSource.loop = IsMissionBGM(newSource) ? loopTracks[currentMissionBGMIndex] : false;
        Debug.Log($"AudioManager: Starting playback for {newSource.name}, loop: {newSource.loop}");
        newSource.Play();

        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            newSource.volume = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            yield return null;
        }

        newSource.volume = 1;
        Debug.Log($"AudioManager: Finished fading in {newSource.name}, volume: {newSource.volume}");
    }

    // Fade out a BGM AudioSource
    private IEnumerator FadeOutBGM(AudioSource source, float fadeDuration)
    {
        if (source == null)
        {
            Debug.LogWarning("AudioManager: FadeOutBGM received a null AudioSource!");
            yield break;
        }

        float startVolume = source.volume;
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeDuration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume; // Reset volume for next play
        Debug.Log($"AudioManager: Finished fading out {source.name}, stopped and reset volume to {source.volume}");
    }

    // Check if AudioSource is a mission BGM
    private bool IsMissionBGM(AudioSource source)
    {
        foreach (var bgm in missionBGMs)
            if (bgm == source)
                return true;
        return false;
    }

    #endregion
}
