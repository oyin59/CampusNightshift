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

        [Header("UI References")]
        [Tooltip("Optional loading screen panel to turn on before async load begins")]
        [SerializeField] private GameObject loadingScreenPanel;
        
        [Tooltip("The panel containing the controls and battery locations")]
        [SerializeField] private GameObject howToPlayPanel;

        [Tooltip("The button that starts the game")]
        [SerializeField] private Button playButton;

        private void Start()
        {
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

            // Begin async operation
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);

            // Wait here until it's done
            while (!asyncLoad.isDone)
            {
                // You could update a progress bar here using asyncLoad.progress (0.0 to 1.0)
                yield return null; 
            }
        }

        public void OnQuitClicked()
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
        }
    }
}
