using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerManager : MonoBehaviour
{

    public void PlaceTower(GameObject towerPrefab)
    {
        Instantiate(towerPrefab,
            Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()),
            Quaternion.identity);
    }
}