using UnityEngine;
using MPUIKIT;

public class HealthBarView : MonoBehaviour
{
    [Header("Which player this UI belongs to")]
    [SerializeField] private int _playerId = 1;

    [Header("UI Refs")]
    [Tooltip("The bar fill. Works with Unity Image, and usually MPUI images if they derive from Image.")]
    [SerializeField] private MPImage _fillImage;

    private Health _boundHealth;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPieceSpawned += HandlePieceSpawned;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPieceSpawned -= HandlePieceSpawned;
        }

        Unbind();
    }

    private void HandlePieceSpawned(int ownerId, GameObject piece)
    {
        if (ownerId != _playerId) return;

        Health health = piece.GetComponent<Health>();
        if (health == null) return;

        Bind(health);
    }

    private void Update()
    {
        if (_boundHealth == null)
        {
            TryBindExistingPiece();
        }
    }

    private void TryBindExistingPiece()
    {
        Health[] allHealth = FindObjectsByType<Health>(FindObjectsSortMode.None);

        for (int i = 0; i < allHealth.Length; i++)
        {
            Health health = allHealth[i];
            if (health == null) continue;

            OwnerInfo owner = health.GetComponent<OwnerInfo>();
            if (owner != null && owner.OwnerID == _playerId)
            {
                Bind(health);
                return;
            }
        }
    }

    private void Bind(Health health)
    {
        Debug.Log($"Binding health bar to player {health.gameObject.name} with health {health.CurrentHealth}/{health.MaxHealth}");
        Unbind();

        _boundHealth = health;

        _boundHealth.OnHealthChanged.AddListener(OnHealthChanged);


        OnHealthChanged(_boundHealth.CurrentHealth, _boundHealth.MaxHealth);
    }

    private void Unbind()
    {
        if (_boundHealth != null)
        {
            _boundHealth.OnHealthChanged.RemoveListener(OnHealthChanged);
            _boundHealth = null;
        }
    }

    private void OnHealthChanged(float current, float max)
    {
        float percentage = (max <= 0f) ? 0f : Mathf.Clamp01(current / max);

        if (_fillImage != null)
        {
            _fillImage.fillAmount = percentage;
        }

    }
}
