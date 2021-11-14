
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
    public GameObject EnterUIButton;
    public GameObject ExitUIButton;
    public GameObject FullscreenButton;
    public GameObject DesktopScreen;
    public Slider BrightnessSlider;
    public MeshRenderer Brightness;
    public Animator URLButton;
    public Button URL;

    private void Start()
    {
        if (MenuOnStart == false)
        {
            Main.SetBool("MenuActive", false);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DesktopScreen.SetActive(false);
        }
    }
    public void BootUI()
    {
        Main.SetBool("MenuActive", true);
        URL.interactable = true;
        ExitUIButton.SetActive(true);
        EnterUIButton.SetActive(false);
        Main.SetTrigger("Fade");
        Debug.Log("UI has been booted");
        if (Networking.LocalPlayer.IsUserInVR() == false)
            FullscreenButton.SetActive(true);
    }
    public void ExitUI()
    {
        Main.SetBool("MenuActive", false);
        Main.SetTrigger("Reset");
        ExitUIButton.SetActive(false);
        EnterUIButton.SetActive(true);
        Debug.Log("UI has been closed");
    }
    public void OnBrightnessValueChanged()
    {
        var color = Brightness.materials[0].GetColor("_Color");
        color.a = 1 - BrightnessSlider.value;
        Brightness.materials[0].SetColor("_Color", color);
    }
    public void Fullscreen()
    {
        if (Networking.LocalPlayer.IsUserInVR() == false)
        {
            DesktopScreen.SetActive(true);
            ClickSound.Play();
        }   
    }
    public void EnterURL()
    {
        Main.SetTrigger("Click");
        URL.interactable = false;
    }
}
