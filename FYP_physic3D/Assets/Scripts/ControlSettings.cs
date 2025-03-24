using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using TMPro;
using TMPro.Examples;

public class ControlSettings : MonoBehaviour
{
    public CinemachineFreeLook cinemachineFreeLook;
    [Header("Mouse Control")]
    public Slider horizontalSenSlider;
    public Slider verticalSenSlider;

    [Header("Keyboard Control")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode rollKey = KeyCode.LeftControl;
    public TextMeshProUGUI forwardText;
    public TextMeshProUGUI backwardText;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI rightText;
    public TextMeshProUGUI sprintText;
    public TextMeshProUGUI jumpText;
    public TextMeshProUGUI rollText;
    public GameObject KeySetPanel;

    private PlayerMovement playerMovement;


    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (PlayerPrefs.HasKey("MouseSensitivityX"))
            LoadSensitivityX();
        else
            SetSensitivityX();

        if (PlayerPrefs.HasKey("MouseSensitivityY"))
            LoadSensitivityY();
        else
            SetSensitivityY();

        LoadKeyBindings();
        UpdateButtonText();
    }

    public void SetSensitivityX()
    {
        float horizontal = horizontalSenSlider.value;
        PlayerPrefs.SetFloat("MouseSensitivityX", horizontal);

        if (cinemachineFreeLook != null)
        {
            cinemachineFreeLook.m_XAxis.m_MaxSpeed = horizontal;
        }
    }

    public void SetSensitivityY()
    {
        float vertical = verticalSenSlider.value;
        PlayerPrefs.SetFloat("MouseSensitivityY", vertical);

        if (cinemachineFreeLook != null)
        {
            cinemachineFreeLook.m_YAxis.m_MaxSpeed = vertical;
        }
    }

    private void LoadSensitivityX()
    {
        horizontalSenSlider.value = PlayerPrefs.GetFloat("MouseSensitivityX");
        SetSensitivityX();
    }

    private void LoadSensitivityY()
    {
        verticalSenSlider.value = PlayerPrefs.GetFloat("MouseSensitivityY");
        SetSensitivityY();
    }

    public void AssignKey(string action)
    {
        StartCoroutine(WaitForKeyPress(action));
    }

    public IEnumerator WaitForKeyPress(string action)
    {
        yield return null;
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (Input.GetKeyDown(key))
                {
                    SetKey(action, key);
                    UpdateButtonText();
                    LoadKeyBindings();
                    KeySetPanel.SetActive(false);
                    break;
                }
            }
        }
    }

    private void UpdateButtonText()
    {
        forwardText.text = forwardKey.ToString();
        backwardText.text = backwardKey.ToString();
        leftText.text = leftKey.ToString();
        rightText.text = rightKey.ToString();
        sprintText.text = sprintKey.ToString();
        jumpText.text = jumpKey.ToString();
        rollText.text = rollKey.ToString();
    }

    public void SetKey(string action, KeyCode newKey)
    {
        PlayerPrefs.SetString(action, newKey.ToString());

        switch (action)
        {
            case "Forward": forwardKey = newKey; break;
            case "Backward": backwardKey = newKey; break;
            case "Left": leftKey = newKey; break;
            case "Right": rightKey = newKey; break;
            case "Sprint": sprintKey = newKey; break;
            case "Jump": jumpKey = newKey; break;
            case "Roll": rollKey = newKey; break;
        }
    }

    private void LoadKeyBindings()
    {
        forwardKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Forward", KeyCode.W.ToString()));
        backwardKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Backward", KeyCode.S.ToString()));
        leftKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Left", KeyCode.A.ToString()));
        rightKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Right", KeyCode.D.ToString()));
        sprintKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Sprint", KeyCode.LeftShift.ToString()));
        jumpKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Jump", KeyCode.Space.ToString()));
        rollKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Roll", KeyCode.LeftControl.ToString()));


        if (playerMovement != null)
        {
            playerMovement.forwardKey = forwardKey;
            playerMovement.backwardKey = backwardKey;
            playerMovement.leftKey = leftKey;
            playerMovement.rightKey = rightKey;
            playerMovement.sprintKey = sprintKey;
            playerMovement.jumpKey = jumpKey;
            playerMovement.rollKey = rollKey;
        }
    }
}
