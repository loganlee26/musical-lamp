using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // Import TextMeshPro namespace
using UnityEngine.UI;  // Import UI namespace for Button

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI timeDisplay;  // Reference to the TextMeshProUGUI component for time display
    public Button settingsButton;  // Reference to the settings button
    public Button restartButton;   // Reference to the restart button inside the settings panel
    public GameObject settingsPanel;  // Reference to the settings panel
    public MazeGenerator mazeGenerator;  // Reference to the MazeGenerator script
    public PlayerController playerController;  // Reference to the PlayerController script

    public GameObject settingsContent;  // The content holding the regular settings UI (buttons, sliders, etc.)
    public GameObject congratsContent;  // The content showing the congrats message inside the settings panel
    public TextMeshProUGUI congratsMessage;  // The TextMeshProUGUI that will show the congratulations message

    private float elapsedTime = 0f;  // Timer to keep track of the elapsed time
    private bool isGameOver = false;  // Flag to check if the game is over

    void Start()
    {
        settingsPanel.SetActive(false);
        // Set the button click events
        settingsButton.onClick.AddListener(ToggleSettingsPanel);
        restartButton.onClick.AddListener(RestartGame);  // Assign the restart functionality

        // Make sure the congrats content is initially hidden
        congratsContent.SetActive(false);
    }

    void Update()
    {
        // Only update the time if the game is not over
        if (!isGameOver)
        {
            elapsedTime += Time.deltaTime;
        }

        // Update the time display text (TextMeshPro)
        timeDisplay.text = "Time: " + elapsedTime.ToString("F2") + "s";  // Display with 2 decimal places
    }

    // Handle what happens when the settings button is clicked
    public void ToggleSettingsPanel()
    {
        // Toggle the visibility of the settings panel
        settingsPanel.SetActive(!settingsPanel.activeSelf);

        // Optionally, you can pause the game when the settings are opened
        if (settingsPanel.activeSelf)
        {
            Time.timeScale = 0;  // Pause the game
        }
        else
        {
            Time.timeScale = 1;  // Resume the game
        }
    }

    // Method to stop the timer when the player reaches the end tile
    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Debug.Log("Maze Solved! You've reached the end!");

        // Show the congrats message in the settings panel
        ShowCongratsMessage();        
    }

    // Show the congratulations message inside the settings panel
    public void ShowCongratsMessage()
    {
        // Hide the regular settings content and show the congrats content
        settingsContent.SetActive(false);
        congratsContent.SetActive(true);

        // Optionally, set the message text if you want to change it dynamically
        congratsMessage.text = "Congratulations! You've solved the maze in " + elapsedTime.ToString("F2") + " seconds!";
        ToggleSettingsPanel();
    }

    // Restart the game by resetting everything
    public void RestartGame()
    {
        // Reset the timer and game state
        elapsedTime = 0f;
        isGameOver = false;
        timeDisplay.text = "Time: 0.00s";  // Reset the time display text
        Time.timeScale = 1;  // Ensure the game is resumed (in case it was paused)

        // Reset the maze (this will regenerate the maze)
        mazeGenerator.GenerateMaze(1, 1);  // You can customize this method to reset the maze to its starting state
        mazeGenerator.DrawMaze();

        // Reset the player's position (assuming the player starts at (0,0))
        playerController.transform.position = new Vector3(1.5f, 1.5f, 0);  // Reset the player's position (you can change it if needed)

        // Hide the settings panel if it's open
        settingsPanel.SetActive(false);

        // Hide congrats content and show settings again if restarting the game
        congratsContent.SetActive(false);
        settingsContent.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit(); // Closes the game

        // NOTE: This won¡¯t work in the Unity Editor. To test it in the editor:
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
