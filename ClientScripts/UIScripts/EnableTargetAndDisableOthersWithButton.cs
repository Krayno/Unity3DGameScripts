using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class EnableTargetAndDisableOthersWithButton : MonoBehaviour
{
    public GameObject[] Group;
    public GameObject[] Target;

    private Button Button;

    void Awake()
    {
        //Initialise Variables.
        Button = gameObject.GetComponent<Button>();

        //Add listener.
        Button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        //Disable all GameObjects in the group.
        foreach (GameObject GameObject in Group)
        {
            GameObject.SetActive(false);
        }

        //Enable the target.
        foreach (GameObject Target in Target)
        {
            Target.SetActive(true);
        }
    }
}
