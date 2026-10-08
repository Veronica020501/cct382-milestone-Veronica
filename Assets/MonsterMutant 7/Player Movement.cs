using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Transform target;
    
    public float walkSpeed = 2f;
    //public float sprintSpeed = 5f;
    public float rotationSpeed = 10f;

    private Vector2 moveInput;
    //private bool sprinting;

    private Animator animator;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    //public void OnSprint(InputValue value)
    //{
    //    sprinting = value.isPressed;
    //}

    void Update()
    {
        animator.SetFloat("MoveX", 0);

        //float animationSpeed = sprinting ? 2f : 1f;

       
        // from lab 3 script
        animator.SetFloat(
            "MoveY",
            moveInput.magnitude
        //* animationSpeed
        );

        Vector3 direction = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        );

        //float currentSpeed = sprinting ? runSpeed : walkSpeed;

        transform.position += direction * walkSpeed * Time.deltaTime; //walkSpeed was currentSpeed

        if (direction != Vector3.zero)
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