using UnityEngine;

public class Projectile : MonoBehaviour
{
    // New parent so they don't turn with the tower
    public static Transform ProjectileParent;

    private TowerData _data;
    private Vector3 _shootDirection;
    private float _projectileDuration;


    private void Start()
    {
        if (Projectile.ProjectileParent == null)
        {
            Projectile.ProjectileParent = GameObject.Find("Projectiles").transform;
        }

        transform.localScale = Vector3.one * _data.projectileSize;
    }
    private void Update()
    {
        if (_projectileDuration <= 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            _projectileDuration -= Time.deltaTime;
            transform.position += _shootDirection * _data.projectileSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            enemy.TakeDamage(_data.damage);
            gameObject.SetActive(false);
        }
    }

    public void Shoot(TowerData data, Vector3 shootDirection)
    {
        _data = data;
        _shootDirection = shootDirection.normalized;
        _projectileDuration = _data.projectileDuration;

        // Change Partent Object
        transform.SetParent(ProjectileParent);

    }
}
