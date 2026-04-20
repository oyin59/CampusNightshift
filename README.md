# 🌙 Campus Night Shift 🔦

Welcome to **Campus Night Shift**! ✨ A charming yet thrilling stealth-adventure prototype built in Unity (C#).

Slip into the role of a sneaky nocturnal explorer wandering a sprawling indoor school environment. Your mission? Collect hidden shiny batteries scattered around the campus. But watch out! 🤫 The friendly (but very strict) security guard is on his night patrol. Keep out of sight, stay quiet, and try not to lose your 3 lives! 

---

## ✨ Features

*   **🕵️‍♀️ Stealthy Gameplay**: Tip-toe through the halls and avoid the security guard's line-of-sight (`Physics.Linecast`) and Field-of-Vision (FOV).
*   **🤖 Smart Enemy AI**: The guard is equipped with a custom Finite State Machine. He smoothly transitions from calmly `Patrolling` the halls to quickly `Chasing` you down when spotted!
*   **🏃‍♂️ Risk & Reward Sprinting**: Need to move fast? Hold `Shift` to sprint, but be careful! Sprinting doubles your speed but expands your noise detection radius to alert the AI.
*   **🗺️ Dual-Map Cartography**: Never get lost. Use the on-screen Minimap during active gameplay, or pause the game (`ESC`) to reveal the full-screen architectural blueprint tracking all entities in real-time.
*   **💾 Persistent Multi-Level Progression**: Safely quit at any time. A custom JSON serializer saves your unlocked levels, while `PlayerPrefs` handles cross-level stat accumulation for the final Night Summary screen.
*   **🎮 Complete Game Loop**: A robust `GameManager` seamlessly handles your 3-Strike life system, respawns, chronological level transitioning, and final grading!
*   **💖 Immersive UI**: Enjoy a beautifully stylized dark UI, a helpful "How to Play" guide, and a dynamic contextual HUD that hides itself cleanly during cutscenes.

---

## 🎮 How to Play

*   **`W A S D`** - Move around 🐾
*   **`Mouse`** - Look around 👀
*   **`Left Shift / R`** - Sprint (Increases noise radius!) 🏃
*   **`F`** - Interact (Open doors, pick up batteries) ✋
*   **`ESC`** - Pause & View Full-Screen Blueprint Map 🗺️
*   **`V`** - [Developer Tool] Toggle God-View/Sky Camera ☁️

---

## 🏗️ Under the Hood (Architecture)

We love clean code! Our architecture follows a strict "One Script, One Job" modular philosophy, neatly organized into dedicated namespaces:

*   **🧠 AI**: `AgentController` gives life to the guard's behavior, sight, and patrol flow.
*   **🏃 Player**: `PlayerController`, `FollowCamera`, and `PlayerInteraction` make sure your movement and interactions feel just right.
*   **⚙️ Systems**: `GameManager` is the heart of the game, while `AudioTriggers` handles the delightful ambiance and sound effects.
*   **🖼️ UI**: `GameHUD` and `MainMenuManager` bridge the gap between complex logic and beautiful visuals.
*   **🌍 World**: `Door` and `Collectible` components bring the static school environment to life.

---

## 🎉 Development Status

The **Campus Night Shift** prototype is officially **100% complete and feature-locked!** 🎉 
Every requirement for the academic module has been successfully integrated, including:
*   Multi-stage levels with JSON progressive saving.
*   A persistent total-score generator (Night Summary).
*   Advanced UI visibility systems.
*   Developer testing modules built natively into the C# GameManager.

---
*Organized with ❤️ and built with C# in Unity.*
