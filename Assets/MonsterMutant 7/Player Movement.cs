using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerControls playerControls;

    public Transform target;

    public float walkSpeed = 2f;
    public float sprintSpeed = 7f;
    public float rotationSpeed = 10f;

    private Vector2 moveInput;

    private Animator animator;

    public bool isSprinting;

    // these are the values of each animation (walk/run) in blend tree
    float walkBlend = 0.5f;
    float runBlend = 1f;

    // jumping values
    public bool isJumping;

    [Tooltip("How long the jump pose lasts (match your jump clip's length). The jump ends after this time even if the key is still held.")]
    public float jumpDuration = 0.8f;

    [Tooltip("Multiplier on move speed while in the air. 1 = same as ground speed.")]
    public float jumpMoveSpeedMultiplier = 1f;

    private float jumpTimer;

    // attack values
    // isAttacking is set/cleared by the ComboController, which owns all attack timing.
    public bool isAttacking;
    public ComboController combo;         // assign in the Inspector

    // Camera
    private Transform cameraTransform;


    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        cameraTransform = Camera.main.transform;  // the Main Camera that has the Cinemachine Brain
    }

    private void Awake()
    {
        playerControls = new PlayerControls();

        playerControls.Player.SprintStart.performed += x => SprintPressed();
        playerControls.Player.SprintFinish.performed += x => SprintReleased();

        // Only the press starts a jump. Releasing the key no longer ends it:
        // the jump now ends on its own timer, so holding space can't leave you
        // stuck in the jump pose and sliding along the ground.
        playerControls.Player.Jump.performed += x => JumpPressed();

        playerControls.Player.Attack.performed += x => AttackPressed();
    }


    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    void Update()
    {
        // End the jump when its time is up, even if the key is still held
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            if (jumpTimer >= jumpDuration) isJumping = false;
        }

        // MoveX = 1 selects the jump pose in the blend tree; 0 is normal locomotion.
        float blendTime = (isAttacking || isJumping) ? 0.1f : 0.25f;
        float moveX = isJumping ? 1f : 0f;
        animator.SetFloat("MoveX", moveX, blendTime, Time.deltaTime);

        // Locomotion animation is held at 0 during jumps/attacks (the jump/attack pose plays instead)
        float blend = isSprinting ? runBlend : walkBlend;    // determines current blend tree value
        float moveY = (isJumping || isAttacking) ? 0f : moveInput.magnitude * blend;
        animator.SetFloat("MoveY", moveY, blendTime, Time.deltaTime);

        // Camera's forward and right, flattened onto the ground
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // W/S move toward/away from where the camera looks, A/D move left/right of it
        Vector3 direction = camForward * moveInput.y + camRight * moveInput.x;
        direction = Vector3.ClampMagnitude(direction, 1f);

        // Movement lock: only attacks plant the character.
        // Jumping does NOT lock movement, so you can jump forward while moving.
        if (!isAttacking)
        {
            float speed = isSprinting ? sprintSpeed : walkSpeed;
            if (isJumping) speed *= jumpMoveSpeedMultiplier;

            transform.position += direction * speed * Time.deltaTime;

            if (direction != Vector3.zero)  //.zero is a static field that represents the constant 0
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    private void SprintPressed()
    {
        isSprinting = true;
    }

    private void SprintReleased()
    {
        isSprinting = false;
    }

    private void JumpPressed()
    {
        // Ignore presses while already jumping or mid-attack
        if (isJumping || isAttacking) return;

        isJumping = true;
        jumpTimer = 0f;

        // Snap straight to the jump pose instead of blending slowly
        animator.SetFloat("MoveX", 1f);
        animator.SetFloat("MoveY", 0f);

        // Restart the blend tree so the jump clip begins at its first frame
        animator.Play("Blend Tree", 0, 0f);
    }

    private void AttackPressed()
    {
        // Don't start an attack mid-jump
        if (isJumping) return;

        // The combo controller decides which hit this press means
        if (combo != null) combo.RequestAttack();
    }
}