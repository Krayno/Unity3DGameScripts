using DarkRift.Client.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionBackButton : MonoBehaviour
{
    private Button Button;
    public List<GameObject> ObjectsToCheckBeforeClosing;

    void Awake()
    {
        Button = GetComponent<Button>();
        Button.onClick.AddListener(BackbuttonClicked);
    }

    private void BackbuttonClicked()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        //Disconnect from Login Server which will trigger the disconnection from other servers and a scene swap.
        ServerManager.Instance.Clients["LoginServer"].Disconnect();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            foreach (GameObject Panel in ObjectsToCheckBeforeClosing)
            {
                if (Panel.activeInHierarchy)
                {
                    return;
                }
            }
            //Play OpenClose Sound.
            SoundManager.Instance.Sounds["UIOpenClose"].Play();

            //Disconnect from Login Server which will trigger the disconnection from other servers and a scene swap.
            ServerManager.Instance.Clients["LoginServer"].Disconnect();
        }
    }
}
