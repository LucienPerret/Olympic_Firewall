using System;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static event Action<int> OnWaveChanged;

    [SerializeField] private WaveData[] waves;
    // Starts at 1 again after all waves are cleared
    private int _currentWaveIndex = -1;
    // Counts further up each wave
    private int _waveCounter = -1;
    private WaveData CurrentWave => waves[_currentWaveIndex];

    private float _spawnTimer;
    private float _spawnCounter;
    private int _enemiesRemoved;
   

    [SerializeField] private ObejctPooler orcPool;
    [SerializeField] private ObejctPooler dragonPool;
    [SerializeField] private ObejctPooler kaijuPool;

    private Dictionary<EnemyType, ObejctPooler> _poolDictionary;

    private float _waveCooldown;
    private bool _runningWave;

    private void Awake()
    {
        _poolDictionary = new Dictionary<EnemyType, ObejctPooler>()
        {
            {EnemyType.Orc, orcPool},
            {EnemyType.Dragon, dragonPool},
            {EnemyType.Kaiju, kaijuPool}
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
                _runningWave = false;
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
        _currentWaveIndex = (_currentWaveIndex + 1) % waves.Length;
        _waveCounter++;
        OnWaveChanged?.Invoke(_waveCounter);
        _spawnCounter = 0;
        _enemiesRemoved = 0;
        _spawnTimer = 0f;
        _runningWave = true;

    }
}
