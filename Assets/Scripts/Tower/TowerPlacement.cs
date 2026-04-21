using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.LowLevelPhysics2D.PhysicsShape;

public class TowerPlacement : MonoBehaviour
{

    private TowerData _data;

    public static event Action<TowerData> OnPlacementConfirmed;

    private void Awake()
    {
        _data = GetComponent<Tower>().Data;
        isPlacing = true;
    }

    [NonSerialized] public bool isPlacing = true;
    private void Update()
    {
        if (!isPlacing) return;

        // Follow mouse
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0f;
        transform.position = worldPos;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isPlacing = false;
            OnPlacementConfirmed?.Invoke(_data);
        }
    }
}