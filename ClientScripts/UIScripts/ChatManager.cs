using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System.IO;

public class ChatManager : MonoBehaviour
{
    public TMP_InputField Inputfield;
    public TMP_Text InputfieldText;
    public GameObject ChatboxMessage;
    public Transform[] ChatboxScrollviews;
    public TMP_Text ChatChannelText;
    public KeybindManager KeybindManager;

    public int ChatboxMessagesCap;

    private enumChatChannel CurrentChatChannel;
    private enum enumChatChannel
    {
        Say = 0,
        World = 1,
        Global = 2,
        Guild = 3,
        Friend = 4,
        Server = 5,
    }

    private List<string> ChatChannelHexColours;
    private Dictionary<enumChatChannel, ChatChannel> ChatChannelInformation;

    public class ChatChannel
    {
        public string ChatChannelColour;
        public int[] ChatboxScrollviews;

        public ChatChannel(string chatChannelColour, int[] chatboxScrollViews)
        {
            ChatChannelColour = chatChannelColour;
            ChatboxScrollviews = chatboxScrollViews;
        }
    }

    private void Awake()
    {
        Inputfield.onSubmit.AddListener(OnInputFieldSubmit);
        Inputfield.onValueChanged.AddListener(OnInputFieldValueChanged);
        Inputfield.onDeselect.AddListener(OnInputFieldDeselect);
        Inputfield.onSelect.AddListener(OnInputFieldSelect);

        ChatChannelHexColours = new List<string> { "#FFFFFF", //Chatchannel: Say, Colour: White.
                                                   "#FFACAC", //Chatchannel: World, Colour: Red.
                                                   "#ACADFF", //Chatchannel: Global, Colour: Purple.
                                                   "#09ff00", //Chatchannel: Guild, Colour: Green;
                                                   "#00fff2", //ChatChannel: Friend, Colour: Blue;
                                                   "yellow" }; //Colour: Yellow.

        ChatChannelInformation = new Dictionary<enumChatChannel, ChatChannel>() 
        {
            { enumChatChannel.Say, new ChatChannel(ChatChannelHexColours[0], new int[] { 0, 2 }) },
            { enumChatChannel.World, new ChatChannel(ChatChannelHexColours[1], new int[] { 0, 2 }) },
            { enumChatChannel.Global, new ChatChannel(ChatChannelHexColours[2], new int[] { 0, 2 }) },
        };

        //Subscribe to Servers.
        ServerManager.Instance.Clients["LoginServer"].MessageReceived += OnMessageReceived;
        ClientGlobals.WorldServer.MessageReceived += OnMessageReceived;
    }

