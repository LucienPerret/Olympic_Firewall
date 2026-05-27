using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const string LevelSelectSceneName = "LevelSelect";

    public static SoundManager Instance { get; private set; }

    [Header("Default SFX")]
    // Assign the default enemy death clip here in the Unity Inspector.
    [Tooltip("Default enemy death sound used when an Enemy does not provide its own clip.")]
    [SerializeField] private AudioClip enemyDeathClip;
    [Tooltip("Volume multiplier for enemy death sounds. Default is quieter than other SFX.")]
    [SerializeField, Range(0f, 1f)] private float enemyDeathVolume = 0.5f;
    // Assign the money spent sound here in the Unity Inspector.
    [Tooltip("Played when the player spends money to place a tower.")]
    [SerializeField] private AudioClip moneySpentClip;
    // Assign the default UI button click sound here in the Unity Inspector.
    [Tooltip("Played when a UI button is clicked.")]
    [SerializeField] private AudioClip buttonClickClip;

    [Header("Music")]
    [Tooltip("Looped song used in the main menu and level select scenes.")]
    [SerializeField] private AudioClip menuMusicClip;
    [Tooltip("Looped song used in gameplay levels.")]
    [SerializeField] private AudioClip levelMusicClip;

    [Header("Playback Settings")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f;
    [SerializeField, Min(1)] private int initialEmitterPoolSize = 4;

    private readonly List<AudioSource> _emitters = new List<AudioSource>();
    private AudioSource _musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.CopySettingsFrom(this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        WarmEmitterPool();
        EnsureMusicSource();
        SceneManager.sceneLoaded += HandleSceneLoaded;
        RefreshMusicForScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            Instance = null;
        }
    }

    private void OnValidate()
    {
        ApplyEmitterSettings();
        ApplyMusicSettings();
    }

    public void PlayEnemyDeath(AudioClip clip, Vector3 position)
    {
        PlaySfx(clip != null ? clip : enemyDeathClip, position, enemyDeathVolume);
    }

    public void PlayMoneySpent()
    {
        PlaySfx(moneySpentClip, transform.position);
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickClip, transform.position);
    }

    private void PlaySfx(AudioClip clip, Vector3 position)
    {
        PlaySfx(clip, position, 1f);
    }

    private void PlaySfx(AudioClip clip, Vector3 position, float volumeMultiplier)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource emitter = GetAvailableEmitter();
        emitter.transform.position = position;
        emitter.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyMusicSettings();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private AudioSource GetAvailableEmitter()
    {
        for (int i = 0; i < _emitters.Count; i++)
        {
            if (!_emitters[i].isPlaying)
            {
                return _emitters[i];
            }
        }

        return CreateEmitter();
    }

    private void WarmEmitterPool()
    {
        while (_emitters.Count < initialEmitterPoolSize)
        {
            CreateEmitter();
        }
    }

    private AudioSource CreateEmitter()
    {
        GameObject emitterObject = new GameObject($"SfxEmitter_{_emitters.Count + 1}");
        emitterObject.transform.SetParent(transform);

        AudioSource emitter = emitterObject.AddComponent<AudioSource>();
        ConfigureEmitter(emitter);
        _emitters.Add(emitter);
        return emitter;
    }

    private void EnsureMusicSource()
    {
        if (_musicSource != null)
        {
            ConfigureMusicSource(_musicSource);
            return;
        }

        Transform existingTransform = transform.Find("MusicSource");
        if (existingTransform != null)
        {
            _musicSource = existingTransform.GetComponent<AudioSource>();
        }

        if (_musicSource == null)
        {
            GameObject musicObject = new GameObject("MusicSource");
            musicObject.transform.SetParent(transform);
            _musicSource = musicObject.AddComponent<AudioSource>();
        }

        ConfigureMusicSource(_musicSource);
    }

    private void ConfigureEmitter(AudioSource emitter)
    {
        emitter.playOnAwake = false;
        emitter.loop = false;
        emitter.volume = 1f;
        emitter.spatialBlend = spatialBlend;
        emitter.dopplerLevel = 0f;
    }

    private void ConfigureMusicSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        source.volume = musicVolume;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        RefreshMusicForScene(scene);
    }

    private void RefreshMusicForScene(Scene scene)
    {
        AudioClip targetClip = GetMusicForScene(scene.name);

        if (targetClip == null)
        {
            StopMusic();
            return;
        }

        EnsureMusicSource();

        if (_musicSource.clip != targetClip)
        {
            _musicSource.clip = targetClip;
            _musicSource.Play();
        }
        else if (!_musicSource.isPlaying)
        {
            _musicSource.Play();
        }

        ApplyMusicSettings();
    }

    private AudioClip GetMusicForScene(string sceneName)
    {
        return IsMenuScene(sceneName) ? menuMusicClip : levelMusicClip;
    }

    private bool IsMenuScene(string sceneName)
    {
        return sceneName == MainMenuSceneName || sceneName == LevelSelectSceneName;
    }

    private void StopMusic()
    {
        if (_musicSource == null)
        {
            return;
        }

        _musicSource.Stop();
        _musicSource.clip = null;
    }

    private void ApplyEmitterSettings()
    {
        for (int i = 0; i < _emitters.Count; i++)
        {
            ConfigureEmitter(_emitters[i]);
        }
    }

    private void ApplyMusicSettings()
    {
        if (_musicSource == null)
        {
            return;
        }

        ConfigureMusicSource(_musicSource);
    }

    private void CopySettingsFrom(SoundManager other)
    {
        enemyDeathClip = other.enemyDeathClip;
        enemyDeathVolume = other.enemyDeathVolume;
        moneySpentClip = other.moneySpentClip;
        buttonClickClip = other.buttonClickClip;
        menuMusicClip = other.menuMusicClip;
        levelMusicClip = other.levelMusicClip;
        sfxVolume = other.sfxVolume;
        musicVolume = other.musicVolume;
        spatialBlend = other.spatialBlend;
        initialEmitterPoolSize = other.initialEmitterPoolSize;

        ApplyEmitterSettings();
        EnsureMusicSource();
        ApplyMusicSettings();
        WarmEmitterPool();
        RefreshMusicForScene(SceneManager.GetActiveScene());
    }
}
