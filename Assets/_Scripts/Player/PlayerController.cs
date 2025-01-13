using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, ThirdPersonInputs.IOverworldActions
{
    CharacterController cc;

    Vector2 direction;
    Vector3 velocity;
    bool isJumpPressed = false;

    private float speed = 5.0f;
    private float jumpHeight = 5f;
    private float jumpTime = 1f; //both upward and downward movement

    //values that are calculated using jump height and jump time
    float timeToApex; //max jump time / 2
    float initialJumpVelocity;

    //this will also be calculated based on our jump values
    float gravity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();

        //fomulas taken from the following video: https://www.youtube.com/watch?v=hG9SzQxaCm8
        timeToApex = jumpTime / 2;
        gravity = (-2 * jumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = -(gravity * timeToApex);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) direction = ctx.ReadValue<Vector2>();

        if (ctx.canceled) direction = Vector2.zero;
    }

    private void FixedUpdate()
    {
        velocity = new Vector3(direction.x * speed, velocity.y, direction.y * speed);

        if (!cc.isGrounded) velocity.y += gravity * Time.fixedDeltaTime;
        else velocity.y = CheckJump();

        cc.Move(velocity * Time.fixedDeltaTime);
    }

    private float CheckJump()
    {
        if (isJumpPressed) return initialJumpVelocity;
        return -cc.minMoveDistance;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }
}
