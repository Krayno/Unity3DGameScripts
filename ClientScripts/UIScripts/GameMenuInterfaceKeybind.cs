using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenuInterfaceKeybind : MonoBehaviour
{
    private TogglePanelButtonFade TogglePanel;
    public Transform SecondLevelPanels;

    private void Awake()
    {
        TogglePanel = gameObject.GetComponent<TogglePanelButtonFade>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //If the Game Menu is currently open, remove it from the PanelGroupList.
            if (TogglePanelButtonFade.PanelGroupList.Count > 0 && TogglePanelButtonFade.PanelGroupList[TogglePanelButtonFade.PanelGroupList.Count - 1].gameObject.name == "GameMenuPanelGroup")
            {
                TogglePanelButtonFade.PanelGroupList.RemoveAt(TogglePanelButtonFade.PanelGroupList.Count - 1);
            }

            //Check if any of the other panels are open before fading in or out the Game Menu.
            foreach (Transform transform in SecondLevelPanels)
            {
                if (transform.name == SecondLevelPanels.name)
                {
                    continue;
                }

                if (transform.gameObject.activeSelf && transform.name != "BackpackPanelGroup")
                {
                    return;
                }
            }
            TogglePanel.InitialiseFade();
        }
    }
}
