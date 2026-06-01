using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField]
    private Transform patrolPointA;

    [SerializeField]
    private Transform patrolPointB;

    [SerializeField]
    private float patrolSpeed = 1.5f;

    [SerializeField, Tooltip("How close to a point counts as 'arrived' before turning around")]
    private float arriveThreshold = 0.1f;

    [Header("Squash")]
    [SerializeField]
    private float squashDuration = 0.3f;

    [SerializeField]
    private float destroyDelay = 0.5f;

    [SerializeField, Tooltip("How far above the enemy the player must be to count as a stomp")]
    private float minStompHeightOffset = 0.5f;

    [Header("Damage")]
    [SerializeField, Tooltip("Damage dealt to the player when touched from the side (not stomped)")]
    private float contactDamage = 20.0f;

    [SerializeField, Tooltip("Seconds before this enemy can damage the player again")]
    private float damageInterval = 1.0f;

    [Header("Audio")]
    [SerializeField]
    private AudioSource sfxSource;

    [SerializeField]
    private AudioClip squashClip;

    [Header("Animation")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private string walkStateName = "Walk";

    private bool movingToB;
    private bool isSquashed;
    private float damageCooldownTimer;

    // Quest 3: remember the spawn state so the enemy can be restored on respawn.
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 startScale;

    void Start()
    {
        this.startPosition = this.transform.position;
        this.startRotation = this.transform.rotation;
        this.startScale = this.transform.localScale;

        this.movingToB = true;
        this.isSquashed = false;
        this.damageCooldownTimer = 0.0f;

        if (this.animator != null && !string.IsNullOrEmpty(this.walkStateName))
        {
            this.animator.Play(this.walkStateName);
        }
    }

    /// <summary>
    /// Quest 3: Restore a squashed enemy to its starting position, scale and
    /// state, and re-activate it. Called by the UIManager when the player
    /// respawns, so the level is replayable after death.
    /// </summary>
    public void ResetEnemy()
    {
        this.transform.position = this.startPosition;
        this.transform.rotation = this.startRotation;
        this.transform.localScale = this.startScale;

        this.movingToB = true;
        this.isSquashed = false;
        this.damageCooldownTimer = 0.0f;

        this.gameObject.SetActive(true);

        if (this.animator != null && !string.IsNullOrEmpty(this.walkStateName))
        {
            this.animator.Play(this.walkStateName);
        }
    }

    void Update()
    {
        if (this.damageCooldownTimer > 0.0f)
        {
            this.damageCooldownTimer -= Time.deltaTime;
        }

        if (this.isSquashed)
        {
            return;
        }

        if (this.patrolPointA == null || this.patrolPointB == null)
        {
            return;
        }

        Transform target = this.movingToB ? this.patrolPointB : this.patrolPointA;

        Vector3 targetPosition = target.position;
        targetPosition.y = this.transform.position.y;

        Vector3 direction = targetPosition - this.transform.position;
        direction.y = 0.0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            this.transform.forward = direction.normalized;
        }

        this.transform.position = Vector3.MoveTowards(this.transform.position, targetPosition, this.patrolSpeed * Time.deltaTime);

        if (Vector3.Distance(this.transform.position, targetPosition) <= this.arriveThreshold)
        {
            this.movingToB = !this.movingToB;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        this.HandlePlayerContact(other);
    }

    void OnTriggerStay(Collider other)
    {
        // Keep damaging the player if they linger against the enemy's side.
        this.HandlePlayerContact(other);
    }

    /// <summary>
    /// Quest 3 - Task 3: If the player stomps from above, the enemy is squashed.
    /// Otherwise the enemy damages the player (on a cooldown so it doesn't drain
    /// health every frame of contact).
    /// </summary>
    private void HandlePlayerContact(Collider other)
    {
        if (this.isSquashed)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        bool playerIsAbove = other.transform.position.y > this.transform.position.y + this.minStompHeightOffset;

        if (playerIsAbove)
        {
            StartCoroutine(this.SquashRoutine());
            return;
        }

        // Side contact -> hurt the player (on a cooldown so a single bump
        // doesn't drain health every physics frame).
        if (this.damageCooldownTimer > 0.0f)
        {
            return;
        }

        Character character = other.GetComponentInChildren<Character>();

        if (character != null && !character.IsDead())
        {
            character.InflictDamage(this.contactDamage);
            this.damageCooldownTimer = this.damageInterval;
        }
    }

    private IEnumerator SquashRoutine()
    {
        this.isSquashed = true;

        if (this.sfxSource != null && this.squashClip != null)
        {
            this.sfxSource.PlayOneShot(this.squashClip);
        }

        Vector3 originalScale = this.transform.localScale;
        Vector3 squashedScale = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.2f, originalScale.z * 1.3f);

        float elapsed = 0.0f;
        while (elapsed < this.squashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / this.squashDuration;
            this.transform.localScale = Vector3.Lerp(originalScale, squashedScale, t);
            yield return null;
        }

        yield return new WaitForSeconds(this.destroyDelay);

        // Quest 3: deactivate instead of Destroy so the enemy can be restored
        // when the player respawns (see ResetEnemy / UIManager.Respawn).
        this.gameObject.SetActive(false);
    }
}
