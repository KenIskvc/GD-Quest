using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    private float platformSpeed;

    [SerializeField]
    private Vector3 start;

    [SerializeField]
    private Vector3 end;

    private Vector3 lastPosition;
    private Vector3 velocity;

    void Start()
    {
        this.lastPosition = this.transform.position;
    }

    void FixedUpdate()
    {
        float pingPong = Mathf.PingPong(Time.fixedTime * this.platformSpeed, 1.0f);
        var newPosition = Vector3.Lerp(this.start, this.end, pingPong);
        this.transform.localPosition = newPosition;

        // Calculate velocity for the character controller (Task 3)
        this.velocity = (this.transform.position - this.lastPosition) / Time.fixedDeltaTime;
        this.lastPosition = this.transform.position;
    }

    /// <summary>
    /// Returns the current velocity of the platform.
    /// Used by the character controller to move the player along with the platform.
    /// </summary>
    public Vector3 GetVelocity()
    {
        return this.velocity;
    }
}
