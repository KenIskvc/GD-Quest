using UnityEngine;

/// <summary>
/// Quest 3 (visual): Rotates and gently bobs an object up and down.
/// Used on the goal jewel to make it read as a special collectible.
/// Purely cosmetic — it does not affect the Jewel trigger.
/// </summary>
public class Spinner : MonoBehaviour
{
    [Header("Spin")]
    [SerializeField, Tooltip("Degrees per second around the chosen axis.")]
    private float spinSpeed = 90.0f;

    [SerializeField]
    private Vector3 spinAxis = Vector3.up;

    [Header("Bob")]
    [SerializeField, Tooltip("How far up/down the object floats, in world units.")]
    private float bobHeight = 0.25f;

    [SerializeField, Tooltip("Bobs per second.")]
    private float bobSpeed = 1.0f;

    private Vector3 startPosition;

    private void Start()
    {
        this.startPosition = this.transform.position;
    }

    private void Update()
    {
        // Spin around the axis.
        this.transform.Rotate(this.spinAxis.normalized * this.spinSpeed * Time.deltaTime, Space.World);

        // Bob up and down with a sine wave.
        float offset = Mathf.Sin(Time.time * this.bobSpeed * Mathf.PI * 2.0f) * this.bobHeight;
        this.transform.position = this.startPosition + Vector3.up * offset;
    }
}
