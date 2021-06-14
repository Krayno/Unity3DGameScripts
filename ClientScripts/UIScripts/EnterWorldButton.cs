using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnterWorldButton : MonoBehaviour
{
    public TMP_Text SelectedWorldText;

    public GameObject CannotConnectToWorldNotification;
    public Button CreateCharacterButton;

    private UnityClient Client;
    private Button EnterWorld;

    private void Start()
    {
        //Get button reference and add listener.
        EnterWorld = transform.GetComponent<Button>();

        //Add listener to Enter World.
        EnterWorld.onClick.AddListener(EnterSelectedWorld);

        //Set the default SelectedWorldText and connect to it.
        //Set and get player preference.
        if (!PlayerPrefs.HasKey("DefaultWorld"))
        {
            PlayerPrefs.SetInt("DefaultWorld", 1);
        }

        //Fetch the server reference.
        Client = ServerManager.Instance.Clients["World" + PlayerPrefs.GetInt("DefaultWorld").ToString() + "Server"];

        //Subscribe to server.
        Client.MessageReceived += OnMessageReceived;
        Client.Disconnected += OnDisconnection;

        //Set the ClientGlobal Server.
        ClientGlobals.WorldServer = Client;

        //Connect to the server.
        if (Client.ConnectionState == ConnectionState.Connected)
        {
            if (ClientGlobals.Characters.Count > 0)
            {
                SelectedWorldText.text = "World " + PlayerPrefs.GetInt("DefaultWorld").ToString();
                ClientGlobals.WorldServer = Client;
                StartCoroutine("DelayEnterWorldButton"); //When switching from the game world, fixes button from turning on, off,
            }
            else
            {
                SelectedWorldText.text = "World " + PlayerPrefs.GetInt("DefaultWorld").ToString();
                ClientGlobals.WorldServer = Client;
                EnterWorld.interactable = false;
            }
        }
        if (Client.ConnectionState == ConnectionState.Disconnected)
        {
            Client.ConnectInBackground(Client.Address, Client.Port, true, ConnectionCallback);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)&& EnterWorld.interactable)
        {
            EnterSelectedWorld();
        }
    }

    private void OnDisconnection(object sender, DisconnectedEventArgs e)
    {
        EnterWorld.interactable = false;

        SelectedWorldText.text = "Not Connected";

        StartCoroutine(Fade(CannotConnectToWorldNotification.GetComponent<CanvasGroup>(), 0, 1, 0.3f));
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.ClientToWorldServerAuthentication)
            {
                ResultOfWorldServerLogin(e);
            }
        }
    }

    private void ResultOfWorldServerLogin(MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            using (DarkRiftReader Reader = Message.GetReader())
            {
                bool Result;

                try
                {
                    Result = Reader.ReadBoolean();
                }
                catch (EndOfStreamException)
                {
                    Debug.Log($"Login Server sent an invalid 'ClientToWorldAuthentication' Packet.");
                    return;
                }

                if (Result)
                {
                    SceneManager.LoadSceneAsync("GameWorld");
                }
                else
                {
                    Debug.Log($"{ClientGlobals.SteamID} failed to be verified by the current WorldServer or you are currently in a World.");
                }
            }
        }
    }

    public void EnterSelectedWorld()
    {
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        //If the Client is not currently the ClientGlobals WorldServer, update the subscriptions.
        if (ClientGlobals.WorldServer != Client)
        {
            Client.MessageReceived -= OnMessageReceived;
            Client = ClientGlobals.WorldServer;
            Client.MessageReceived += OnMessageReceived;
        }

        if (ClientGlobals.WorldServer.ConnectionState == ConnectionState.Connected)
        {
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(ClientGlobals.SteamID);
                Writer.Write(ClientGlobals.SelectedCharacter.CharacterSlot);

                using (Message Message = Message.Create(PacketTags.ClientToWorldServerAuthentication, Writer))
                {
                    ClientGlobals.WorldServer.SendMessage(Message, SendMode.Reliable);
                }
            }
        }
        else
        {
            SelectedWorldText.text = "Not Connected";
        }
    }

    IEnumerator DelayEnterWorldButton()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        EnterWorld.interactable = true;
    }

    private void ConnectionCallback(Exception e)
    {
        if (Client.ConnectionState == ConnectionState.Connected)
        {
            if (ClientGlobals.Characters.Count > 0)
            {
                SelectedWorldText.text = "World " + PlayerPrefs.GetInt("DefaultWorld").ToString();
                ClientGlobals.WorldServer = Client;
                if (EnterWorld != null)
                {
                    StartCoroutine("DelayEnterWorldButton");
                }
            }
            else
            {
                SelectedWorldText.text = "World " + PlayerPrefs.GetInt("DefaultWorld").ToString();
                ClientGlobals.WorldServer = Client;
                EnterWorld.interactable = false;
            }
        }
        else
        {
            SelectedWorldText.text = "Not Connected";

            if (SceneManager.GetActiveScene().name == "CharacterSelectScreen")
            {
                CannotConnectToWorldNotification.SetActive(true);
            }

            //DarkRift Bug Github Issue #81 - DarkRiftClient holds on Connecting after failed connection attempt
            try
            {
                Client.Disconnect();
            }
            catch (SocketException) { };
        }
    }

    private void OnDestroy()
    {
        //Unsubscribe to the event.
        Client.MessageReceived -= OnMessageReceived;
        Client.Disconnected -= OnDisconnection;
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
