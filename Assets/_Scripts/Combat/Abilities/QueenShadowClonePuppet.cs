using System.Collections;
using UnityEngine;

public class QueenShadowClonePuppet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private OwnerInfo _ownerInfo;

    [Header("Hitbox Prefabs")]
    [SerializeField] private AttackHitBox _lightAttackHitboxPrefab;
    [SerializeField] private AttackHitBox _finisherAttackHitboxPrefab;

    [Header("Light Attack")]
    [SerializeField] private float _lightDamage = 10f;
    [SerializeField] private float _lightKnockback = 8f;
    [SerializeField] private float _lightHitboxLife = 0.10f;
    [SerializeField] private float _lightLungeImpulse = 4f;

    [Header("Finisher Attack")]
    [SerializeField] private float _finisherDamage = 18f;
    [SerializeField] private float _finisherKnockback = 14f;
    [SerializeField] private float _finisherHitboxLife = 0.14f;
    [SerializeField] private float _finisherLungeImpulse = 6f;

    private Coroutine _moveRoutine;
    [SerializeField] GameObject _selfTargettingPoint;
    private GameObject _targetingPoint;


    private void Awake()
    {
        if (_rigidBody == null)
        {
            _rigidBody = GetComponent<Rigidbody2D>();
        }
        if (_ownerInfo == null)
        {
            _ownerInfo = GetComponent<OwnerInfo>();
        }
    }

    public void InitMove(Vector2 direction, float speed, float duration, GameObject targettingPoint)
    {
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
        }
        _moveRoutine = StartCoroutine(MoveRoutine(direction.normalized, speed, duration));
        _targetingPoint = targettingPoint;
    }

    private IEnumerator MoveRoutine(Vector2 dir, float speed, float duration)
    {
        float movementTime = 0f;

        while (duration <= 0f || movementTime < duration)
        {
            if (_rigidBody != null)
            {
                _rigidBody.linearVelocity = dir * speed;
            }
            else
            {
                transform.position += (Vector3)(dir * speed * Time.deltaTime);
            }

            movementTime += Time.deltaTime;
            yield return null;
        }

        if (_rigidBody != null)
        {
            _rigidBody.linearVelocity = Vector2.zero;
        }
    }

    public void MirrorPrimaryAttack(Vector2 direction, bool isFinisher)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = transform.right;
        }

        direction.Normalize();

        float impulse = isFinisher ? _finisherLungeImpulse : _lightLungeImpulse;

        if (_rigidBody != null)
        {
            _rigidBody.AddForce(direction * impulse, ForceMode2D.Impulse);
        }

        AttackHitBox prefab = isFinisher ? _finisherAttackHitboxPrefab : _lightAttackHitboxPrefab;
        if (prefab == null) return;

        Vector3 spawnPosition = _attackPoint != null ? _attackPoint.position : transform.position;
        Quaternion spawnRotation = _attackPoint != null ? _attackPoint.rotation : Quaternion.identity;
        Transform parent = _attackPoint != null ? _attackPoint : transform;

        AttackHitBox hitBox = Instantiate(prefab, spawnPosition, spawnRotation, parent);

        float damage = isFinisher ? _finisherDamage : _lightDamage;
        float knock = isFinisher ? _finisherKnockback : _lightKnockback;
        float life = isFinisher ? _finisherHitboxLife : _lightHitboxLife;

        hitBox.Initialize(damage, knock, life, destroyOnHitting: false, ownerInfo: _ownerInfo);
    }

    public void Update()
    {
        _selfTargettingPoint.transform.rotation = _targetingPoint.transform.rotation;
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }
}
