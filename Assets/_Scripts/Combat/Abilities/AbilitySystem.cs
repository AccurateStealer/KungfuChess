using UnityEngine;
using UnityEngine.InputSystem;

public class AbilitySystem : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private AbilityBase _primaryAttack;
    [SerializeField] private AbilityBase _secondaryAttack;

    private OwnerInfo _ownerInfo;

    private InputAction _primaryActionWhite;
    private InputAction _specialActionWhite;
    private InputAction _primaryActionBlack;
    private InputAction _specialActionBlack;

    private void Awake()
    {
        _ownerInfo = GetComponent<OwnerInfo>();
        _primaryActionWhite = InputSystem.actions["PrimaryAttackWHITE"];
        _specialActionWhite = InputSystem.actions["SpecialAttackWHITE"];
        _primaryActionBlack = InputSystem.actions["PrimaryAttackBLACK"];
        _specialActionBlack = InputSystem.actions["SpecialAttackBLACK"];
    }

    public bool TryUsePrimary()
    {
        if (_primaryAttack == null) return false;

        return _primaryAttack.TryUse();
    }

    public bool TryUseSecondary()
    {
        if (_secondaryAttack == null) return false;

        return _secondaryAttack.TryUse();
    }

    private void Update()
    {
        PlayerState locks = GetComponent<PlayerState>();
        if (locks != null && locks.CanAttack)
        {
            if (_ownerInfo.OwnerID == 1)
            {
                if (_primaryActionWhite.WasPressedThisFrame())
                {
                    TryUsePrimary();
                }
                else if (_specialActionWhite.WasPressedThisFrame())
                {
                    TryUseSecondary();
                }
            }
            else if (_ownerInfo.OwnerID == 2)
            {
                if (_primaryActionBlack.WasPressedThisFrame())
                {
                    TryUsePrimary();
                }
                else if (_specialActionBlack.WasPressedThisFrame())
                {
                    TryUseSecondary();
                }
            }
        }
        //if (Mouse.current.leftButton.wasPressedThisFrame)
        //{
        //    TryUsePrimary();
        //}

        //if (Mouse.current.rightButton.wasPressedThisFrame)
        //{
        //    TryUseSecondary();
        //}
    }
}
