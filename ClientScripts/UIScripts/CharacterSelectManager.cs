using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class CharacterSelectManager : MonoBehaviour
{
    public GameObject CharacterElementPrefab;
    public GameObject CharacterPrefab;

    public TMP_Text CharacterNameText;
    public GameObject MiddleBarsGroup;

    public List<Image> HighlightImages;
    public Image CurrentlySelectedCharacterImage;

    public GameObject CharacterHolder;

    public ColoursScriptableObject Colours;

    public Button EnterWorldButton;

    public Button CreateCharacter;
    public Button CharacterDeleteButton;

    public enum Zones
    {
        Tutorial_Island
    }

    //First Child of CharacterGroup must be the HighlightImage.

    private void Awake()
    {
        //Check if player is at the maximum amount of characters.
        if (ClientGlobals.Characters.Count > 7)
        {
            CreateCharacter.interactable = false;
        }

        //Add each character the account has to the CharacterPanel and spawn the character.
        foreach (Character Character in ClientGlobals.Characters)
        {
            //Spawn and Setup CharacterBar information.
            GameObject CharacterBar = Instantiate(CharacterElementPrefab, MiddleBarsGroup.transform, false);
            CharacterBar.GetComponent<CharacterElementReferences>().CharacterName.text = Character.Name;
            CharacterBar.GetComponent<CharacterElementReferences>().CharacterTotalLevel.text = Character.TotalLevel.ToString();
            CharacterBar.GetComponent<CharacterElementReferences>().CharacterLocation.text = ((Zones)Character.ZoneID).ToString().Replace("_", " ");
            CharacterBar.name = Character.Name;

            //Retrieve the index of the colour from the appearance int.
            int Gender = int.Parse(Character.Appearance.ToString().Substring(0, 1));
            int SkinColour = int.Parse(Character.Appearance.ToString().Substring(1, 2));
            int UnderwearColour = int.Parse(Character.Appearance.ToString().Substring(3, 2));
            int ScleraColour = int.Parse(Character.Appearance.ToString().Substring(5, 2));
            int LeftEyeColour = int.Parse(Character.Appearance.ToString().Substring(7, 2));
            int RightEyeColour = int.Parse(Character.Appearance.ToString().Substring(9, 2));
            int BraColour = Gender == 1 ? SkinColour : UnderwearColour; //If male, set bra to skin colour.
            int MouthColour = int.Parse(Character.Appearance.ToString().Substring(13, 2));
            int EyebrowColour = int.Parse(Character.Appearance.ToString().Substring(15, 2));
            int HairStyle = int.Parse(Character.Appearance.ToString().Substring(17, 2));

            //Spawn, Setup the Character then disable.
            GameObject GameCharacter = Instantiate(CharacterPrefab, CharacterHolder.transform, false);
            GameCharacter.SetActive(false);

            //Set the name of the character.
            GameCharacter.name = Character.Name;

            //Get the model.
            SkinnedMeshRenderer Model = GameCharacter.GetComponent<NewCharacterReferences>().Model;

            //Colour the model.
            Model.materials[0].color = Colours.Colours[SkinColour];
            Model.materials[1].color = Colours.Colours[UnderwearColour];
            Model.materials[2].color = Colours.Colours[ScleraColour];
            Model.materials[3].color = Colours.Colours[LeftEyeColour];
            Model.materials[4].color = Colours.Colours[RightEyeColour];
            Model.materials[5].color = Colours.Colours[BraColour];
            Model.materials[6].color = Colours.Colours[MouthColour];
            Model.materials[7].color = Colours.Colours[EyebrowColour];

            //Set the hairstyle of the model and colour it.
            GameCharacter.GetComponent<NewCharacterReferences>().Hairstyles[HairStyle].gameObject.SetActive(true);
            GameCharacter.GetComponent<NewCharacterReferences>().Hairstyles[HairStyle].material.color = Colours.Colours[EyebrowColour];
        }

        //Initialise List.
        HighlightImages = new List<Image>();

        //Add to the list and add a listener to each HighlightImage button if there are characters.
        if (ClientGlobals.Characters.Count > 0)
        {
            foreach (Transform x in MiddleBarsGroup.transform)
            {
                HighlightImages.Add(x.GetChild(0).GetComponent<Image>());
                x.GetComponent<Button>().onClick.AddListener(OnCharacterClick);
            }

            //Select the first character in the list and set the character active.
            HighlightImages[0].gameObject.SetActive(true);
            ClientGlobals.SelectedCharacter = ClientGlobals.Characters[0];
            CharacterHolder.transform.GetChild(0).gameObject.SetActive(true);
            CurrentlySelectedCharacterImage = HighlightImages[0];
            CharacterNameText.text = CurrentlySelectedCharacterImage.transform.parent.name;
        }
        else
        {
            CharacterDeleteButton.interactable = false;
            EnterWorldButton.interactable = false;
        }
    }

    private void OnCharacterClick()
    {
        //Play OpenClose Sound.
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        foreach (Image Image in HighlightImages)
        {
            //If the image that they clicked on is in the Highlight Images, highlight it and unhighlight the other images.
            if (EventSystem.current.currentSelectedGameObject.name == Image.transform.parent.name)
            {
                Image.gameObject.SetActive(true);
                Image.transform.parent.GetChild(2).gameObject.SetActive(true);
                CurrentlySelectedCharacterImage = Image;
                CharacterNameText.text = CurrentlySelectedCharacterImage.transform.parent.name;
                CharacterDeleteButton.interactable = true;

                //Set the Character Model active.
                CharacterHolder.transform.GetChild(HighlightImages.IndexOf(Image)).gameObject.SetActive(true);

                //Update the ClientGlobalsSelectedCharacter.
                foreach (Character Character in ClientGlobals.Characters)
                {
                    if (Character.Name == Image.transform.parent.name)
                    {
                        ClientGlobals.SelectedCharacter = Character;
                        break;
                    }
                }

                //Allow the player to enter world if they are connected.
                if (ClientGlobals.WorldServer != null && ClientGlobals.WorldServer.ConnectionState == DarkRift.ConnectionState.Connected)
                {
                    EnterWorldButton.interactable = true;
                }
            }
            else
            {
                Image.gameObject.SetActive(false);
                CharacterHolder.transform.GetChild(HighlightImages.IndexOf(Image)).gameObject.SetActive(false);
            }

        }
    }

    public void NewCharacterCreated(Image HighlightImage)
    {
        //Add the Highlight Image to the list.
        HighlightImages.Add(HighlightImage);

        //Add the button listener.
        MiddleBarsGroup.transform.GetChild(MiddleBarsGroup.transform.childCount - 1).GetComponent<Button>().onClick.AddListener(OnCharacterClick);

        foreach (Image Image in HighlightImages)
        {
            //Highlight the image.
            if (HighlightImage == Image)
            {
                HighlightImage.gameObject.SetActive(true);
                CurrentlySelectedCharacterImage = HighlightImage;
                CharacterNameText.text = CurrentlySelectedCharacterImage.transform.parent.name;
            }
            else
            {
                Image.gameObject.SetActive(false);
                CharacterHolder.transform.GetChild(HighlightImages.IndexOf(Image)).gameObject.SetActive(false);
            }

        }

        //Check if player is at the maximum amount of characters.
        if (ClientGlobals.Characters.Count >= 8)
        {
            CreateCharacter.interactable = false;
        }
    }

    public void ReselectSelectedCharacter()
    {
        //Set the Character Model active.
        if (ClientGlobals.Characters.Count > 0)
        {
            if (CurrentlySelectedCharacterImage != null)
            {
                CharacterHolder.transform.GetChild(HighlightImages.IndexOf(CurrentlySelectedCharacterImage)).gameObject.SetActive(true);
            }
        }
    }

    public void DeleteCharacter()
    {
        foreach (Image Image in HighlightImages)
        {
            if (Image == CurrentlySelectedCharacterImage)
            {
                ClientGlobals.Characters.RemoveAt(HighlightImages.IndexOf(Image));
                Destroy(Image.transform.parent.gameObject);
                Destroy(CharacterHolder.transform.GetChild(HighlightImages.IndexOf(Image)).gameObject);
            }
        }
        HighlightImages.Remove(CurrentlySelectedCharacterImage);
        CurrentlySelectedCharacterImage = null;

        CharacterDeleteButton.interactable = false;
    }
}
