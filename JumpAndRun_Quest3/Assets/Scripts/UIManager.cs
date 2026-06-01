using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quest 3: Central UI manager (singleton), following the Level 7 lecture.
/// One manager owns:
///  - the coin counter (CollectCoin / reset)
///  - the health bar (fillAmount updated each frame from the Character)
///  - fading the HUD out and the GameOver canvas in when the player dies
///  - respawning the player and exiting the game (called from the buttons)
///  - fading in a Victory canvas when the jewel is collected
///
/// Place this on a single GameObject in the scene and wire the references.
/// Access it from anywhere via UIManager.Instance.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField]
    private Character character;

    [SerializeField]
    private CharacterController playerController;

    [SerializeField]
    private Transform respawnPoint;

    [Header("HUD")]
    [SerializeField]
    private CanvasGroup hudCanvasGroup;

    [SerializeField]
    private Image healthBar;

    [SerializeField]
    private TextMeshProUGUI coinCounterText;

    [Header("Game Over")]
    [SerializeField]
    private CanvasGroup gameOverCanvasGroup;

    [Header("Victory")]
    [SerializeField]
    private CanvasGroup victoryCanvasGroup;

    [Header("Fading")]
    [SerializeField]
    private float fadingTime = 2.0f;

    [Header("Coin Audio")]
    [SerializeField]
    private AudioSource sfxSource;

    [SerializeField]
    private AudioClip coinClip;

    private static UIManager instance = null;
    public static UIManager Instance => instance;

    private class PlayerStatistics
    {
        public int coinCounter = 0;
        //... add more statistics here later (e.g. enemies jumped on etc.)
    }

    private PlayerStatistics statistics;
    private bool isFadingInGameOver = false;
    private Coroutine gameOverFade;

    private void Awake()
    {
        instance = this;
        this.statistics = new PlayerStatistics() { coinCounter = 0 };
    }

    private void Start()
    {
        // HUD visible, GameOver and Victory hidden at the start.
        if (this.hudCanvasGroup != null) this.hudCanvasGroup.alpha = 1.0f;
        if (this.gameOverCanvasGroup != null) this.SetCanvasHidden(this.gameOverCanvasGroup);
        if (this.victoryCanvasGroup != null) this.SetCanvasHidden(this.victoryCanvasGroup);

        this.RefreshCoinText();
    }

    private void Update()
    {
        // Quest 3: drive the health bar from the character's health each frame.
        if (this.character != null && this.healthBar != null)
        {
            float percent = this.character.GetCurrentHealth() / this.character.GetMaxHealth();
            this.healthBar.fillAmount = percent;

            // When health hits 0, fade the HUD out and GameOver in (once).
            if (percent <= 0.0f && !this.isFadingInGameOver)
            {
                this.gameOverFade = this.StartCoroutine(this.FadeInGameOver());
            }
        }
    }

    // --- Coins -------------------------------------------------------------

    /// <summary>Quest 3: increment the coin counter (called from Coin.cs).</summary>
    public void CollectCoin()
    {
        this.statistics.coinCounter++;
        this.RefreshCoinText();

        if (this.sfxSource != null && this.coinClip != null)
        {
            this.sfxSource.PlayOneShot(this.coinClip);
        }
    }

    private void RefreshCoinText()
    {
        if (this.coinCounterText != null)
        {
            this.coinCounterText.text = $"{this.statistics.coinCounter}";
        }
    }

    // --- Game Over / Respawn ----------------------------------------------

    private IEnumerator FadeInGameOver()
    {
        this.isFadingInGameOver = true;

        // GameOver must block clicks so its buttons are usable.
        if (this.gameOverCanvasGroup != null)
        {
            this.gameOverCanvasGroup.interactable = true;
            this.gameOverCanvasGroup.blocksRaycasts = true;
        }

        float timer = 0.0f;
        while (timer < this.fadingTime)
        {
            float percent = timer / this.fadingTime;
            if (this.hudCanvasGroup != null) this.hudCanvasGroup.alpha = 1.0f - percent;
            if (this.gameOverCanvasGroup != null) this.gameOverCanvasGroup.alpha = percent;
            yield return null;
            timer += Time.deltaTime;
        }

        if (this.hudCanvasGroup != null) this.hudCanvasGroup.alpha = 0.0f;
        if (this.gameOverCanvasGroup != null) this.gameOverCanvasGroup.alpha = 1.0f;
    }

    /// <summary>
    /// Quest 3 - Task 1: Hook this to the Respawn button. Moves the player to the
    /// respawn point, restores health to 100%, resets coins to 0, then fades the
    /// GameOver canvas out and the HUD back in.
    /// </summary>
    public void Respawn()
    {
        // Stop any in-progress GameOver fade so it can't keep overwriting the
        // canvas alpha after we hide it (otherwise GameOver "sticks" on screen).
        if (this.gameOverFade != null)
        {
            this.StopCoroutine(this.gameOverFade);
            this.gameOverFade = null;
        }

        this.MovePlayerToRespawn();

        if (this.character != null)
        {
            this.character.ResetHealth();
        }

        this.statistics.coinCounter = 0;
        this.RefreshCoinText();

        if (this.gameOverCanvasGroup != null) this.SetCanvasHidden(this.gameOverCanvasGroup);
        if (this.hudCanvasGroup != null) this.hudCanvasGroup.alpha = 1.0f;

        this.isFadingInGameOver = false;
    }

    /// <summary>Quest 3 - Task 1: Hook this to the Exit button.</summary>
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void MovePlayerToRespawn()
    {
        if (this.playerController == null || this.respawnPoint == null)
        {
            return;
        }

        this.playerController.enabled = false;
        this.playerController.transform.position = this.respawnPoint.position;
        this.playerController.enabled = true;
    }

    // --- Victory -----------------------------------------------------------

    /// <summary>
    /// Quest 3 - Task 2: Fade the victory canvas in and the HUD out
    /// (called from Jewel.cs when the player reaches the goal).
    /// </summary>
    public void ShowVictory()
    {
        this.StartCoroutine(this.FadeInVictory());
    }

    private IEnumerator FadeInVictory()
    {
        if (this.victoryCanvasGroup != null)
        {
            this.victoryCanvasGroup.interactable = true;
            this.victoryCanvasGroup.blocksRaycasts = true;
        }

        float timer = 0.0f;
        while (timer < this.fadingTime)
        {
            float percent = timer / this.fadingTime;
            if (this.hudCanvasGroup != null) this.hudCanvasGroup.alpha = 1.0f - percent;
            if (this.victoryCanvasGroup != null) this.victoryCanvasGroup.alpha = percent;
            yield return null;
            timer += Time.deltaTime;
        }

        if (this.hudCanvasGroup != null) this.hudCanvasGroup.alpha = 0.0f;
        if (this.victoryCanvasGroup != null) this.victoryCanvasGroup.alpha = 1.0f;
    }

    private void SetCanvasHidden(CanvasGroup group)
    {
        group.alpha = 0.0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}
