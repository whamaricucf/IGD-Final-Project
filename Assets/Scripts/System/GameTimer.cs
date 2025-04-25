using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText; // Reference to the TextMeshProUGUI element
    private float startTime;
    private float pausedTime;
    private bool isTimerActive = false;
    private bool isPaused = false;

    void Start()
    {
        // Initialize the timer and start tracking the time
        startTime = Time.time;
        isTimerActive = true;
    }

    void Update()
    {
        if (isTimerActive)
        {
            if (isPaused)
            {
                // Save the time when the game was paused
                pausedTime = Time.time - startTime;
            }
            else
            {
                // Track the time based on the Time.time and the paused time
                float elapsedTime = Time.time - startTime - pausedTime;

                // Convert to minutes and seconds
                int minutes = Mathf.FloorToInt(elapsedTime / 60f);
                int seconds = Mathf.FloorToInt(elapsedTime % 60f);

                // Update the timer UI
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    // Call this method from the GameManager when the game is paused
    public void PauseTimer()
    {
        isPaused = true;
    }

    // Call this method from the GameManager when the game is resumed
    public void ResumeTimer()
    {
        isPaused = false;
        startTime = Time.time - pausedTime;
    }

    // Call this method if you want to stop the timer
    public void StopTimer()
    {
        isTimerActive = false;
    }
}
