# Console UNO v1.0 - Cross-Platform & Enhanced

We're excited to announce the first major release of Console UNO, bringing a host of improvements and new features to enhance your gaming experience!

### ✨ New Features & Improvements:

*   **Cross-Platform Sound:** Replaced Windows-specific `Console.Beep()` with `NetCoreAudio`, enabling sound effects and music across various operating systems.
*   **Improved Settings Menu:**
    *   Corrected swapped functionality for music and effects volume controls.
    *   Enhanced volume adjustment with intuitive `+/-` key input.
*   **Interactive Tutorial Page:** A new "Tutorial" option is now available from the main menu, providing clear keybinds and game controls.
*   **Centered UI Elements:** All main menu and game UI elements are now dynamically centered on the console screen for a cleaner, more polished look.
*   **Optimized Animations:** Animation speeds have been fine-tuned for a smoother visual experience.
*   **Code Refactoring:** The codebase has been significantly refactored into separate, organized files (`Game.cs`, `SoundManager.cs`, `Card.cs`, `Program.cs`) for improved maintainability and future development.
*   **Self-Contained Executable:** A self-contained `.exe` build for Windows is now available, allowing the game to be run on machines without the .NET SDK or runtime installed.

### 🛠️ Technical Details:

*   **Build System:** Utilizes `dotnet publish` for self-contained, platform-specific executables.
*   **Dependencies:** Integrated `NetCoreAudio` for cross-platform audio playback.
*   **Project Structure:** Enhanced modularity and readability.

### ⚠️ Important Notes:

*   **Audio Files Required:** For sound effects and music to function, please place `beep.wav` and `music.wav` files in the same directory as the executable (`uno-game/uno-game/` within the project structure). These files are not included in the release package.
*   **Console Environment:** For the best experience, run the game in a standard command prompt or terminal (e.g., PowerShell, Command Prompt on Windows, or Bash on Linux/macOS) as some console features might not be fully supported in all IDE output windows.

We hope you enjoy this enhanced version of Console UNO!