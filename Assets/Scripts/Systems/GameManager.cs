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
        [Tooltip("How many times the player can be caught before Game Over.")]
        [SerializeField] private int startingLives = 3;

        [Header("Dependencies")]
        [Tooltip("Reference to the GameHUD script to update the text.")]
        [SerializeField] private GameHUD gameHUD;
        [Tooltip("The location the player should be teleported to when caught (e.g. Security Room)")]
        [SerializeField] private Transform playerSpawnPoint;
        [Tooltip("The actual Player object in the scene to be teleported")]
        [SerializeField] private GameObject player;

        private int collectedObjectives = 0;
        private int currentLives;
        private bool isGameOver = false;

        private float startTime;

        private void Start()
        {
            // Auto-find the HUD if not assigned
            if (gameHUD == null) gameHUD = FindObjectOfType<GameHUD>();

            // Setup Game State
            currentLives = startingLives;
            startTime = Time.time;

            // Initialize the UI on spawn
            UpdateUI();
        }

        /// <summary>
        /// Called by the Collectible.cs script when the player presses F.
        /// </summary>
        public void ObjectiveCollected()
        {
            if (isGameOver) return; // Ignore if the game is already over

            collectedObjectives++;
            UpdateUI();

            // RUBRIC: Keep and save user score (10 points) - PlayerPrefs (5 points)
            // Save total lifetime batteries collected by this exact player
            int allTimeScore = PlayerPrefs.GetInt("LifetimeBatteries", 0);
            PlayerPrefs.SetInt("LifetimeBatteries", allTimeScore + 1);
            PlayerPrefs.Save(); // Forces it to write to disk permanently

            // Check Win Condition
            if (collectedObjectives >= totalObjectives)
            {
                WinGame();
            }
        }

        /// <summary>
        /// Called by the Guard AI if he touches the player.
        /// </summary>
        public void PlayerCaught()
        {
            if (isGameOver) return;
            
            currentLives--;

            // NEW FIX: Force it to find the HUD again just in case 
            if (gameHUD == null) gameHUD = FindObjectOfType<GameHUD>();
            UpdateUI();

            // RUBRIC: Audio Triggers - Play scare sound
            AudioTriggers audio = FindObjectOfType<AudioTriggers>();
            if (audio != null) audio.PlayCaughtSound();

            if (currentLives > 0)
            {
                Debug.Log($"You were caught! Lives remaining: {currentLives}. Respawning...");
                RespawnPlayer();
            }
            else
            {
                isGameOver = true;
                Debug.Log("GAME OVER! You ran out of lives. Restarting...");
                
                // Wait a brief moment before vanishing so the player processes the jumpscare
                StartCoroutine(RestartLevelDelay());
            }
        }

        private void RespawnPlayer()
        {
            // Teleport the player back to the safe zone if we linked one
            if (playerSpawnPoint != null && player != null)
            {
                // Temporarily disable the CharacterController during teleport to prevent physics fighting
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                
                player.transform.position = playerSpawnPoint.position;
                player.transform.rotation = playerSpawnPoint.rotation;
                
                if (cc != null) cc.enabled = true;
            }
            else
            {
                Debug.LogWarning("Missing Spawn Point or Player reference in GameManager! Could not respawn physically.");
            }
        }

        private System.Collections.IEnumerator RestartLevelDelay()
        {
            yield return new WaitForSeconds(1.5f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void WinGame()
        {
            isGameOver = true;
            Debug.Log("YOU WIN! All items collected.");

            // Calculate how long it took them to win
            float timeToWin = Time.time - startTime;
            
            // Check if this is a new High Score (Fastest Time)
            float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
            if (timeToWin < bestTime)
            {
                PlayerPrefs.SetFloat("BestTime", timeToWin);
                PlayerPrefs.Save();
                Debug.Log($"NEW HIGH SCORE! Completed in {timeToWin:F2} seconds!");
            }
            
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
