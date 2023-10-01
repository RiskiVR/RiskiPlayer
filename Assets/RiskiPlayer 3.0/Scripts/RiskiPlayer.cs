using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class RiskiPlayer : UdonSharpBehaviour
{
    [Header("RiskiPlayer Settings")]
    [SerializeField] bool menuOnStart;
    [SerializeField] bool disableClicks;

    [Header("Internal Elements")]
    [SerializeField] AudioSource clickSound;
    [SerializeField] Animator main;
    [SerializeField] GameObject enterUIButton;
    [SerializeField] GameObject exitUIButton;
    [SerializeField] Slider brightnessSlider;
    [SerializeField] Image brightness;
    private void Start()
    {
        if (menuOnStart) BootUI();
        else ExitUI();
    }
    
    public void BootUI()
    {
        main.SetTrigger("BootUI");
        exitUIButton.SetActive(true);
        enterUIButton.SetActive(false);
        Debug.Log("UI has been booted");
    }
    
    public void ExitUI()
    {
        main.SetTrigger("ExitUI");
        exitUIButton.SetActive(false);
        enterUIButton.SetActive(true);
        Debug.Log("UI has been closed");
    }
    
    public void OnBrightnessValueChanged()
    {
        brightness.color = new Color(0, 0, 0, 1 - brightnessSlider.value);
    }

    public void Click()
    {
        if (!disableClicks) clickSound.Play();
    }
    
    public void EnterURL()
    {
        main.SetTrigger("Click");
    }
}