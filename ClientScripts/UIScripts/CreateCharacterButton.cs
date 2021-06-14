using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarkRift.Client;
using DarkRift;
using System;
using TMPro;
using System.IO;

public class CreateCharacterButton : MonoBehaviour
{
    public TMP_InputField NameInputField;
    public GameObject NotificationPanel;
    public TMP_Text InformationText;
    public GameObject CharacterHolder;
    public ColoursScriptableObject Colours;

    public GameObject CharacterElementPrefab;
    public GameObject CharacterPrefab;
    public TMP_Text CharacterNameText;
    public GameObject MiddleBarsGroup;
    public CharacterSelectManager CharacterSelectManager;
    public ToggleNewCharacter ToggleNewCharacter;
    public CharacterCreationApplyHairStyle HairstyleSelected;

    public Button DeleteCharacterButton;
    public Button EnterWorldButton;

    private Button Button;
    private SwitchCanvasFade SwitchCanvasFade;

    private char[] BannedCharacters;

    private string Username;
    private long Appearance;

    void Awake()
    {
        SwitchCanvasFade = transform.GetComponent<SwitchCanvasFade>();

        //Setup Button Reference.
        Button = transform.GetComponent<Button>();
        //Add listener.
        Button.onClick.AddListener(OnCreateCharacterClick);

        //Setup BannedCharacters.
        BannedCharacters = new char[] {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
                                       '!', '@', '#', '$', '%', '^', '&', '*', '(', ')',
                                       '-', '_', '+', '=', '\'', '\\', '|', '[', '{', ']', '}',
                                       ';', ':', '"', '<', ',', '>', '.', '/', '?', '`', '~'};

        //Subscribe to server messages.
        ServerManager.Instance.Clients["LoginServer"].MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.RequestCreateCharacter)
            {
                CreateCharacterResult(Message);
            }
        }
    }

    private void CreateCharacterResult(Message message)
    {
        using (DarkRiftReader Reader = message.GetReader())
        {
            bool UsernameTaken;

            try
            {
                UsernameTaken = Reader.ReadBoolean();
            }
            catch (EndOfStreamException)
            {
                Debug.Log("Login Server sent an invalid 'RequestCreateCharacter' Packet.");
                return;
            }

            if (!UsernameTaken)
            {
                //Spawn and Setup CharacterBar information.
                GameObject CharacterBar = Instantiate(CharacterElementPrefab, MiddleBarsGroup.transform, false);
                CharacterBar.GetComponent<CharacterElementReferences>().CharacterName.text = Username;
                CharacterBar.GetComponent<CharacterElementReferences>().CharacterTotalLevel.text = "0";
                CharacterBar.GetComponent<CharacterElementReferences>().CharacterLocation.text = "Tutorial Island";
                CharacterBar.name = Username;

                //Retrieve the index of the colours and hairstyles from the appearance long.
                int Gender = int.Parse(Appearance.ToString().Substring(0, 1));
                int SkinColour = int.Parse(Appearance.ToString().Substring(1, 2));
                int UnderwearColour = int.Parse(Appearance.ToString().Substring(3, 2));
                int ScleraColour = int.Parse(Appearance.ToString().Substring(5, 2));
                int LeftEyeColour = int.Parse(Appearance.ToString().Substring(7, 2));
                int RightEyeColour = int.Parse(Appearance.ToString().Substring(9, 2));
                int BraColour = Gender == 1 ? SkinColour : UnderwearColour; //If male, set bra to skin colour.
                int MouthColour = int.Parse(Appearance.ToString().Substring(13, 2));
                int EyebrowColour = int.Parse(Appearance.ToString().Substring(15, 2));
                int HairStyle = int.Parse(Appearance.ToString().Substring(17, 2));

                //Destroy the NewCharacter Model.
                Destroy(ToggleNewCharacter.NewCharacter);

                //Spawn, Setup the Character then set it to be the currently selected.
                GameObject GameCharacter = Instantiate(CharacterPrefab, CharacterHolder.transform, false);
                GameCharacter.name = Username;

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

                //Add the character to ClientGlobals.
                ClientGlobals.Characters.Add(new Character((byte)CharacterBar.transform.GetSiblingIndex(), Username, 0, 0, Appearance));
                ClientGlobals.SelectedCharacter = ClientGlobals.Characters[ClientGlobals.Characters.Count - 1];

                CharacterSelectManager.NewCharacterCreated(CharacterBar.GetComponent<CharacterElementReferences>().HighlightImage.GetComponent<Image>());

                //Reset NameInputField.
                NameInputField.text = string.Empty;

                //Allow character to be deleted.
                DeleteCharacterButton.interactable = true;

                //Allow you to enter the world if connected to a world server.
                if (ClientGlobals.WorldServer != null && ClientGlobals.WorldServer.ConnectionState == ConnectionState.Connected)
                {
                    EnterWorldButton.interactable = true;
                }

                //Go to Character Select Canvas.
                SwitchCanvasFade.ManualInitialiseFade();
            }
            else
            {
                Username = string.Empty;

                InformationText.text = "Name has already been taken.";
                NotificationPanel.SetActive(true);
                NotificationPanel.GetComponent<CanvasGroup>().alpha = 1;
            }

            Button.interactable = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnCreateCharacterClick();
        }
    }

    private void OnCreateCharacterClick()
    {
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        Button.interactable = false;

        //Check if the name is valid.
        if (NameInputField.text.Contains(" ")) //Does the name contain spaces?
        {
            InformationText.text = "Name cannot contain spaces.";
            NotificationPanel.SetActive(true);
            NotificationPanel.GetComponent<CanvasGroup>().alpha = 1;

            Button.interactable = true;
            return;
        }

        if (NameInputField.text == string.Empty) //Is the name empty?
        {
            InformationText.text = "Name cannot be empty.";
            NotificationPanel.SetActive(true);
            NotificationPanel.GetComponent<CanvasGroup>().alpha = 1;

            Button.interactable = true;
            return;
        }
        
        foreach (char Character in NameInputField.text) //Does the name contain any special characters or numbers?
        {
            foreach (char BannedCharacter in BannedCharacters)
            {
                if (Character == BannedCharacter)
                {
                    InformationText.text = "Name cannot contain special characters or numbers.";
                    NotificationPanel.SetActive(true);
                    NotificationPanel.GetComponent<CanvasGroup>().alpha = 1;

                    Button.interactable = true;
                    return;
                }
            }
        }

        //Assign the chosen name.
        Username = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(NameInputField.text.ToLower());

        //Get the model.
        SkinnedMeshRenderer Model = ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Model;

        //Form the appearance long.
        int iGender = Model.materials[1].color != Model.materials[5].color ? 1 : 2; //If underwear colour does not equal bra colour, then the gender is a male.
        int iSkinColour = Colours.Colours.IndexOf(Model.materials[0].color);
        int iUnderwearColour = Colours.Colours.IndexOf(Model.materials[1].color);
        int iScleraColour = Colours.Colours.IndexOf(Model.materials[2].color);
        int iLeftEyeColour = Colours.Colours.IndexOf(Model.materials[3].color);
        int iRightEyeColour = Colours.Colours.IndexOf(Model.materials[4].color);
        int iBraColour = Colours.Colours.IndexOf(Model.materials[5].color);
        int iMouthColour = Colours.Colours.IndexOf(Model.materials[6].color);
        int iEyebrowColour = Colours.Colours.IndexOf(Model.materials[7].color);

        //Debug.Log($"Gender:{iGender} - SkinColour:{iSkinColour} - UnderwearColour:{iUnderwearColour} - ScleraColour:{iScleraColour} - LeftEyeColour:{iLeftEyeColour} - RightEyeColour:{iRightEyeColour} - BraColour:{iBraColour} - MouthColour:{iMouthColour} - EyebrowColour:{iEyebrowColour}"); //Remove

        //Get the hairstyle of the randomly generated character.
        int Counter = 0;
        int iHairStyle = Counter;
        foreach (SkinnedMeshRenderer Hair in ToggleNewCharacter.NewCharacter.GetComponent<NewCharacterReferences>().Hairstyles)
        {
            if (Hair.gameObject.activeSelf)
            {
                iHairStyle = Counter;
            }
            Counter++;
        }

        Appearance = long.Parse(iGender.ToString() + 
                              ((iSkinColour < 10) ? "0" + iSkinColour.ToString() : iSkinColour.ToString()) +
                              ((iUnderwearColour < 10) ? "0" + iUnderwearColour.ToString() : iUnderwearColour.ToString()) +
                              ((iScleraColour < 10) ? "0" + iScleraColour.ToString() : iScleraColour.ToString()) +
                              ((iLeftEyeColour < 10) ? "0" + iLeftEyeColour.ToString() : iLeftEyeColour.ToString()) +
                              ((iRightEyeColour < 10) ? "0" + iRightEyeColour.ToString() : iRightEyeColour.ToString()) +
                              ((iBraColour < 10) ? "0" + iBraColour.ToString() : iBraColour.ToString()) +
                              ((iMouthColour < 10) ? "0" + iMouthColour.ToString() : iMouthColour.ToString()) +
                              ((iEyebrowColour < 10) ? "0" + iEyebrowColour.ToString() : iEyebrowColour.ToString()) +
                              ((iHairStyle < 10) ? "0" + iHairStyle.ToString() : iHairStyle.ToString()));

        //Send Message to LoginServer.
        using (DarkRiftWriter Writer = DarkRiftWriter.Create())
        {
            Writer.Write(ClientGlobals.SteamID);
            Writer.Write(Appearance);
            Writer.Write(Username);

            using (Message Message = Message.Create(PacketTags.RequestCreateCharacter, Writer))
            {
                ServerManager.Instance.Clients["LoginServer"].SendMessage(Message, SendMode.Reliable);
            }
        }
    }

    private void OnEnable()
    {
        if (ServerManager.Instance.Clients["LoginServer"].ConnectionState == ConnectionState.Connected)
        {
            Button.interactable = true;
        }
        else
        {
            Button.interactable = false;
        }
    }

    private void OnDestroy()
    {
        //Unsubscribe to server messages.
        ServerManager.Instance.Clients["LoginServer"].MessageReceived -= OnMessageReceived;
    }
}
