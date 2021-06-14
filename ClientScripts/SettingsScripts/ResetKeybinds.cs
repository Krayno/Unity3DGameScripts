using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Old Code. Can not be bothered remaking the whole keybind system.

public class ResetKeybinds : MonoBehaviour
{
    public KeybindManager KeybindManager;

    public TMP_Text SelectText;
    public TMP_Text InteractText;
    public TMP_Text JumpText;
    public TMP_Text MoveForwardText;
    public TMP_Text MoveLeftText;
    public TMP_Text MoveBackwardText;
    public TMP_Text MoveRightText;
    public TMP_Text SocialText;
    public TMP_Text SkillsText;
    public TMP_Text BackpackText;
    public TMP_Text CharacterText;
    public TMP_Text MapText;
    public TMP_Text GameMenuText;
    public TMP_Text ActionButton1Text;
    public TMP_Text ActionButton2Text;
    public TMP_Text ActionButton3Text;
    public TMP_Text ActionButton4Text;
    public TMP_Text ActionButton5Text;
    public TMP_Text ActionButton6Text;
    public TMP_Text ActionButton7Text;
    public TMP_Text ActionButton8Text;
    public TMP_Text ActionButton9Text;
    public TMP_Text ActionButton10Text;
    private Button Button;
    private GameObject BindStatus;

    void Awake()
    {
        Button = gameObject.GetComponent<Button>();
        BindStatus = GameObject.Find("BindStatus");

        Button.onClick.AddListener(delegate { ResetKeybindings(); });
    }


    public void ResetBindStatusText()
    {
        foreach (TMP_Text x in BindStatus.GetComponentsInChildren<TMP_Text>())
        {
            x.text = "";
        }
    }

    private void ResetKeybindings()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        //Set UpdatingKey to false to cancel a Keybind Update.
        KeybindManager.UpdatingKey = false;

        //Clear original Keybind List and re-add the keybinds back to set them to default.
        KeybindManager.Keybinds.Clear();

        //Set the Player Preference for each keybind.
        foreach (KeybindManager.PlayerPreferenceStringInt x in KeybindManager.PlayerPreferences)
        {

            KeybindManager.Keybinds.Add(x.Name, new KeybindManager.Keybind(x.Name, x.DefaultKey,
                                                    false, false, false));

            PlayerPrefs.SetInt(x.Name, (int)KeybindManager.Keybinds[x.Name].Key);
            PlayerPrefs.SetString(x.Name + "Shift", KeybindManager.Keybinds[x.Name].Shift.ToString());
            PlayerPrefs.SetString(x.Name + "Control", KeybindManager.Keybinds[x.Name].Control.ToString());
            PlayerPrefs.SetString(x.Name + "Alt", KeybindManager.Keybinds[x.Name].Shift.ToString());
        }

        SelectText.text = "Left Mouse Button";
        InteractText.text = "Right Mouse Button";
        JumpText.text = "Space";
        MoveForwardText.text = "W";
        MoveLeftText.text = "A";
        MoveBackwardText.text = "S";
        MoveRightText.text = "D";
        SocialText.text = "O";
        SkillsText.text = "K";
        BackpackText.text = "B";
        CharacterText.text = "H";
        MapText.text = "M";
        GameMenuText.text = "Escape";
        ActionButton1Text.text = "Q";
        ActionButton2Text.text = "E";
        ActionButton3Text.text = "R";
        ActionButton4Text.text = "F";
        ActionButton5Text.text = "C";
        ActionButton6Text.text = "1";
        ActionButton7Text.text = "2";
        ActionButton8Text.text = "3";
        ActionButton9Text.text = "4";
        ActionButton10Text.text = "5";

        ResetBindStatusText();
        BindStatus.GetComponentsInChildren<TMP_Text>()[0].text = "Keybinds Reset";
    }
}
