using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShowFpsSetting : MonoBehaviour
{
    public TMP_Text FpsText;
    public Toggle ShowFpsToggle;
    public bool DefaultShowFpsValue;
    public float HudRefreshRate;

    private float FrameCount;
    private float DeltaTime;

    private void Awake()
    {
        //Initialise Variables
        FrameCount = 0f;
        DeltaTime = 0f;

        //Add the Player Preference if they do not have any.
        if (!PlayerPrefs.HasKey("ShowFps"))
        {
            PlayerPrefs.SetString("ShowFps", DefaultShowFpsValue.ToString());
        }

        //Set the ShowFps Toggle to the Player Preference.
        ShowFpsToggle.isOn = bool.Parse(PlayerPrefs.GetString("ShowFps"));

        //Apply the ShowFps Preference.
        FpsText.transform.parent.gameObject.SetActive(bool.Parse(PlayerPrefs.GetString("ShowFps")));
        if (ShowFpsToggle.isOn)
        {
            StartCoroutine(UpdateFps());
        }

        //Add listener.
        ShowFpsToggle.onValueChanged.AddListener(delegate { ShowFpsToggleChanged(); });
    }

    private void ShowFpsToggleChanged()
    {
        //Set the Player Preference to the Toggle state.
        PlayerPrefs.SetString("ShowFps", ShowFpsToggle.isOn.ToString());

        //Stop the coroutine then start if the toggle is on.
        StopCoroutine(UpdateFps());
        if (ShowFpsToggle.isOn)
        {
            StartCoroutine(UpdateFps());
        }

        //Apply the ShowFps Preference.
        FpsText.transform.parent.gameObject.SetActive(bool.Parse(PlayerPrefs.GetString("ShowFps")));

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();
    }

    private IEnumerator UpdateFps()
    {
        while (ShowFpsToggle.isOn)
        {
            FrameCount++;
            DeltaTime += Time.unscaledDeltaTime;
            if (DeltaTime > 1 / HudRefreshRate)
            {
                FpsText.text = $"Framerate: {(int)(FrameCount / DeltaTime)}";
                FrameCount = 0;
                DeltaTime -= 1 / HudRefreshRate;
            }

            yield return null;
        }
    }
}
