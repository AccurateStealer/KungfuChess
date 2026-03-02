using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//controls pawn movement and handles input  from different sources. requires owner info
[RequireComponent(typeof(OwnerInfo))]
public class Movement : MonoBehaviour
{
    private OwnerInfo _ownerInfo;
    private Rigidbody2D _rigidBody;
    private InputAction _movementActionWhite;
    private InputAction _movementActionBlack;
    private enum MovementType
    {
        CONTROLLER, //can control forward movement seperately 
        FORWARD //the forward of the player is where they're pointing
    }

    [Header("Movement Variables")]
    [SerializeField] private float movementMultiplier = 1;
    [SerializeField] private MovementType _movementType = MovementType.FORWARD;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _movementActionWhite = InputSystem.actions["Move"];

        _ownerInfo = GetComponent<OwnerInfo>();
    }

    // this isn't in a callback because of the need to check multiple input types. could probably fix that by making a overlying set of inputs 
    void FixedUpdate()
    {
        Vector2 movementDelta = Vector2.zero;

        //if we aren't action locked, move
        ActionLocks locks = GetComponent<ActionLocks>();
        if (locks != null && !locks.CanMove)
        {
            //check to see if we are white or black
            if (_ownerInfo.OwnerID == 1)
            {
                movementDelta = _movementActionWhite.ReadValue<Vector2>();
            }
            else if (_ownerInfo.OwnerID == 2)
            {
                movementDelta = _movementActionBlack.ReadValue<Vector2>();
            }
        }

        Vector2 movementToApply = movementDelta * movementMultiplier;
        _rigidBody.linearVelocity += movementToApply;
        
    }
}
