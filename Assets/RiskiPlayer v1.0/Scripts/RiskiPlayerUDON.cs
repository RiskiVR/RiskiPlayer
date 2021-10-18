
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class RiskiPlayerUDON : UdonSharpBehaviour
{
    [Header("RiskiPlayer Settings")]
    public bool MenuOnStart;

    [Header("Internal Elements")]
    public AudioSource ClickSound;
    public Animator Main;
    public GameObject EnterUI;
    public GameObject ExitUI1;
    public GameObject ExitUI2;
    public GameObject ExitUI3;
    public GameObject Settings;
    public Button VideoControlsButton;
    public Button SettingsButton;
    public Slider BrightnessSlider;
    public MeshRenderer Brightness;

    private void Start()
    {
        if (MenuOnStart == false)
        {
            Main.SetBool("MenuActive", false);
        }
    }
    public void BootUI()
    {
        Main.SetBool("MenuActive", true);
        Settings.SetActive(false);
        VideoControlsButton.interactable = true;
        SettingsButton.interactable = true;
        ExitUI1.SetActive(true);
        EnterUI.SetActive(false);
        Debug.Log("UI has been booted");
    }

    public void ExitUI()
    {
        Main.SetBool("MenuActive", false);
        Settings.SetActive(false);
        VideoControlsButton.interactable = false;
        SettingsButton.interactable = false;
        ExitUI1.SetActive(false);
        EnterUI.SetActive(true);
        Debug.Log("UI has been closed");
    }

    public void ExitVideoUI()
    {
        Main.SetBool("VideoActive", false);
        Main.SetBool("MenuActive", false);
        Settings.SetActive(false);
        ExitUI2.SetActive(false);
        EnterUI.SetActive(true);
        Debug.Log("Video UI has been closed");
    }

    public void VideoControl()
    {
        Main.SetBool("VideoActive", true);
        VideoControlsButton.interactable = false;
        SettingsButton.interactable = false;
        Settings.SetActive(false);
        ExitUI1.SetActive(false);
        ExitUI2.SetActive(true);
        ClickSound.Play();
        Debug.Log("Entered Video UI");
    }

    public void SettingsUI()
    {
        Main.SetBool("SettingsActive", true);
        VideoControlsButton.interactable = false;
        SettingsButton.interactable = false;
        ExitUI1.SetActive(false);
        ExitUI3.SetActive(true);
        ClickSound.Play();
        Debug.Log("Entered Settings UI");
    }

    public void ExitSettingsUI()
    {
        Main.SetBool("SettingsActive", false);
        Main.SetBool("MenuActive", false);
        ExitUI3.SetActive(false);
        EnterUI.SetActive(true);
        Debug.Log("Settings UI has been closed");
    }

    public void OnBrightnessValueChanged()
    {
        var color = Brightness.materials[0].GetColor("_Color");
        color.a = 1 - BrightnessSlider.value;
        Brightness.materials[0].SetColor("_Color", color);
    }
}
