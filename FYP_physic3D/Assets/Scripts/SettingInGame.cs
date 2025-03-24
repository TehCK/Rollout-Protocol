using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingInGame : MonoBehaviour
{
    public GameObject settingsMenu;
    private bool isPaused = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            settingsMenu.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            settingsMenu.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void CloseSettings()
    {
        isPaused = false;
        settingsMenu.SetActive(false);
        Time.timeScale = 1f;
    }
}
