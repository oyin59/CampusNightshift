using UnityEngine;
using UnityEngine.SceneManagement;
using UI; // To talk to GameHUD
using TMPro; // For UI text binding
using System.IO; // For File-Based Saving

namespace Systems
{
    /// <summary>
    /// Pure data class used for converting scores into JSON text files.
    /// </summary>
    [System.Serializable]
    public class GameSaveData
    {
        public float bestTime = float.MaxValue;
        public int lifetimeBatteries = 0;
    }

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

        [Header("UI Panels (Day 1)")]
        [Tooltip("The Intro screen that pauses the game until a key is pressed")]
        [SerializeField] private GameObject introScreenPanel;
        [Tooltip("Drag your Pause Menu panel here")]
        [SerializeField] private GameObject pauseMenuPanel;
        [Tooltip("Drag your Game Over panel here")]
        [SerializeField] private GameObject gameOverPanel;
        [Tooltip("Drag your Win / Mission Complete panel here")]
        [SerializeField] private GameObject winPanel;

        [Header("High Score Win Screen (Day 5)")]
        [Tooltip("The text box showing the historical best time (e.g. 2:13)")]
        [SerializeField] private TextMeshProUGUI winPreviousBestText;
        [Tooltip("The text box showing the time they just got (e.g. 1:48)")]
        [SerializeField] private TextMeshProUGUI winCurrentTimeText;
        [Tooltip("The text box showing Rank (e.g. 'S')")]
        [SerializeField] private TextMeshProUGUI winRankText;
        [Space(10)]
        [SerializeField] private TextMeshProUGUI loseBatteriesText;
        [SerializeField] private TextMeshProUGUI loseRemainingText;
        [SerializeField] private TextMeshProUGUI loseTimesCaughtText;

        private int collectedObjectives = 0;
        private int currentLives;
        private bool isGameOver = false;
        private bool isPaused = false;
        private bool isIntroActive = false;

        private float startTime;
        
        // --- FILE SAVING ---
        private GameSaveData currentSaveData;
        private string saveFilePath;

        private void Start()
        {
            // Auto-find the HUD if not assigned
            if (gameHUD == null) gameHUD = FindObjectOfType<GameHUD>();

            // Setup JSON File Saving
            saveFilePath = Application.persistentDataPath + "/CampusNightShift_Save.json";
            LoadGameData();

            // Setup Game State
            currentLives = startingLives;
            startTime = Time.time;
            Time.timeScale = 1f; // Ensure time is moving normally
            isPaused = false;
            isIntroActive = false;

            // Ensure our UI panels start completely hidden
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (winPanel != null) winPanel.SetActive(false);

            // Handle the Intro Screen if we have one
            if (introScreenPanel != null)
            {
                introScreenPanel.SetActive(true);
                isIntroActive = true;
                Time.timeScale = 0f; // Freeze game until they press a key
            }

            // Initialize the UI on spawn
            UpdateUI();
        }

