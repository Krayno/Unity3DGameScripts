using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarkRift.Client;
using DarkRift;
using TMPro;
using System.IO;

public class DeleteCharacterButton : MonoBehaviour
{
    public CharacterSelectManager CharacterSelectManager;
    public GameObject DeleteCharacterConfirmationNotification;
    public Button DeleteCharacterConfirmationYesButton;
    public GameObject DeletingCharacterNotification;
    public GameObject CharacterDeletionFailedNotification;
    public GameObject ButtonGroup;
    public TMP_Text CharacterName;

    public Button CharacterCreateButton;
    private Button CharacterDeleteButton;

    void Awake()
    {
        //Get our button.
        CharacterDeleteButton = transform.GetComponent<Button>();

        //Subscribe to server messages.
        ServerManager.Instance.Clients["LoginServer"].MessageReceived += OnMessageReceived;

        DeleteCharacterConfirmationYesButton.onClick.AddListener(OnYesButtonClicked);

        //Allow character deletion if more than one character.
        if (ClientGlobals.Characters.Count > 0)
        {
            CharacterDeleteButton.interactable = true;
        }
        else
        {
            CharacterDeleteButton.interactable = false;
        }
    }

    private void OnYesButtonClicked()
    {
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        DeleteCharacter();
    }

    private void DeleteCharacter()
    {
        //Set other notification panels as not active.
        DeletingCharacterNotification.SetActive(true);
        CharacterDeletionFailedNotification.SetActive(false);

        if (ClientGlobals.SelectedCharacter != null)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(ClientGlobals.SteamID);
                Writer.Write(CharacterName.text);

                using (Message Message = Message.Create(PacketTags.RequestDeleteCharacter, Writer))
                {
                    ServerManager.Instance.Clients["LoginServer"].SendMessage(Message, SendMode.Reliable);
                }
            }
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.RequestDeleteCharacter)
            {
                DeleteCharacterResult(Message);
            }
        }
    }

    private void DeleteCharacterResult(Message message)
    {
        using (DarkRiftReader Reader = message.GetReader())
        {
            bool Result;

            try
            {
                Result = Reader.ReadBoolean();
            }
            catch (EndOfStreamException)
            {
                Debug.Log($"Login Server sent an invalid 'RequestDeleteCharacter' Packet.");
                return;
            }

            if (Result)
            {
                CharacterSelectManager.DeleteCharacter();
                CharacterName.text = string.Empty;

                //Set other notification panels as not active.
                DeleteCharacterConfirmationNotification.SetActive(false);
                DeletingCharacterNotification.SetActive(false);
                CharacterDeletionFailedNotification.SetActive(false);

                //Allow you to create a character if you have less than 8 characters.
                if (ClientGlobals.Characters.Count < 8)
                {
                    CharacterCreateButton.interactable = true;
                }

                //Don't allow the player to delete a character if less than 1 character.
                if (ClientGlobals.Characters.Count < 1)
                {
                    CharacterDeleteButton.interactable = false;
                }
            }
            else
            {
                //Set other notification panels as not active.
                DeleteCharacterConfirmationNotification.SetActive(false);
                DeletingCharacterNotification.SetActive(false);
                StartCoroutine(Fade(CharacterDeletionFailedNotification.GetComponent<CanvasGroup>(), 0, 1, 0.3f));
            }
        }
    }

    private void OnDestroy()
    {
        ServerManager.Instance.Clients["LoginServer"].MessageReceived -= OnMessageReceived;
    }

    private IEnumerator Fade(CanvasGroup panel, float start, float end, float duration)
    {
        float Counter = 0;
        float Duration = duration;

        panel.gameObject.SetActive(true);

        while (Counter < Duration)
        {
            Counter += Time.deltaTime;

            panel.alpha = Mathf.Lerp(start, end, Counter / Duration);

            yield return null;
        }
    }
}