    private void OnInputFieldSelect(string arg0)
    {
        foreach (KeybindManager.Keybind Keybind in KeybindManager.Keybinds.Values)
        {
            Keybind.DisableKeybind = true;
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.ChatMessage)
            {
                HandleChatMessage(e);
            }
        }
    }

    private void HandleChatMessage(MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            using (DarkRiftReader Reader = Message.GetReader())
            {
                enumChatChannel ChatChannel;
                string Name;
                string SentMessage;

                try
                {
                    ChatChannel = (enumChatChannel)Reader.ReadByte();
                    Name = Reader.ReadString();
                    SentMessage = Reader.ReadString();
                }
                catch (EndOfStreamException)
                {
                    Debug.Log($"Login Server or {ClientGlobals.WorldServer.Address} sent an invalid 'ChatMessage' Packet.");
                    return;
                }

                foreach(enumChatChannel ChatChannelInfoEnum in ChatChannelInformation.Keys)
                {
                    if (ChatChannelInfoEnum == ChatChannel)
                    {
                        ChatChannel Channel = ChatChannelInformation[ChatChannelInfoEnum];
                        foreach(int i in Channel.ChatboxScrollviews)
                        {
                            //Add the message to the Scrollview.
                            GameObject ChatMessage = Instantiate(ChatboxMessage, ChatboxScrollviews[i], false);
                            TMP_Text ChatMessageText = ChatMessage.GetComponent<TMP_Text>();
                            ChatMessageText.text = $"<color=#797979>[{DateTime.Now.ToString("hh:mm:ss")}] <color={Channel.ChatChannelColour}>[{ChatChannel}] [{Name}]: {SentMessage}";

                            //Check if the scrollview is over the ChatboxMessageCap.
                            if (ChatboxScrollviews[i].childCount > ChatboxMessagesCap)
                            {
                                Destroy(ChatboxScrollviews[i].GetChild(0).gameObject);
                            }
                        }

                        break;
                    }
                }
            }
        }
    }

    //Select/Deselect the Inputfield and disable/enable keybinds.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (EventSystem.current.currentSelectedGameObject == Inputfield.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
                foreach (KeybindManager.Keybind Keybind in KeybindManager.Keybinds.Values)
                {
                    Keybind.DisableKeybind = false;
                }
            }
            else
            {
                Inputfield.Select();
                foreach (KeybindManager.Keybind Keybind in KeybindManager.Keybinds.Values)
                {
                    Keybind.DisableKeybind = true;
                }
            }
        }
    }

    //Reset Inputfield Text.
    private void OnInputFieldDeselect(string arg0)
    {
        Inputfield.text = string.Empty;
        foreach (KeybindManager.Keybind Keybind in KeybindManager.Keybinds.Values)
        {
            Keybind.DisableKeybind = false;
        }
    }

    //Change Chat Channel.
    private void OnInputFieldValueChanged(string arg0)
    {
        if (arg0 == "/world ")
        {
            Color WorldColour;
            if (ColorUtility.TryParseHtmlString(ChatChannelHexColours[1], out WorldColour))
            {
                ChatChannelText.text = $"<color={ChatChannelHexColours[1]}>World:";
                Inputfield.text = string.Empty;
                InputfieldText.color = WorldColour;
                CurrentChatChannel = enumChatChannel.World;
            }
            else
            {
                Debug.Log("Failed to Parse World Html Colour string.");
            }
        }
        if (arg0 == "/global ")
        {
            Color GlobalColour;
            if (ColorUtility.TryParseHtmlString(ChatChannelHexColours[2], out GlobalColour))
            {
                ChatChannelText.text = $"<color={ChatChannelHexColours[2]}>Global:";
                Inputfield.text = string.Empty;
                InputfieldText.color = GlobalColour;
                CurrentChatChannel = enumChatChannel.Global;
            }
            else
            {
                Debug.Log("Failed to Parse Global Html Colour string.");
            }
        }
        //if (arg0 == "/guild ") -> Not Implemented <-
        //{
        //    ChatChannel.text = "<color=#09ff00>Guild:"; //Green
        //    Inputfield.text = string.Empty;
        //    InputfieldText.color = GuildColor;
        //    CurrentChatChannel = enumChatChannel.Guild;
        // }
        //if (arg0 == "/friend ") -> Not Implemented <-
        //{
        //    ChatChannel.text = "<color=#00fff2>Friend:"; //Light Blue
        //    Inputfield.text = string.Empty;
        //    InputfieldText.color = FriendColor;
        //}
        if (arg0 == "/say ")
        {
            Color SayColour;
            if (ColorUtility.TryParseHtmlString(ChatChannelHexColours[0], out SayColour))
            {
                ChatChannelText.text = $"<color={ChatChannelHexColours[0]}>Say:";
                Inputfield.text = string.Empty;
                InputfieldText.color = SayColour;
                CurrentChatChannel = enumChatChannel.Say;
            }
            else
            {
                Debug.Log("Failed to Parse Say Html Colour string.");
            }
        }
        //Server Messages = Yellow.
    }

    //Send Messages to Server.
    private void OnInputFieldSubmit(string arg0)
    {
        if (arg0 == string.Empty)
        {
            return;
        }

        if (arg0 == "/help")
        {
            GameObject ChatMessage = Instantiate(ChatboxMessage, ChatboxScrollviews[0], false);
            TMP_Text ChatMessageText = ChatMessage.GetComponent<TMP_Text>();
            int Hour = DateTime.Now.Hour;
            int Minutes = DateTime.Now.Minute;
            int Seconds = DateTime.Now.Second;
            ChatMessageText.text = $"<color=#797979>[{Hour}:{Minutes}:{Seconds}] <color=yellow> :- Helpful Commands -:" +
                $"                                                             \n<color=yellow>- \"/say \" to toggle Say chat." +
                $"                                                             \n<color=yellow>- \"/world \" to toggle World chat." +
                $"                                                             \n<color=yellow>- \"/global \" to toggle Global chat." +
                $"                                                             \n<color=yellow>If you ever feel stuck, feel free to join the official Discord to ask for some help!";

            return;
        }

        using (DarkRiftWriter Writer = DarkRiftWriter.Create())
        {
            Writer.Write(ClientGlobals.SteamID);
            Writer.Write((byte)CurrentChatChannel);
            Writer.Write(arg0);

            using (Message Message = Message.Create(PacketTags.ChatMessage, Writer))
            {
                UnityClient Client = (byte)CurrentChatChannel < 2 ? ClientGlobals.WorldServer : ServerManager.Instance.Clients["LoginServer"];

                Client.SendMessage(Message, SendMode.Reliable);
            }
        }
        Inputfield.text = string.Empty;
    }

    private void OnDestroy()
    {
        //Unsubscribe to all servers.
        foreach (UnityClient Server in ServerManager.Instance.Clients.Values)
        {
            Server.MessageReceived -= OnMessageReceived;
        } 
    }
}