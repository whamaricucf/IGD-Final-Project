using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveScenes : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        PauseFunc();
    }

    void PauseFunc()
    {
        // if PauseButton pressed
          // if Time.timeScale != 0
            // PauseGame()
            // PauseMenu()
          // else 
            // Resume()
    }

    void PauseMenu()
    {
        // load Pause scene additively
    }

    void UpgradeMenu()
    {
        // PauseGame()
        // load Upgrade menu additively
    }
    
    void PauseGame()
    {
        // Time.timeScale = 0
    }
    void Resume()
    {
        // unload Pause scene & Upgrade scene
        // Time.timeScale = 1
    }
}
