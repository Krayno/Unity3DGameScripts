using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FieldOfViewSetting : MonoBehaviour
{
    public int DefaultFieldOfViewValue;

    public Slider FieldOfViewSlider;
    public TMP_InputField FieldOfViewInputField;

    public Button ApplyButton;

    private int FieldOfViewValue;

    public Button[] ButtonsThatSwitchTab;

    private void Awake()
    {

        //Add the Player Preference if they do not have any.
        if (!PlayerPrefs.HasKey("FieldOfView"))
        {
            PlayerPrefs.SetInt("FieldOfView", DefaultFieldOfViewValue);
        }

        //Set FieldOfViewValue equal to the Player Preference.
        FieldOfViewValue = PlayerPrefs.GetInt("FieldOfView");

        //Set FieldOfView Slider and FieldOfView InputField to the FieldOfViewValue;
        FieldOfViewSlider.value = FieldOfViewValue;
        FieldOfViewInputField.text = FieldOfViewValue.ToString();

        //Set the Apply Button to not be interactable.
        ApplyButton.interactable = false;

        //Add listeners to the Slider, InputField and Apply Button as well as the buttons that switch tabs.
        FieldOfViewSlider.onValueChanged.AddListener(delegate { FieldOfViewSliderChanged(); });
        FieldOfViewInputField.onEndEdit.AddListener(delegate { FieldOfViewInputFieldChanged(); });
        ApplyButton.onClick.AddListener(delegate { ApplySetting(); });

        foreach (Button TabSwitch in ButtonsThatSwitchTab)
        {
            TabSwitch.onClick.AddListener(OnTabSwitch);
        }
    }

    private void OnTabSwitch()
    {
        //Reset the FieldOfViewValue to the Player Preference
        FieldOfViewValue = PlayerPrefs.GetInt("FieldOfView");

        //Reset the FieldOfView Slider and the FieldOfView Inputfield to the Player Preference.
        FieldOfViewSlider.value = FieldOfViewValue;
        FieldOfViewInputField.text = FieldOfViewValue.ToString();

        //Reset the Apply Button
        ApplyButton.interactable = false;
    }

    private void FieldOfViewSliderChanged()
    {
        FieldOfViewValue = (int)FieldOfViewSlider.value;
        FieldOfViewInputField.text = FieldOfViewSlider.value.ToString();

        ApplyButton.interactable = true;
    }

    private void FieldOfViewInputFieldChanged()
    {
        FieldOfViewValue = int.Parse(FieldOfViewInputField.text);
        FieldOfViewSlider.value = float.Parse(FieldOfViewInputField.text);

        ApplyButton.interactable = true;

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();
    }

    private void ApplySetting()
    {
        Camera.main.fieldOfView = FieldOfViewValue;
        PlayerPrefs.SetInt("FieldOfView", (int)FieldOfViewSlider.value);

        ApplyButton.interactable = false;
    }
}
