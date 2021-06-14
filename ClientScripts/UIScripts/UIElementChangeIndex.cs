using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIElementChangeIndex : MonoBehaviour
{
    public bool MoveUp;

    private Button Button;
    private Transform ParentGroup;

    private static int SiblingCount;

    private void Awake()
    {
        //Initialise variables.
        Button = gameObject.GetComponent<Button>();
        ParentGroup = transform.parent.parent;

        //Add listener.
        Button.onClick.AddListener(OnButtonClicked);
    }

    private void Start()
    {
        SiblingCount = ParentGroup.parent.childCount;
    }

    private void OnButtonClicked()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        int x = ParentGroup.GetSiblingIndex();
        int MoveToIndex = MoveUp ? x - 1 : x + 1;
        ParentGroup.SetSiblingIndex(MoveToIndex);

        transform.parent.gameObject.SetActive(false);
    }

    //Probably need to move this off Update... or just leave it as its not harmful.
    private void Update()
    {
        //If the button moves up but the current index is at the top, disable.
        if (ParentGroup.GetSiblingIndex() == 0 && MoveUp)
        {
            Button.interactable = false;
        }
        else if (MoveUp)
        {
            Button.interactable = true;
        }

        //If the button moves down but the current index is at the bottom, disable.
        if (ParentGroup.GetSiblingIndex() + 1 == SiblingCount && !MoveUp)
        {
            Button.interactable = false;
        }
        else if (!MoveUp)
        {
            Button.interactable = true;
        }
    }
}
