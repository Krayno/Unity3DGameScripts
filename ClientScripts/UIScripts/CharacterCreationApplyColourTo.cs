using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterCreationApplyColourTo : MonoBehaviour
{
    public string ApplyTo;
    public ToggleNewCharacter ToggleNewCharacter;
    public Button[] Buttons;

    public Color SelectedColour;
    public int SelectedColourIndex;

    public CharacterCreationApplyHairStyle HairstyleSelected;

    //private List<Color> ButtonColours;

    void Awake()
    {
        foreach (Button Button in Buttons)
        {
            Button.onClick.AddListener(OnButtonClicked);
        }

        SelectedColourIndex = -1;
    }

    private void OnButtonClicked()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        //Colour in the scripts body part.
        //Get the model.
        SkinnedMeshRenderer Model = ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Model;

        switch (ApplyTo)
        {
            case "Skin":
                if (Model.materials[5].color == Model.materials[0].color) //If male.
                {
                    ApplyColour(Model.materials[0]);
                    ApplyColour(Model.materials[5]);
                }
                ApplyColour(Model.materials[0]);
                break;
            case "Undergarments":
                if (Model.materials[5].color == Model.materials[0].color) //If male.
                {
                    ApplyColour(Model.materials[1]);
                }
                else //If female.
                {
                    ApplyColour(Model.materials[1]);
                    ApplyColour(Model.materials[5]);
                }
                break;
            case "Sclera":
                ApplyColour(Model.materials[2]);
                break;
            case "LeftEye":
                ApplyColour(Model.materials[3]);
                break;
            case "RightEye":
                ApplyColour(Model.materials[4]);
                break;
            case "Mouth":
                ApplyColour(Model.materials[6]);
                break;
            case "Hair":
                ApplyColour(Model.materials[7]); //Eyebrows
                foreach (SkinnedMeshRenderer Hair in ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Hairstyles)
                {
                    ApplyColour(Hair.material);
                }
                break;
            default:
                Debug.Log("CharacterCreationApplyColourTo has an improperly set \"ApplyTo\" string.");
                break;
        }
    }

    private void ApplyColour(Material Material)
    {
        int Counter = 0;
        foreach (Button Button in Buttons)
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent<Button>() == Button)
            {
                Material.color = Button.transform.GetChild(0).GetComponent<Image>().color;
                SelectedColour = Button.transform.GetChild(0).GetComponent<Image>().color;
                SelectedColourIndex = Counter;
            }
            Counter++;
        }
    }
}
