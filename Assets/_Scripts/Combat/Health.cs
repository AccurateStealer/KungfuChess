using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth;

    [Header("Hit VFX Settings")]
    [SerializeField] private GameObject _hitVFXPrefab;

    [Header("I-Frames Settings")]
    [SerializeField, Range(0, 10)] private float _iFramesDuration = 0.2f;
    private float _iFramesTimer = 0f;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private Tween _iFramesTween;

    [Header("Knockback Settings")]
    // [SerializeField] private float _knockBackResistance = 20f;
    [SerializeField] private float _knockBackMaxSpeed = 25f;
    // private Vector2 _currentKnockBackVelocity = Vector2.zero;


    [Header("Events")]
    public UnityEvent<float, float> OnHealthChanged = new UnityEvent<float, float>();
    public UnityEvent OnDied = new UnityEvent();

    private Rigidbody2D _rigidbody2D;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public bool IsInvulnerable => _iFramesTimer > 0f;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool _debugMode = false;
#endif

    void Awake()
    {
        _currentHealth = _maxHealth;

        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        _iFramesTimer -= Time.fixedDeltaTime;

        // _currentKnockBackVelocity = Vector2.Lerp(_currentKnockBackVelocity, Vector2.zero, 1f - Mathf.Exp(-_knockBackResistance * Time.fixedDeltaTime));
    }

    public void TakeDamage(float damage)
    {
        if (IsInvulnerable)
        {
            return;
        }

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged.Invoke(_currentHealth, _maxHealth);

        if (_hitVFXPrefab != null)
        {
            Instantiate(_hitVFXPrefab, transform.position, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartIFrames();
        }
    }

    public void TakeDamage(float damage, Vector2 forceVector)
    {
        TakeDamage(damage);

        if (_rigidbody2D != null)
        {
            Vector2 impulse = Vector2.ClampMagnitude(forceVector, _knockBackMaxSpeed);
            _rigidbody2D.linearVelocity += impulse;
        }
        // _currentKnockBackVelocity += forceVector;
    }

    public void StartIFrames()
    {
        _iFramesTimer = Mathf.Max(_iFramesTimer, _iFramesDuration);

        if (_spriteRenderer != null)
        {
            _iFramesTween?.Kill();

            _iFramesTween = _spriteRenderer
                .DOFade(0.5f, 0.08f)
                .SetLoops(Mathf.CeilToInt(_iFramesDuration / 0.1f) * 2, LoopType.Yoyo)
                .OnKill(() => {_spriteRenderer.color = new Color(1, 1, 1, 1); }); ;
        }
    }

    public void Die()
    {
#if UNITY_EDITOR
        Debug.Log(this + "has died.");
#endif
        OnDied.Invoke();
    }

    private void OnDisable()
    {
        _iFramesTween?.Kill();
    }

    // Context menu debug functions for testing
    [ContextMenu("Test StartIFrames")]
    private void TestStartIFrames()
    {
        StartIFrames();
    }



#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_debugMode)
        {
            return;
        }

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.lawnGreen;
        style.fontSize = 24;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Bold;

        Vector3 position = transform.position;
        string healthText = $"{_currentHealth:0}/{_maxHealth:0}";
        UnityEditor.Handles.Label(position, healthText, style);
    }
#endif
}
