using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float moveSpeed = 4f;

    private Rigidbody2D body;
    private Vector2 moveInput;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        moveInput = Vector2.zero;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null || !Application.isFocused)
            return;

        if (keyboard.aKey.isPressed ||
            keyboard.leftArrowKey.isPressed)
            moveInput.x -= 1f;

        if (keyboard.dKey.isPressed ||
            keyboard.rightArrowKey.isPressed)
            moveInput.x += 1f;

        if (keyboard.sKey.isPressed ||
            keyboard.downArrowKey.isPressed)
            moveInput.y -= 1f;

        if (keyboard.wKey.isPressed ||
            keyboard.upArrowKey.isPressed)
            moveInput.y += 1f;

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    private void FixedUpdate()
    {
        body.linearVelocity = moveInput * moveSpeed;
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;

        if (body != null)
            body.linearVelocity = Vector2.zero;
    }
}