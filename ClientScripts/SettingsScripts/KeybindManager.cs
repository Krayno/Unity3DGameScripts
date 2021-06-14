using System.Collections.Generic;
using UnityEngine;

public class KeybindManager : MonoBehaviour
{
    public static Dictionary<string, Keybind> Keybinds;
    public List<PlayerPreferenceStringInt> PlayerPreferences;

    [HideInInspector]
    public static bool UpdatingKey;

    public class Keybind
    {
        public string Name { get; set; }
        public KeyCode Key { get; set; }
        public bool Shift { get; set; }
        public bool Control { get; set; }
        public bool Alt { get; set; }

        public bool DisableKeybind { get; set; }

        public Keybind(string name, int key, bool shift, bool control, bool alt) //Constructor
        {
            Name = name;
            Key = (KeyCode)key;
            Shift = shift;
            Control = control;
            Alt = alt;
            DisableKeybind = false;
        }

        public bool IsPressed()
        {
            if (DisableKeybind || UpdatingKey)
            {
                return false;
            }

            KeyCode ShiftKey = KeyCode.LeftShift;
            KeyCode ControlKey = KeyCode.LeftControl;
            KeyCode AltKey = KeyCode.LeftAlt;
            if (Shift == true && Control == true && Alt == true)
            {
                if (Input.GetKeyDown(Key) && Input.GetKey(ShiftKey) && Input.GetKey(ControlKey) && Input.GetKey(AltKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Shift == true && Control == true)
            {
                if (Input.GetKeyDown(Key) && Input.GetKey(ShiftKey) && Input.GetKey(ControlKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Shift == true && Alt == true)
            {
                if (Input.GetKeyDown(Key) && Input.GetKey(ShiftKey) && Input.GetKey(AltKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Control == true && Alt == true)
            {
                if (Input.GetKeyDown(Key) && Input.GetKey(ControlKey) && Input.GetKey(AltKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Shift == true)
            {
                if (Input.GetKeyDown(Key) && Input.GetKey(ShiftKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Control == true)
            {
                if (Input.GetKeyDown(Key) && Input.GetKey(ControlKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Alt == true)
            {
                if (Input.GetKeyDown(Key) && Input.GetKey(AltKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Input.GetKeyDown(Key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsHeld()
        {
            if (DisableKeybind)
            {
                return false;
            }

            KeyCode ShiftKey = KeyCode.LeftShift;
            KeyCode ControlKey = KeyCode.LeftControl;
            KeyCode AltKey = KeyCode.LeftAlt;
            if (Shift == true && Control == true && Alt == true)
            {
                if (Input.GetKey(Key) && Input.GetKey(ShiftKey) && Input.GetKey(ControlKey) && Input.GetKey(AltKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Shift == true && Control == true)
            {
                if (Input.GetKey(Key) && Input.GetKey(ShiftKey) && Input.GetKey(ControlKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Shift == true && Alt == true)
            {
                if (Input.GetKey(Key) && Input.GetKey(ShiftKey) && Input.GetKey(AltKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Control == true && Alt == true)
            {
                if (Input.GetKey(Key) && Input.GetKey(ControlKey) && Input.GetKey(AltKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Shift == true)
            {
                if (Input.GetKey(Key) && Input.GetKey(ShiftKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Control == true)
            {
                if (Input.GetKey(Key) && Input.GetKey(ControlKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Alt == true)
            {
                if (Input.GetKey(Key) && Input.GetKey(AltKey))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (Input.GetKey(Key))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    public class PlayerPreferenceStringInt
    {
        public string Name { get; set; }
        public int DefaultKey { get; set; }

        public PlayerPreferenceStringInt(string name, int defaultKey)
        {
            Name = name;
            DefaultKey = defaultKey;
        }
    }
    //Add Keybinds to "PlayerPreferences".
    private void Awake()
    {
        //Initialise variables.
        Keybinds = new Dictionary<string, Keybind>();
        PlayerPreferences = new List<PlayerPreferenceStringInt>();
        UpdatingKey = false;

        //Add all the Keybinds available in the game.
        PlayerPreferences.Add(new PlayerPreferenceStringInt("Jump", 32));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("MoveForward", 119));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("MoveRight", 100));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("MoveBackward", 115));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("MoveLeft", 97));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("Social", 111));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("Skills", 107));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("Backpack", 98));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("Character", 104));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("Map", 109));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("ActionButton1", 113));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("ActionButton2", 101));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("ActionButton3", 114));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("ActionButton4", 102));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("ActionButton5", 99));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("ActionButton6", 49));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("ActionButton7", 50));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("ActionButton8", 51));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("ActionButton9", 52));
        PlayerPreferences.Add(new PlayerPreferenceStringInt("ActionButton10", 53));

        //Set the Player Preference for each keybind.
        foreach (PlayerPreferenceStringInt x in PlayerPreferences)
        {

            Keybinds.Add(x.Name, new Keybind(x.Name, PlayerPrefs.GetInt(x.Name, x.DefaultKey),
                                                    bool.Parse(PlayerPrefs.GetString(x.Name + "Shift", "False")),
                                                    bool.Parse(PlayerPrefs.GetString(x.Name + "Control", "False")),
                                                    bool.Parse(PlayerPrefs.GetString(x.Name + "Alt", "False"))));

            PlayerPrefs.SetInt(x.Name, (int)Keybinds[x.Name].Key);
            PlayerPrefs.SetString(x.Name + "Shift", Keybinds[x.Name].Shift.ToString());
            PlayerPrefs.SetString(x.Name + "Control", Keybinds[x.Name].Control.ToString());
            PlayerPrefs.SetString(x.Name + "Alt", Keybinds[x.Name].Shift.ToString());
        }
    }
}
