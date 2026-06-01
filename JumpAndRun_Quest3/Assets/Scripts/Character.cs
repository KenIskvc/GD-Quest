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

    [Header("Health")]
    [SerializeField]
    private float maxHealth = 100.0f;

    private float currentHealth;

    [Header("Audio")]
    [SerializeField]
    private AudioSource footstepsSource;

    [SerializeField]
    private AudioSource sfxSource;

    [SerializeField]
    private AudioClip footstepsClip;

    [SerializeField]
    private AudioClip jumpClip;

    [SerializeField]
    private AudioClip landClip;

    [SerializeField, Range(0.0f, 1.0f)]
    private float minMoveSpeedForFootsteps = 0.1f;

    [SerializeField]
    private float minAirTimeForLandSound = 0.15f;

    private Vector3 characterMovement;
    private Vector3 jumpVelocity;
    private Vector3 characterGravity;
    private Vector3 platformVelocity;
    private float airTime;

    void Start()
    {
        this.controller = this.GetComponent<CharacterController>();
        this.currentHealth = this.maxHealth;
        this.moveAction = InputSystem.actions.FindAction("Move");
        this.jumpAction = InputSystem.actions.FindAction("Jump");
        this.jumpCooldownTimer = 0.0f;

        if (this.footstepsSource != null && this.footstepsClip != null)
        {
            this.footstepsSource.clip = this.footstepsClip;
            this.footstepsSource.loop = true;
            this.footstepsSource.playOnAwake = false;
        }

        this.airTime = 0.0f;
    }

    /// <summary>
    /// Quest 3: Health accessors used by the UIManager (to drive the health
    /// bar) and by traps/enemies (to deal damage).
    /// </summary>
    public float GetCurrentHealth() => this.currentHealth;

    public float GetMaxHealth() => this.maxHealth;

    public bool IsDead() => this.currentHealth <= 0.0f;

    /// <summary>
    /// Quest 3 - Task 3: Reduce health by <paramref name="amount"/>, clamped to
    /// [0, maxHealth]. The UIManager watches the resulting health each frame and
    /// triggers the GameOver fade when it hits 0.
    /// </summary>
    public void InflictDamage(float amount)
    {
        this.currentHealth -= amount;
        this.currentHealth = Mathf.Clamp(this.currentHealth, 0.0f, this.maxHealth);
    }

    /// <summary>Quest 3 - Task 1: Restore full health (called on respawn).</summary>
    public void ResetHealth()
    {
        this.currentHealth = this.maxHealth;
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

            if (this.sfxSource != null && this.jumpClip != null)
            {
                this.sfxSource.PlayOneShot(this.jumpClip);
            }
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
        // Quest 3: When the player is dead (health <= 0), ignore all input.
        // Gravity still runs so the body settles to the ground instead of
        // freezing mid-air, but the player can no longer move or jump.
        bool isDead = this.currentHealth <= 0.0f;

        if (!isDead)
        {
            this.HandleJumping();
        }

        var inputMovement = isDead ? Vector2.zero : this.moveAction.ReadValue<Vector2>();

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

        this.HandleAudio(inputMovement);
    }

    /// <summary>
    /// Quest 2 - Task 1: Play footsteps loop while moving on the ground,
    /// and play a landing sound when the character touches ground after a jump/fall.
    /// </summary>
    private void HandleAudio(Vector2 inputMovement)
    {
        bool isGrounded = this.controller.isGrounded;
        bool isMoving = inputMovement.sqrMagnitude > this.minMoveSpeedForFootsteps * this.minMoveSpeedForFootsteps;

        if (this.footstepsSource != null)
        {
            bool shouldPlayFootsteps = isGrounded && isMoving && !this.isJumping;

            if (shouldPlayFootsteps && !this.footstepsSource.isPlaying)
            {
                this.footstepsSource.Play();
            }
            else if (!shouldPlayFootsteps && this.footstepsSource.isPlaying)
            {
                this.footstepsSource.Pause();
            }
        }

        if (!isGrounded)
        {
            this.airTime += Time.fixedDeltaTime;
        }
        else
        {
            if (this.airTime >= this.minAirTimeForLandSound && this.sfxSource != null && this.landClip != null)
            {
                this.sfxSource.PlayOneShot(this.landClip);
            }
            this.airTime = 0.0f;
        }
    }
}
