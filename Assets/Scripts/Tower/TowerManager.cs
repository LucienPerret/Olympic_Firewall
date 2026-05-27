using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class TowerManager : MonoBehaviour
{
    public static TowerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate TowerManager found. Destroying the newer instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void PlaceTower(GameObject towerPrefab)
    {
        Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        spawnPosition.z = 0f;

        Instantiate(towerPrefab, spawnPosition, Quaternion.identity);
    }
}
