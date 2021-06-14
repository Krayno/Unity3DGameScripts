using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MaximumFpsSetting : MonoBehaviour
{
    public int DefaultMaximumFpsValue;

    public Slider MaximumFpsSlider;
    public TMP_InputField MaximumFpsInputField;

    public Button ApplyButton;

    private int MaximumFpsValue;

    public Button[] ButtonsThatSwitchTab;

    private void Awake()
    {
        //Add the Player Preference if they do not have any.
        if (!PlayerPrefs.HasKey("MaximumFps"))
        {
            PlayerPrefs.SetInt("MaximumFps", DefaultMaximumFpsValue);
        }

        //Set MaximumFpsValue equal to the Player Preference.
        MaximumFpsValue = PlayerPrefs.GetInt("MaximumFps");

        //Set MaximumFps Slider and MaximumFps InputField to the Player Preference.
        MaximumFpsSlider.value = MaximumFpsValue;
        MaximumFpsInputField.text = MaximumFpsValue.ToString();

        //Apply the MaximumFps Player Preference.
        Application.targetFrameRate = MaximumFpsValue;

        //Set the Apply Button to not be interactable.
        ApplyButton.interactable = false;

        //Add listeners to the Slider, InputField and Apply Button as well as the buttons that switch tabs.
        MaximumFpsSlider.onValueChanged.AddListener(delegate { MaximumFpsSliderChanged(); });
        MaximumFpsInputField.onEndEdit.AddListener(delegate { MaximumFpsInputFieldChanged(); });
        ApplyButton.onClick.AddListener(delegate { ApplySetting(); });

        foreach (Button TabSwitch in ButtonsThatSwitchTab)
        {
            TabSwitch.onClick.AddListener(OnTabSwitch);
        }
    }

    private void OnTabSwitch()
    {
        //Reset the MaximumFpsValue to the Player Preference
        MaximumFpsValue = PlayerPrefs.GetInt("MaximumFps");
        //Reset the MaximumFps Slider and the MaximumFps Inputfield to the Player Preference.
        MaximumFpsSlider.value = MaximumFpsValue;
        MaximumFpsInputField.text = MaximumFpsValue.ToString();

        //Reset the Apply Button
        ApplyButton.interactable = false;
    }

    private void MaximumFpsSliderChanged()
    {
        if (MaximumFpsSlider.value < 10)
        {
            MaximumFpsSlider.value = 10;
        }

        MaximumFpsValue = (int)MaximumFpsSlider.value;
        MaximumFpsInputField.text = (MaximumFpsSlider.value).ToString();

        ApplyButton.interactable = true;
    }

    private void MaximumFpsInputFieldChanged()
    {
        if (int.Parse(MaximumFpsInputField.text) < 10)
        {
            MaximumFpsInputField.text = "10";
        }

        MaximumFpsValue = int.Parse(MaximumFpsInputField.text);
        MaximumFpsSlider.value = float.Parse(MaximumFpsInputField.text);

        ApplyButton.interactable = true;

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();
    }

    private void ApplySetting()
    {
        Application.targetFrameRate = MaximumFpsValue;
        PlayerPrefs.SetInt("MaximumFPS", MaximumFpsValue);

        ApplyButton.interactable = false;
    }
}
