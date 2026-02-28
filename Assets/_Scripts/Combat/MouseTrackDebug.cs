using UnityEngine;
using UnityEngine.InputSystem;

public class MouseTrackDebug : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;

    void Update()
    {
        Vector2 input = Keyboard.current != null
            ? new Vector2(
                (Keyboard.current.aKey.isPressed ? -1 : 0) + (Keyboard.current.dKey.isPressed ? 1 : 0),
                (Keyboard.current.wKey.isPressed ? 1 : 0) + (Keyboard.current.sKey.isPressed ? -1 : 0)
              )
            : Vector2.zero;

        Vector3 movement = new Vector3(input.x, 0, input.y) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}
