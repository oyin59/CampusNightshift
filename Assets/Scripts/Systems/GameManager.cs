using UnityEngine;
using UnityEngine.SceneManagement;
using UI; // To talk to GameHUD

namespace Systems
{
    /// <summary>
    /// The central brain of the game loop. 
    /// Tracks objectives, talks to the UI, and handles Win/Lose conditions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [Tooltip("How many items the player needs to collect to win.")]
        [SerializeField] private int totalObjectives = 3;

        [Header("Dependencies")]
        [Tooltip("Reference to the GameHUD script to update the text.")]
        [SerializeField] private GameHUD gameHUD;

        private int collectedObjectives = 0;
        private bool isGameOver = false;

        private void Start()
        {
            // Auto-find the HUD if not assigned
            if (gameHUD == null) gameHUD = FindObjectOfType<GameHUD>();

            // Initialize the UI on spawn
            UpdateUI();
        }

        /// <summary>
        /// Called by the Collectible.cs script when the player presses E.
        /// </summary>
        public void ObjectiveCollected()
        {
            if (isGameOver) return; // Ignore if the game is already over

            collectedObjectives++;
            UpdateUI();

            // Check Win Condition
            if (collectedObjectives >= totalObjectives)
            {
                WinGame();
            }
        }

        /// <summary>
        /// Called by the Guard AI (in a future step) if he touches the player.
        /// </summary>
        public void PlayerCaught()
        {
            if (isGameOver) return;
            
            isGameOver = true;
            Debug.Log("GAME OVER! The guard caught you.");
            
            // For now, we will simply restart the level immediately.
            // In polish phase, we will show a UI screen and delay the reload.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void WinGame()
        {
            isGameOver = true;
            Debug.Log("YOU WIN! All items collected.");
            
            // Unlock cursor so they can click Main Menu buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Load the Main Menu
            SceneManager.LoadScene("MainMenu");
        }

        private void UpdateUI()
        {
            if (gameHUD != null)
            {
                int remaining = totalObjectives - collectedObjectives;
                gameHUD.UpdateObjectiveText(remaining);
            }
        }
    }
}
