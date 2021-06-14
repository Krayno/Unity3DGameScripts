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
using UnityEngine.UI;

public class WorldServerButton : MonoBehaviour
{
    public TMP_Text WorldNumberText;
    public TMP_Text WorldCurrentPlayersText;
    public TMP_Text WorldLatencyText;
    public TMP_Text SelectedWorldText;
    public Image HighlightImage;
    public Button EnterWorld;

    public static Dictionary<int, Image> HighlightImages = new Dictionary<int, Image>();

    private UnityClient Client;
    private Button Button;
    private byte WorldNumber;

    void Awake()
    {
        //Fetch the server reference and subscribe to MessageReceived.
        Client = ServerManager.Instance.Clients[name.Replace("Button", "") + "Server"];
        Client.MessageReceived += OnMessageReceived;
        Client.Disconnected += OnClientDisconnected;

        //Connect to the server.
        if (Client.ConnectionState == ConnectionState.Disconnected)
        {
            Client.ConnectInBackground(Client.Address, Client.Port, true, ConnectionCallback);
        }

        //Instance World Number.
        WorldNumber = byte.Parse(name.Replace("World", "").Replace("Button", ""));

        //Setup the Texts.
        WorldNumberText.text = WorldNumber.ToString();
        WorldCurrentPlayersText.text = "0";
        WorldLatencyText.text = "Offline";

        //Fetch gameobject's button.
        Button = transform.GetComponent<Button>();

        //Add listener.
        Button.onClick.AddListener(OnButtonClicked);

        //Add highlight image.
        HighlightImages.Add(WorldNumber, HighlightImage);
    }

    private void Start()
    {
        //Select the default world.
        if (HighlightImages[PlayerPrefs.GetInt("DefaultWorld")] == HighlightImage)
        {
            if (Client.ConnectionState == ConnectionState.Connected)
            {
                HighlightImage.gameObject.SetActive(true);
            }
        }
        
    }

    private void OnClientDisconnected(object sender, DisconnectedEventArgs e)
    {
        WorldCurrentPlayersText.text = "0";
        WorldLatencyText.text = "Offline";

        WorldLatencyText.text = "Offline";
        Button.interactable = false;

        HighlightImage.gameObject.SetActive(false);

        if (Client == ClientGlobals.WorldServer)
        {
            SelectedWorldText.text = "Not Connected";
            EnterWorld.interactable = false;
        }
    }

    private void OnButtonClicked()
    {
        SoundManager.Instance.Sounds["UIOpenClose"].Play();

        //Set the ClientGlobal WorldServer equal to the Client variable if conneted.
        if (Client.ConnectionState == ConnectionState.Connected)
        {
            if (ClientGlobals.Characters.Count > 0)
            {
                EnterWorld.interactable = true;
            }
            ClientGlobals.WorldServer = Client;

            //Disable all other HighlightImages except this one.
            foreach (Image Image in HighlightImages.Values)
            {
                Image.gameObject.SetActive(Image == HighlightImage);
            }

            //Set the Default World to instance WorldNumber.
            PlayerPrefs.SetInt("DefaultWorld", WorldNumber);

            SelectedWorldText.text = "World " + WorldNumber.ToString();
        }
    }

    private void OnEnable()
    {
        RefreshInformation();
    }

    private void RefreshInformation()
    {
        if (Client.ConnectionState == ConnectionState.Connected)
        {
            Button.interactable = true;

            //Request PlayerCount and send a Ping.
            using (Message PlayerCount = Message.CreateEmpty(PacketTags.PlayerCount))
            {
                Client.SendMessage(PlayerCount, SendMode.Reliable);
            }
            using (Message Ping = Message.CreateEmpty(PacketTags.Heartbeat))
            {
                Ping.MakePingMessage();
                Client.SendMessage(Ping, SendMode.Reliable);
            }
        }
        else if (Client.ConnectionState == ConnectionState.Disconnected)
        {
            //Try connect again.
            Client.ConnectInBackground(Client.Address, Client.Port, true, ConnectionCallback);
        }
    }

    private void ConnectionCallback(Exception e)
    {
        if (Client.ConnectionState == ConnectionState.Connected)
        {
            WorldLatencyText.text = "0";
            Button.interactable = true;

            //Request PlayerCount and send a Ping.
            using (Message PlayerCount = Message.CreateEmpty(PacketTags.PlayerCount))
            {
                Client.SendMessage(PlayerCount, SendMode.Reliable);
            }
            using (Message Ping = Message.CreateEmpty(PacketTags.Heartbeat))
            {
                Ping.MakePingMessage();
                Client.SendMessage(Ping, SendMode.Reliable);
            }
        }
        else
        {
            WorldLatencyText.text = "Offline";
            Button.interactable = false;

            if (HighlightImage != null)
            {
                HighlightImage.gameObject.SetActive(false);
            }

            //DarkRift Bug Github Issue #81 - DarkRiftClient holds on Connecting after failed connection attempt
            try
            {
                Client.Disconnect();
            }
            catch (SocketException) { };
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.PlayerCount)
            {
                PlayerCountUpdate(Message);
            }
            if (Message.IsPingAcknowledgementMessage)
            {
                double ms = Math.Round(Client.Client.RoundTripTime.SmoothedRtt, 3);

                int multiplier = 1000;
                int formattedMs = (int)(ms * multiplier);
                WorldLatencyText.text = $"{formattedMs}ms";
            }
        }
    }

    private void PlayerCountUpdate(Message message)
    {
        using(DarkRiftReader Reader = message.GetReader())
        {
            short PlayerCount;

            try
            {
                PlayerCount = Reader.ReadInt16();
            }
            catch (EndOfStreamException)
            {
                Debug.Log($"{Client.Address}  sent an invalid 'PlayerCount' Packet.");
                return;
            }

            WorldCurrentPlayersText.text = PlayerCount.ToString();
        }
    }

    private void OnDestroy()
    {
        Client.MessageReceived -= OnMessageReceived;
        Client.Disconnected -= OnClientDisconnected;

        HighlightImages.Clear();
    }
}
