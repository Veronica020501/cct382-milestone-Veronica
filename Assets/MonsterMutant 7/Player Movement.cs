using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float turnSpeed = 10f;

    private Vector2 moveInput;
    private bool sprinting;

    private Animator animator;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        sprinting = value.isPressed;
    }

    void Update()
    {
        animator.SetFloat("MoveX", 0);

        float animationSpeed = sprinting ? 2f : 1f;

        animator.SetFloat(
            "MoveY",
            moveInput.magnitude * animationSpeed
        );

        Vector3 movement = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        );

        float currentSpeed = sprinting ? runSpeed : walkSpeed;

        transform.position += movement * currentSpeed * Time.deltaTime;

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
    }

}