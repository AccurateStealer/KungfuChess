using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth;

    [Header("Events")]
    public UnityEvent<float, float> OnHealthChanged = new UnityEvent<float, float>();
    public UnityEvent OnDied = new UnityEvent();

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool _debugMode = false;
#endif



    void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage, Vector2 forceVector)
    {
        TakeDamage(damage);
        // Force logic to be implemented
    }

    public void Die()
    {
#if UNITY_EDITOR
        Debug.Log(this + "has died.");
#endif
        OnDied.Invoke();
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {

        if (!Application.isPlaying && !_debugMode)
        {
            return;
        }

        UnityEditor.Handles.color = Color.lawnGreen;
        Vector3 position = transform.position;
        string healthText = $"{_currentHealth:0}/{_maxHealth:0}";
        UnityEditor.Handles.Label(position, healthText);
    }
#endif
}
