using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour, ThirdPersonInputs.IOverworldActions
{
    CharacterController cc;
    ThirdPersonInputs inputs;
    Camera mainCamera;
    Animator anim;

    public LayerMask raycastCollisionLayer;

    Vector2 direction;
    Vector3 velocity;
    bool isJumpPressed = false;

    private float initSpeed = 5.0f;
    private float curSpeed = 5.0f;
    private float moveAccel = 0.2f;
    private float maxSpeed = 1.0f;
    private float jumpHeight = 5f;
    private float jumpTime = 0.7f; //both upward and downward movement

    //values that are calculated using jump height and jump time
    float timeToApex; //max jump time / 2
    float initialJumpVelocity;

    //this will also be calculated based on our jump values
    float gravity;

    void Awake()
    {
        inputs = new ThirdPersonInputs();
    }

    private void OnEnable()
    {
        inputs.Enable();
        inputs.Overworld.SetCallbacks(this);
    }

    private void OnDisable()
    {
        inputs.Disable();
        inputs.Overworld.RemoveCallbacks(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        mainCamera = Camera.main;

        //fomulas taken from the following video: https://www.youtube.com/watch?v=hG9SzQxaCm8
        timeToApex = jumpTime / 2;
        gravity = (-2 * jumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = -(gravity * timeToApex);
    }

    void Update()
    {
        Vector2 moveVel = new Vector2(velocity.x, velocity.z);
        anim.SetFloat("blend", moveVel.magnitude);

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;

        Debug.DrawLine(transform.position, transform.position + (transform.forward * 10.0f), Color.red);

        if (Physics.Raycast(ray, out hitInfo, 10.0f, raycastCollisionLayer))
        {
            Debug.Log(hitInfo);
        }
    }

    private void FixedUpdate()
    {
        if (direction.magnitude == 0)
        {
            velocity.x = 0;
            velocity.z = 0;
            curSpeed = initSpeed;
            return;
        }
 
        //grabbing fwd and right vectors for camera relative movement
        Vector3 cameraFwd = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        //remove yaw rotration
        cameraFwd.y = 0;
        cameraRight.y = 0;

        //camera projection formula for relative movement
        Vector3 desiredMoveDirection = cameraFwd * direction.y + cameraRight * direction.x;

        //rotate towards direction of movement
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), 0.1f);

        desiredMoveDirection *= (curSpeed * Time.fixedDeltaTime);
        if (desiredMoveDirection.magnitude < maxSpeed) curSpeed += moveAccel;

        velocity = desiredMoveDirection;

        if (!cc.isGrounded) velocity.y += gravity * Time.fixedDeltaTime;
        else velocity.y = CheckJump();

        cc.Move(velocity);
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
    }

    private float CheckJump()
    {
        if (isJumpPressed) return initialJumpVelocity;
        return -cc.minMoveDistance;
    }

    public void OnJump(InputAction.CallbackContext context) => isJumpPressed = context.ReadValueAsButton();
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) direction = ctx.ReadValue<Vector2>();
        if (ctx.canceled) direction = Vector2.zero;
    }
}
