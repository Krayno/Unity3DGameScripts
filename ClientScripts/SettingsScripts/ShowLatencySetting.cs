using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Client.Unity;
using TMPro;
using DarkRift.Client;
using System;
using DarkRift;
using UnityEngine.UI;

public class ShowLatencySetting : MonoBehaviour
{
    public TMP_Text LatencyText;
    public Toggle ShowLatencyToggle;
    public bool DefaultShowLatencyValue;
    public float HudRefreshRate;

    private void Awake()
    {
        //Add the Player Preference if they do not have any.
        if (!PlayerPrefs.HasKey("ShowLatency"))
        {
            PlayerPrefs.SetString("ShowLatency", DefaultShowLatencyValue.ToString());
        }

        //Apply the ShowFps Preference.
        LatencyText.transform.parent.gameObject.SetActive(bool.Parse(PlayerPrefs.GetString("ShowLatency")));

        //Set the ShowFps Toggle to the Player Preference.
        ShowLatencyToggle.isOn = bool.Parse(PlayerPrefs.GetString("ShowLatency"));

        if (ShowLatencyToggle.isOn)
        {
            StartCoroutine("ShowPing");
        }

        //Add listener.
        ShowLatencyToggle.onValueChanged.AddListener(delegate { ShowLatencyToggleChanged(); });

        //Start the coroutine.
        StartCoroutine("SendConstantPingMessage");
    }

    private void ShowLatencyToggleChanged()
    {
        //Set the Player Preference to the Toggle state.
        PlayerPrefs.SetString("ShowLatency", ShowLatencyToggle.isOn.ToString());

        if (ShowLatencyToggle.isOn)
        {
            StartCoroutine("ShowPing");
        }

        //Apply the ShowFps Preference.
        LatencyText.transform.parent.gameObject.SetActive(bool.Parse(PlayerPrefs.GetString("ShowLatency")));

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();
    }

    

    private IEnumerator SendConstantPingMessage()
    {
        while (true)
        {
            if (ClientGlobals.WorldServer != null && ClientGlobals.WorldServer.ConnectionState == ConnectionState.Connected)
            {
                using (Message Message = Message.CreateEmpty(PacketTags.Heartbeat))
                {
                    Message.MakePingMessage();
                    ClientGlobals.WorldServer.SendMessage(Message, SendMode.Unreliable);
                }
            }
            yield return new WaitForSecondsRealtime(HudRefreshRate);
        }
    }

    private IEnumerator ShowPing()
    {
        while (ShowLatencyToggle.isOn)
        {
            if (ClientGlobals.WorldServer != null && ClientGlobals.WorldServer.ConnectionState == ConnectionState.Connected)
            {
                double ms = Math.Round(ClientGlobals.WorldServer.Client.RoundTripTime.SmoothedRtt, 3);

                int multiplier = 1000;
                int formattedMs = (int)(ms * multiplier);
                LatencyText.text = $"Latency (World): {formattedMs}ms";
            }
            else
            {
                LatencyText.text = $"Latency (World): Not Connected";
            }
            yield return new WaitForSecondsRealtime(HudRefreshRate);
        }
    }
}
