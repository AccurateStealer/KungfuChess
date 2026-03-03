using System.Collections;
using UnityEngine;

public class AbilityRookIronFortressAssault : AbilityBase
{
    [Header("Forward Hitbox")]
    [SerializeField] private AttackHitBox _hitboxPrefab;
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private OwnerInfo _ownerInfo;

    [Header("Damage")]
    [SerializeField] private float _minDamage = 12f;
    [SerializeField] private float _maxDamage = 28f;
    [SerializeField] private float _minKnockback = 10f;
    [SerializeField] private float _maxKnockback = 60f;

    [Header("Charge / Movement")]
    [SerializeField] private float _maxChargeDuration = 1f;
    [SerializeField] private float _startSpeed = 4f;
    [SerializeField] private float _maxSpeed = 18f;
    [SerializeField] private float _acceleration = 80f;

    [Header("Hitbox Growth")]
    [Tooltip("Local forward offset at minimum charge.")]
    [SerializeField] private float _minForwardOffset = 0.6f;
    [Tooltip("Local forward offset at maximum charge.")]
    [SerializeField] private float _maxForwardOffset = 1.6f;
    [Tooltip("BoxCollider2D size at minimum charge.")]
    [SerializeField] private Vector2 _minHitboxSize = new Vector2(1.1f, 1.1f);
    [Tooltip("BoxCollider2D size at maximum charge.")]
    [SerializeField] private Vector2 _maxHitboxSize = new Vector2(2.4f, 1.6f);

    [Header("Collision / Stop")]
    [SerializeField] private LayerMask _solidMask;
    [Tooltip("Extra distance to check in front of the player each frame.")]
    [SerializeField] private float _wallCheckDistance = 0.35f;

    [Header("Invulnerability")]
    [SerializeField] private bool _beInvulnerableDuringStartup = true;
    [SerializeField] private bool _beInvulnerableDuringActive = true;

    [Header("VFX")]
    [SerializeField] private GameObject _chargingVFX;
    [SerializeField] private GameObject _airRingVFX;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _startUpShakeStrength = 0f;
    [SerializeField] private float _EndShakeStrength = 0f;

    private GameObject _spawnedChargingVFX;
    private GameObject _spawnedAirRingVFX;

    private AttackHitBox _spawnedHitbox;
    private BoxCollider2D _spawnedCollider;

    private Vector2 _chargeDirection;
    private float _elapsedActive;
    private float _currentSpeed;
    private bool _shouldEndActive = false;

    protected override void Awake()
    {
        base.Awake();

        _active = 0f;

        if (_ownerInfo == null)
        {
            _ownerInfo = GetComponent<OwnerInfo>();
        }
    }

    protected override void OnStart()
    {
        _shouldEndActive = false;
        _elapsedActive = 0f;
        _currentSpeed = 0f;

        _chargeDirection = GetSnappedCardinalDirection();

        _spawnedChargingVFX = Instantiate(_chargingVFX, transform);

        StartCoroutine(ShakePlayer(_startup, _startUpShakeStrength));

        if (_beInvulnerableDuringStartup)
        {
            SetInvulnerable(true);
        }
    }

    private IEnumerator ShakePlayer(float duration, float shakeStrength)
    {
        if (_spriteRenderer == null)
        {
            yield break;
        }

        Transform spriteTransform = _spriteRenderer.transform;
        Vector3 startLocalPosition = spriteTransform.localPosition;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-shakeStrength, shakeStrength);
            float y = Random.Range(-shakeStrength, shakeStrength);

