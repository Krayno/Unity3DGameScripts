using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;

public class RefreshRateSetting : MonoBehaviour
{
    public TMP_Dropdown RefreshRateDropdown;
    public FullscreenSetting FullscreenSetting;
    public ResolutionSetting ResolutionSetting;
    public Button ApplyButton;
    public Button[] ButtonsThatSwitchTab;

    public List<Resolution> ResolutionRefreshRates;
    [HideInInspector]
    public List<string> StringRefreshRates;

    private int RefreshRateValue;
    private Resolution[] RawResolutions;
    

    void Awake()
    {
        //Initialise Lists
        RawResolutions = Screen.resolutions;
        ResolutionRefreshRates = RawResolutions.GroupBy(x => x.refreshRate).Select(g => g.First()).ToList();
        StringRefreshRates = new List<string>();

        //Populate IntRefreshRate List
        foreach (Resolution Resolution in ResolutionRefreshRates)
        {
            StringRefreshRates.Add(Resolution.refreshRate.ToString());
        }

        //Add the Player Preference if they do not have any.
        if (!PlayerPrefs.HasKey("RefreshRate"))
        {
            PlayerPrefs.SetInt("RefreshRate", Screen.currentResolution.refreshRate);
        }

        //Set RefreshRateValue equal to the Player Preference.
        RefreshRateValue = PlayerPrefs.GetInt("RefreshRate");

        //Order Refresh Rates and add Refresh Rates to the Refresh Rate Dropdown && Apply the Dropdown value.
        RefreshRateDropdown.AddOptions(StringRefreshRates);
        RefreshRateDropdown.value = StringRefreshRates.FindIndex(x => x == PlayerPrefs.GetInt("RefreshRate").ToString());

        //Set the Apply Button to not be interactable.
        ApplyButton.interactable = false;

        //Add listeners to the Dropdown and the Apply Button as well as the buttons that switch tabs.
        RefreshRateDropdown.onValueChanged.AddListener(delegate { RefreshRateChanged(); });
        ApplyButton.onClick.AddListener(delegate { ApplySetting(); });

        foreach (Button TabSwitch in ButtonsThatSwitchTab)
        {
            TabSwitch.onClick.AddListener(OnTabSwitch);
        }

    }

    private void OnTabSwitch()
    {
        //Reset the RefreshRateValue to the Player Preference
        RefreshRateValue = PlayerPrefs.GetInt("RefreshRate");

        //Reset the RefreshRate Dropdown to the Player Preference.
        RefreshRateDropdown.value = StringRefreshRates.FindIndex(x => x == PlayerPrefs.GetInt("RefreshRate").ToString());

        //Reset the Apply Button
        ApplyButton.interactable = false;
    }

    private void RefreshRateChanged()
    {
        RefreshRateValue = int.Parse(StringRefreshRates[RefreshRateDropdown.value]);

        ApplyButton.interactable = true;

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();
    }

    private void ApplySetting()
    {
        Screen.SetResolution(ResolutionSetting.FilteredRawResolutions[ResolutionSetting.ResolutionDropdown.value].width,
                             ResolutionSetting.FilteredRawResolutions[ResolutionSetting.ResolutionDropdown.value].height,
                             FullscreenSetting.FullscreenToggle.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed,
                             RefreshRateValue);

        PlayerPrefs.SetInt("RefreshRate", RefreshRateValue);

        ApplyButton.interactable = false;
    }
}
