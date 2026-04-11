# 🌙 Campus Night Shift 🔦

Welcome to **Campus Night Shift**! ✨ A charming yet thrilling stealth-adventure prototype built in Unity (C#).

Slip into the role of a sneaky nocturnal explorer wandering a sprawling indoor school environment. Your mission? Collect hidden shiny batteries scattered around the campus. But watch out! 🤫 The friendly (but very strict) security guard is on his night patrol. Keep out of sight, stay quiet, and try not to lose your 3 lives! 

---

## ✨ Features

*   **🕵️‍♀️ Stealthy Gameplay**: Tip-toe through the halls and avoid the security guard's line-of-sight and Field-of-Vision (FOV).
*   **🤖 Smart Enemy AI**: The guard is equipped with a custom Finite State Machine. He smoothly transitions from calmly `Patrolling` the halls to quickly `Chasing` you down when spotted!
*   **🚪 Interactive World**: Peek around corners and interact with single and double doors. Find and collect hidden batteries using a precise physics-based raycast system.
*   **🎮 Complete Game Loop**: A robust `GameManager` seamlessly handles your 3-Strike life system, respawns, Win/Lose conditions, and magically remembers your Best Time and Lifetime Score. 🏆
*   **💖 Immersive UI**: Enjoy a beautifully stylized Main Menu with smooth scene loading, a helpful "How to Play" guide, and a dynamic in-game HUD that tracks your important stats.

---

## 🎮 How to Play

*   **`W A S D`** - Move around 🐾
*   **`Mouse`** - Look around 👀
*   **`F`** - Interact (Open doors, pick up batteries) ✋

---

## 🏗️ Under the Hood (Architecture)

We love clean code! Our architecture follows a strict "One Script, One Job" modular philosophy, neatly organized into dedicated namespaces:

*   **🧠 AI**: `AgentController` gives life to the guard's behavior, sight, and patrol flow.
*   **🏃 Player**: `PlayerController`, `FollowCamera`, and `PlayerInteraction` make sure your movement and interactions feel just right.
*   **⚙️ Systems**: `GameManager` is the heart of the game, while `AudioTriggers` handles the delightful ambiance and sound effects.
*   **🖼️ UI**: `GameHUD` and `MainMenuManager` bridge the gap between complex logic and beautiful visuals.
*   **🌍 World**: `Door` and `Collectible` components bring the static school environment to life.

---

## 🛠️ What's Next? (Development Status)

The prototype is currently **fully playable** from start to finish! 🎉 We are now in the cozy **polish phase**, squashing bugs and adding finishing touches:

*   🚪 Smoothing out interaction physics (so doors don't bump you around too much!).
*   🎵 Adding tippy-tap footstep audio to sync perfectly with walking animations.
*   🐞 Fixing a silly visual glitch where the UI gets overwhelmed if you are caught multiple times really fast.
*   🗺️ Crafting a handy little minimap to help you safely navigate the campus.

---
*Organized with ❤️ and built with C# in Unity.*
