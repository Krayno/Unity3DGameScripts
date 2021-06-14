using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationApplyHairStyle : MonoBehaviour
{
    public ToggleNewCharacter ToggleNewCharacter;
    public List<Button> Buttons;

    public string SelectedHairStyle;
    public int SelectedHairIndex;

    private List<string> HairStyles;

    void Awake()
    {
        HairStyles = new List<string>();

        foreach (Button Button in Buttons)
        {
            Button.onClick.AddListener(OnButtonClicked);
            HairStyles.Add(Button.transform.GetComponentInChildren<TMP_Text>().text);
        }

        SelectedHairIndex = -1;
    }

    private void OnEnable()
    {
        //Disable the currently active hair styles button.
        int Counter = 0;
        foreach (SkinnedMeshRenderer Hair in ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Hairstyles)
        {
            if (Hair.gameObject.activeSelf)
            {
                Buttons[Counter].interactable = false;
            }
            else
            {
                Buttons[Counter].interactable = true;
            }
            Counter++;
        }
    }

    private void OnButtonClicked()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        foreach (Button Button in Buttons)
        {   
            if (!Button.interactable)
            {
                SelectedHairIndex = Buttons.IndexOf(Button);
                Color CurrentHairStyleColour = Color.black; //Default hairstyle colour.

                //Disable the active hairstyle then enable the hair style selected.
                foreach (SkinnedMeshRenderer Hair in ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Hairstyles)
                {
                    if (Hair.gameObject.activeSelf)
                    {
                        Hair.gameObject.SetActive(false);
                        CurrentHairStyleColour = Hair.material.color;
                    }
                }
                ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Hairstyles[SelectedHairIndex].gameObject.SetActive(true);
                ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Hairstyles[SelectedHairIndex].material.color = CurrentHairStyleColour;

                return;
            }
        }

    }
}
