using System;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // publish actions
    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnResourcesChanged;
    public static event Action OnGameOver;

    // Game Data (could be outsources to a Scriptable Object)
    private int _lives = 1;
    private int _resources = 50;
    private bool _isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate GameManager found. Destroying the newer instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
        TowerButton.OnTowerbuttonClick += HandleTowerPlacement;

        TowerPlacement.OnPlacementConfirmed += HandleTowerPlaced;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
        TowerButton.OnTowerbuttonClick -= HandleTowerPlacement;

        TowerPlacement.OnPlacementConfirmed -= HandleTowerPlaced;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        OnLivesChanged?.Invoke(_lives);
        OnResourcesChanged?.Invoke(_resources);
    }

    private void HandleEnemyReachedEnd(EnemyData data)
    {
        if (_isGameOver)
        {
            return;
        }

        _lives = Mathf.Max(0, _lives - data.damage);
        OnLivesChanged?.Invoke(_lives);

        if (_lives <= 0)
        {
            _isGameOver = true;
            OnGameOver?.Invoke();
        }
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        AddResources(Mathf.RoundToInt(enemy.Data.resourceReward));
    }

    private void AddResources(int amount)
    {
        _resources += amount;
        OnResourcesChanged?.Invoke(_resources);
        Debug.Log("Resources added");
    }

    public bool HasEnoughResources(int amount)
    {
        return _resources > amount;
    }

    public void SpendResources(int amount)
    {
        _resources -= amount;
        OnResourcesChanged?.Invoke(_resources);
        SoundManager.Instance?.PlayMoneySpent();
        Debug.Log("Resources spent");
    }

    public void HandleTowerPlacement(GameObject towerPrefab)
    {
        if (_isGameOver)
        {
            return;
        }

        Debug.Log("Placing Tower");
        int cost = towerPrefab.GetComponent<Tower>().Data.cost;
        if (!HasEnoughResources(cost)) return;

        if (TowerManager.Instance == null)
        {
            Debug.LogError("TowerManager.Instance is missing from the scene.");
            return;
        }

        TowerManager.Instance.PlaceTower(towerPrefab);

    }

    private void HandleTowerPlaced(TowerData towerType)
    {
        SpendResources(towerType.cost);
    }
}
