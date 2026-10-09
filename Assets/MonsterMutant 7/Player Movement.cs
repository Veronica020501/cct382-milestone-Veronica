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
    //private bool sprinting;

    private Animator animator;

    public bool isSprinting;

    // these are the values of each animation (walk/run) in blend tree
    float walkBlend = 0.5f;
    float runBlend = 1f;

    // jumping values
    public bool isJumping;

    public float jumpWindUpTime = 0.4f;   // seconds of build-up before the character moves
    //private float jumpTimer;

    // attack values
    public bool isAttacking;
    public float attackDuration = 0.6f;
    private float attackTimer;


    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        playerControls = new PlayerControls();

        playerControls.Player.SprintStart.performed += x => SprintPressed();
        playerControls.Player.SprintFinish.performed += x => SprintReleased();

        playerControls.Player.Jump.performed += x => JumpPressed();
        playerControls.Player.Jump.canceled += x => JumpReleased();

        playerControls.Player.Attack.performed += x => AttackPressed();

    }


    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    //public void OnSprint(InputValue value)
    //{
    //    sprinting = value.isPressed;
    //}



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

        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer > attackDuration) isAttacking = false;
        }

        //if (isJumping) jumpTimer += Time.deltaTime;
        //bool windingUp = isJumping && jumpTimer < jumpWindUpTime;

        //animator.SetFloat("MoveX", 0);
        //animator.SetFloat("MoveX", isJumping ? 1f : 0f, 0.1f, Time.deltaTime);

        float blendTime = (isAttacking || isJumping) ? 0.1f : 0.25f;
        float moveX = isAttacking ? -1f : (isJumping ? 1f : 0f);
        animator.SetFloat("MoveX", moveX, blendTime, Time.deltaTime);

        //float animationSpeed = sprinting ? 2f : 1f;


        // from lab 3 script
        //animator.SetFloat(
        //    "MoveY",
        //    moveInput.magnitude
        ////* animationSpeed
        //);

        float blend = isSprinting ? runBlend : walkBlend;    // determines current blend tree value
        //float moveY = isJumping ? 0f : moveInput.magnitude * blend;
        float moveY = (isJumping || isAttacking) ? 0f : moveInput.magnitude * blend;
        animator.SetFloat("MoveY", moveY, blendTime, Time.deltaTime);

        //animator.SetFloat("MoveY", moveInput.magnitude * blend);

        Vector3 direction = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        );

        //float currentSpeed = sprinting ? runSpeed : walkSpeed;

        //if (!windingUp)
        // {
        if (isSprinting)
        {
            transform.position += direction * sprintSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += direction * walkSpeed * Time.deltaTime; //walkSpeed was currentSpeed
        }
        //}

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
        isJumping = true;
        //jumpTimer = 0f;

        // Snap straight to the jump pose instead of blending slowly
        animator.SetFloat("MoveX", 1f);
        animator.SetFloat("MoveY", 0f);

        // Restart the blend tree so the jump clip begins at its first frame
        animator.Play("Blend Tree", 0, 0f);
    }

    private void JumpReleased()
    {
        isJumping = false;
    }

    private void AttackPressed()
    {
        if (isAttacking) return;

        isAttacking = true;
        attackTimer = 0f;

        animator.SetFloat("MoveX", -1f);
        animator.SetFloat("MoveY", 0f);
        animator.Play("Blend Tree", 0, 0f);
    }
}