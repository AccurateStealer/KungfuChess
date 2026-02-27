using UnityEngine;

public interface IDamageable
{
    public void TakeDamage(float damage);
    public void TakeDamage(float damage, Vector2 forceVector);
    public void Die();
}
