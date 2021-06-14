using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class VerticalSyncSetting : MonoBehaviour
{
    public bool DefaultVerticalSyncValue;

    public Toggle VerticalSyncToggle;

    public Button ApplyButton;

    private string VerticalSyncValue;

    public Button[] ButtonsThatSwitchTab;

    private void Awake()
    {
        //Add the Player Preference if they do not have any.
        if (!PlayerPrefs.HasKey("VerticalSync"))
        {
            PlayerPrefs.SetString("VerticalSync", DefaultVerticalSyncValue.ToString());
        }

        //Set VerticalSyncValue equal to the Player Preference.
        VerticalSyncValue = PlayerPrefs.GetString("VerticalSync");

        //Set the VerticalSync Toggle to the Player Preference.
        VerticalSyncToggle.isOn = bool.Parse(VerticalSyncValue);

        //Apply the VerticalSync Player Preference.
        QualitySettings.vSyncCount = bool.Parse(VerticalSyncValue) ? 1 : 0;

        //Set the Apply Button to not be interactable.
        ApplyButton.interactable = false;

        //Add listeners to the Toggle and the Apply Button as well as the buttons that switch tabs.
        VerticalSyncToggle.onValueChanged.AddListener(delegate { VerticalSyncToggleChanged(); });
        ApplyButton.onClick.AddListener(delegate { ApplySetting(); });

        foreach (Button TabSwitch in ButtonsThatSwitchTab)
        {
            TabSwitch.onClick.AddListener(OnTabSwitch);
        }
    }

    private void OnTabSwitch()
    {
        //Reset the VerticalSyncValue to the Player Preference
        VerticalSyncValue = PlayerPrefs.GetString("VerticalSync");

        //Reset the Toggle to the Player Preference.
        VerticalSyncToggle.isOn = bool.Parse(VerticalSyncValue);

        //Reset the Apply Button
        ApplyButton.interactable = false;
    }

    private void OnDisable()
    {
        //Reset the VerticalSyncValue to the Player Preference
        VerticalSyncValue = PlayerPrefs.GetString("VerticalSync");

        //Reset the Toggle to the Player Preference.
        VerticalSyncToggle.isOn = bool.Parse(VerticalSyncValue);

        //Reset the Apply Button
        ApplyButton.interactable = false;
    }

    private void VerticalSyncToggleChanged()
    {
        VerticalSyncValue = VerticalSyncToggle.isOn.ToString();

        ApplyButton.interactable = true;

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();
    }

    private void ApplySetting()
    {
        QualitySettings.vSyncCount = bool.Parse(VerticalSyncValue) ? 1 : 0;
        PlayerPrefs.SetString("VerticalSync", VerticalSyncValue);

        ApplyButton.interactable = false;
    }
}
