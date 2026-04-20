# Campus Night Shift - Prototype Status Report

Here is a comprehensive breakdown of everything we have built and configured in the project so far, organized exactly as you requested for your submission documentation:

---

## 1. Code & Scripts
All scripts were written natively in C# and organized into modular namespaces (`AI`, `Player`, `Systems`, `UI`, `World`) following the "One Script, One Job" philosophy.

**AI folder**
*   `AgentController.cs`: The brain of the Security Guard. It acts as a Finite State Machine managing his `Patrolling` and `Chasing` states. It features a complex detection algorithm using Vector3 distances, a Field-of-Vision cone angle check, and a Physics.Linecast to ensure he cannot see (or catch) the player through solid walls.

**Player folder**
*   `PlayerController.cs`: (Assumed from Standard Assets) Handles standard WASD movement and gravity for the player character.
*   `FollowCamera.cs`: Updates the camera's position to trail behind the player smoothly.
*   `PlayerInteraction.cs`: Sends a Physics Raycast out from the center of the screen every frame. If it hits an object with the `IInteractable` interface, it changes the crosshair color and allows the player to press 'F' to interact.

**Systems folder**
*   `GameManager.cs`: The core game loop manager. It tracks the number of collected objectives, manages a 3-Strike Health/Lives system with respawning, controls the "Game Over" and "Win" scene reloads, and saves the player's Best Time and Lifetime Score using Unity's `PlayerPrefs` system.
*   `AudioTriggers.cs`: Manages all non-spatial sound effects, including looping ambient music, the "item collected" ping, and the jumpscare sound when the Guard catches the player.

**UI folder**
*   `GameHUD.cs`: A purely visual script that receives integers from the GameManager and updates the TextMeshPro elements on the screen without touching the underlying game logic.
*   `MainMenuManager.cs`: Controls the Flow of the Main Menu scene, including asynchronous loading (`SceneManager.LoadSceneAsync`) of the GameLevel, quitting the application, and toggling the "How To Play" panel.

**World folder**
*   `Collectible.cs`: Inherits from `IInteractable`. Attached to the hidden batteries. When interacted with, it notifies the GameManager, plays a sound, and destroys itself.
*   `Door.cs`: Inherits from `IInteractable`. Uses Coroutines and Quaternions to smoothly swing doors open and closed over a set duration. Supports linking a second door to create synchronized Double Doors.

---

## 2. Scenes (What is built and how it looks)
*   **GameLevel**: A sprawling, fully enclosed indoor school environment. It consists of interconnected hallways, a starting Security Room, a Classroom, a Computer Lab, and a Generator Room. The environment is entirely static and baked into a precise Navigation Mesh. Visuals are intentionally dark with limited lighting to emphasize the "Night Shift" theme.
*   **MainMenu**: A separate, lightweight scene featuring a dark, stylized UI canvas overlaid on a static background.

---

## 3. Animations
The Guard character (Mixamo) utilizes an `Animator` component with the following states driven by the `NavMeshAgent`'s speed:
*   **Idle**: Plays when the guard reaches a waypoint and pauses (if a pause was implemented), or before the game begins.
*   **Walking**: The standard patrol loop, tracking his movement speed while he navigates between his 4 designated waypoints around the central hallway loop.
*   **Running/Chasing**: (If implemented in your Animator tree) A faster animation state triggered when his speed increases to intercept the player during the Chase state.

---

## 4. UI in Unity (Implemented vs. Mockup)
*   **In-Game HUD**: Fully implemented. Features a center-screen Crosshair image (that changes color upon detecting interactables), a TextMeshPro objective counter (`Objectives Remaining: 3`), and a dynamically updating Lives counter (`Lives: 3`).
*   **Main Menu**: Implemented your custom UI design. Includes the main stylized title (`NIGHT SHIFT`), a functional `SNEAK IN` button (loads the game asynchronously), a `QUIT` button, and a fully functional `HOW TO PLAY` button that toggles a dark overlay panel containing your custom graphics (Controls list, Mission Objective text, and Battery Locations map).

---

## 5. Recently Implemented Polish & Fixes
*   **Dual-Map System**: Refined the map logic into two specific modes. A small **Minimap** is now visible on the HUD during gameplay, and a **Full-screen Map** automatically appears when the game is paused (ESC). Both maps use the architectural blueprint and track player/guard positions.
*   **Map Calibration Tool**: Developed a custom Unity Editor Utility (`MapCalibrator.cs`) that allows for perfect manual alignment of the minimap tracking coordinates by snapping box-bounds to the edge of the 3D level architecture.
*   **Multi-Level Progression**: Added `GameLevel_2`. The game now supports unlocking and progressing through multiple levels, saving your progress automatically to a JSON file. Level 2 features a higher difficulty (5 batteries) and a faster Security Guard.
*   **Sprinting Mechanic**: Added a sprinting function. Holding `Left Shift` or `R` doubles the player's animation speed and actual movement speed while simultaneously increasing their noise emission to 95%.
*   **Night Summary Page**: Implemented a cross-level tracking system. When the final level is completed, the game calculates the combined Total Time and Total Catches across your entire session, displaying a final operation UI with a grand rank.
*   **Win Screen & Stats Fix**: Corrected the logic for the Mission Complete screen. It now accurately displays "Time Spent," the number of times you were caught, and compare your time against your "Best Time" saved on your computer. If a high score is broken, a celebratory "New High Score!" window will pop up for 3 seconds.
*   **UI Visibility Sync**: The HUD Timer and Minimap are now dynamically hidden during the Intro, Pause Menu, and Game Over states to keep the player's screen visually uncluttered.
*   **Catch Cooldown**: Implemented a 1.5-second invulnerability/cooldown window after being caught to prevent the "frame-lock" glitch where a player could be caught multiple times instantly during a respawn.

---

## 6. Final Assessment Checklist
✔ **Menu and in-game UI**: Main Menu, Game HUD, and Win/Lose Panels are fully functional.
✔ **Several game levels**: Level 1 (3 items) and Level 2 (starts in Bathroom, 5 items) are implemented.
✔ **Non-player character**: NavMesh-driven Security Guard with detection logic is active.
✔ **Save and reload game state**: Progression and high scores are saved to `CampusNightShift_Save.json`.
✔ **Keep and save user score**: Lifetime batteries and Best Times are tracked.
✔ **Audio (Bonus)**: Ambient music and event-triggered sounds are implemented.
