using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour
{
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

    [Header("Playback Settings")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 0f;
    [SerializeField, Min(1)] private int initialEmitterPoolSize = 4;

    private readonly List<AudioSource> _emitters = new List<AudioSource>();

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
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

    private void ConfigureEmitter(AudioSource emitter)
    {
        emitter.playOnAwake = false;
        emitter.loop = false;
        emitter.volume = 1f;
        emitter.spatialBlend = spatialBlend;
        emitter.dopplerLevel = 0f;
    }

    private void CopySettingsFrom(SoundManager other)
    {
        enemyDeathClip = other.enemyDeathClip;
        enemyDeathVolume = other.enemyDeathVolume;
        moneySpentClip = other.moneySpentClip;
        buttonClickClip = other.buttonClickClip;
        sfxVolume = other.sfxVolume;
        spatialBlend = other.spatialBlend;
        initialEmitterPoolSize = other.initialEmitterPoolSize;

        for (int i = 0; i < _emitters.Count; i++)
        {
            ConfigureEmitter(_emitters[i]);
        }

        WarmEmitterPool();
    }
}
