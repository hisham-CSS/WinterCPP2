using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour, ThirdPersonInputs.IOverworldActions
{
    //controller components
    CharacterController cc;
    ThirdPersonInputs inputs;
    Camera mainCamera;
    Animator anim;

    //movement and rotation
    [Header("Movement Variables")]
    [SerializeField] private float initSpeed = 5.0f;
    [SerializeField] private float maxSpeed = 15.0f;
    [SerializeField] private float moveAccel = 0.2f;
    [SerializeField] private float rotationSpeed = 30.0f;
    private float curSpeed = 5.0f;

    //Jump Variables
    [Header("Jump Variables")]
    [SerializeField] private float jumpHeight = 0.1f;
    [SerializeField] private float jumpTime = 0.7f; //both upward and downward movement
    //values that are calculated using jump height and jump time
    private float timeToApex; //max jump time / 2
    private float initialJumpVelocity;

    //weapon system variables
    [Header("Weapon Variables")]
    [SerializeField] private Transform weaponAttachPoint;
    Weapon weapon = null;

    //Test Variables
    public LayerMask raycastCollisionLayer;

    //Action for ControllerColliderHit - Assumes that there is only one controller in the scene
    public static event Action<Collider, ControllerColliderHit> OnControllerColliderHitInternal;

    //Character movement
    Vector2 direction;
    Vector3 velocity;
    
    //this will also be calculated based on our jump values
    private float gravity;

    //jump input
    private bool isJumpPressed = false;
    //TODO: creating isJumpReleased and fixing the jump to add mechanics such as variable jump height
    
    

    #region Setup Functions
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
        InitJump();
    }

    private void InitJump()
    {
        //fomulas taken from the following video: https://www.youtube.com/watch?v=hG9SzQxaCm8
        timeToApex = jumpTime / 2;
        gravity = (-2 * jumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = -(gravity * timeToApex);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus) Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion
    #region Input
    public void OnJump(InputAction.CallbackContext context) => isJumpPressed = context.ReadValueAsButton();
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) direction = ctx.ReadValue<Vector2>();
        if (ctx.canceled) direction = Vector2.zero;
    }

    public void OnDropWeapon(InputAction.CallbackContext context)
    {
        if (weapon) {
            weapon.Drop(GetComponent<Collider>(), transform.forward);
            weapon = null;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Attack")) return;

        if (weapon) anim.SetTrigger("Attack");
    }
    #endregion
    #region Player Movement
    private Vector3 ProjectedMoveDirection()
    {
        //grabbing fwd and right vectors for camera relative movement
        Vector3 cameraFwd = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        //remove yaw rotration
        cameraFwd.y = 0;
        cameraRight.y = 0;

        cameraFwd.Normalize();
        cameraRight.Normalize();

        return cameraFwd * direction.y + cameraRight * direction.x;
    }

    private void UpdateCharacteVelocity(Vector3 dir)
    {
        if (direction == Vector2.zero) curSpeed = initSpeed;

        //set velocity to desired move direction
        velocity.x = dir.x;
        velocity.z = dir.z;

        //ensure we are clamped to max speed
        curSpeed = Mathf.Clamp(curSpeed, initSpeed, maxSpeed);

        //Debug.Log($"Player Controller current speed is : {curSpeed}");
        //move along projected axis
        velocity = new Vector3(velocity.x * curSpeed, velocity.y, velocity.z * curSpeed);

        curSpeed += moveAccel * Time.fixedDeltaTime;

        if (!cc.isGrounded) velocity.y += gravity * Time.fixedDeltaTime;
        else velocity.y = CheckJump();
    }

    private float CheckJump()
    {
        if (isJumpPressed) return initialJumpVelocity;
        return -cc.minMoveDistance;
    }
    #endregion

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
        Vector3 desiredMoveDirection = ProjectedMoveDirection();

        if (!anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Attack"))
        {
            UpdateCharacteVelocity(desiredMoveDirection);
            cc.Move(velocity);
        }

        //rotate towards direction of movement
        if (direction.magnitude > 0)
        {
            float timeStep = rotationSpeed * Time.fixedDeltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), timeStep);
        }

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        OnControllerColliderHitInternal?.Invoke(GetComponent<Collider>(), hit);
        if (hit.collider.CompareTag("Weapon") && weapon == null) {
            weapon = hit.gameObject.GetComponent<Weapon>();
            weapon.Equip(GetComponent<Collider>(), weaponAttachPoint);
        }
    }


}
