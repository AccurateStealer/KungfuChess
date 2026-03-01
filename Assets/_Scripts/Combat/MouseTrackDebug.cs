using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class MouseTrackDebug : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] bool IsControlled = false;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;

    [Header("External Velocity (knockback)")]
    [SerializeField] float externalDecay = 12f; 
    [SerializeField] float externalMax = 25f;   

    Rigidbody2D _rigidBody;

    Vector2 input;
    Vector2 prevMoveVel;
    Vector2 externalVel;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!IsControlled) return;

        if (Keyboard.current != null)
        {
            input = new Vector2(
                (Keyboard.current.aKey.isPressed ? -1 : 0) + (Keyboard.current.dKey.isPressed ? 1 : 0),
                (Keyboard.current.wKey.isPressed ? 1 : 0) + (Keyboard.current.sKey.isPressed ? -1 : 0)
            );
            input = input.normalized;
        }
        else
        {
            input = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        Vector2 moveVel = input * moveSpeed;

        externalVel = Vector2.ClampMagnitude(externalVel, externalMax);

        externalVel = Vector2.Lerp(externalVel, Vector2.zero, 1f - Mathf.Exp(-externalDecay * Time.fixedDeltaTime));

        _rigidBody.linearVelocity = moveVel + externalVel;

        var locks = GetComponent<ActionLocks>();
        if (locks != null && !locks.CanMove)
        {
            moveVel = Vector2.zero;
        }

        prevMoveVel = moveVel;
    }

    public void AddExternalImpulse(Vector2 impulse)
    {
        externalVel += impulse;
        externalVel = Vector2.ClampMagnitude(externalVel, externalMax);
    }
}