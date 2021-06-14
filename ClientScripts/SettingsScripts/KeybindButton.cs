using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class KeybindButton : MonoBehaviour
{
    public KeybindManager KeybindManager;

    private string Keybind; //Relies on parent objects name to get the reference of the key to change.
    private TMP_Text KeybindText;
    private Button ClickButton;
    private Button UnbindButton;
    private GameObject BindStatus;
    private List<KeyCode> BannedKeys;
    private GameObject HighlightImage;

    static KeybindButton Instance;

    private void Start()
    {
        Keybind = transform.parent.name;
        KeybindText = transform.GetChild(1).GetComponent<TMP_Text>();

        ClickButton = gameObject.GetComponent<Button>();
        ClickButton.onClick.AddListener(delegate { OnKeyBindClick(); });

        UnbindButton = GameObject.Find("UnbindKey").GetComponent<Button>();
        UnbindButton.onClick.AddListener(delegate { OnUnBindKeyClick(); });
        UnbindButton.interactable = false;

        HighlightImage = transform.GetChild(0).gameObject;

        BindStatus = GameObject.Find("BindStatus");

        //Add banned keys to this list.
        BannedKeys = new List<KeyCode>();
        BannedKeys.Add(KeyCode.LeftShift);
        BannedKeys.Add(KeyCode.LeftControl);
        BannedKeys.Add(KeyCode.LeftAlt);
        BannedKeys.Add(KeyCode.RightShift);
        BannedKeys.Add(KeyCode.RightControl);
        BannedKeys.Add(KeyCode.RightAlt);
        BannedKeys.Add(KeyCode.Mouse0);
        BannedKeys.Add(KeyCode.Mouse1);
        BannedKeys.Add(KeyCode.Escape);
        BannedKeys.Add(KeyCode.Return);
        BannedKeys.Add(KeyCode.Backspace);

        //Set Default Key Text.
        string keyString = KeybindManager.Keybinds[Keybind].Key.ToString();
        keyString = keyString.Contains("Alpha") ? keyString.Substring(5) : keyString;
        if (keyString.Contains("Alpha"))
        {
            keyString = keyString.Substring(5);
        }
        if (KeybindManager.Keybinds[Keybind].Shift)
        {
            keyString = "Shift + " + keyString;
        }
        if (KeybindManager.Keybinds[Keybind].Control)
        {
            keyString = "Control + " + keyString;
        }
        if (KeybindManager.Keybinds[Keybind].Alt)
        {
            keyString = "Alt + " + keyString;
        }
        KeybindText.text = keyString;
    }

    public void ResetBindStatusText()
    {
        foreach (TMP_Text x in BindStatus.GetComponentsInChildren<TMP_Text>())
        {
            x.text = "";
        }
    }

    void OnKeyBindClick()
    {

        //If already updating a keybind, ignore the click and return.
        if (KeybindManager.UpdatingKey == true)
        {
            return;
        }

        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        HighlightImage.SetActive(true);

        ResetBindStatusText();
        KeybindManager.UpdatingKey = true;
        StartCoroutine(AwaitingValidKey());
        Instance = gameObject.GetComponent<KeybindButton>();

        ResetBindStatusText();
        string actionName = ClickButton.transform.parent.name;
        BindStatus.GetComponentsInChildren<TMP_Text>()[2].text = "Press a key to bind to: " + actionName;
    }

    private IEnumerator AwaitingValidKey()
    {
        while (KeybindManager.UpdatingKey)
        {
            UpdateKey();
            yield return null;
        }

        ClickButton.interactable = true;
        HighlightImage.SetActive(false);

    }

    void OnUnBindKeyClick()
    {
        HighlightImage.SetActive(false);
        KeybindManager.UpdatingKey = false;
        UnbindButton.interactable = false;
        KeybindManager.Keybinds[Instance.Keybind].Key = KeyCode.None;
        KeybindManager.Keybinds[Instance.Keybind].Shift = false;
        KeybindManager.Keybinds[Instance.Keybind].Control = false;
        KeybindManager.Keybinds[Instance.Keybind].Alt = false;
        PlayerPrefs.SetInt(Keybind, (int)KeybindManager.Keybinds[Keybind].Key);
        PlayerPrefs.SetString(Keybind + "Shift", KeybindManager.Keybinds[Keybind].Shift.ToString());
        PlayerPrefs.SetString(Keybind + "Control", KeybindManager.Keybinds[Keybind].Control.ToString());
        PlayerPrefs.SetString(Keybind + "Alt", KeybindManager.Keybinds[Keybind].Alt.ToString());
        Instance.KeybindText.text = KeyCode.None.ToString();

        BindStatus.GetComponentsInChildren<TMP_Text>()[2].text = "";
        BindStatus.GetComponentsInChildren<TMP_Text>()[3].text = "Unbound Key";
    }

    void UpdateKey()
    {
        KeybindManager.UpdatingKey = true;

        ClickButton.interactable = false;
        UnbindButton.interactable = true;

        foreach (KeyCode Key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(Key))
            {
                //Check if the key is a banned key.
                foreach(KeyCode BannedKey in BannedKeys)
                {
                    if (Key == BannedKey)
                    {
                        return;
                    }
                }

                //Check if any of the modifier keys are being held at the same time.
                bool LeftShiftHeld = Input.GetKey(KeyCode.LeftShift);
                bool LeftControlHeld = Input.GetKey(KeyCode.LeftControl);
                bool LeftAltHeld = Input.GetKey(KeyCode.LeftAlt);

                //Check if the Key Conflicted with any other Keybind.
                KeybindConflictCheck(Key, LeftShiftHeld, LeftControlHeld, LeftAltHeld);

                //Update the keybind with the new key and update the Keybind Object Properties.
                KeybindManager.Keybinds[Keybind].Key = Key;
                KeybindManager.Keybinds[Keybind].Shift = LeftShiftHeld;
                KeybindManager.Keybinds[Keybind].Control = LeftControlHeld;
                KeybindManager.Keybinds[Keybind].Alt = LeftAltHeld;

                //Update the Player Preference for the Keybind.
                PlayerPrefs.SetInt(Keybind, (int)KeybindManager.Keybinds[Keybind].Key);
                PlayerPrefs.SetString(Keybind + "Shift", KeybindManager.Keybinds[Keybind].Shift.ToString());
                PlayerPrefs.SetString(Keybind + "Control", KeybindManager.Keybinds[Keybind].Control.ToString());
                PlayerPrefs.SetString(Keybind + "Alt", KeybindManager.Keybinds[Keybind].Alt.ToString());

                //Create the string to apply to the Keybind Button Text and remove "Alpha" from the string if it contains.
                string KeyBindButtonText = Key.ToString();
                if (KeyBindButtonText.Contains("Alpha"))
                {
                    KeyBindButtonText = KeyBindButtonText.Substring(5);
                }

                //Check if the Key is a Mouse Button and change the text to a more appropriate string.
                if (KeyBindButtonText == "Mouse2")
                {
                    KeyBindButtonText = "Middle Mouse Button";
                }
                else if (KeyBindButtonText.Contains("Mouse"))
                {
                    KeyBindButtonText = KeyBindButtonText.Insert(5, " Button ");
                }

                //Concatenate the Modifier Keys into the KeyBindButtonText.
                if (LeftAltHeld)
                {
                    KeyBindButtonText = "Alt + " + KeyBindButtonText;
                }
                if (LeftControlHeld)
                {
                    KeyBindButtonText = "Control + " + KeyBindButtonText;
                }
                if (LeftShiftHeld)
                {
                    KeyBindButtonText = "Shift + " + KeyBindButtonText;
                }
                KeybindText.text = KeyBindButtonText;

                //Completed Keybinding.
                KeybindManager.UpdatingKey = false;
            }
        }
    }

    void KeybindConflictCheck(KeyCode newKey, bool shift, bool control, bool alt)
    {
        ResetBindStatusText();

        //Check for confliction.
        foreach (KeybindManager.Keybind x in KeybindManager.Keybinds.Values)
        {
            if (x.Key == newKey && x.Shift == shift && x.Control == control && x.Alt == alt)
            {
                if (x.Name == ClickButton.transform.parent.name)
                //If the button that matches is the same button then it isn't a conflict.
                {
                    break;
                }
                //Bring up a message of confliction
                string conflictAction = x.Name;
                BindStatus.GetComponentsInChildren<TMP_Text>()[1].text = "Action Unbound: " + conflictAction;

                //Get the keybind's button that was conflicted and set the text child of the button to None & the Keybind to None as well.
                GameObject conflictActionObject = null;
                GameObject[] AllGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject y in AllGameObjects)
                {
                    if (y.name == conflictAction)
                    {
                        conflictActionObject = y;
                        break;
                    }
                }
                conflictActionObject.GetComponentsInChildren<Button>()[1].GetComponentInChildren<TMP_Text>().text = "None";
                x.Key = KeyCode.None;
                x.Shift = false;
                x.Control = false;
                x.Alt = false;

                PlayerPrefs.SetInt(x.Name, (int)KeyCode.None);
                PlayerPrefs.SetString(x.Name + "Shift", "False");
                PlayerPrefs.SetString(x.Name + "Control", "False");
                PlayerPrefs.SetString(x.Name + "Alt", "False");

                return;
            }
        }
        BindStatus.GetComponentsInChildren<TMP_Text>()[0].text = "Key Bound Successfully";
    }

    private void OnDisable()
    {
        //Reset everything back to normal.
        HighlightImage.SetActive(false);
        ResetBindStatusText();
        KeybindManager.UpdatingKey = false;
        UnbindButton.interactable = false;
        ClickButton.interactable = true;
        StopAllCoroutines();
    }
}
