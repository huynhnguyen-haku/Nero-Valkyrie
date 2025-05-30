using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    private Player player;
    private PlayerControls controls;
    private CharacterController controller;
    private Animator animator;

    [Header("Movement Settings")]
    private Vector3 moveDirection;
    public Vector2 moveInput { get; private set; }
    public bool isInCar;

    private float verticalVelocity;
    private float speed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float gravityScale = 9.81f;

    private bool isRunning;

    private AudioSource walkSFX;
    private AudioSource runSFX;
    private bool canPlayFootstepsSFX;

    private void Start()
    {
        player = GetComponent<Player>();

        walkSFX = player.sfx.walkSFX;
        runSFX = player.sfx.runSFX;
        Invoke(nameof(EnableFootstepsSFX), 1f);

        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        speed = walkSpeed;

        AssignInputEvents();
    }

    private void Update()
    {
        if (player.health.playerIsDead)
            return;

        if (isInCar)
            return;

        if (!controller.enabled)
            return;

        ApplyMovement();
        ApplyRotation();
        AnimatorControllers();
    }

    #region Pause Logic

    // Pause/unpause movement and animation
    public void SetPaused(bool isPaused)
    {
        // Do not change controller state if player is in car
        if (isInCar)
        {
            Debug.Log("Player is in car, skipping CharacterController state change.");
            return;
        }

        // Only change state if controller is valid and state is different
        if (controller != null && controller.enabled != !isPaused)
        {
            controller.enabled = !isPaused;

            // Pause or resume animator
            if (animator != null)
                animator.speed = isPaused ? 0 : 1;
            
        }
    }

    #endregion

    #region Animation Logic

    // Update animator parameters for movement
    private void AnimatorControllers()
    {
        float xVelocity = Vector3.Dot(moveDirection.normalized, transform.right);
        float zVelocity = Vector3.Dot(moveDirection.normalized, transform.forward);

        animator.SetFloat("xVelocity", xVelocity, 0.1f, Time.deltaTime);
        animator.SetFloat("zVelocity", zVelocity, 0.1f, Time.deltaTime);

        bool playRunAnimation = isRunning && moveDirection.magnitude > 0;
        animator.SetBool("isRunning", playRunAnimation);
    }

    #endregion

    #region Movement Logic

    // Rotate player toward aim direction unless locked-on
    // Because if the player is locked on, the player should rotate to face the enemy
    private void ApplyRotation()
    {
        if (player.aim.isLockedOn && player.aim.lockedEnemy != null)
        {
            // Xoay về phía mục tiêu khóa, giữ nguyên logic hiện tại
            Vector3 directionToTarget = player.aim.lockedEnemy.position - transform.position;
            directionToTarget.y = 0;
            if (directionToTarget.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Xoay nhân vật theo hướng camera khi không khóa mục tiêu
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0; // Giữ nhân vật thẳng đứng
            if (cameraForward.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }
    }

    // Move player based on input and play footstep SFX
    private void ApplyMovement()
    {
        // Lấy hướng của camera
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // Loại bỏ thành phần y để đảm bảo di chuyển trên mặt phẳng XZ
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Tính toán hướng di chuyển dựa trên input và hướng camera
        moveDirection = cameraRight * moveInput.x + cameraForward * moveInput.y;

        ApplyGravity();

        if (moveDirection.magnitude > 0)
        {
            PlayFootstepsSFX();
            controller.Move(moveDirection * Time.deltaTime * speed);
        }
    }

    // Apply gravity to movement
    private void ApplyGravity()
    {
        if (!controller.isGrounded)
        {
            verticalVelocity -= gravityScale * Time.deltaTime;
            moveDirection.y = verticalVelocity;
        }
        else
        {
            verticalVelocity = -0.5f;
        }
    }

    #endregion

    #region Input Events

    // Assign input events for movement and sprint
    private void AssignInputEvents()
    {
        controls = player.controls;

        controls.Character.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Character.Movement.canceled += ctx =>
        {
            StopFootstepsSFX();
            moveInput = Vector2.zero;
        };

        controls.Character.Sprint.performed += ctx =>
        {
            speed = runSpeed;
            isRunning = true;
        };

        controls.Character.Sprint.canceled += ctx =>
        {
            speed = walkSpeed;
            isRunning = false;
        };
    }

    #endregion

    #region Footstep Sound Effects

    private void EnableFootstepsSFX() => canPlayFootstepsSFX = true;

    private void PlayFootstepsSFX()
    {
        if (!canPlayFootstepsSFX)
            return;

        if (isRunning)
        {
            if (!runSFX.isPlaying)
                runSFX.Play();
        }
        else
        {
            if (!walkSFX.isPlaying)
                walkSFX.Play();
        }
    }

    // Stop all footstep SFX
    private void StopFootstepsSFX()
    {
        walkSFX.Stop();
        runSFX.Stop();
    }

    #endregion
}
