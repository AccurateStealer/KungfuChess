using System;
using UnityEngine;

public class AbilityBishopBlessdouken : AbilityBase
{
    [Header("Facing / Spawn")]
    [SerializeField] private Transform _attackPoint;     
    [SerializeField] private float _projectileSpawnDistance;

    [Header("Projectile stats")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed = 12f;
    [SerializeField] private float _projectileLifetime = 3f;
    [SerializeField] private float _ProjectileDamage = 10f;
    [SerializeField] private float _ProjectileKnockback = 10f;
    [SerializeField] private bool _destroyedOnHit = true;
    [SerializeField] private OwnerInfo _ownerInfo;

    private bool _fired;

    private void Awake()
    {
        if (_ownerInfo == null)
        {
            _ownerInfo = GetComponent<OwnerInfo>();
        }
    }

    protected override void OnStart()
    {
        _fired = false;
    }

    protected override void OnActiveStart()
    {
        FireOnce();
    }

    private void FireOnce()
    {
        if (_fired) return;
        _fired = true;

        if (_projectilePrefab == null)
        {
            Debug.LogWarning($"{name}: No projectile prefab assigned.");
            return;
        }

        if (_attackPoint == null)
        {
            Debug.LogWarning($"{name}: No attackPoint assigned.");
            return;
        }

        Vector2 facing = (Vector2)(_attackPoint.position - transform.position);
        Vector2 direction = GetSnappedDiagonal(facing);

        Vector3 spawnPosition = (Vector2)transform.position + (direction * _projectileSpawnDistance);

        GameObject projectile = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);

        //if (projectile.TryGetComponent(out BlessdoukenProjectile mover))
        //{
        //    mover.Initialize(direction, _projectileSpeed, _projectileLifetime);
        //}
        if (projectile.TryGetComponent(out Rigidbody2D rigidBody))
        {
            rigidBody.linearVelocity = direction * _projectileSpeed;
        }

        AttackHitBox projectileHitbox = projectile.GetComponent<AttackHitBox>();
        projectileHitbox.Initialize(
            _ProjectileDamage,
            _ProjectileKnockback,
            _projectileLifetime,
            _destroyedOnHit,
            _ownerInfo
            );


    }

    private Vector2 GetSnappedDiagonal(Vector2 facing)
    {
        if (facing.sqrMagnitude < 0.0001f)
        {
            facing = Vector2.up;
        }
        facing.Normalize();

        float facingX = (facing.x >= 0f) ? 1f : -1f;
        float facingY = (facing.y >= 0f) ? 1f : -1f;

        return new Vector2(facingX, facingY).normalized;
    }
}
