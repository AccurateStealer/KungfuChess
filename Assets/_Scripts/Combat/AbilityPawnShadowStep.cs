using UnityEngine;
using DG.Tweening;

public class AbilityPawnShadowStep : AbilityBase
{
    [Header("Direction Source")]
    [SerializeField] private Transform _attackPoint;

    [Header("Step Settings")]
    [SerializeField] private float _stepDistance = 1.8f;
    [SerializeField] private float _travelTime = 0.06f;
    [SerializeField] private float _cooldownTime = 0.6f;

    [Header("Landing Hit")]
    [SerializeField] private float _landingRadius = 0.45f;
    [SerializeField] private float _damage = 12f;
    [SerializeField] private float _knockback = 10f;
    [SerializeField] private LayerMask _damageableMask = ~0;

    [Header("Owner")]
    [SerializeField] private OwnerInfo _ownerInfo;

    private Tween _moveTween;
    private Vector2 _targetPos;

    protected override void Awake()
    {
        base.Awake();

        _cooldown = _cooldownTime;

        _startup = 0f;
        _active = _travelTime;

        if (_ownerInfo == null) _ownerInfo = GetComponent<OwnerInfo>();
    }

    protected override void OnActiveStart()
    {
        if (_rigidBody == null) return;

        _targetPos = CalculateTargetPosition();

        _rigidBody.linearVelocity = Vector2.zero;

        _moveTween?.Kill();
        _moveTween = _rigidBody.DOMove(_targetPos, _travelTime).SetEase(Ease.OutQuad);
    }

    private Vector2 CalculateTargetPosition()
    {
        Vector2 facingDirection = Vector2.right;
        if (_attackPoint != null)
        {
            Vector2 delta = (Vector2)_attackPoint.position - (Vector2)transform.position;
            if (delta.sqrMagnitude > 0.0001f) facingDirection = delta.normalized;
        }

        float xDirection = facingDirection.x >= 0f ? 1f : -1f;
        float yDirection = facingDirection.y >= 0f ? 1f : -1f;
        Vector2 diagonalDirection = new Vector2(xDirection, yDirection).normalized;

        Vector2 startPosition = _rigidBody.position;
        Vector2 newTargetPosition = startPosition + diagonalDirection * _stepDistance;
        return newTargetPosition;
    }

    protected override void OnActiveEnd()
    {
        if (_rigidBody != null)
        {
            _moveTween?.Kill();
            _rigidBody.position = _targetPos;
            _rigidBody.linearVelocity = Vector2.zero;
        }

        TryLandingHit();
    }

    protected override void OnEnd()
    {
        _moveTween?.Kill();
    }

    private void TryLandingHit()
    {
        Collider2D[] landedHits = Physics2D.OverlapCircleAll(_targetPos, _landingRadius, _damageableMask);

        for (int i = 0; i < landedHits.Length; i++)
        {
            Collider2D collider = landedHits[i];
            if (collider == null) continue;

            IDamageable damage = collider.GetComponentInParent<IDamageable>();
            if (damage == null) continue;

            OwnerInfo otherOwner = collider.GetComponentInParent<OwnerInfo>();
            if (_ownerInfo != null && otherOwner != null && otherOwner.OwnerID == _ownerInfo.OwnerID) continue;

            MonoBehaviour damageScript = damage as MonoBehaviour;
            Vector2 targetCenter = damageScript != null ? (Vector2)damageScript.transform.position : (Vector2)collider.transform.position;

            Vector2 direction = (targetCenter - (Vector2)transform.position);
            if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;
            direction.Normalize();

            Vector2 impulse = direction * _knockback;
            damage.TakeDamage(_damage, impulse);

            break;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)CalculateTargetPosition() : transform.position, _landingRadius);
    }
#endif
}
