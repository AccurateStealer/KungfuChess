using UnityEngine;

public class AbilityKnightSkySplit : AbilityBase
{
    [Header("Facing Source (like Pawn)")]
    [SerializeField] private Transform _attackPoint;

    [Header("Clone Visual Source")]
    [SerializeField] private SpriteRenderer _sourceRenderer;

    [Header("Knight Offsets")]
    [SerializeField] private float _unitSize = 1f;
    [SerializeField] private int _longLegUnits = 2;
    [SerializeField] private int _shortLegUnits = 1;

    [Header("Scatter (optional)")]
    [SerializeField] private float _scatterRadius = 0.10f;

    [Header("Timing")]
    [SerializeField] private float _riseTime = 0.06f;
    [SerializeField] private float _travelTime = 0.10f;
    [SerializeField] private float _fallTime = 0.06f;

    [Header("Air Height (purely visual)")]
    [SerializeField] private float _airHeight = 0.30f;

    [Header("Cooldown")]
    [SerializeField] private float _cooldownTime = 0.9f;

    [Header("Landing Hit")]
    [SerializeField] private float _landingRadius = 0.55f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _knockback = 10f;
    [SerializeField] private LayerMask _damageableMask = ~0;

    [Header("Owner")]
    [SerializeField] private OwnerInfo _ownerInfo;

    [Header("VFX")]
    [SerializeField] private GameObject _takeoffPuffPrefab;
    [SerializeField] private GameObject _landingPuffPrefab;
    [SerializeField] private GameObject _disapearPuffPrefab;

    [Header("Clone")]
    [SerializeField] private GameObject _shadowClone;
    [SerializeField] private Color _cloneTint = new Color(0f, 0f, 0f, 0.55f);
    [SerializeField] private float _cloneLingerTime = 0.35f;
    [SerializeField] private float _cloneFadeOutTime = 0.15f;

    protected override void Awake()
    {
        base.Awake();

        _cooldown = _cooldownTime;
        _startup = 0f;
        _active = _riseTime + _travelTime + _fallTime;
        _recovery = 0f;

        if (_ownerInfo == null)
        {
            _ownerInfo = GetComponent<OwnerInfo>();
        }
    }

    protected override void OnActiveStart()
    {
        if (_rigidBody == null) return;
        if (_sourceRenderer == null) return;

        Vector2 start = _rigidBody.position;

        GetTwoKnightTargets(start, out Vector2 target1, out Vector2 target2);

        SpawnClone(start, target1);
        SpawnClone(start, target2);
    }

    private void SpawnClone(Vector2 startPosition, Vector2 endPosition)
    {
        GameObject shadowCloneGameObject = Instantiate(_shadowClone);
        shadowCloneGameObject.transform.position = startPosition;

        KnightShadowClone clone = shadowCloneGameObject.GetComponent<KnightShadowClone>();

        SpriteRenderer spriteRenderer = clone.SpriteRenderer;
        spriteRenderer.sortingLayerID = _sourceRenderer.sortingLayerID;
        spriteRenderer.sortingOrder = _sourceRenderer.sortingOrder - 1;
        spriteRenderer.flipX = _sourceRenderer.flipX;
        spriteRenderer.flipY = _sourceRenderer.flipY;


        clone.Init(
            _sourceRenderer.sprite,
            _sourceRenderer.sharedMaterial,
            _cloneTint,
            startPosition,
            endPosition,
            _airHeight,
            _riseTime,
            _travelTime,
            _fallTime,
            _landingRadius,
            _damage,
            _knockback,
            _damageableMask,
            _ownerInfo,
            _takeoffPuffPrefab,
            _landingPuffPrefab,
            _disapearPuffPrefab,
            _cloneLingerTime,
            _cloneFadeOutTime
        );
    }

    private void GetTwoKnightTargets(Vector2 start, out Vector2 target1, out Vector2 target2)
    {
        Vector2 facing = GetSnappedCardinalFacing();

        float longLeg = _longLegUnits * _unitSize;
        float shortLeg = _shortLegUnits * _unitSize;

        if (facing == Vector2.up)
        {
            target1 = start + new Vector2(+shortLeg, +longLeg);
            target2 = start + new Vector2(-shortLeg, +longLeg);
        }
        else if (facing == Vector2.down)
        {
            target1 = start + new Vector2(+shortLeg, -longLeg);
            target2 = start + new Vector2(-shortLeg, -longLeg);
        }
        else if (facing == Vector2.right)
        {
            target1 = start + new Vector2(+longLeg, +shortLeg);
            target2 = start + new Vector2(+longLeg, -shortLeg);
        }
        else // left
        {
            target1 = start + new Vector2(-longLeg, +shortLeg);
            target2 = start + new Vector2(-longLeg, -shortLeg);
        }

        if (_scatterRadius > 0f)
        {
            target1 += Random.insideUnitCircle * _scatterRadius;
            target2 += Random.insideUnitCircle * _scatterRadius;
        }
    }

    private Vector2 GetSnappedCardinalFacing()
    {
        Vector2 direction = Vector2.right;

        if (_attackPoint != null)
        {
            Vector2 delta = (Vector2)_attackPoint.position - (Vector2)transform.position;
            if (delta.sqrMagnitude > 0.0001f)
            {
                direction = delta.normalized;
            }
        }

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            return direction.x >= 0f ? Vector2.right : Vector2.left;
        }
        else
        {
            return direction.y >= 0f ? Vector2.up : Vector2.down;
        }
    }
}