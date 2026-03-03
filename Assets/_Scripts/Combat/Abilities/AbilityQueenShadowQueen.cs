using System;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityQueenShadowQueen : AbilityBase
{
    [Header("Clone")]
    [SerializeField] private QueenShadowClonePuppet _clonePrefab;
    [SerializeField] private Transform _aimPoint;
    [SerializeField] private GameObject _aimRotationPoint;

    [SerializeField] private float _cloneMoveSpeed = 6f;
    [SerializeField] private float _cloneMoveDuration = 1f;

    [Header("Mirror Attack Source")]
    [SerializeField] private PrimaryAttackAbility _queenPrimary;

    private QueenShadowClonePuppet _activeClone;

    protected override void Awake()
    {
        base.Awake();

        _startup = 0f;
        _active = 0f;
        _recovery = 0f;

        if (_queenPrimary == null)
        {
            _queenPrimary = GetComponent<PrimaryAttackAbility>();
        }
    }

    private void OnEnable()
    {
        if (_queenPrimary != null)
        {
            _queenPrimary.OnPrimaryAttackStarted += HandleQueenPrimaryAttack;
        }
    }

    private void OnDisable()
    {
        if (_queenPrimary != null)
        {
            _queenPrimary.OnPrimaryAttackStarted -= HandleQueenPrimaryAttack;
        }
    }

    protected override void OnStart()
    {
        base.OnStart();

        if (_activeClone == null)
        {
            SummonClone();
        }
        else
        {
            SwapToClone();
        }
    }

    private void SummonClone()
    {
        Vector2 direction = GetAimDirection();

        _activeClone = Instantiate(_clonePrefab, transform.position, Quaternion.identity);

        _activeClone.InitMove(direction, _cloneMoveSpeed, _cloneMoveDuration, _aimRotationPoint);
    }

    private void SwapToClone()
    {
        if (_activeClone == null) return;

        transform.position = _activeClone.transform.position;

        _activeClone.Despawn();
        _activeClone = null;
    }

    private void HandleQueenPrimaryAttack(Vector2 direction, bool isFinisher)
    {
        if (_activeClone == null) return;

        _activeClone.MirrorPrimaryAttack(direction, isFinisher);
    }

    private Vector2 GetAimDirection()
    {
        if (_aimPoint != null)
        {
            Vector2 direction = (Vector2)_aimPoint.position - (Vector2)transform.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                return direction.normalized;
            }
        }
        return transform.right;
    }

}
