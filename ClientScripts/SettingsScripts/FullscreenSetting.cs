using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FullscreenSetting : MonoBehaviour
{
    public bool DefaultFullscreenValue;

    public Toggle FullscreenToggle;
    public ResolutionSetting ResolutionSetting;
    public RefreshRateSetting RefreshRateSetting;

    public Button ApplyButton;

    private string FullscreenValue;

    public Button[] ButtonsThatSwitchTab;

    private void Awake()
    {
        //Add the Player Preference if they do not have any.
        if (!PlayerPrefs.HasKey("Fullscreen"))
        {
            PlayerPrefs.SetString("Fullscreen", DefaultFullscreenValue.ToString());
        }

        //Set FullscreenValue equal to the Player Preference.
        FullscreenValue = PlayerPrefs.GetString("Fullscreen");

        //Set the Fullscreen Toggle to the Player Preference.
        FullscreenToggle.isOn = bool.Parse(FullscreenValue);

        //Set the Apply Button to not be interactable.
        ApplyButton.interactable = false;

        //Add listeners to the Toggle and the Apply Button as well as the buttons that switch tabs.
        FullscreenToggle.onValueChanged.AddListener(delegate { FullscreenToggleChanged(); });
        ApplyButton.onClick.AddListener(delegate { ApplySetting(); });

        foreach (Button TabSwitch in ButtonsThatSwitchTab)
        {
            TabSwitch.onClick.AddListener(OnTabSwitch);
        }
    }

    private void OnTabSwitch()
    {
        //Reset the FullscreenValue to the Player Preference
        FullscreenValue = PlayerPrefs.GetString("Fullscreen");

        //Reset the Toggle to the Player Preference.
        FullscreenToggle.isOn = bool.Parse(FullscreenValue);

        //Reset the Apply Button
        ApplyButton.interactable = false;
    }

    private void FullscreenToggleChanged()
    {
        FullscreenValue = FullscreenToggle.isOn.ToString();

        ApplyButton.interactable = true;

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();
    }

    private void ApplySetting()
    {
        Screen.SetResolution(ResolutionSetting.FilteredRawResolutions[ResolutionSetting.ResolutionDropdown.value].width,
                             ResolutionSetting.FilteredRawResolutions[ResolutionSetting.ResolutionDropdown.value].height,
                             bool.Parse(FullscreenValue) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed,
                             int.Parse(RefreshRateSetting.StringRefreshRates[RefreshRateSetting.RefreshRateDropdown.value]));
        
        PlayerPrefs.SetString("Fullscreen", FullscreenValue);


        ApplyButton.interactable = false;
    }
}
