using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnButtonPressKeybind : MonoBehaviour
{
    public KeybindManager KeybindManager;
    public string Keybind;
    public bool Independent;

    private TogglePanelButtonFade TogglePanel;
    private PanelButtonFadeIn PanelButtonIn;
    private PanelButtonFadeOut PanelButtonOut;

    private void Awake()
    {
        if (!Independent)
        {
            TogglePanel = gameObject.GetComponent<TogglePanelButtonFade>();
        }
        else
        {
            PanelButtonIn = gameObject.GetComponent<PanelButtonFadeIn>();
            PanelButtonOut = gameObject.GetComponent<PanelButtonFadeOut>();
        }
    }

    void Update()
    {
        if (KeybindManager.Keybinds[Keybind].IsPressed())
        {
            //If the Game Menu is currently open, don't open any other panels.
            if (TogglePanelButtonFade.PanelGroupList.Count > 0 && TogglePanelButtonFade.PanelGroupList[TogglePanelButtonFade.PanelGroupList.Count - 1].gameObject.name == "GameMenuPanelGroup")
            {
                return;
            }

            if (!Independent)
            {
                TogglePanel.InitialiseFade();
            }
            else
            {
                if (PanelButtonIn.PanelGroup.gameObject.activeInHierarchy)
                {
                    PanelButtonOut.InitialiseFade();
                    return;
                }
                PanelButtonIn.InitialiseFade();
            }
        }
    }
}