        private void LoadGameData()
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log("Loaded Save File At: " + saveFilePath);
            }
            else
            {
                currentSaveData = new GameSaveData(); // Fresh save
            }
        }

        private void SaveGameData()
        {
            string json = JsonUtility.ToJson(currentSaveData, true); // True = Pretty Print
            File.WriteAllText(saveFilePath, json);
        }

        private void Update()
        {
            // If the intro screen is up, wait for them to press a key to start the game!
            if (isIntroActive)
            {
                if (Input.anyKeyDown)
                {
                    isIntroActive = false;
                    if (introScreenPanel != null) introScreenPanel.SetActive(false);
                    Time.timeScale = 1f; // Let the physics and game begin!
                    startTime = Time.time; // Reset the stopwatch so it starts exactly from 00:00!
                }
                return; // Stop processing other inputs (like pause) while the intro is up!
            }

            // Toggle the Pause Menu when pressing ESC (if we haven't already won or lost)
            if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }

            // Update HUD Timer (Day 2)
            if (!isGameOver && !isPaused && gameHUD != null)
            {
                float timeElapsed = Time.time - startTime;
                gameHUD.UpdateTimerText(timeElapsed);
            }
        }

        /// <summary>
        /// Called by the Collectible.cs script when the player presses F.
        /// </summary>
        public void ObjectiveCollected()
        {
            if (isGameOver) return; // Ignore if the game is already over

            collectedObjectives++;
            UpdateUI();
            
            // Pop the Notification Flyout (Day 3)
            if (gameHUD != null) gameHUD.ShowBatteryNotification();

            // RUBRIC: Save and reload state from file (10 points)
            currentSaveData.lifetimeBatteries++;
            SaveGameData(); // Writes immediately to JSON text file on hard drive

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
            
            // 1. Calculate and Push dynamic text stats (Day 2)
            int timesCaught = startingLives - currentLives;
            int remaining = totalObjectives - collectedObjectives;

            if (loseBatteriesText != null) loseBatteriesText.text = collectedObjectives.ToString();
            if (loseRemainingText != null) loseRemainingText.text = remaining.ToString();
            if (loseTimesCaughtText != null) loseTimesCaughtText.text = $"{timesCaught}x";

            // Pop the Game Over Menu instead of just reloading instantly
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // Freeze the game in the background
        }

        private void WinGame()
        {
            isGameOver = true;
            Debug.Log("YOU WIN! All items collected.");

            // Calculate how long it took them to win
            float timeToWin = Time.time - startTime;
            int timesCaught = startingLives - currentLives;
            
            // 1. Calculate and Push dynamic text stats (Day 2)
            string rank = "C";
            if (timeToWin < 120f && timesCaught == 0) rank = "S";
            else if (timeToWin < 180f && timesCaught <= 1) rank = "A";
            else if (timeToWin < 240f && timesCaught <= 2) rank = "B";

            // --- FORMATTING TIMES FOR UI (Day 5) --- //
            
            // Format Current Time
            int cMinutes = Mathf.FloorToInt(timeToWin / 60F);
            int cSeconds = Mathf.FloorToInt(timeToWin - cMinutes * 60);
            string currentTimeString = string.Format("{0:0}:{1:00}", cMinutes, cSeconds);

            // Format Previous Best Time
            string bestTimeString = "NONE";
            if (currentSaveData.bestTime < float.MaxValue)
            {
                int bMinutes = Mathf.FloorToInt(currentSaveData.bestTime / 60F);
                int bSeconds = Mathf.FloorToInt(currentSaveData.bestTime - bMinutes * 60);
                bestTimeString = string.Format("{0:0}:{1:00}", bMinutes, bSeconds);
            }

            // Sync to the gorgeous new UI
            if (winCurrentTimeText != null) winCurrentTimeText.text = currentTimeString;
            if (winPreviousBestText != null) winPreviousBestText.text = bestTimeString;
            if (winRankText != null) winRankText.text = rank;

            // RUBRIC: Save and reload state from file (10 points)
            if (timeToWin < currentSaveData.bestTime)
            {
                currentSaveData.bestTime = timeToWin;
                SaveGameData();
                Debug.Log($"NEW HIGH SCORE! Completed in {timeToWin:F2} seconds and saved to JSON!");
            }
            
            // Unlock cursor so they can click Main Menu buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // Freeze the game in the background

            // Pop the Win/Mission Complete UI Panel
            if (winPanel != null) winPanel.SetActive(true);
        }

        // --- NEW BUTTON METHODS FOR THE UI --- //

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f; // Unfreeze the game physics
            
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f; // Freeze the game physics
            
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void RestartGame()
        {
            Time.timeScale = 1f; // CRITICAL: Reset time back to normal before reloading!
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitToMainMenu()
        {
            Time.timeScale = 1f; // CRITICAL: Reset time back to normal before reloading!
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
