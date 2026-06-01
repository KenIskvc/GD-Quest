using UnityEngine;

/// <summary>
/// Quest 3 - Task 2: The goal jewel placed at the end of the level.
/// When the player enters its trigger, the UIManager fades in the victory
/// canvas. Add this to the Assets/Prefabs/World/jewel model and make sure it
/// has a collider with 'Is Trigger' enabled.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Jewel : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField]
    private AudioSource sfxSource;

    [SerializeField]
    private AudioClip collectClip;

    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (this.collected || !other.CompareTag("Player"))
        {
            return;
        }

        this.collected = true;

        if (this.sfxSource != null && this.collectClip != null)
        {
            this.sfxSource.PlayOneShot(this.collectClip);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictory();
        }

        // Hide the jewel's visuals but keep the GameObject alive so any sound
        // finishes playing. Disable the collider so it can't trigger twice.
        Collider col = this.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        foreach (Renderer renderer in this.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
    }
}
