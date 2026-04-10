using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// Handles the complex bindings between your UI Sliders/Toggles and the player's hard drive saves.
    /// Also applies things like Master Volume instantly.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [Header("Audio Settings")]
        public Slider masterVolumeSlider;
        public TMP_Text masterVolumeText;
        [Space]
        public Slider ambientSoundSlider;
        public TMP_Text ambientSoundText;

        [Header("Gameplay Settings")]
        public Slider mouseSensitivitySlider;
        public TMP_Text mouseSensitivityText;
        [Space]
        public Toggle showHUDToggle;
        public Toggle showMinimapToggle;

        [Header("Account Settings")]
        public TMP_InputField playerNameInput;
        public TMP_Text playerNameDisplay; // The light gray "Currently: GHD" text

        private void Start()
        {
            LoadSettings();

            // Auto-Add listeners to sliders so you don't even have to click the (+) in the inspector manually!
            if (masterVolumeSlider) masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            if (ambientSoundSlider) ambientSoundSlider.onValueChanged.AddListener(SetAmbientSound);
            if (mouseSensitivitySlider) mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
            if (showHUDToggle) showHUDToggle.onValueChanged.AddListener(SetShowHUD);
            if (showMinimapToggle) showMinimapToggle.onValueChanged.AddListener(SetShowMinimap);
            if (playerNameInput) playerNameInput.onValueChanged.AddListener(SetPlayerName);
        }

        /// <summary>
        /// Reads all currently saved values from the Registry / JSON and snaps the UI to match.
        /// </summary>
        public void LoadSettings()
        {
            // 1. Master Volume
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", 80f);
            if (masterVolumeSlider) masterVolumeSlider.value = masterVol;
            SetMasterVolume(masterVol);

            // 2. Ambient Sound
            float ambient = PlayerPrefs.GetFloat("AmbientSound", 60f);
            if (ambientSoundSlider) ambientSoundSlider.value = ambient;
            SetAmbientSound(ambient);

            // 3. Mouse Sensitivity
            float sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 45f);
            if (mouseSensitivitySlider) mouseSensitivitySlider.value = sensitivity;
            SetMouseSensitivity(sensitivity);

            // 4. UI Toggles (1 = true, 0 = false)
            bool showHUD = PlayerPrefs.GetInt("ShowHUD", 1) == 1;
            if (showHUDToggle) showHUDToggle.isOn = showHUD;
            
            bool showMinimap = PlayerPrefs.GetInt("ShowMinimap", 1) == 1;
            if (showMinimapToggle) showMinimapToggle.isOn = showMinimap;

            // 5. Account Name
            string pName = PlayerPrefs.GetString("PlayerName", "GHD");
            if (playerNameInput) playerNameInput.text = pName;
            SetPlayerName(pName);
        }

        // --- PUBLIC HOOKS (Fires dynamically when user drags sliders) ---

        public void SetMasterVolume(float vol)
        {
            PlayerPrefs.SetFloat("MasterVolume", vol);
            if (masterVolumeText) masterVolumeText.text = vol.ToString("0");
            AudioListener.volume = vol / 100f; // Translates 80 down to 0.8
        }

        public void SetAmbientSound(float vol)
        {
            PlayerPrefs.SetFloat("AmbientSound", vol);
            if (ambientSoundText) ambientSoundText.text = vol.ToString("0");
        }

        public void SetMouseSensitivity(float val)
        {
            PlayerPrefs.SetFloat("MouseSensitivity", val);
            if (mouseSensitivityText) mouseSensitivityText.text = val.ToString("0");
        }

        public void SetShowHUD(bool state)
        {
            PlayerPrefs.SetInt("ShowHUD", state ? 1 : 0);
        }

        public void SetShowMinimap(bool state)
        {
            PlayerPrefs.SetInt("ShowMinimap", state ? 1 : 0);
        }

        public void SetPlayerName(string nameStr)
        {
            // Only save if it's not totally empty
            if (string.IsNullOrEmpty(nameStr)) return;

            PlayerPrefs.SetString("PlayerName", nameStr);
            if (playerNameDisplay) playerNameDisplay.text = $"Currently: {nameStr}";
        }

        // --- BUTTON ACTIONS ---
        
        public void SaveSettings()
        {
            // Explicitly force Unity to write keys to hard drive
            PlayerPrefs.Save();
            Debug.Log("Settings completely locked and saved.");
        }

        public void ResetToDefaults()
        {
            // Deletes the saved configuration so it falls back to programmatic defaults
            PlayerPrefs.DeleteKey("MasterVolume");
            PlayerPrefs.DeleteKey("AmbientSound");
            PlayerPrefs.DeleteKey("MouseSensitivity");
            PlayerPrefs.DeleteKey("ShowHUD");
            PlayerPrefs.DeleteKey("ShowMinimap");

            // Refresh the UI bars back to the defaults
            LoadSettings(); 
        }
    }
}
