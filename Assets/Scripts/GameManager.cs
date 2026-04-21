using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // Additional Manager
    [SerializeField] private TowerManager towerManager;

    // publish actions
    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnResourcesChanged;

    // Game Data (could be outsources to a Scriptable Object)
    private int _lives = 20;
    private int _resources = 50;

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

    private void Start()
    {
        OnLivesChanged?.Invoke(_lives);
        OnResourcesChanged?.Invoke(_resources);
    }

    private void HandleEnemyReachedEnd(EnemyData data)
    {
        _lives = Mathf.Max(0, _lives - data.damage);
        OnLivesChanged?.Invoke(_lives);
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
        Debug.Log("Resources spent");
    }

    public void HandleTowerPlacement(GameObject towerPrefab)
    {
        Debug.Log("Placing Tower");
        //replace
        int cost = towerPrefab.GetComponent<Tower>().Data.cost;
        if (!HasEnoughResources(cost)) return;

        towerManager.PlaceTower(towerPrefab);

    }

    private void HandleTowerPlaced(TowerData towerType)
    {
        SpendResources(towerType.cost);
    }
}