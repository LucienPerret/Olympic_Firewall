using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Spawner : MonoBehaviour
{
    public static event Action<int> OnWaveChanged;
    public static event Action<bool> OnWaveStateChanged;
    public static event Action OnAllWavesCompleted;

    [SerializeField] private WaveData[] waves;
    // Starts at 1 again after all waves are cleared
    private int _currentWaveIndex = -1;
    // Counts further up each wave
    private int _waveCounter = -1;
    private WaveData CurrentWave => waves[_currentWaveIndex];

    private float _spawnTimer;
    private float _spawnCounter;
    private int _enemiesRemoved;
   

    [FormerlySerializedAs("orcPool")]
    [SerializeField] private ObejctPooler viroxPool;
    [FormerlySerializedAs("dragonPool")]
    [SerializeField] private ObejctPooler skitterPool;
    [FormerlySerializedAs("kaijuPool")]
    [SerializeField] private ObejctPooler corruptorPool;

    private Dictionary<EnemyType, ObejctPooler> _poolDictionary;

    private float _waveCooldown;
    private bool _runningWave;
    private bool _allWavesCompleted;

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObejctPooler>()
        {
            {EnemyType.Virox, viroxPool},
            {EnemyType.Skitter, skitterPool},
            {EnemyType.Corruptor, corruptorPool}
        };
    }

    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        OnWaveChanged?.Invoke(_waveCounter);
        OnWaveStateChanged?.Invoke(_runningWave);
    }
    private void Update()
    {
        if (_runningWave)
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0 && _spawnCounter < CurrentWave.enemiesPerWave)
            {
                _spawnTimer = CurrentWave.spawnInterval;
                SpawnEnemy();
                _spawnCounter++;
            }
            else if (_spawnCounter >= CurrentWave.enemiesPerWave && _enemiesRemoved >= CurrentWave.enemiesPerWave)
            {
                SetWaveRunning(false);

                if (_waveCounter >= waves.Length - 1)
                {
                    CompleteAllWaves();
                }
            }

        }
    }

    private void SpawnEnemy()
    {
        if (_poolDictionary.TryGetValue(CurrentWave.enemyType, out var pool))
        {
            GameObject spawnedObject = pool.GetPooledObject();
            spawnedObject.transform.position = transform.position;

            float healthMultiplier = 1f + (_waveCounter * 0.1f); // +10% per wave
            Enemy enemy = spawnedObject.GetComponent<Enemy>();
            enemy.Initialize(healthMultiplier);

            spawnedObject.SetActive(true);

        }
        
    }

    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _enemiesRemoved++;
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        _enemiesRemoved++;
    }

    public void StartWave()
    {
        if (_runningWave || _allWavesCompleted || waves == null || waves.Length == 0)
        {
            return;
        }

        SoundManager.Instance?.PlayButtonClick();
        _currentWaveIndex = (_currentWaveIndex + 1) % waves.Length;
        _waveCounter++;
        OnWaveChanged?.Invoke(_waveCounter);
        _spawnCounter = 0;
        _enemiesRemoved = 0;
        _spawnTimer = 0f;
        SetWaveRunning(true);

    }

    private void SetWaveRunning(bool isRunning)
    {
        if (_runningWave == isRunning)
        {
            return;
        }

        _runningWave = isRunning;
        OnWaveStateChanged?.Invoke(_runningWave);
    }

    private void CompleteAllWaves()
    {
        if (_allWavesCompleted)
        {
            return;
        }

        _allWavesCompleted = true;
        OnAllWavesCompleted?.Invoke();
    }
}
