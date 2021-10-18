
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class RiskiSettings : UdonSharpBehaviour
{
    public AudioSource ClickSound;
    public Button FullscreenButton;
    public GameObject DesktopScreen;
    public GameObject MirrorObject;
    
    private bool IsFullscreen;

    void Start()
    {
        if (Networking.LocalPlayer.IsUserInVR() == true)
        {
            FullscreenButton.interactable = false;
        }
    }
    public void Fullscreen()
    {
        DesktopScreen.SetActive(true);
        ClickSound.Play();
    }
    void Update()
    {
        if (IsFullscreen == true && Input.GetKeyDown(KeyCode.Escape))
        {
            IsFullscreen = false;
            DesktopScreen.SetActive(false);
        }
    }
    public void MirrorToggle()
    {
        MirrorObject.SetActive(!MirrorObject.activeSelf);
    }
}

// Public build of RiskiPlayer doesn't have alot of settings yet..
// There will be more to come!
