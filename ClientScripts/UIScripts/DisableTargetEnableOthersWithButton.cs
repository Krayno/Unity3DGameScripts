using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisableTargetEnableOthersWithButton : MonoBehaviour
{
    public GameObject[] Group;
    public GameObject Target;

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
        //Disable all GameObjects in the group, alternative code for buttons..
        foreach (GameObject GameObject in Group)
        {
            if (GameObject.TryGetComponent(out Button GroupButton))
            {
                GroupButton.interactable = true;
            }
            else
            {
                GameObject.SetActive(true);
            }
        }

        //Enable the target, alternative code for buttons.
        if (Target.TryGetComponent(out Button TargetButton))
        {
            TargetButton.interactable = false;
        }
        else
        {
            Target.SetActive(false);
        }

    }
}
