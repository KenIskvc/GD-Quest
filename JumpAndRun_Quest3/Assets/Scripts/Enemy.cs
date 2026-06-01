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

    void Start()
    {
        this.movingToB = true;
        this.isSquashed = false;

        if (this.animator != null && !string.IsNullOrEmpty(this.walkStateName))
        {
            this.animator.Play(this.walkStateName);
        }
    }

    void Update()
    {
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

        Destroy(this.gameObject);
    }
}
