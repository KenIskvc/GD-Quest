using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    private bool isJumping = false;
    private float jumpCooldownTimer;
    private CharacterController controller;
    private InputAction moveAction;
    private InputAction jumpAction;

    [SerializeField]
    private float jumpCooldown;

    [SerializeField]
    private float gravity;

    [SerializeField]
    private float characterSpeed;

    [SerializeField]
    private float jumpSpeed;

    [SerializeField]
    private float dampening;

    [SerializeField]
    private Transform cameraTransform;

    [Header("Platform Detection")]
    [SerializeField]
    private LayerMask platformLayer;

    private Vector3 characterMovement;
    private Vector3 jumpVelocity;
    private Vector3 characterGravity;
    private Vector3 platformVelocity;

    void Start()
    {
        this.controller = this.GetComponent<CharacterController>();
        this.moveAction = InputSystem.actions.FindAction("Move");
        this.jumpAction = InputSystem.actions.FindAction("Jump");
        this.jumpCooldownTimer = 0.0f;
    }

    void HandleJumping()
    {
        if (this.controller.isGrounded && this.isJumping && this.jumpCooldownTimer <= 0.0f)
        {
            this.jumpVelocity = Vector3.zero;
            this.isJumping = false;
        }

        if (this.controller.isGrounded && !this.isJumping && this.jumpAction.WasPressedThisFrame())
        {
            this.characterGravity = Vector3.zero;
            this.jumpVelocity = Vector3.zero;
            this.jumpVelocity.y = this.jumpSpeed;
            this.jumpCooldownTimer = this.jumpCooldown;
            this.isJumping = true;
        }

        if (this.jumpVelocity.y > 0.0f)
        {
            this.jumpVelocity.y -= Time.fixedDeltaTime;
        }
        else
        {
            this.jumpVelocity = Vector3.zero;
        }

        this.jumpCooldownTimer -= Time.fixedDeltaTime;
    }

    /// <summary>
    /// Quest 1 - Task 3: Detect if the player is standing on a moving platform
    /// and retrieve its velocity.
    /// </summary>
    private void GetPlatformVelocity()
    {
        this.platformVelocity = Vector3.zero;

        if (!this.controller.isGrounded || this.isJumping)
        {
            return;
        }

        if (Physics.Raycast(this.transform.position, Vector3.down, out RaycastHit hit, 2.0f, this.platformLayer))
        {
            MovingPlatform platform = hit.collider.GetComponent<MovingPlatform>();

            if (platform != null)
            {
                this.platformVelocity = platform.GetVelocity();
            }
        }
    }

    void FixedUpdate()
    {
        this.HandleJumping();

        var inputMovement = this.moveAction.ReadValue<Vector2>();

        var inputRightDirection = this.cameraTransform.right;
        var inputForwardDirection = this.cameraTransform.forward;

        inputRightDirection.y = 0.0f;
        inputForwardDirection.y = 0.0f;
        inputRightDirection.Normalize();
        inputForwardDirection.Normalize();

        // Simulate gravity
        if (this.controller.isGrounded)
        {
            this.characterGravity.y = 0.0f;
        }

        this.characterGravity.y += this.gravity * Time.fixedDeltaTime;
        this.characterMovement += this.characterGravity * Time.fixedDeltaTime;
        this.characterMovement += this.jumpVelocity * Time.fixedDeltaTime;

        this.characterMovement += inputRightDirection * inputMovement.x * this.characterSpeed * Time.fixedDeltaTime;
        this.characterMovement += inputForwardDirection * inputMovement.y * this.characterSpeed * Time.fixedDeltaTime;

        this.characterMovement *= (1 - this.dampening);

        // Face the direction the character is moving
        Vector3 characterForward = this.characterMovement;
        characterForward.y = 0.0f;

        if (characterForward.sqrMagnitude > 0.0f && characterForward != Vector3.zero)
        {
            this.transform.forward = characterForward.normalized;
        }

        // Quest 1 - Task 3: Get platform velocity and combine movement
        this.GetPlatformVelocity();

        var combinedMovement = this.characterMovement + this.platformVelocity * Time.fixedDeltaTime;
        this.controller.Move(combinedMovement);
    }
}
