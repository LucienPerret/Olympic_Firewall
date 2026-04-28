using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData data;
    public TowerData Data => data;

    private CircleCollider2D _circleCollider;
    private Animator _animator;

    private List<Enemy> _enemiesInRange;
    private ObejctPooler _projectilePool;
    private Transform _projectileOrigin;

    private float _damageTimer;
    private bool _active = false;

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
        TowerPlacement.OnPlacementConfirmed += ActivateTower;
    }
    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
        TowerPlacement.OnPlacementConfirmed -= ActivateTower;
    }

    private void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _animator = GetComponent<Animator>();
        _circleCollider.radius = data.range;
        _enemiesInRange = new List<Enemy>();
        _damageTimer = data.damageInterval;
        _projectileOrigin = transform.Find("ShootOrigin");
        if (data.targetType == TargetType.Single ||
            data.targetType == TargetType.Multi)
        {
            _projectilePool = GetComponent<ObejctPooler>();
        }
        
        
    }

    private void Update()
    {
        _damageTimer -= Time.deltaTime;

        if (!_active) { return; };

        // Not sure if this needs to run every Update
        _enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
        if (_enemiesInRange.Count == 0) 
        {
            PlayNothing();
            return;
        }

        if ( _damageTimer <= 0 )
        {
            _damageTimer = data.damageInterval;
            DealDamage(data.targetType);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, data.range);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            _enemiesInRange.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Remove(enemy);
            }
        }

    }

    private void DealDamage(TargetType towerType)
    {
        Debug.Log("DealingDamage");
        switch (towerType)
        {
            case TargetType.Single:
                StartAnimation();
                //Shoot(); need a fix for enemies that die 
                break;
            case TargetType.Multi:
                Shoot();
                break;
            case TargetType.Area:
                AreaDamage();
                break;
            default:
                Debug.Log("Tower Type not assigned");
                break;
        }
    }

    private void StartAnimation()
    {
        Enemy target = GetTargetForShot();
        if (target == null)
        {
            PlayNothing();
            return;
        }

        if (_animator == null)
        {
            Shoot();
            return;
        }

        _animator.SetTrigger("Shoot");
    }

    public void Shoot()
    {
        Enemy target = GetTargetForShot();
        if (target == null)
        {
            PlayNothing();
            return;
        }

        if (_projectilePool == null || _projectileOrigin == null)
        {
            return;
        }

        GameObject projectile = _projectilePool.GetPooledObject();
        if (projectile == null)
        {
            return;
        }

        projectile.transform.position = _projectileOrigin.position;

        Vector2 shootDirection = (GetTargetPosition(target) - _projectileOrigin.position).normalized;
        Rotate(shootDirection);

        // Recalculate after rotating so off-center shoot origins still point at the live target.
        shootDirection = (GetTargetPosition(target) - _projectileOrigin.position).normalized;
        projectile.SetActive(true);
        projectile.GetComponent<Projectile>().Shoot(data, shootDirection);
    }

    private void AreaDamage()
    {
        _enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);

        if (_enemiesInRange.Count > 0)
        {
            for (int i = 0; i < _enemiesInRange.Count; i++)
            {
                _enemiesInRange[i].TakeDamage(data.damage);
            }
        }

    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        _enemiesInRange.Remove(enemy);
    }

    private void Rotate(Vector2 direction)
    {
        // Rotate so that local Y (up) aligns with shootDir
        Quaternion rotation = Quaternion.FromToRotation(transform.up, direction);

        // Apply it
        transform.rotation = rotation * transform.rotation;
    }

    //TowerData not used
    private void ActivateTower(TowerData towerType)
    {
        _active = true;
    }

    private void PlayNothing()
    {
        // no clean fix
        if (_animator == null) { return; }
        _animator.Play("Idle");
    }

    private Enemy GetValidTarget()
    {
        _enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);

        if (_enemiesInRange.Count == 0)
            return null;

        return _enemiesInRange[0];
    }

    private Enemy GetTargetForShot()
    {
        if (data.targetType != TargetType.Single)
        {
            return GetValidTarget();
        }

        return GetLeadingTarget();
    }

    private Enemy GetLeadingTarget()
    {
        _enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
        if (_enemiesInRange.Count == 0)
        {
            return null;
        }

        Enemy selectedTarget = _enemiesInRange[0];
        float highestProgress = selectedTarget.GetPathProgress();

        for (int i = 1; i < _enemiesInRange.Count; i++)
        {
            Enemy currentEnemy = _enemiesInRange[i];
            float currentProgress = currentEnemy.GetPathProgress();

            if (currentProgress > highestProgress)
            {
                selectedTarget = currentEnemy;
                highestProgress = currentProgress;
            }
        }

        return selectedTarget;
    }

    private Vector3 GetTargetPosition(Enemy target)
    {
        Collider2D targetCollider = target.GetComponent<Collider2D>();
        if (targetCollider != null)
        {
            return targetCollider.bounds.center;
        }

        return target.transform.position;
    }


}
