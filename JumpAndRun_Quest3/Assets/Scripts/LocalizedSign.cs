using UnityEngine;
using UnityEngine.Localization;
using TMPro;

/// <summary>
/// Quest 3 - Task 3: A sign that shows a localized string.
/// Each sign references one entry in a Localization String Table via the
/// <see cref="LocalizedString"/> field (pick the table + key in the Inspector).
/// The text updates automatically if the active locale changes.
///
/// Two display modes:
///  - Assign a worldText (a world-space TMP_Text on/near the sign) to show the
///    message permanently.
///  - And/or assign a popupText + popupRoot to only reveal the message while the
///    player stands in the sign's trigger.
///
/// Requires the "com.unity.localization" package. See the setup guide.
/// </summary>
public class LocalizedSign : MonoBehaviour
{
    [Header("Localized content")]
    [SerializeField, Tooltip("Pick the String Table + entry for this sign's text.")]
    private LocalizedString message;

    [Header("Always-on world text (optional)")]
    [SerializeField]
    private TMP_Text worldText;

    [Header("Proximity popup (optional)")]
    [SerializeField]
    private TMP_Text popupText;

    [SerializeField, Tooltip("Object toggled on/off as the player enters/leaves the trigger.")]
    private GameObject popupRoot;

    void OnEnable()
    {
        // Re-resolve whenever the value changes (e.g. the locale is switched).
        this.message.StringChanged += this.OnStringChanged;
    }

    void OnDisable()
    {
        this.message.StringChanged -= this.OnStringChanged;
    }

    void Start()
    {
        if (this.popupRoot != null)
        {
            this.popupRoot.SetActive(false);
        }
    }

    private void OnStringChanged(string localizedValue)
    {
        if (this.worldText != null)
        {
            this.worldText.text = localizedValue;
        }

        if (this.popupText != null)
        {
            this.popupText.text = localizedValue;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.popupRoot != null && other.CompareTag("Player"))
        {
            this.popupRoot.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (this.popupRoot != null && other.CompareTag("Player"))
        {
            this.popupRoot.SetActive(false);
        }
    }
}
