using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    public EnemyData Data => data;

    public static event Action<EnemyData> OnEnemyReachedEnd;
    public static event Action<Enemy> OnEnemyDestroyed;

    private Path _currentPath;
    private Vector3 _targetPosition;
    private int _currentWaypoint;
    private float _lives;
    private float _maxlives;

    [SerializeField] private Transform healthBar;
    private Vector3 _healthBarOriginalScale;
    private bool _hasBeenCounted = false;

    private void Awake()
    {
        GameObject pathObject = GameObject.Find("Path");
        if (pathObject != null)
        {
            _currentPath = pathObject.GetComponent<Path>();
        }

        _healthBarOriginalScale = healthBar.localScale;
    }

    private void OnEnable()
    {
        _currentWaypoint = 0;
        if (_currentPath == null || _currentPath.GetWaypointCount() == 0)
        {
            _targetPosition = transform.position;
            return;
        }

        _targetPosition = _currentPath.GetPosition(_currentWaypoint);
    }

    private void Update()
    {
        if (_hasBeenCounted || _currentPath == null || _currentPath.GetWaypointCount() == 0)
        {
            return;
        }

        // move towards target position
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, data.speed * Time.deltaTime);

        // set Waypoint when last is reached
        float relativeDistance = (transform.position - _targetPosition).magnitude;
        if (relativeDistance < 0.1f)
        {
            if (_currentWaypoint < _currentPath.GetWaypointCount() - 1)
            {
                _currentWaypoint++;
                _targetPosition = _currentPath.GetPosition(_currentWaypoint);
            }
            else
            {
                _hasBeenCounted = true;
                OnEnemyReachedEnd?.Invoke(data);
                gameObject.SetActive(false);
                
            }
            
        }

    }

    public float GetPathProgress()
    {
        if (_currentPath == null || _currentPath.GetWaypointCount() < 2)
        {
            return 0f;
        }

        float totalPathLength = _currentPath.GetTotalPathLength();
        if (totalPathLength <= Mathf.Epsilon || _currentWaypoint <= 0)
        {
            return 0f;
        }

        int segmentStartIndex = Mathf.Clamp(_currentWaypoint - 1, 0, _currentPath.GetWaypointCount() - 2);
        Vector3 segmentStart = _currentPath.GetPosition(segmentStartIndex);
        Vector3 segmentEnd = _currentPath.GetPosition(segmentStartIndex + 1);
        Vector3 segment = segmentEnd - segmentStart;
        float segmentLength = segment.magnitude;

        float distanceTravelled = _currentPath.GetDistanceToWaypoint(segmentStartIndex);
        if (segmentLength > Mathf.Epsilon)
        {
            Vector3 segmentDirection = segment / segmentLength;
            float projectedDistance = Vector3.Dot(transform.position - segmentStart, segmentDirection);
            distanceTravelled += Mathf.Clamp(projectedDistance, 0f, segmentLength);
        }

        return Mathf.Clamp01(distanceTravelled / totalPathLength);
    }

    public void TakeDamage(float damage)
    {
        _lives -= damage;
        _lives = Math.Max(0, _lives);
        UpdateHealthBar();

        if (_lives <= 0)
        {
            if (_hasBeenCounted) return;
            _hasBeenCounted = true;
            OnEnemyDestroyed?.Invoke(this);
            gameObject.SetActive(false);
        }
    }

    private void UpdateHealthBar()
    {
        float healthPercent = _lives / _maxlives;
        Vector3 scale = _healthBarOriginalScale;
        scale.x = _healthBarOriginalScale.x * healthPercent;
        healthBar.localScale = scale;
    }

    public void Initialize(float healthMultiplier)
    {
        _hasBeenCounted = false;
        _maxlives = data.lives * healthMultiplier;
        _lives = _maxlives;
        UpdateHealthBar();
    }
}
