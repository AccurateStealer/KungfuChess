using Unity.VisualScripting;
using UnityEngine;

public class PrimaryAttackAbility : AbilityBase
{
    [Header("Hitbox Prefabs")]
    [SerializeField] private AttackHitBox _lightAttackHitboxPrefab;
    [SerializeField] private AttackHitBox _finisherAttackHitboxPrefab;

    [Header("Spawn Point")]
    [SerializeField] private Transform _attackPoint;

    [Header("Owner")]
    [SerializeField] private OwnerInfo _ownerInfo;

    [Header("Light Attack")]
    [SerializeField] private float _lightDamage = 10f;
    [SerializeField] private float _lightKnockback = 8f;
    [SerializeField] private float _lightHitboxLife = 0.10f;
    [SerializeField] private float _lightLungeImpulse = 4f;
    [SerializeField] private float _lightRecovery = 0.18f;

    [Header("Finisher (3rd hit)")]
    [SerializeField] private float _finisherDamage = 18f;
    [SerializeField] private float _finisherKnockback = 14f;
    [SerializeField] private float _finisherHitboxLife = 0.14f;
    [SerializeField] private float _finisherLungeImpulse = 6f;
    [SerializeField] private float _finisherRecovery = 0.30f;

    [Header("Combo")]
    [SerializeField] private float _comboWindow = 0.5f;

    private int _comboCount = 0;
    private float _comboExpireTime = 0f;
    private bool _isFinisher = false;

    protected override void Awake()
    {
        base.Awake();

        _cooldown = 0f;

        if (_ownerInfo == null)
        {
            _ownerInfo = GetComponent<OwnerInfo>();
        }
    }

    public override bool TryUse()
    {
        if (!IsReady) return false;

        if (Time.time > _comboExpireTime)
        {
            _comboCount = 0;
        }

        _comboCount++;
        _comboExpireTime = Time.time + _comboWindow;

        _isFinisher = (_comboCount >= 3);

        _recovery = _isFinisher ? _finisherRecovery : _lightRecovery;

        if (_isFinisher)
        {
            _comboCount = 0;
        }

        return base.TryUse();
    }

    protected override void OnActiveStart()
    {
        base.OnActiveStart();

        Vector2 forward = transform.right;

        if (_rigidBody != null)
        {
            float impulse = _isFinisher ? _finisherLungeImpulse : _lightLungeImpulse;
            MouseTrackDebug mover = GetComponent<MouseTrackDebug>();
            if (mover != null)
            {
                mover.AddExternalImpulse((Vector2)transform.right * impulse);
            }
            else
            {
                _rigidBody.AddForce((Vector2)transform.right * impulse, ForceMode2D.Impulse);
            }


        }

        AttackHitBox prefab = _isFinisher ? _finisherAttackHitboxPrefab : _lightAttackHitboxPrefab;
        if (prefab == null || _attackPoint == null) return;

        AttackHitBox hitBox = Instantiate(prefab, _attackPoint.position, _attackPoint.rotation, _attackPoint);

        float damage = _isFinisher ? _finisherDamage : _lightDamage;
        float KnockBack = _isFinisher ? _finisherKnockback : _lightKnockback;
        float abilityLifeTime = _isFinisher ? _finisherHitboxLife : _lightHitboxLife;

        hitBox.Initialize(damage, KnockBack, abilityLifeTime, destroyOnHitting: false, ownerInfo: _ownerInfo);
    }
}
