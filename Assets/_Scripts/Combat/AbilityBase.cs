using System.Collections;
using UnityEngine;

public enum AbilityState 
{ 
    Ready, 
    Startup, 
    Active, 
    Recovery, 
    Cooldown 
}


public abstract class AbilityBase : MonoBehaviour
{

    [Header("Ability Timings")]
    [SerializeField] protected float _startup = 0.05f;
    [SerializeField] protected float _active = 0.10f;
    [SerializeField] protected float _recovery = 0.15f;
    [SerializeField] protected float _cooldown = 0.50f;

    [Header("Things locked During Ability")]
    [SerializeField] protected bool _lockMove = true;
    [SerializeField] protected bool _lockAttack = true;

    protected ActionLocks _actionLocks;
    protected Rigidbody2D _rigidBody;

    public AbilityState CurrentState { get; private set; } = AbilityState.Ready;
    public bool IsReady => CurrentState == AbilityState.Ready;

    // By default this make active "frames" imeadiate if _active set to 0 or negative by leaving this,
    // but this can be overridden to allow abilities which are active till a certain conditions.
    // For example, a charge ability which is perpetually active until hitting a wall.
    protected virtual bool ShouldEndActive() => true;

    Coroutine _abilityCoroutine;
    float _currentCooldown;

    protected virtual void Awake()
    {
        _actionLocks = GetComponent<ActionLocks>();
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    public virtual bool TryUse()
    {
        if (!IsReady) return false;

        if (Time.time < _currentCooldown) return false;

        _abilityCoroutine = StartCoroutine(Run());
        return true;
    }

    IEnumerator Run()
    {
        SetState(AbilityState.Startup);
        ApplyLocks(true);
        OnStart();

        if (_startup > 0)
        {
            yield return new WaitForSeconds(_startup);
        }

        SetState(AbilityState.Active);
        OnActiveStart();

        if (_active > 0)
        {
            yield return new WaitForSeconds(_active);
        }
        else
        {
            while (!ShouldEndActive())
            {
                yield return null;
            }
        }

        SetState(AbilityState.Recovery);
        OnActiveEnd();

        if (_recovery > 0)
        {
            yield return new WaitForSeconds(_recovery);
        }

        ApplyLocks(false);
        OnEnd();

        SetState(AbilityState.Cooldown);
        _currentCooldown = Time.time + _cooldown;

        if (_cooldown > 0f)
        {
            yield return new WaitForSeconds(_cooldown);
        }

        SetState(AbilityState.Ready);
        _abilityCoroutine = null;
    }

    void ApplyLocks(bool isLocked)
    {
        if (_actionLocks == null) return;

        if (isLocked)
        {
            if (_lockMove)
            {
                _actionLocks.LockMove();
            } 
            if (_lockAttack)
            {
                _actionLocks.LockAttack();
            }
        }
        else
        {
            if (_lockMove)
            {
                _actionLocks.UnlockMove();
            }
            if (_lockAttack)
            {
                _actionLocks.UnlockAttack();
            }
        }
    }

    void SetState(AbilityState abilityState) => CurrentState = abilityState;

    protected virtual void OnStart() { }
    protected virtual void OnActiveStart() { }
    protected virtual void OnActiveEnd() { }
    protected virtual void OnEnd() { }

    public float CooldownRemaining()
    {
        return Mathf.Max(0, _currentCooldown - Time.time);
    }
}
