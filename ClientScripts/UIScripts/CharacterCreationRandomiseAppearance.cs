using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationRandomiseAppearance : MonoBehaviour
{
    public ColoursScriptableObject Colours;
    public CharacterCreationApplyHairStyle SelectedHairstyles;
    public Button MaleButton;
    public Button FemaleButton;

    private Button RandomiseButton;

    void Awake()
    {
        //Get the button attached to the gameobject.
        RandomiseButton = GetComponent<Button>();

        //Add a listener to the button.
        RandomiseButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        //Generate random values for the model.
        System.Random r = new System.Random();
        int iRandomGender = r.Next(1, 3); // 1 = male, 2 = female.
        int iRandomSkinColour = r.Next(Colours.Colours.Count - 6, Colours.Colours.Count); //Skin Colours will be the last six colours in the array.
        int iRandomUnderwearColour = r.Next(18);
        int iRandomScleraColour = 13; //Sclera always white for players.
        int iRandomLeftEyeColour = r.Next(18);
        int iRandomRightEyeColour = r.Next(18);
        int iRandomBraColour = iRandomGender == 1 ? iRandomSkinColour : iRandomUnderwearColour;
        int iRandomMouthColour = 12; //Sclera always black for players.
        int iRandomEyebrowColour = r.Next(18);
        int iRandomHairStyle = r.Next(2);

        //Get the model.
        SkinnedMeshRenderer Model = ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Model;

        //Colour the model.
        Model.materials[0].color = Colours.Colours[iRandomSkinColour];
        Model.materials[1].color = Colours.Colours[iRandomUnderwearColour];
        Model.materials[2].color = Colours.Colours[iRandomScleraColour];
        Model.materials[3].color = Colours.Colours[iRandomLeftEyeColour];
        Model.materials[4].color = Colours.Colours[iRandomRightEyeColour];
        Model.materials[5].color = Colours.Colours[iRandomBraColour];
        Model.materials[6].color = Colours.Colours[iRandomMouthColour];
        Model.materials[7].color = Colours.Colours[iRandomEyebrowColour];

        //Disable all hairstyles of the model.
        foreach(SkinnedMeshRenderer Hairstyle in ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Hairstyles)
        {
            Hairstyle.gameObject.SetActive(false);
        }

        //Enable all button hairstyles.
        foreach (Button Hairstyle in SelectedHairstyles.Buttons)
        {
            Hairstyle.interactable = true;
        }

        //Set the hairstyle of the model and colour it.
        ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Hairstyles[iRandomHairStyle].gameObject.SetActive(true);
        ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Hairstyles[iRandomHairStyle].material.color = Colours.Colours[iRandomEyebrowColour];

        //Set the selected hairstyle disabled.
        SelectedHairstyles.Buttons[iRandomHairStyle].interactable = false;

        //Set the selected gender disabled.
        if (iRandomGender == 1)
        {
            MaleButton.interactable = false;
            FemaleButton.interactable = true;
        }
        else
        {
            FemaleButton.interactable = false;
            MaleButton.interactable = true;
        }
    }
}
