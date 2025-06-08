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

    public void SetPaused(bool isPaused)
    {
        if (isInCar)
        {
            Debug.Log("Player is in car, skipping CharacterController state change.");
            return;
        }

        if (controller != null && controller.enabled != !isPaused)
        {
            controller.enabled = !isPaused;

            if (animator != null)
                animator.speed = isPaused ? 0 : 1;
        }
    }

    #endregion

    #region Animation Logic

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

    private void ApplyRotation()
    {
        if (player.health.playerIsDead || isInCar)
            return;

        // Lấy vị trí aim từ Player_AimController
        Transform aim = player.aim.Aim();
        if (aim != null)
        {
            Vector3 aimDirection = aim.position - transform.position;
            aimDirection.y = 0; // Giữ xoay trên mặt phẳng XZ
            if (aimDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }
    }

    private void ApplyMovement()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        moveDirection = cameraRight * moveInput.x + cameraForward * moveInput.y;

        ApplyGravity();

        if (moveDirection.magnitude > 0)
        {
            PlayFootstepsSFX();
            controller.Move(moveDirection * Time.deltaTime * speed);
        }
    }

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

    private void StopFootstepsSFX()
    {
        walkSFX.Stop();
        runSFX.Stop();
    }

    #endregion
}