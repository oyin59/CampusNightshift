using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace UI
{
    /// <summary>
    /// Handles the introductory Splash Screen sequence.
    /// Automatically transitions to the Main Menu after a duration,
    /// or allowing the player to "Click to Skip".
    /// </summary>
    public class SplashManager : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The exact name of your Main Menu scene")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        
        [Tooltip("How many seconds to force the splash screen to stay before auto-loading")]
        [SerializeField] private float autoLoadTime = 4f;

        private bool hasLoaded = false;

        private void Start()
        {
            Debug.Log("SplashManager started! Waiting for input or timer...");
            // Start the countdown timer
            StartCoroutine(AutoLoadMenu());
        }

        private void Update()
        {
            // Check for keyboard press, mouse click, or mobile screen tap
            bool tapOrClick = Input.anyKeyDown || 
                              Input.GetMouseButtonDown(0) || 
                              (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

            // If the player clicks the mouse, taps the screen, or presses any keyboard key, instantly skip!
            if (!hasLoaded && tapOrClick)
            {
                LoadMainMenu();
            }
        }

        private IEnumerator AutoLoadMenu()
        {
            // Wait for the specified time, using Realtime so it's not affected by any Time.timeScale pausing
            yield return new WaitForSecondsRealtime(autoLoadTime);
            
            // Time is up, load the menu automatically!
            if (!hasLoaded)
            {
                LoadMainMenu();
            }
        }

        private void LoadMainMenu()
        {
            hasLoaded = true;
            // Stop any pending auto-load coroutines so it doesn't try to load twice
            StopAllCoroutines();
            
            // Jump to the Main Menu
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
