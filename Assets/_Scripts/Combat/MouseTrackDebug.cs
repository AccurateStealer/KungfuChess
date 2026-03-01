using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class MouseTrackDebug : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private bool _isControlled = false;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] Transform _facingDirectionPointer;

    [Header("External Velocity (knockback)")]
    [SerializeField] private float _externalDecay = 12f; 
    [SerializeField] private float _externalMax = 25f;   

    Rigidbody2D _rigidBody;

    private Vector2 _inputDirection;
    private Vector2 _prevMoveVelocity;
    private Vector2 _externalVelocity;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!_isControlled) return;

        if (Keyboard.current != null)
        {
            _inputDirection = new Vector2(
                (Keyboard.current.aKey.isPressed ? -1 : 0) + (Keyboard.current.dKey.isPressed ? 1 : 0),
                (Keyboard.current.wKey.isPressed ? 1 : 0) + (Keyboard.current.sKey.isPressed ? -1 : 0)
            );

            _inputDirection = _inputDirection.normalized;
        }
        else
        {
            _inputDirection = Vector2.zero;
        }

        Vector2 faceDirection = _inputDirection.sqrMagnitude > 0.01f ? _inputDirection : (_prevMoveVelocity.sqrMagnitude > 0.01f ? _prevMoveVelocity.normalized : Vector2.zero);

        if (faceDirection.sqrMagnitude > 0.01f && _facingDirectionPointer != null)
        {
            float targetAngle = Mathf.Atan2(faceDirection.y, faceDirection.x) * Mathf.Rad2Deg;

            float currentAngle = _facingDirectionPointer.eulerAngles.z; // ✅ use pointer, not player
            float lerpedAngle = Mathf.LerpAngle(currentAngle, targetAngle, 0.2f);

            _facingDirectionPointer.rotation = Quaternion.Euler(0f, 0f, lerpedAngle);
        }


        //Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        //Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        //mouseWorldPos.z = 0f;

        //Vector3 direction = (mouseWorldPos - transform.position).normalized;

        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0f, 0f, angle);


    }

    void FixedUpdate()
    {
        Vector2 moveVelocity = _inputDirection * _moveSpeed;

        ActionLocks locks = GetComponent<ActionLocks>();

        if (locks != null && !locks.CanMove)
        {
            moveVelocity = Vector2.zero;
        }

        _externalVelocity = Vector2.ClampMagnitude(_externalVelocity, _externalMax);
        _externalVelocity = Vector2.Lerp(_externalVelocity, Vector2.zero, 1f - Mathf.Exp(-_externalDecay * Time.fixedDeltaTime));

        _rigidBody.linearVelocity = moveVelocity + _externalVelocity;

        _prevMoveVelocity = moveVelocity;
    }

    public void AddExternalImpulse(Vector2 impulse)
    {
        _externalVelocity += impulse;
        _externalVelocity = Vector2.ClampMagnitude(_externalVelocity, _externalMax);
    }
}