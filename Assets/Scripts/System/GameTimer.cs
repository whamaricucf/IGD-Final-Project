using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    private float elapsedTime = 0f;
    private bool isTimerActive = false;
    private bool isPaused = false;

    void Start()
    {
        elapsedTime = 0f;
        isTimerActive = true;
    }

    void Update()
    {
        if (isTimerActive && !isPaused)
        {
            elapsedTime += Time.deltaTime; // <- Add deltaTime manually (time since last frame)

            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        isPaused = false;
    }

    public void StopTimer()
    {
        isTimerActive = false;
    }
}
