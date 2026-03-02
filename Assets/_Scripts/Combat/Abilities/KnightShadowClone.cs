using UnityEngine;
using DG.Tweening;

public class KnightShadowClone : MonoBehaviour
{
    [SerializeField] public SpriteRenderer SpriteRenderer;

    public void Init(
        Sprite sprite,
        Material material,
        Color tint,
        Vector2 startPosition,
        Vector2 endPosition,
        float airHeight,
        float riseTime,
        float travelTime,
        float fallTime,

        float landingRadius,
        float damage,
        float knockback,
        LayerMask mask,
        OwnerInfo ownerInfo,

        GameObject takeoffVfx,
        GameObject landingVfx,
        GameObject disapearVfx,

        float lingerTime,
        float fadeOutTime
    )
    {
        transform.position = startPosition;

        SpriteRenderer.sprite = sprite;
        //SpriteRenderer.material = material;
        //SpriteRenderer.color = tint;

        if (takeoffVfx != null)
        {
            Instantiate(takeoffVfx, startPosition, Quaternion.identity);
        }

        Vector3 groundPosition = transform.position;


        Vector3 airStart = new Vector3(groundPosition.x - (airHeight / 4), groundPosition.y + airHeight, groundPosition.z);
        Vector3 airEnd = new Vector3(endPosition.x, endPosition.y + airHeight, groundPosition.z);
        Vector3 groundEnd = new Vector3(endPosition.x, endPosition.y, groundPosition.z);

        Sequence leapSequence = DOTween.Sequence();

        leapSequence.Append(transform.DOMove(airStart, riseTime).SetEase(Ease.OutQuad));

        leapSequence.Append(transform.DOMove(airEnd, travelTime).SetEase(Ease.OutQuad));

        leapSequence.Append(transform.DOMove(groundEnd, fallTime).SetEase(Ease.InQuad));

        leapSequence.AppendCallback(() =>
        {
            if (landingVfx != null)
            {
                Instantiate(landingVfx, endPosition, Quaternion.identity);
            }

            DoLandingHit(endPosition, landingRadius, damage, knockback, mask, ownerInfo);
        });

        if (lingerTime > 0f)
        {
            leapSequence.AppendInterval(lingerTime);
        }

        if (fadeOutTime > 0f)
        {
            leapSequence.Append(SpriteRenderer.DOFade(0f, fadeOutTime));
        }

        leapSequence.OnComplete(() =>
        {
            if (disapearVfx != null)
            {
                Instantiate(disapearVfx, endPosition, Quaternion.identity);
            }

            Destroy(gameObject);
        });
    }

    private void DoLandingHit(
        Vector2 landingPosition,
        float radius,
        float damage,
        float knockback,
        LayerMask mask,
        OwnerInfo ownerInfo
    )
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(landingPosition, radius, mask);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D collider = hits[i];
            if (collider == null) continue;

            IDamageable damagable = collider.GetComponentInParent<IDamageable>();
            if (damagable == null) continue;

            OwnerInfo other = collider.GetComponentInParent<OwnerInfo>();
            if (ownerInfo != null && other != null && other.OwnerID == ownerInfo.OwnerID) continue;

            Vector2 targetPosition = collider.transform.position;
            Vector2 direction = targetPosition - landingPosition;
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.right;
            }
            direction.Normalize();

            damagable.TakeDamage(damage, direction * knockback);
        }
    }
}