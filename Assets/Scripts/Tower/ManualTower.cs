using UnityEngine;
using UnityEngine.InputSystem;

public class ManualTower : MonoBehaviour
{
    [Header("Laser")]
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private LayerMask raycastMask = Physics2D.DefaultRaycastLayers;
    [SerializeField] private float laserWidth = 0.15f;
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private int sortingOrder = 10;

    private Camera _mainCamera;
    private LineRenderer _lineRenderer;
    private Collider2D[] _ownColliders;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        _ownColliders = GetComponents<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        ConfigureLaserRenderer();
    }

    private void OnValidate()
    {
        if (_lineRenderer == null)
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        if (_lineRenderer == null && gameObject.scene.IsValid())
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        ConfigureLaserRenderer();
    }

    private void Update()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_mainCamera == null || Mouse.current == null)
        {
            HideLaser();
            return;
        }

        UpdateLaser();
    }

    private void UpdateLaser()
    {
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.z = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);

        Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = transform.position.z;

        Vector2 laserDirection = mouseWorldPosition - transform.position;
        if (laserDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            HideLaser();
            return;
        }

        float laserDistance = laserDirection.magnitude;
        laserDirection.Normalize();

        RotateTowards(laserDirection);

        // Cast along the mouse direction, but ignore this tower's own colliders.
        RaycastHit2D hit = GetClosestValidHit(laserDirection, laserDistance);

        Vector3 laserEndPoint = hit.collider != null
            ? (Vector3)hit.point
            : mouseWorldPosition;

        DrawLaser(transform.position, laserEndPoint);

        if (hit.collider != null)
        {
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }

    private RaycastHit2D GetClosestValidHit(Vector2 direction, float distance)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, distance, raycastMask);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || IsOwnCollider(hit.collider))
            {
                continue;
            }

            return hit;
        }

        return default;
    }

    private bool IsOwnCollider(Collider2D colliderToCheck)
    {
        foreach (Collider2D ownCollider in _ownColliders)
        {
            if (ownCollider == colliderToCheck)
            {
                return true;
            }
        }

        return false;
    }

    private void DrawLaser(Vector3 startPoint, Vector3 endPoint)
    {
        if (_lineRenderer == null)
        {
            return;
        }

        _lineRenderer.enabled = true;
        _lineRenderer.SetPosition(0, startPoint);
        _lineRenderer.SetPosition(1, endPoint);
    }

    private void HideLaser()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }
    }

    private void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void ConfigureLaserRenderer()
    {
        if (_lineRenderer == null)
        {
            return;
        }

        _lineRenderer.positionCount = 2;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.startWidth = laserWidth;
        _lineRenderer.endWidth = laserWidth;
        _lineRenderer.startColor = laserColor;
        _lineRenderer.endColor = laserColor;
        _lineRenderer.numCapVertices = 4;
        _lineRenderer.sortingOrder = sortingOrder;
        _lineRenderer.textureMode = LineTextureMode.Stretch;
        _lineRenderer.enabled = false;

        if (_spriteRenderer != null)
        {
            _lineRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
            _lineRenderer.sortingOrder = _spriteRenderer.sortingOrder + sortingOrder;
        }

        if (_lineRenderer.material == null)
        {
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                _lineRenderer.material = new Material(spriteShader);
            }
        }
    }
}
