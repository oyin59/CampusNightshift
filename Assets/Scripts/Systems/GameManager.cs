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
        public int lastUnlockedLevel = 1; // 1 = Level 1, 2 = Level 2, etc.
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
        [Tooltip("The text box showing the total number of times the player was caught on the Win Screen")]
        [SerializeField] private TextMeshProUGUI winTimesCaughtText;
        [Tooltip("A UI Panel that pops up to celebrate a new high score before the Win screen appears")]
        [SerializeField] private GameObject newHighScorePanel;

        [Header("Night Summary Screen (Level 2 Finale)")]
        [SerializeField] private GameObject nightSummaryPanel;
        [SerializeField] private TextMeshProUGUI summaryBestTimeText;
        [SerializeField] private TextMeshProUGUI summaryTotalTimeText;
        [SerializeField] private TextMeshProUGUI summaryObjectivesText;
        [SerializeField] private TextMeshProUGUI summaryTotalCatchesText;
        [SerializeField] private TextMeshProUGUI summaryRankText;
        [Space(10)]
        [SerializeField] private TextMeshProUGUI loseBatteriesText;
        [SerializeField] private TextMeshProUGUI loseRemainingText;
        [SerializeField] private TextMeshProUGUI loseTimesCaughtText;

        [Header("Level Flow")]
        [Tooltip("The names of the scenes in order (Level 1, Level 2...)")]
        [SerializeField] private string[] levelSequence = { "GameLevel", "GameLevel_2" };
        [SerializeField] private GameObject nextLevelButton; // Button to show instead of Main Menu on Win Panel

        [Header("Developer Tools (Demo Use)")]
        [Tooltip("The main camera attached to the player")]
        [SerializeField] private Camera mainPlayerCamera;
        [Tooltip("The overhead Dev Camera to show the whole map")]
        [SerializeField] private Camera devGodCamera;

        private int collectedObjectives = 0;
        private int currentLives;
        private bool isGameOver = false;
        private bool isPaused = false;
        private bool isIntroActive = false;
        private bool isRecentlyCaught = false; // Prevents being caught multiple times in one frame

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
            if (newHighScorePanel != null) newHighScorePanel.SetActive(false);
            if (nightSummaryPanel != null) nightSummaryPanel.SetActive(false);

            // Handle the Intro Screen if we have one
            if (introScreenPanel != null)
            {
                introScreenPanel.SetActive(true);
                isIntroActive = true;
                Time.timeScale = 0f; // Freeze game until they press a key
                if (gameHUD != null) 
                {
                    gameHUD.SetMiniMapVisibility(false); // Hide minimap while intro is up
                    gameHUD.SetTimerVisibility(false);
                }
            }

            // Initialize the UI on spawn
            isRecentlyCaught = false;
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
                    if (gameHUD != null) 
                    {
                        gameHUD.SetMiniMapVisibility(true); // Show minimap when gameplay starts
                        gameHUD.SetTimerVisibility(true); // Show timer
                    }
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

            // Developer God-View Toggle (Press 'V')
            if (Input.GetKeyDown(KeyCode.V) && !isPaused && !isIntroActive)
            {
                if (mainPlayerCamera != null && devGodCamera != null)
                {
                    bool isDevActive = devGodCamera.gameObject.activeSelf;
                    // Toggle exactly the opposite!
                    devGodCamera.gameObject.SetActive(!isDevActive);
                    mainPlayerCamera.gameObject.SetActive(isDevActive);
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
            if (isGameOver || isRecentlyCaught) return;
            
            isRecentlyCaught = true;
            currentLives--;

            // Refresh HUD UI
            UpdateUI();

            // RUBRIC: Audio Triggers - Play scare sound
            AudioTriggers audio = FindObjectOfType<AudioTriggers>();
            if (audio != null) audio.PlayCaughtSound();

            if (currentLives > 0)
            {
                Debug.Log($"You were caught! Lives remaining: {currentLives}. Respawning...");
                RespawnPlayer();
                StartCoroutine(CatchCooldownRoutine());
            }
            else
            {
                isGameOver = true;
                Debug.Log("GAME OVER! You ran out of lives. Restarting...");
                
                // Wait a brief moment before vanishing so the player processes the jumpscare
                StartCoroutine(RestartLevelDelay());
            }
        }

        private System.Collections.IEnumerator CatchCooldownRoutine()
        {
            // Wait 1.5 seconds after being caught before you can be caught again
            yield return new WaitForSeconds(1.5f);
            isRecentlyCaught = false;
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
            if (gameHUD != null) 
            {
                gameHUD.SetMiniMapVisibility(false); // Hide the minimap
                gameHUD.SetTimerVisibility(false);
            }
            
            // Sync current level to save data on failure too just in case
            currentSaveData.lastUnlockedLevel = GetCurrentLevelIndex() + 1;
            SaveGameData();

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
            if (winTimesCaughtText != null) winTimesCaughtText.text = $"{timesCaught}x";

            // RUBRIC: Save and reload state from file (10 points)
            int currentLevel = GetCurrentLevelIndex() + 1;
            if (currentLevel >= currentSaveData.lastUnlockedLevel)
            {
                currentSaveData.lastUnlockedLevel = currentLevel + 1;
            }

            bool isNewHighScore = false;
            if (timeToWin < currentSaveData.bestTime)
            {
                currentSaveData.bestTime = timeToWin;
                isNewHighScore = true;
                Debug.Log($"NEW HIGH SCORE! Completed in {timeToWin:F2} seconds and saved to JSON!");
            }
            SaveGameData();
            
            // Check if there is a next level
            bool hasNextLevel = currentLevel < levelSequence.Length;
            
            // --- NIGHT SUMMARY TRACKING ---
            if (hasNextLevel)
            {
                // Save Level 1 stats to pass to Level 2
                PlayerPrefs.SetFloat("L1_Time", timeToWin);
                PlayerPrefs.SetInt("L1_Catches", timesCaught);
                PlayerPrefs.Save();
            }
            else
            {
                // This is the final level! Prepare the Night Summary stats.
                float l1Time = PlayerPrefs.GetFloat("L1_Time", 0f);
                int l1Catches = PlayerPrefs.GetInt("L1_Catches", 0);
                
                float totalNightTime = l1Time + timeToWin;
                int totalNightCatches = l1Catches + timesCaught;

                // Format the Total Night Time
                int nMinutes = Mathf.FloorToInt(totalNightTime / 60F);
                int nSeconds = Mathf.FloorToInt(totalNightTime - nMinutes * 60);

                // Calculate Grand Rank
                string grandRank = "C";
                if (totalNightTime < 240f && totalNightCatches == 0) grandRank = "S";
                else if (totalNightTime < 360f && totalNightCatches <= 2) grandRank = "A";
                else if (totalNightTime < 480f && totalNightCatches <= 4) grandRank = "B";

                // Map to UI
                if (summaryTotalTimeText != null) summaryTotalTimeText.text = string.Format("{0:0}:{1:00}", nMinutes, nSeconds);
                if (summaryTotalCatchesText != null) summaryTotalCatchesText.text = $"{totalNightCatches}x";
                if (summaryRankText != null) summaryRankText.text = grandRank;
                
                // Extra 2 Fields to match your custom grid exactly!
                if (summaryObjectivesText != null) summaryObjectivesText.text = "8 / 8"; // Total items in the entire game (3+5)
                
                if (summaryBestTimeText != null)
                {
                    if (currentSaveData.bestTime < float.MaxValue)
                    {
                        int bMinutes = Mathf.FloorToInt(currentSaveData.bestTime / 60F);
                        int bSeconds = Mathf.FloorToInt(currentSaveData.bestTime - bMinutes * 60);
                        summaryBestTimeText.text = string.Format("{0:0}:{1:00}", bMinutes, bSeconds);
                    }
                    else
                    {
                        summaryBestTimeText.text = "NONE";
                    }
                }
            }

            if (nextLevelButton != null) nextLevelButton.SetActive(hasNextLevel);

            // Unlock cursor so they can click Main Menu buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // Freeze the game in the background

            if (gameHUD != null) 
            {
                gameHUD.SetMiniMapVisibility(false); // Hide the minimap
                gameHUD.SetTimerVisibility(false);
            }

            if (isNewHighScore && newHighScorePanel != null)
            {
                StartCoroutine(ShowHighScorePopupRoutine());
            }
            else
            {
                // Pop the Win/Mission Complete UI Panel
                if (winPanel != null) winPanel.SetActive(true);
            }
        }

        private System.Collections.IEnumerator ShowHighScorePopupRoutine()
        {
            // Show the celebration popup
            if (newHighScorePanel != null) newHighScorePanel.SetActive(true);
            
            // Wait 3 seconds in real-time (because Time.timeScale is currently 0)
            yield return new WaitForSecondsRealtime(3.0f);
            
            // Hide the popup and show the actual win screen
            if (newHighScorePanel != null) newHighScorePanel.SetActive(false);
            if (winPanel != null) winPanel.SetActive(true);
        }

        public void LoadNextLevel()
        {
            int currentLvl = GetCurrentLevelIndex();
            if (currentLvl + 1 < levelSequence.Length)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(levelSequence[currentLvl + 1]);
            }
            else
            {
                QuitToMainMenu();
            }
        }

        private int GetCurrentLevelIndex()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            for (int i = 0; i < levelSequence.Length; i++)
            {
                if (levelSequence[i] == currentScene) return i;
            }
            return 0;
        }

        // --- NEW BUTTON METHODS FOR THE UI --- //

        public void OpenNightSummary()
        {
            if (winPanel != null) winPanel.SetActive(false);
            if (nightSummaryPanel != null) nightSummaryPanel.SetActive(true);
        }

        public void PlayAgain()
        {
            Time.timeScale = 1f; // CRITICAL: Reset time back to normal before reloading!
            
            // Clear stats for a fresh run
            PlayerPrefs.SetFloat("L1_Time", 0f);
            PlayerPrefs.SetInt("L1_Catches", 0);
            PlayerPrefs.Save();
            
            // Force load Level 1
            if (levelSequence.Length > 0)
            {
                SceneManager.LoadScene(levelSequence[0]);
            }
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f; // Unfreeze the game physics
            
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (gameHUD != null) 
            {
                gameHUD.SetFullMapVisibility(false); // Hide the large map when resuming
                gameHUD.SetMiniMapVisibility(true);  // Show the minimap
                gameHUD.SetTimerVisibility(true);    // Show the timer
            }
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f; // Freeze the game physics
            
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            if (gameHUD != null) 
            {
                gameHUD.SetFullMapVisibility(true); // Automatically show the large map when pausing
                gameHUD.SetMiniMapVisibility(false); // Hide the minimap
                gameHUD.SetTimerVisibility(false);   // Hide the timer
            }
            
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
            if (gameHUD == null) gameHUD = FindObjectOfType<GameHUD>();
            
            if (gameHUD != null)
            {
                int remaining = totalObjectives - collectedObjectives;
                gameHUD.UpdateObjectiveText(remaining);
                gameHUD.UpdateLivesText(currentLives);
            }
        }
    }
}
