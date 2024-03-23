using System;
using System.Collections;
using ArchiTech.ProTV;
using UdonSharp;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using UnityEditor;
#endif

public class RiskiPlayer : TVPlugin
{
    public override void Start()
    {
        if (init) return;
        base.Start();

        versionText.text = version;
        brightnessSlider.value = brightness;
        volumeSlider.value = volume;
        tv._ChangeVolume(volume);
        OnBrightnessUpdate();
        OnVolumeUpdate();
        
        UpdateStatusText("Initializing");
        progressSeek.text = "";
        progressBar.gameObject.SetActive(false);
        playObject.SetActive(false);

        if (menuOnStart) BootUI();
        else ExitUI();
    }
    
    [Header("RiskiPlayer Settings")]
    public bool menuOnStart;
    public bool disableClicks;

    [Header("Default Slider Values")]
    [Range(0, 1)]
    public float brightness = 0.9f;
    [Range(0,1)]
    public float volume = 0.75f;

    [Header("Internal Elements")]
    public AudioSource videoAudio;
    public AudioSource clickSound;
    public Animator mainUI;
    public Animator dimLoad;
    public GameObject enterUIButton;
    public GameObject exitUIButton;
    public GameObject playObject;
    public GameObject[] playIconList;
    public GameObject[] loopIconList;
    public GameObject[] lockIconList;
    public Text ownerText;
    public Slider brightnessSlider;
    public Slider volumeSlider;
    public Image[] volumeBarColor;
    public GameObject[] volumeIconList;
    public Image brightnessImage;
    public Text titleText;
    public Text statusText;
    public Animator statusAnim;
    public VRCUrlInputField urlButton;
    public Slider progressBar;
    public Text progressSeek;
    public Text progressTime;
    public Text progressLength;
    
    [Header("Version Info")]
    public Text versionText;
    public string version;
    public void BootUI()
    {
        mainUI.SetTrigger("BootUI");
        exitUIButton.SetActive(true);
        enterUIButton.SetActive(false);
    }
    
    public void ExitUI()
    {
        mainUI.SetTrigger("ExitUI");
        exitUIButton.SetActive(false);
        enterUIButton.SetActive(true);
    }
    
    public void Click()
    {
        if (!disableClicks) clickSound.Play();
    }
    
    public void OnBrightnessUpdate()
    {
        brightnessImage.color = new Color(0, 0, 0, 1 - brightnessSlider.value);
    }

    public void OnVolumeUpdate()
    {
        videoAudio.volume = volumeSlider.value;
        if (videoAudio.mute) return;
        if (videoAudio.volume == 0f) UpdateVolumeIcon(1);
        if (videoAudio.volume > 0f) UpdateVolumeIcon(2);
        if (videoAudio.volume > 0.5f) UpdateVolumeIcon(3);
    }

    public void OnSeekUpdate()
    {
        progressSeek.text = _GetReadableTime(tv.currentTime);
        tv._ChangeSeekPercent(progressBar.value - tv.seekOffset);
    }

    public void OnSeekEnd()
    {
        
    }

    public void Mute() => tv._ToggleMute();
    public void Loop() => tv._ToggleLoop();
    public void Reload() => tv._RefreshMedia();
    public void Resync() => tv._ReSync();
    public void Lock() => tv._ToggleLock();

    public void Play()
    {
        if (tv.IsPaused)
        {
            tv._Play();
            return;
        }
        if (tv.IsPlaying)
        {
            if (tv.isLive) tv._Stop();
            else tv._Pause();
        }
    }
    public void URL()
    {
        tv._ChangeMedia(urlButton.GetUrl(), null, string.Empty);
        urlButton.SetUrl(VRCUrl.Empty);
    }

    public void UpdateVolumeIcon(int use)
    {
        foreach (GameObject g in volumeIconList) g.SetActive(false);
        volumeIconList[use].SetActive(true);
    }
    
    public void UpdateLockIcon(int use)
    {
        foreach (GameObject g in lockIconList) g.SetActive(false);
        lockIconList[use].SetActive(true);
    }

    public void UpdateLoopIcon(int use)
    {
        foreach (GameObject g in loopIconList) g.SetActive(false);
        loopIconList[use].SetActive(true);
    }
    
    public void UpdatePlayIcon(int use)
    {
        foreach (GameObject g in playIconList) g.SetActive(false);
        playIconList[use].SetActive(true);
    }
    
    public void UpdateStatusText(string text)
    {
        statusText.color = Color.white;
        statusAnim.SetTrigger(text == "" ? "DismissText" : "StatusText");
        if (text != "") statusText.text = text;
    }
    
    private void Update()
    {
        if (tv.IsPlaying)
        {
            progressTime.text = _GetReadableTime(tv.currentTime);
            progressBar.SetValueWithoutNotify(tv.currentTime / tv.videoDuration);
        }
    }

