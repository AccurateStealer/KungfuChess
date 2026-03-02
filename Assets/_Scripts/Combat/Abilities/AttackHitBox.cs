using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackHitBox : MonoBehaviour
{
    [SerializeField] public float _damage = 1f;
    [SerializeField] public float _knockback = 0f;
    [SerializeField] public float _lifeTime = 1000f;
    [SerializeField] public bool destroyOnHitting = false;
    [SerializeField] public bool _knockBackFromOwnerCenter = false;

    private OwnerInfo _ownerInfo;
    private readonly HashSet<GameObject> _previouslyHitObjects = new HashSet<GameObject>();


    public void Initialize(float damage, float knockback, float lifeTime, bool destroyOnHitting, OwnerInfo ownerInfo)
    {
        _damage = damage;
        _knockback = knockback;
        _lifeTime = lifeTime;
        this.destroyOnHitting = destroyOnHitting;
        _ownerInfo = ownerInfo;


        _previouslyHitObjects.Clear();
        CancelInvoke(nameof(DestroySelf));
        Invoke(nameof(DestroySelf), _lifeTime);
    }

    private void Awake()
    {
        Invoke(nameof(DestroySelf), _lifeTime);

        if (GetComponent<Collider2D>().isTrigger == false)
        {
            Debug.LogWarning("Collider2D should be set as trigger.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponentInParent<IDamageable>();
        if (damageable == null)
        {
            return;
        }

        //OwnerInfo ownerInfo = collision.GetComponent<OwnerInfo>();
        //if (ownerInfo.OwnerID == _ownerInfo.OwnerID)
        //{
        //    return;
        //}

        if (_previouslyHitObjects.Contains(collision.gameObject))
        {
            return;
        }

        Vector2 knockbackDirection;
        Vector2 knockbackForce;

        if (_knockBackFromOwnerCenter)
        {
            knockbackDirection = (collision.transform.position - _ownerInfo.transform.position).normalized;
            knockbackForce = knockbackDirection * _knockback;
        }
        else
        {
            knockbackDirection = (collision.transform.position - transform.position).normalized;
            knockbackForce = knockbackDirection * _knockback;
        }
        

        damageable.TakeDamage(_damage, knockbackForce);

        _previouslyHitObjects.Add(collision.gameObject);

        if (destroyOnHitting)
        {
            DestroySelf();
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
