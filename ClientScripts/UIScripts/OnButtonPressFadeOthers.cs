using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnButtonPressFadeOthers : MonoBehaviour
{
    public CanvasGroup[] PanelGroup;
    public CanvasGroup Target;

    public Button Button;

    void Awake()
    {
        Button.onClick.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed()
    {
        foreach (CanvasGroup Panel in PanelGroup)
        {
            if (Panel != Target)
            {
                Panel.alpha = 0.3f;
            }
        }
    }
}
