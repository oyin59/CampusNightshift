using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections; // Required for Coroutines

namespace UI
{
    /// <summary>
    /// Handles the Main Menu logic.
    /// Requirement: Asynchronous scene loading (Lab 2).
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [Tooltip("The exact name of your main gameplay scene (e.g., 'GameLevel')")]
        [SerializeField] private string gameSceneName = "GameLevel";
        
        [Header("Save Data Integration")]
        [Tooltip("The names of the levels in the same order as GameManager")]
        [SerializeField] private string[] levelList = { "GameLevel", "GameLevel_2" };

        [Header("UI References")]
        [Tooltip("Optional loading screen panel to turn on before async load begins")]
        [SerializeField] private GameObject loadingScreenPanel;
        
        [Tooltip("The panel containing the controls and battery locations")]
        [SerializeField] private GameObject howToPlayPanel;

        [Tooltip("The button that starts the game")]
        [SerializeField] private Button playButton;

        private void Start()
        {
            // CRITICAL FIX: If the player returned from a Pause or Game Over screen, 
            // Time.timeScale was left at 0! We must reset it so Main Menu animations run!
            Time.timeScale = 1f;

            // 1. Ensure the cursor is unlocked and visible so the player can click the menu
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 2. Hide loading screen and HowToPlay panel
            if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
            
            if (playButton != null)
            {
                // This is the code version of clicking the '+' in the inspector
                playButton.onClick.AddListener(OnPlayClicked);
            }
            else
            {
                Debug.LogWarning("MainMenuManager: Play button is not assigned!");
            }
        }

        public void OpenHowToPlay()
        {
            if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        }

        public void CloseHowToPlay()
        {
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        }

        public void OnPlayClicked()
        {
            // Disable the button so they can't spam click it
            playButton.interactable = false;

            // Start the asynchronous loading Coroutine
            StartCoroutine(LoadGameAsync());
        }

        /// <summary>
        /// Coroutine to load the game scene in the background without freezing the app.
        /// </summary>
        private IEnumerator LoadGameAsync()
        {
            // Show loading screen if assigned
            if (loadingScreenPanel != null) loadingScreenPanel.SetActive(true);

            // --- SAVE FILE LOGIC ---
            string finalSceneToLoad = gameSceneName;
            string savePath = Application.persistentDataPath + "/CampusNightShift_Save.json";
            
            if (System.IO.File.Exists(savePath))
            {
                string json = System.IO.File.ReadAllText(savePath);
                // We use a simple JSON wrapper to just get the level index
                var data = JsonUtility.FromJson<Systems.GameSaveData>(json);
                int index = data.lastUnlockedLevel - 1; // 1-indexed to 0-indexed
                
                if (index >= 0 && index < levelList.Length)
                {
                    finalSceneToLoad = levelList[index];
                }
            }

            // Begin async operation
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(finalSceneToLoad);
            
            // Prevent Unity from jumping to the next scene the millisecond it finishes loading
            // We want to force it to watch our beautiful animation hit 100% first!
            asyncLoad.allowSceneActivation = false;

            float visualProgress = 0f;

            // Wait until the real load and our fake visual load are both fully finished
            while (!asyncLoad.isDone)
            {
                // Unity's load stops at 0.9. We map that back to a 0.0 -> 1.0 target scale.
                float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                // ANIMATION MAGIC: Smoothly glide our visual progress towards the target progress
                // Since there are no moving visuals anymore, 5 seconds is perfect to hold the Title Screen.
                visualProgress = Mathf.MoveTowards(visualProgress, targetProgress, 0.2f * Time.unscaledDeltaTime);

                // If the scene is fully loaded AND our completely 100% finished waiting
                if (asyncLoad.progress >= 0.9f && visualProgress >= 1f)
                {
                    // Unleash the scene transition!
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null; 
            }
        }

        public void OnQuitClicked()
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
