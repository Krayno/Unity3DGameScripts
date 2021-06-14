using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GenderButtons : MonoBehaviour
{
    public Button MaleButton;
    public Button FemaleButton;

    public ToggleNewCharacter ToggleNewCharacter;

    void Awake()
    {
        //Add listeners to the buttons.
        MaleButton.onClick.AddListener(OnMaleButtonClicked);
        FemaleButton.onClick.AddListener(OnFemaleButtonClicked);
    }

    private void OnEnable()
    {
        //Get the current gender of the model and set the appropriate button to be inactive.
        SkinnedMeshRenderer Model = ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Model;

        //If the bra colour is equal to the underwear colour, then the current character is a female.
        if (Model.materials[5].color == Model.materials[1].color)
        {
            FemaleButton.interactable = false;
            MaleButton.interactable = true;
        }
        else
        {
            MaleButton.interactable = false;
            FemaleButton.interactable = true;
        }
    }

    private void OnFemaleButtonClicked()
    {
        //Get the model.
        SkinnedMeshRenderer Model = ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Model;

        //Set the bra colour to be equal to underwear colour.
        Model.materials[5].color = Model.materials[1].color;

        //Set the button inactive and the other gender active.
        FemaleButton.interactable = false;
        MaleButton.interactable = true;
    }

    private void OnMaleButtonClicked()
    {
        //Get the model.
        SkinnedMeshRenderer Model = ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Model;

        //Set the bra colour to be equal to skin colour.
        Model.materials[5].color = Model.materials[0].color;

        //Set the button inactive and the other gender active.
        FemaleButton.interactable = true;
        MaleButton.interactable = false;
    }
}
