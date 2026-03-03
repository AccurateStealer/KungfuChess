using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(OwnerInfo))]
[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    private OwnerInfo _ownerInfo;
    private Rigidbody2D _rigidBody;
    [SerializeField] private InputAction _movementActionWhite;
    [SerializeField] private InputAction _movementActionBlack;
    private enum MovementType
    {
        CONTROLLER, //can control forward movement seperately 
        FORWARD //the forward of the player is where they're pointing
    }

    [Header("Movement Variables")]
    [SerializeField] private float movementMultiplier = 1;
    [SerializeField] private MovementType _movementType = MovementType.FORWARD;

    [Tooltip("Optional: rotate this transform to face the current/last move direction (eg. an arrow child).")]
    [SerializeField] private Transform _facingDirectionPointer;

    [Header("External Velocity (knockback)")]
    [SerializeField] private float _externalDecay = 6f;
    [SerializeField] private float _externalMax = 100f;

    [Header("Facing")]
    [SerializeField] private float _facingLerp = 0.2f;

    private Vector2 _inputDirection;
    private Vector2 _prevMoveVelocity;
    private Vector2 _externalVelocity;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _movementActionWhite = InputSystem.actions["Move"];
        _movementActionBlack = InputSystem.actions["MoveBLACK"];

        _rigidBody = GetComponent<Rigidbody2D>();

        _ownerInfo = GetComponent<OwnerInfo>();
    }

    // this isn't in a callback because of the need to check multiple input types. could probably fix that by making a overlying set of inputs 
    void FixedUpdate()
    {
        Vector2 movementDelta = Vector2.zero;

        //if we aren't action locked, move
        PlayerState locks = GetComponent<PlayerState>();
        if (locks != null && locks.CanMove)
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

        _inputDirection = movementDelta.normalized;

        Vector2 faceDirection = movementDelta.sqrMagnitude > 0.01f ? movementDelta : (movementDelta.sqrMagnitude > 0.01f ? movementDelta.normalized : Vector2.zero);

        if (faceDirection.sqrMagnitude > 0.01f && _facingDirectionPointer != null)
        {
            float targetAngle = Mathf.Atan2(faceDirection.y, faceDirection.x) * Mathf.Rad2Deg;

            float currentAngle = _facingDirectionPointer.eulerAngles.z;
            float lerpedAngle = Mathf.LerpAngle(currentAngle, targetAngle, 0.2f);

            _facingDirectionPointer.rotation = Quaternion.Euler(0f, 0f, lerpedAngle);
        }

        Vector2 moveVelocity = _inputDirection * movementMultiplier;

        _externalVelocity = Vector2.ClampMagnitude(_externalVelocity, _externalMax);
        _externalVelocity = Vector2.Lerp(
            _externalVelocity,
            Vector2.zero,
            1f - Mathf.Exp(-_externalDecay * Time.fixedDeltaTime)
        );

        _rigidBody.linearVelocity = moveVelocity + _externalVelocity;

        _prevMoveVelocity = moveVelocity;
    }

    public void AddExternalImpulse(Vector2 impulse)
    {
        _externalVelocity += impulse;
        _externalVelocity = Vector2.ClampMagnitude(_externalVelocity, _externalMax);
    }
}
