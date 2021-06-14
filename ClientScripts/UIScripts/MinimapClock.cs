using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class MinimapClock : MonoBehaviour
{

    public TMP_Text ClockText;

    void Awake()
    {
        StartCoroutine("ClockTextUpdate");
    }

    IEnumerator ClockTextUpdate()
    {
        while (true)
        {
            ClockText.text = DateTime.UtcNow.ToString("HH:mm");
            yield return new WaitForSecondsRealtime(1);
        }
    }
}
