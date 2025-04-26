using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float startTime;
    private float elapsedTime;
    private bool isTimerActive = false;
    private bool isPaused = false;

    void Start()
    {
        startTime = Time.realtimeSinceStartup;
        isTimerActive = true;
    }

    void Update()
    {
        if (isTimerActive && !isPaused)  // Check if the timer is not paused
        {
            elapsedTime = Time.realtimeSinceStartup - startTime; // Calculate elapsed time
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);  // Update timer UI
        }
    }

    public void PauseTimer()
    {
        isPaused = true;  // Pause the timer
    }

    public void ResumeTimer()
    {
        isPaused = false;
        startTime = Time.realtimeSinceStartup - elapsedTime;  // Resume from where it left off
    }

    public void StopTimer()
    {
        isTimerActive = false;
    }
}
