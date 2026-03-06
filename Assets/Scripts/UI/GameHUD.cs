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
        [Tooltip("Optional panel to show when caught or winning")] // For Day 7
        [SerializeField] private GameObject gameOverPanel;

        private void Start()
        {
            // 1. Ensure the cursor is locked and hidden when the game starts
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 2. Hide game over panel if it was left on by accident
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            // 3. Set default text
            UpdateObjectiveText(3); // Assuming 3 objectives to start
            UpdateLivesText(3); // Assuming 3 lives to start
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
    }
}
