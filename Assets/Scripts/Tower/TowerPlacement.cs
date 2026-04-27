using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacement : MonoBehaviour
{
    private TowerData _data;
    private bool _canBePlaced;
    private SpriteColorFilter _colorFilter;
    private PlacementRangePreview _rangePreview;

    [SerializeField] private float placementRadius = 0.4f;
    [SerializeField] private LayerMask blockedLayer;
    [SerializeField] private Color validColor = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.7f);

    [NonSerialized] public bool isPlacing = true;

    public static event Action<TowerData> OnPlacementConfirmed;

    private void Awake()
    {
        _data = GetComponent<Tower>().Data;
        _colorFilter = GetComponent<SpriteColorFilter>();
        if (_colorFilter == null)
        {
            _colorFilter = gameObject.AddComponent<SpriteColorFilter>();
        }

        _rangePreview = GetComponent<PlacementRangePreview>();
        if (_rangePreview == null)
        {
            _rangePreview = gameObject.AddComponent<PlacementRangePreview>();
        }

        _rangePreview.Configure(_data.range);
        blockedLayer = LayerMask.GetMask("Restricted");
        isPlacing = true;
    }

    private void Update()
    {
        if (!isPlacing)
        {
            return;
        }

        FollowMouse();
        CheckPlacementValidity();
        UpdateVisual();

        if (Mouse.current.leftButton.wasPressedThisFrame && _canBePlaced)
        {
            isPlacing = false;
            ResetVisual();
            OnPlacementConfirmed?.Invoke(_data);
        }
    }

    private void FollowMouse()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0f;
        transform.position = worldPos;
    }

    private void CheckPlacementValidity()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            placementRadius,
            blockedLayer
        );

        _canBePlaced = hit == null;
    }

    private void UpdateVisual()
    {
        Color placementColor = _canBePlaced ? validColor : invalidColor;

        if (_colorFilter != null)
        {
            _colorFilter.SetColor(placementColor);
        }

        if (_rangePreview != null)
        {
            _rangePreview.ApplyFilter(placementColor);
        }
    }

    private void ResetVisual()
    {
        if (_colorFilter != null)
        {
            _colorFilter.ResetColor();
        }

        if (_rangePreview != null)
        {
            _rangePreview.Hide();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, placementRadius);
    }
}
