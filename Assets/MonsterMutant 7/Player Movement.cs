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
    float walkBlend = 0.5f;   // walk clip's threshold
    float runBlend = 1f;     // run clip's threshold

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.Player.SprintStart.performed += x => SprintPressed();
        playerControls.Player.SprintFinish.performed += x => SprintReleased();

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

        animator.SetFloat("MoveX", 0);

        //float animationSpeed = sprinting ? 2f : 1f;

       
        // from lab 3 script
        //animator.SetFloat(
        //    "MoveY",
        //    moveInput.magnitude
        ////* animationSpeed
        //);

        float blend = isSprinting ? runBlend: walkBlend;
        animator.SetFloat("MoveY", moveInput.magnitude * blend);

        Vector3 direction = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        );

        //float currentSpeed = sprinting ? runSpeed : walkSpeed;
       
        if (isSprinting)
        {
            transform.position += direction * sprintSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += direction * walkSpeed * Time.deltaTime; //walkSpeed was currentSpeed
        }


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

}