using UnityEngine;
using TMPro; // Required for TextMeshPro

namespace UI
{
    /// <summary>
    /// Manages the in-game Heads Up Display (HUD).
    /// Modular design: This script ONLY updates the UI text, it does not calculate the game logic.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("The UI Image used for the center screen crosshair")]
        [SerializeField] private UnityEngine.UI.Image crosshair;
        [Tooltip("The TextMeshPro UI element that shows 'Objectives Remaining: X'")]
        [SerializeField] private TextMeshProUGUI objectivesText;
        [Tooltip("The TextMeshPro UI element that shows 'Lives: X'")]
        [SerializeField] private TextMeshProUGUI livesText;
        [Tooltip("The TextMeshPro UI element that shows the elapsed time (Day 2)")]
        [SerializeField] private TextMeshProUGUI timerText;
        [Tooltip("Optional panel to show when caught or winning")] // For Day 7
        [SerializeField] private GameObject gameOverPanel;

        [Header("Notification Popups (Day 3)")]
        [SerializeField] private TextMeshProUGUI batteryNotificationText;
        [SerializeField] private TextMeshProUGUI guardWarningText;
        
        [Header("Noise Meter (Day 3)")]
        [Tooltip("The Slider that visually represents how much noise the player is making")]
        [SerializeField] private UnityEngine.UI.Slider noiseMeterSlider;
        
        private Coroutine batteryRoutine;
        private Coroutine guardRoutine;
        private Player.PlayerController localPlayer;

        private void Start()
        {
            // 1. Ensure the cursor is locked and hidden when the game starts
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 2. Hide game over panel if it was left on by accident
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            // Hide popups by default
            if (batteryNotificationText != null) batteryNotificationText.gameObject.SetActive(false);
            if (guardWarningText != null) guardWarningText.gameObject.SetActive(false);

            // 3. Set default text
            UpdateObjectiveText(3); // Assuming 3 objectives to start
            UpdateLivesText(3); // Assuming 3 lives to start

            // --- SETTINGS (Day 5) ---
            // If the player disabled the HUD in the main menu, turn off all the UI visuals!
            bool showHUD = PlayerPrefs.GetInt("ShowHUD", 1) == 1;
            if (!showHUD)
            {
                // We disable all children (like texts/bars) rather than the parent Canvas so scripts don't break searching for GameHUD
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Updates the Objectives remaining on screen.
        /// </summary>
        public void UpdateObjectiveText(int remaining)
        {
            if (objectivesText != null)
            {
                objectivesText.text = $"Objectives Remaining: {remaining}";
            }
        }

        /// <summary>
        /// Updates the Lives remaining on screen.
        /// </summary>
        public void UpdateLivesText(int lives)
        {
            if (livesText != null)
            {
                livesText.text = $"Lives: {lives}";
            }
        }

        /// <summary>
        /// Public method for PlayerInteraction to change crosshair color when hovering over an item.
        /// </summary>
        public void SetCrosshairColor(Color newColor)
        {
            if (crosshair != null)
            {
                crosshair.color = newColor;
            }
        }

        /// <summary>
        /// Formats and updates the elapsed time on the HUD.
        /// </summary>
        public void UpdateTimerText(float timeElapsed)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeElapsed / 60F);
                int seconds = Mathf.FloorToInt(timeElapsed - minutes * 60);
                timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
            }
        }

        private void Update()
        {
            // Continuously map the Player's acoustic noise to the UI Slider!
            if (noiseMeterSlider != null)
            {
                if (localPlayer == null) localPlayer = FindObjectOfType<Player.PlayerController>();
                
                if (localPlayer != null)
                {
                    // Assuming the Slider expects a value between 0.0 and 1.0, and 100 is max noise
                    noiseMeterSlider.value = localPlayer.CurrentNoiseLevel / 100f;
                }
            }
        }

        // --- NOTIFICATION FLYOUTS --- //

        public void ShowBatteryNotification(string message = "BATTERY SECURED")
        {
            if (batteryNotificationText != null)
            {
                if (batteryRoutine != null) StopCoroutine(batteryRoutine);
                batteryRoutine = StartCoroutine(FadeTextRoutine(batteryNotificationText, message));
            }
        }

        public void ShowGuardWarning(string message = "WARNING: GUARD NEARBY")
        {
            if (guardWarningText != null)
            {
                if (guardRoutine != null) StopCoroutine(guardRoutine);
                guardRoutine = StartCoroutine(FadeTextRoutine(guardWarningText, message));
            }
        }

        private System.Collections.IEnumerator FadeTextRoutine(TextMeshProUGUI txt, string msg)
        {
            txt.text = msg;
            txt.gameObject.SetActive(true);
            
            // Snap to 100% visible
            Color c = txt.color;
            c.a = 1f;
            txt.color = c;

            // Wait 1.5 seconds so the player can perfectly read it
            yield return new WaitForSeconds(1.5f);

            // Smoothly fade out over 1 second
            float fadeSpeed = 1f;
            while (c.a > 0f)
            {
                c.a -= Time.deltaTime * fadeSpeed;
                txt.color = c;
                yield return null;
            }

            // Hide
            txt.gameObject.SetActive(false);
        }
    }
}
