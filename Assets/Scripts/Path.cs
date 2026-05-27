using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Path : MonoBehaviour
{
    private const string ColliderObjectName = "PathCollider";
    private const float MinSegmentLength = 0.0001f;

    public GameObject[] Waypoints;

    [SerializeField] private float pathWidth = 1f;

    private PolygonCollider2D _collider;
    private int _lastHash = int.MinValue;
    private Vector3[] _cachedWaypointPositions = Array.Empty<Vector3>();
    private float[] _cumulativeDistances = Array.Empty<float>();
    private float _totalPathLength;

    public Vector3 GetPosition(int index)
    {
        EnsurePathData();

        if (index < 0 || index >= _cachedWaypointPositions.Length)
        {
            return transform.position;
        }

        return _cachedWaypointPositions[index];
    }

    public int GetWaypointCount()
    {
        EnsurePathData();
        return _cachedWaypointPositions.Length;
    }

    public float GetTotalPathLength()
    {
        EnsurePathData();
        return _totalPathLength;
    }

    public float GetDistanceToWaypoint(int waypointIndex)
    {
        EnsurePathData();

        if (_cumulativeDistances.Length == 0 || waypointIndex <= 0)
        {
            return 0f;
        }

        if (waypointIndex >= _cumulativeDistances.Length)
        {
            return _totalPathLength;
        }

        return _cumulativeDistances[waypointIndex];
    }

    public bool ContainsPosition(Vector2 position, float extraRadius = 0f)
    {
        EnsurePathData();

        if (_cachedWaypointPositions.Length == 0)
        {
            return false;
        }

        float allowedDistance = Mathf.Max(0f, pathWidth * 0.5f + extraRadius);
        float allowedDistanceSquared = allowedDistance * allowedDistance;

        if (_cachedWaypointPositions.Length == 1)
        {
            return ((Vector2)_cachedWaypointPositions[0] - position).sqrMagnitude <= allowedDistanceSquared;
        }

        for (int i = 1; i < _cachedWaypointPositions.Length; i++)
        {
            Vector2 start = _cachedWaypointPositions[i - 1];
            Vector2 end = _cachedWaypointPositions[i];

            if (DistanceToSegmentSquared(position, start, end) <= allowedDistanceSquared)
            {
                return true;
            }
        }

        return false;
    }

    private void OnEnable()
    {
        pathWidth = Mathf.Max(0f, pathWidth);
        _lastHash = int.MinValue;
        RefreshPath();
    }

    private void OnValidate()
    {
        pathWidth = Mathf.Max(0f, pathWidth);
        _lastHash = int.MinValue;
        RefreshPath();
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            return;
        }

        RefreshPath();
    }

    private void EnsurePathData()
    {
        if (ComputeHash() != _lastHash)
        {
            RefreshPath();
        }
    }

    private void RefreshPath()
    {
        int currentHash = ComputeHash();
        if (currentHash == _lastHash)
        {
            return;
        }

        _lastHash = currentHash;
        RebuildPathMetrics();
        RebuildCollider();
    }

    private int ComputeHash()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + pathWidth.GetHashCode();

            if (Waypoints == null)
            {
                return hash;
            }

            hash = hash * 31 + Waypoints.Length;

            for (int i = 0; i < Waypoints.Length; i++)
            {
                GameObject waypoint = Waypoints[i];
                if (waypoint == null)
                {
                    hash = hash * 31;
                    continue;
                }

                Vector3 position = waypoint.transform.position;
                hash = hash * 31 + position.x.GetHashCode();
                hash = hash * 31 + position.y.GetHashCode();
                hash = hash * 31 + position.z.GetHashCode();
            }

            return hash;
        }
    }

    private void RebuildPathMetrics()
    {
        List<Vector3> positions = new List<Vector3>();

        if (Waypoints != null)
        {
            for (int i = 0; i < Waypoints.Length; i++)
            {
                GameObject waypoint = Waypoints[i];
                if (waypoint == null)
                {
                    continue;
                }

                Vector3 position = waypoint.transform.position;
                if (positions.Count > 0 && Vector3.Distance(positions[positions.Count - 1], position) < MinSegmentLength)
                {
                    continue;
                }

                positions.Add(position);
            }
        }

        _cachedWaypointPositions = positions.ToArray();
        _cumulativeDistances = new float[_cachedWaypointPositions.Length];
        _totalPathLength = 0f;

        for (int i = 1; i < _cachedWaypointPositions.Length; i++)
        {
            _totalPathLength += Vector3.Distance(_cachedWaypointPositions[i - 1], _cachedWaypointPositions[i]);
            _cumulativeDistances[i] = _totalPathLength;
        }
    }

    private void RebuildCollider()
    {
        PolygonCollider2D polygonCollider = GetOrCreateCollider();
        if (polygonCollider == null)
        {
            return;
        }

        List<Vector2> localPoints = CollectLocalPoints(polygonCollider.transform);
        if (localPoints.Count < 2 || pathWidth <= 0f)
        {
            polygonCollider.enabled = false;
            polygonCollider.pathCount = 0;
            return;
        }

        List<Vector2> polygon = BuildPolygon(localPoints, pathWidth * 0.5f);
        if (polygon.Count < 3)
        {
            polygonCollider.enabled = false;
            polygonCollider.pathCount = 0;
            return;
        }

        polygonCollider.enabled = true;
        polygonCollider.pathCount = 1;
        polygonCollider.SetPath(0, polygon);
    }

    private PolygonCollider2D GetOrCreateCollider()
    {
        if (_collider != null)
        {
            PrepareColliderObject(_collider.gameObject);
            return _collider;
        }

        Transform child = transform.Find(ColliderObjectName);
        if (child == null)
        {
            child = new GameObject(ColliderObjectName).transform;
            child.SetParent(transform, false);
        }

        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
        PrepareColliderObject(child.gameObject);

        _collider = child.GetComponent<PolygonCollider2D>();
        if (_collider == null)
        {
            _collider = child.gameObject.AddComponent<PolygonCollider2D>();
        }

        _collider.isTrigger = true;
        return _collider;
    }

    private void PrepareColliderObject(GameObject colliderObject)
    {
        colliderObject.layer = LayerMask.NameToLayer("Restricted");
    }

    private List<Vector2> CollectLocalPoints(Transform space)
    {
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < _cachedWaypointPositions.Length; i++)
        {
            Vector2 localPosition = space.InverseTransformPoint(_cachedWaypointPositions[i]);
            if (points.Count > 0 && Vector2.Distance(points[points.Count - 1], localPosition) < MinSegmentLength)
            {
                continue;
            }

            points.Add(localPosition);
        }

        return points;
    }

    private List<Vector2> BuildPolygon(List<Vector2> points, float halfWidth)
    {
        List<Vector2> left = new List<Vector2>();
        List<Vector2> right = new List<Vector2>();

        Vector2 startDirection = (points[1] - points[0]).normalized;
        Vector2 startNormal = Perpendicular(startDirection);
        Vector2 startBase = points[0] - startDirection * halfWidth;
        left.Add(startBase + startNormal * halfWidth);
        right.Add(startBase - startNormal * halfWidth);

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 previousDirection = (points[i] - points[i - 1]).normalized;
            Vector2 nextDirection = (points[i + 1] - points[i]).normalized;
            Vector2 miter = Perpendicular(previousDirection) + Perpendicular(nextDirection);

            if (miter.sqrMagnitude < MinSegmentLength)
            {
                miter = Perpendicular(nextDirection);
            }
            else
            {
                miter.Normalize();
            }

            float dot = Vector2.Dot(miter, Perpendicular(nextDirection));
            float clampedDot = Mathf.Abs(dot) < 0.2f
                ? Mathf.Sign(dot == 0f ? 1f : dot) * 0.2f
                : dot;

            float miterLength = halfWidth / clampedDot;
            left.Add(points[i] + miter * miterLength);
            right.Add(points[i] - miter * miterLength);
        }

        Vector2 endDirection = (points[points.Count - 1] - points[points.Count - 2]).normalized;
        Vector2 endNormal = Perpendicular(endDirection);
        Vector2 endBase = points[points.Count - 1] + endDirection * halfWidth;
        left.Add(endBase + endNormal * halfWidth);
        right.Add(endBase - endNormal * halfWidth);

        List<Vector2> polygon = new List<Vector2>(left.Count + right.Count);
        polygon.AddRange(left);
        for (int i = right.Count - 1; i >= 0; i--)
        {
            polygon.Add(right[i]);
        }

        return polygon;
    }

    private static Vector2 Perpendicular(Vector2 direction)
    {
        return new Vector2(-direction.y, direction.x);
    }

    private static float DistanceToSegmentSquared(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float segmentLengthSquared = segment.sqrMagnitude;
        if (segmentLengthSquared < MinSegmentLength)
        {
            return (point - start).sqrMagnitude;
        }

        float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / segmentLengthSquared);
        Vector2 closestPoint = start + segment * t;
        return (point - closestPoint).sqrMagnitude;
    }

    private void OnDrawGizmos()
    {
        EnsurePathData();
        if (_cachedWaypointPositions.Length == 0)
        {
            return;
        }

        Gizmos.color = Color.yellow;

        for (int i = 0; i < _cachedWaypointPositions.Length; i++)
        {
            Vector3 position = _cachedWaypointPositions[i];

#if UNITY_EDITOR
            string label = i < Waypoints?.Length && Waypoints[i] != null ? Waypoints[i].name : $"Waypoint {i}";
            GUIStyle style = new GUIStyle { alignment = TextAnchor.MiddleCenter };
            style.normal.textColor = Color.white;
            Handles.Label(position + Vector3.up * 0.7f, label, style);
#endif

            if (i < _cachedWaypointPositions.Length - 1)
            {
                Gizmos.DrawLine(position, _cachedWaypointPositions[i + 1]);
            }
        }
    }
}
