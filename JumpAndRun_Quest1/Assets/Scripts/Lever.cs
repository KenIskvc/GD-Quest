using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lever : MonoBehaviour
{
    private bool on = false;
    private bool interpolating = false;
    private float currentInterpolationTime = 0.0f;
    private InputAction interactAction;

    // Quest 1 - Task 4: Track if the player is within range
    private bool playerInRange = false;

    [SerializeField]
    private float switchTime;

    [SerializeField]
    private Transform onPosition;

    [SerializeField]
    private Transform offPosition;

    [SerializeField]
    private GameObject leverHandle;

    [Header("Quest 1 - Proximity Detection")]
    [SerializeField]
    private LayerMask characterLayer;

    void Start()
    {
        this.interactAction = InputSystem.actions.FindAction("Interact");
    }

    // Quest 1 - Task 4: Detect when the player enters the sphere trigger
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & this.characterLayer) != 0)
        {
            this.playerInRange = true;
        }
    }

    // Quest 1 - Task 4: Detect when the player leaves the sphere trigger
    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & this.characterLayer) != 0)
        {
            this.playerInRange = false;
        }
    }

    IEnumerator InterpolateLeverCoroutine()
    {
        this.interpolating = true;

        Vector3 startPosition, targetPosition;
        Quaternion startRotation, targetRotation;

        if (this.on)
        {
            startPosition = this.offPosition.position;
            startRotation = this.offPosition.rotation;
            targetPosition = this.onPosition.position;
            targetRotation = this.onPosition.rotation;
        }
        else
        {
            startPosition = this.onPosition.position;
            startRotation = this.onPosition.rotation;
            targetPosition = this.offPosition.position;
            targetRotation = this.offPosition.rotation;
        }

        this.currentInterpolationTime = 0.0f;

        while (this.currentInterpolationTime < this.switchTime)
        {
            float percentage = this.currentInterpolationTime / this.switchTime;
            var currentPosition = Vector3.Lerp(startPosition, targetPosition, percentage);
            var currentRotation = Quaternion.Slerp(startRotation, targetRotation, percentage);

            this.leverHandle.transform.SetPositionAndRotation(currentPosition, currentRotation);

            yield return null;
            this.currentInterpolationTime += Time.deltaTime;
        }

        this.leverHandle.transform.SetPositionAndRotation(targetPosition, targetRotation);
        this.interpolating = false;
    }

    void ToggleLever()
    {
        this.on = !this.on;
        this.StartCoroutine(this.InterpolateLeverCoroutine());
    }

    // Quest 1 - Task 4: Changed from Update() to FixedUpdate()
    // and added playerInRange check
    void FixedUpdate()
    {
        if (this.playerInRange && this.interactAction.WasPressedThisFrame() && !this.interpolating)
        {
            this.ToggleLever();
        }
    }
}