    public override void _TvReady()
    {
        UpdateStatusText("");
        ownerText.text = tv.Owner.displayName;
    }
    
    public override void _TvLoading()
    {
        titleText.text = "";
        UpdateStatusText("Loading");
        UpdatePlayIcon(0);
        playObject.SetActive(true);
        progressBar.gameObject.SetActive(false);
        if (!tv.IsPlaying) dimLoad.SetBool("Dim", true);
    }
    
    public override void _TvMediaReady()
    {
        UpdateStatusText("");
        UpdatePlayIcon(tv.isLive ? 2 : 3);
        progressTime.text = _GetReadableTime(tv.currentTime);
        progressLength.text = _GetReadableTime(tv.videoDuration);
        progressBar.gameObject.SetActive(!tv.isLive);
        dimLoad.SetBool("Dim", false);
    }

    public override void _TvTitleChange() => titleText.text = tv.title == string.Empty ? tv._GetUrlDomain() : tv.title;
    public override void _TvStop() => playObject.SetActive(false);
    public override void _TvPause() => UpdatePlayIcon(1);
    public override void _TvPlay() => UpdatePlayIcon(tv.isLive ? 2 : 3);
    public override void _TvSeekChange() => progressTime.text = _GetReadableTime(tv.currentTime);
    public override void _TvDisableLoop() => UpdateLoopIcon(0);
    public override void _TvEnableLoop() => UpdateLoopIcon(1);
    public override void _TvUnLock() => UpdateLockIcon(0);
    public override void _TvLock() => UpdateLockIcon(1);
    public override void _TvOwnerChange() => ownerText.text = tv.Owner.displayName;

    public override void _TvMediaEnd()
    {
        progressBar.gameObject.SetActive(false);
        playObject.SetActive(false);
    }
    
    public override void _TvVideoPlayerError()
    {
        UpdateStatusText("ERROR");
        progressBar.gameObject.SetActive(false);
        statusText.color = new Color(1, 0.2877358f, 0.2877358f);
    }
    
    public override void _TvMute()
    {
        foreach (Image i in volumeBarColor) i.color = new Color(1, 0.2877358f, 0.2877358f, i.color.a);
        videoAudio.mute = true;
        UpdateVolumeIcon(0);
    }
    
    public override void _TvUnMute()
    {
        foreach (Image i in volumeBarColor) i.color = new Color(1, 1, 1, i.color.a);
        videoAudio.mute = false;
        OnVolumeUpdate();
    }

    public static string _GetReadableTime(float _time, bool negativeZero = false)
    {
        if (_time == float.PositiveInfinity) return "Live";
        if (float.IsNaN(_time)) _time = 0f;
        string early = _time < 0 ? "-" : "";
        if (negativeZero && _time == 0) early = "-";
        _time = Mathf.Abs(_time);
        int seconds = (int)_time % 60;
        int minutes = (int)(_time / 60) % 60;
        int hours = (int)(_time / 60 / 60) % 60;
        return hours > 0 ? $"{early}{hours}:{minutes:D2}:{seconds:D2}" : $"{early}{minutes:D2}:{seconds:D2}";
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RiskiPlayer))]
class RiskiPlayerEditor : Editor
{
    int toolbarInt = 0;
    string[] toolbarStrings = { "Credits", "Internals" };
    
    public override void OnInspectorGUI()
    {
        Texture2D imageBanner = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets\\RiskiPlayer 3.1\\Textures\\RiskiPlayer LogoTXT.png", typeof(Texture2D));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        GUILayout.Label(imageBanner, GUILayout.Width(400f), GUILayout.Height(100f));
        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();
        
        #region TextStyles
        GUIStyle headerText = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        GUIStyle lowerText = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 15,
            fontStyle = FontStyle.Normal
        };
        GUIStyle centerText = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            fontStyle = FontStyle.Normal
        };
        #endregion
        
        GUILayout.Label("Made by RiskiVR", headerText);
        GUILayout.Label("Modified from ProTV by ArchiTechVR", lowerText);

        RiskiPlayer riskiPlayer = (RiskiPlayer)serializedObject.targetObject;

        toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);
        switch (toolbarInt)
        {
            case 0:
                GUILayout.Label($"RiskiPlayer {riskiPlayer.version}", centerText);
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("ArchiTechVR", GUILayout.Width(100f))) Application.OpenURL("https://protv.dev/");;
                GUILayout.Label("- ProTV");
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("RiskiVR", GUILayout.Width(100f))) Application.OpenURL("https://riskivr.com");
                GUILayout.Label("- RiskiPlayer");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("MerlinVR", GUILayout.Width(100f))) Application.OpenURL("https://github.com/MerlinVR/UdonSharp");;
                GUILayout.Label("- UdonSharp");
                GUILayout.EndHorizontal();
                break;
            
            case 1:
                DrawDefaultInspector();
                break;
        }
    }
}
#endif