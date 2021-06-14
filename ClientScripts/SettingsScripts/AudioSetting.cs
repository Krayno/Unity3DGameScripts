using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System;

public class AudioSetting: MonoBehaviour
{
    public Toggle Toggle;
    public Slider Slider;
    public TMP_InputField InputField;
    public AudioMixerGroup AudioMixerGroup;
    public float DefaultVolume;
    public bool DefaultMute;

    private float Volume;
    private bool Muted;

    void Awake()
    {
        //Add the Player Preferences if they do not have any.
        if (!PlayerPrefs.HasKey(AudioMixerGroup.name))
        {
            PlayerPrefs.SetFloat(AudioMixerGroup.name, DefaultVolume);
        }
        if (!PlayerPrefs.HasKey(AudioMixerGroup.name + "Mute"))
        {
            PlayerPrefs.SetString(AudioMixerGroup.name + "Mute", DefaultMute.ToString());
        }

        //Set Volume and Muted to the Player Preference.
        Volume = PlayerPrefs.GetFloat(AudioMixerGroup.name);
        Muted = bool.Parse(PlayerPrefs.GetString(AudioMixerGroup.name + "Mute"));

        //Set the Toggle, Slider and InputFIeld to their values. Set the AudioMixerGroup volume as well. (Bugged in editor).
        Toggle.isOn = Muted;
        Slider.value = Volume;
        InputField.text = Volume.ToString();

        AudioMixerGroup.audioMixer.SetFloat(AudioMixerGroup.name + "Volume", 
                                            Mathf.Log10(Volume == 0 ? 0.0001f : Volume/100f) * 20);

        AudioMixerGroup.audioMixer.SetFloat(AudioMixerGroup.name + "Volume",
                                    Mathf.Log10(Toggle.isOn ? 0.0001f : Volume / 100f) * 20);

        //Add listeners.
        Toggle.onValueChanged.AddListener(delegate { ToggleChanged(); });
        Slider.onValueChanged.AddListener(delegate { SliderChanged(); });
        InputField.onEndEdit.AddListener(delegate { InputFieldChanged(); });
    }

    private void ToggleChanged()
    {
        //Set the Player Preference.
        PlayerPrefs.SetString(AudioMixerGroup.name + "Mute", Toggle.isOn.ToString());

        //Apply the Player Preference to the volume.
        AudioMixerGroup.audioMixer.SetFloat(AudioMixerGroup.name + "Volume",
                            Mathf.Log10(Toggle.isOn ? 0.0001f : PlayerPrefs.GetFloat(AudioMixerGroup.name) / 100) * 20);

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

    }

    private void SliderChanged()
    {
        //Set Volume to the Slider Value.
        Volume = Slider.value;

        //Set the Player Preference.
        PlayerPrefs.SetFloat(AudioMixerGroup.name, Volume);

        //Apply the Payer Preference to the volume.
        AudioMixerGroup.audioMixer.SetFloat(AudioMixerGroup.name + "Volume",
                                    Mathf.Log10(Volume == 0 ? 0.0001f : Volume / 100f) * 20);

        //Set the Inputfield equal to the Slider value.
        InputField.text = Slider.value.ToString();
    }

    private void InputFieldChanged()
    {
        //Check if the InputField Text is greater than 100.
        if (float.Parse(InputField.text) > 100)
        {
            InputField.text = "100";
        }

        //Set Volume to the Inputfield text.
        Volume = float.Parse(InputField.text);

        //Set the Player Preference.
        PlayerPrefs.SetFloat(AudioMixerGroup.name, Volume);

        //Apply the Payer Preference to the volume.
        AudioMixerGroup.audioMixer.SetFloat(AudioMixerGroup.name + "Volume",
                                    Mathf.Log10(Volume == 0 ? 0.0001f : Volume / 100f) * 20);

        //Set the Slider Value equal to the InputField text.
        Slider.value = Volume;

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();
    }
}
