using UnityEngine;
using UnityEngine.InputSystem;

public class AbilitySystem : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private AbilityBase _primaryAttack;

    public bool TryUsePrimary()
    {
        if (_primaryAttack == null) return false;

        return _primaryAttack.TryUse();
    }

    private void Update()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        Vector3 direction = (mouseWorldPos - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryUsePrimary();
        }
    }
}
