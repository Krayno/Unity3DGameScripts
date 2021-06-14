using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ChangeTextWithButton : MonoBehaviour
{
    public TMP_Text TargetText;

    private Button Button;

    // Start is called before the first frame update
    void Awake()
    {
        //Initialise Variables.
        Button = gameObject.GetComponent<Button>();

        //Add listener.
        Button.onClick.AddListener(OnButtonClicked);

    }

    private void OnButtonClicked()
    {
        //Change TargetText Text.
        TargetText.text = transform.GetComponentInChildren<TMP_Text>().text;
    }
}
