using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ResolutionSetting : MonoBehaviour
{
    public TMP_Dropdown ResolutionDropdown;
    public FullscreenSetting FullscreenSetting;
    public RefreshRateSetting RefreshRateSetting;

    public Button ApplyButton;
    public int MinimumWidth;
    public int MinimumHeight;

    private Resolution[] RawResolutions;
    public List<Resolution> FilteredRawResolutions;
    private List<string> Resolutions;
    [HideInInspector]
    public int CurrentResolutionIndex;

    public Button[] ButtonsThatSwitchTab;

    void Awake()
    {
        //Initialise Lists, References, and Variables.
        FilteredRawResolutions = new List<Resolution>();
        Resolutions = new List<string>();

        //Set the Apply Button to not be interactable.
        ApplyButton.interactable = false;

        //Add Resolutions to Resolution Dropdown and set the Dropdown index.
        RawResolutions = Screen.resolutions;
        foreach (Resolution Resolution in RawResolutions)
        {
            if (!Resolutions.Contains($"{Resolution.width} x {Resolution.height}"))
            {
                Resolutions.Add($"{Resolution.width} x {Resolution.height}");
                FilteredRawResolutions.Add(Resolution);
            }
        }
        ResolutionDropdown.AddOptions(Resolutions);

        //Add the Player Preference if they do not have any.
        if (!PlayerPrefs.HasKey("IndexResolution"))
        {
            foreach (Resolution Resolution in FilteredRawResolutions)
            {
                if (Resolution.width == Screen.currentResolution.width && 
                    Resolution.height == Screen.currentResolution.height)
                {
                    CurrentResolutionIndex = FilteredRawResolutions.IndexOf(Resolution);
                    PlayerPrefs.SetInt("IndexResolution", CurrentResolutionIndex);
                    break;
                }

            }
        }

        //Set the Resolution Dropdown equal to the CurrentResolutionIndex
        CurrentResolutionIndex = PlayerPrefs.GetInt("IndexResolution");
        ResolutionDropdown.value = CurrentResolutionIndex;

        //Apply the Resolution && Fullscreen Player Preference if the player hasn't already set it.
        if (!ClientGlobals.InitialResolutionSetup)
        {
            FullScreenMode FullScreenMode = PlayerPrefs.HasKey("Fullscreen") ? (bool.Parse(PlayerPrefs.GetString("Fullscreen")) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed) :
                                                                    FullScreenMode.FullScreenWindow;

            int RefreshRate = PlayerPrefs.HasKey("RefreshRate") ? PlayerPrefs.GetInt("RefreshRate") : Screen.currentResolution.refreshRate;
            Screen.SetResolution(FilteredRawResolutions[CurrentResolutionIndex].width,
                                 FilteredRawResolutions[CurrentResolutionIndex].height,
                                 FullScreenMode, RefreshRate);
            
            ClientGlobals.InitialResolutionSetup = true;
        }

        //Add listeners to the Dropdown and the Apply Button as well as the buttons that switch tabs.
        ResolutionDropdown.onValueChanged.AddListener(delegate { ScreenResolutionChanged(); });
        ApplyButton.onClick.AddListener(delegate { ApplySetting(); });

        foreach (Button TabSwitch in ButtonsThatSwitchTab)
        {
            TabSwitch.onClick.AddListener(OnTabSwitch);
        }
    }

    private void OnTabSwitch()
    {
        //Reset the CurrentResolutionIndex to the Player Preference
        CurrentResolutionIndex = PlayerPrefs.GetInt("IndexResolution");

        //Reset the Dropdown Index to the Player Preference.
        ResolutionDropdown.value = CurrentResolutionIndex;

        //Reset the Apply Button
        ApplyButton.interactable = false;
    }

    private void ScreenResolutionChanged()
    {
        CurrentResolutionIndex = ResolutionDropdown.value;

        ApplyButton.interactable = true;

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();
    }

    private void ApplySetting()
    {

        Screen.SetResolution(FilteredRawResolutions[CurrentResolutionIndex].width,
                             FilteredRawResolutions[CurrentResolutionIndex].height,
                             FullscreenSetting.FullscreenToggle.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed,
                             int.Parse(RefreshRateSetting.StringRefreshRates[RefreshRateSetting.RefreshRateDropdown.value]));
        
        PlayerPrefs.SetInt("IndexResolution", CurrentResolutionIndex);

        ApplyButton.interactable = false;
    }
}
