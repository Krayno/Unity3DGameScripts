using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DarkRift.Client;
using DarkRift;
using DarkRift.Client.Unity;
using System.Net.Sockets;
using System.IO;

public class LoginButton : MonoBehaviour
{
    private Button Button;

    public GameObject ConnectingToServerPanel;
    public GameObject SteamNotOpenPanel;
    public GameObject VersionMismatchNotification;
    public GameObject FailedSteamAuthenticationNotification;
    public GameObject AlreadyLoggedInNotification;
    public GameObject LostConnectionToLoginServerPanel;

    private TMP_Text ConnectingToServerText;

    private UnityClient ClientOfLoginServer;

    private void Awake()
    {
        //Get the Login Button and add a listener to it.
        Button = GetComponent<Button>();
        Button.onClick.AddListener(Login);

        //Set up the remaining private variables.
        ConnectingToServerText = ConnectingToServerPanel.transform.GetChild(0).GetComponent<TMP_Text>();
    }

    private void Start()
    {
        //Done in Start instead of Awake because the ServerManager intialises its lists in Awake.

        //Assign the server.
        ClientOfLoginServer = ServerManager.Instance.Clients["LoginServer"];

        //Subscribe to server messages.
        ClientOfLoginServer.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.RequestLogin)
            {
                RequestLoginResult(Message);
            }
        }
    }

    private void RequestLoginResult(Message Message)
    {
        using (DarkRiftReader Reader = Message.GetReader())
        {
            bool Authenticated;
            bool VersionMismatch;
            bool AlreadyLoggedIn;

            try
            {
                Authenticated = Reader.ReadBoolean();
                VersionMismatch = Reader.ReadBoolean();
                AlreadyLoggedIn = Reader.ReadBoolean();
            }
            catch (EndOfStreamException)
            {
                Debug.Log($"Login Server sent an invalid 'RequestLogin' Packet.");
                return;
            }

            if (VersionMismatch)
            {
                ConnectingToServerPanel.SetActive(false);
                ConnectingToServerPanel.GetComponent<CanvasGroup>().alpha = 0;
                StartCoroutine(Fade(VersionMismatchNotification.GetComponent<CanvasGroup>(), 0, 1, 0.3f));
                Button.interactable = true;
                return;
            }
            if (AlreadyLoggedIn)
            {
                ConnectingToServerPanel.SetActive(false);
                ConnectingToServerPanel.GetComponent<CanvasGroup>().alpha = 0;
                StartCoroutine(Fade(AlreadyLoggedInNotification.GetComponent<CanvasGroup>(), 0, 1, 0.3f));
                Button.interactable = true;
                return;
            }
            if (Authenticated)
            {
                ConnectingToServerText.text = "Login Request Successful.";

                //Loop through all the characters sent.
                if (Reader.Length - Reader.Position  != 0) //If the message still has bytes to be read, characters are present.
                {
                    while (Reader.Position < Reader.Length)
                    {
                        byte CharacterSlot;
                        string Name;
                        ushort TotalLevel;
                        byte ZoneID;
                        long Appearance;

                        try
                        {
                            CharacterSlot = Reader.ReadByte();
                            Name = Reader.ReadString();
                            TotalLevel = Reader.ReadUInt16();
                            ZoneID = Reader.ReadByte();
                            Appearance = Reader.ReadInt64();
                        }
                        catch (EndOfStreamException)
                        {
                            Debug.Log($"Login Server sent an invalid character in the 'RequestLogin' Packet.");
                            Button.interactable = true;
                            ClientGlobals.Characters.Clear();
                            return;
                        }

                        ClientGlobals.Characters.Add(new Character(CharacterSlot, Name, TotalLevel, ZoneID, Appearance)); 
                    }
                }

                SceneManager.LoadSceneAsync("CharacterSelection");
            }
            else
            {
                ConnectingToServerPanel.SetActive(false);
                ConnectingToServerPanel.GetComponent<CanvasGroup>().alpha = 0;
                StartCoroutine(Fade(FailedSteamAuthenticationNotification.GetComponent<CanvasGroup>(), 0, 1, 0.3f));
                Button.interactable = true;
            }
        }
    }

    private void SendLoginRequest(ulong SteamID)
    {
        using (DarkRiftWriter Writer = DarkRiftWriter.Create())
        {
            Writer.Write(ClientGlobals.ClientVersion);
            Writer.Write(SteamID);

            using (Message Message = Message.Create(PacketTags.RequestLogin, Writer))
            {
                ClientOfLoginServer.SendMessage(Message, SendMode.Reliable);
            }
        }
    }

    private void Login()
    {
        SoundManager.Instance.Sounds["UIOpenClose"].Play();
        Button.interactable = false;

        //Check if Steam was open when the game launched.
        if (SteamManager.Initialized)
        {
            StartCoroutine(Fade(ConnectingToServerPanel.GetComponent<CanvasGroup>(), 0, 1, 0.3f));

            //Connect to server.
            ClientOfLoginServer.ConnectInBackground(ClientOfLoginServer.Host, ClientOfLoginServer.Port, true, ConnectionCallback);
        }
        else
        {
            StartCoroutine(Fade(SteamNotOpenPanel.GetComponent<CanvasGroup>(), 0, 1, 0.3f));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && Button.interactable)
        {
            Login();
        }
    }

    private void ConnectionCallback(Exception e)
    {
        if (ClientOfLoginServer.ConnectionState == ConnectionState.Connected)
        {
            //Request a login. This whole section will need to be replaced with the proper way. 
            //Steamworks.SteamUser.GetSteamID().m_SteamID; <-- How you get a steamID without a gameID.
            //(ulong)UnityEngine.Random.Range(10000000000000000000, 18446744073709551615) <-- Random Generator to test multiple clients.

            ClientGlobals.SteamID = Steamworks.SteamUser.GetSteamID().m_SteamID;
            Debug.Log(ClientGlobals.SteamID); //Remove later.

            SendLoginRequest(ClientGlobals.SteamID);

            ConnectingToServerText.text = "Requesting login...";
        }
        else
        {
            Button.interactable = true;
            ConnectingToServerPanel.SetActive(false);
            ConnectingToServerPanel.GetComponent<CanvasGroup>().alpha = 0;
            ConnectingToServerText.text = "Connecting to Server...";
            StartCoroutine(Fade(LostConnectionToLoginServerPanel.GetComponent<CanvasGroup>(), 0, 1, 0.3f));

            //DarkRift Bug Github Issue #81 - DarkRiftClient holds on Connecting after failed connection attempt
            try
            {
                ClientOfLoginServer.Disconnect();
            }
            catch (SocketException) { };
        }
    }

    private void OnDestroy()
    {
        ClientOfLoginServer.MessageReceived -= OnMessageReceived;
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