            spriteTransform.localPosition = startLocalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteTransform.localPosition = startLocalPosition;
    }

    protected override void OnActiveStart()
    {
        if (_beInvulnerableDuringActive)
        {
            SetInvulnerable(true);
        }

        _currentSpeed = _startSpeed;

        _spawnedAirRingVFX = Instantiate(_airRingVFX, transform);

        SpawnOrResetHitbox();
    }

    protected override bool ShouldEndActive()
    {
        if (_shouldEndActive) return true;

        _elapsedActive += Time.deltaTime;

        if (_elapsedActive >= _maxChargeDuration)
        {
            _shouldEndActive = true;
            return true;
        }

        TickMovement();
        TickHitbox();

        if (IsWallInFront())
        {
            _shouldEndActive = true;
            StopMovement();
            return true;
        }

        return false;
    }

    protected override void OnActiveEnd()
    {
        StopMovement();

        if (_spawnedHitbox != null)
        {
            Destroy(_spawnedHitbox.gameObject);
            _spawnedHitbox = null;
            _spawnedCollider = null;
        }

        StartCoroutine(ShakePlayer(_startup, _EndShakeStrength));

        SetInvulnerable(false);
    }

    protected override void OnEnd()
    {
        base.OnEnd();

        ParticleSystem[] particleSystems = _spawnedChargingVFX.GetComponentsInChildren<ParticleSystem>();
        ParticleSystem[] particleSystemsAirRing = _spawnedAirRingVFX.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem particle in particleSystems)
        {
            particle.Stop();
        }

        foreach (ParticleSystem particle in particleSystemsAirRing)
        {
            particle.Stop();
        }

        SetInvulnerable(false);
    }

    private Vector2 GetSnappedCardinalDirection()
    {
        Vector2 rawDirection = Vector2.zero;

        if (_attackPoint != null)
        {
            Vector2 delta = (Vector2)_attackPoint.position - (Vector2)transform.position;
            if (delta.sqrMagnitude > 0.0001f)
            {
                rawDirection = delta.normalized;
            }
            else
            {
                rawDirection = _attackPoint.right;
            }
        }
        else if (_rigidBody != null && _rigidBody.linearVelocity.sqrMagnitude > 0.0001f)
        {
            rawDirection = _rigidBody.linearVelocity.normalized;
        }
        else
        {
            rawDirection = Vector2.right;
        }

        if (Mathf.Abs(rawDirection.x) >= Mathf.Abs(rawDirection.y))
        {
            return (rawDirection.x >= 0f) ? Vector2.right : Vector2.left;
        }

        return (rawDirection.y >= 0f) ? Vector2.up : Vector2.down;
    }

    private void TickMovement()
    {
        if (_rigidBody == null) return;

        _currentSpeed = Mathf.MoveTowards(_currentSpeed, _maxSpeed, _acceleration * Time.deltaTime);

        Vector2 desiredVelocity = _chargeDirection * _currentSpeed;

        Vector2 velocityDelta = desiredVelocity - _rigidBody.linearVelocity;
        _rigidBody.AddForce(velocityDelta, ForceMode2D.Force);

    }

    private void StopMovement()
    {
        if (_rigidBody == null) return;

        _rigidBody.linearVelocity = Vector2.zero;
    }

    private bool IsWallInFront()
    {
        if (_rigidBody == null) return false;

        Vector2 origin = _rigidBody.position;
        float checkDistance = _wallCheckDistance;

        if (_spawnedHitbox != null)
        {
            checkDistance = Mathf.Max(checkDistance, _maxForwardOffset * 0.75f);
        }

        RaycastHit2D hit = Physics2D.Raycast(origin, _chargeDirection, checkDistance, _solidMask);
        return hit.collider != null;
    }


    private void SpawnOrResetHitbox()
    {
        if (_hitboxPrefab == null) return;

        if (_spawnedHitbox != null)
        {
            Destroy(_spawnedHitbox.gameObject);
        }


        _spawnedHitbox = Instantiate(_hitboxPrefab, transform.position, transform.rotation, transform);
        _spawnedCollider = _spawnedHitbox.GetComponent<BoxCollider2D>();

        _spawnedHitbox.Initialize(_minDamage, _minKnockback, lifeTime: 9999f, destroyOnHitting: false, ownerInfo: _ownerInfo);
        _spawnedHitbox._knockBackFromOwnerCenter = true;

        TickHitbox();
    }

    private void TickHitbox()
    {
        if (_spawnedHitbox == null) return;

        float chargeTime = Mathf.Clamp01(_elapsedActive / Mathf.Max(0.0001f, _maxChargeDuration));

        float damage = Mathf.Lerp(_minDamage, _maxDamage, chargeTime);
        _spawnedHitbox._damage = damage;
        float knockback = Mathf.Lerp(_minKnockback, _maxKnockback, chargeTime);
        _spawnedHitbox._knockback = knockback;

        float angle = Mathf.Atan2(_chargeDirection.y, _chargeDirection.x) * Mathf.Rad2Deg;
        _spawnedHitbox.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (_spawnedCollider != null)
        {
            Vector2 size = Vector2.Lerp(_minHitboxSize, _maxHitboxSize, chargeTime);
            _spawnedCollider.size = size;
        }
    }

    private void SetInvulnerable(bool value)
    {
        // SET UP LATER
    }
}