using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleNewCharacter : MonoBehaviour
{
    public GameObject PrefabNewCharacter;
    public Transform ParentTransform;
    public Transform CharacterHolder;
    public CharacterSelectManager CharacterSelectManager;
    public ColoursScriptableObject Colours;
    public TMP_InputField NameInputfield;

    [HideInInspector]
    public static GameObject NewCharacter;

    private Button Button;

    void Awake()
    {
        //Check if the NewCharacter has been initialised.
        if (NewCharacter == default(GameObject))
        {
            NewCharacter = null;
        }

        Button = GetComponent<Button>();
        Button.onClick.AddListener(ToggleNewPlayer);
    }

    private void OnEnable()
    {
        StartCoroutine("WaitForFade");
    }

    IEnumerator WaitForFade()
    {
        CanvasGroup Canvas = GetComponent<SwitchCanvasFade>().FadeOutCanvas;

        while (Canvas.alpha != 1)
        {
            yield return null;
        }

        Button.interactable = true;
    }

    private void ToggleNewPlayer()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        if (NewCharacter == null)
        {
            //Disable the button.
            Button.interactable = false;

            //Set the Characters inactive and spawn the new character.
            for (int i = 0; i < CharacterHolder.childCount; i++)
            {
                Transform child = CharacterHolder.GetChild(i);
                child.gameObject.SetActive(false);
            }

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

            NewCharacter = Instantiate(PrefabNewCharacter, ParentTransform, false);

            //Get the model.
            SkinnedMeshRenderer Model = NewCharacter.GetComponent<NewCharacterReferences>().Model;

            //Colour the model.
            Model.materials[0].color = Colours.Colours[iRandomSkinColour];
            Model.materials[1].color = Colours.Colours[iRandomUnderwearColour];
            Model.materials[2].color = Colours.Colours[iRandomScleraColour];
            Model.materials[3].color = Colours.Colours[iRandomLeftEyeColour];
            Model.materials[4].color = Colours.Colours[iRandomRightEyeColour];
            Model.materials[5].color = Colours.Colours[iRandomBraColour];
            Model.materials[6].color = Colours.Colours[iRandomMouthColour];
            Model.materials[7].color = Colours.Colours[iRandomEyebrowColour];

            //Set the hairstyle of the model and colour it.
            NewCharacter.GetComponent<NewCharacterReferences>().Hairstyles[iRandomHairStyle].gameObject.SetActive(true);
            NewCharacter.GetComponent<NewCharacterReferences>().Hairstyles[iRandomHairStyle].material.color = Colours.Colours[iRandomEyebrowColour];
        }
        else
        {
            Destroy(NewCharacter);

            NewCharacter = null;
            NameInputfield.text = string.Empty;

            CharacterSelectManager.ReselectSelectedCharacter();
            Button.interactable = false;
        }
    }
}
