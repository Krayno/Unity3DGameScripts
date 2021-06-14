using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Client.Unity;
using DarkRift.Server;
using DarkRift.Server.Unity;
using DarkRift;
using System.IO;

public class GlobalChat : MonoBehaviour
{
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;
    private List<MessageUpdate> MessageUpdates;

    public AccurateClock AccurateClock; //Event Based Timer.

    private class MessageUpdate
    {
        public IClient Client { get; set; }
        public byte ChatChannel { get; set; }
        public string Name { get; set; }
        public bool ToAllPlayers { get; set; }
        public string Message { get; set; }

        public MessageUpdate(IClient client, byte chatChannel, string name, bool toAllPlayers, string message)
        {
            Client = client;
            ChatChannel = chatChannel;
            Name = name;
            ToAllPlayers = toAllPlayers;
            Message = message;
        }
    }

    private enum enumChatChannel
    {
        Say = 0,
        World = 1,
        Global = 2,
        Guild = 3,
        Friend = 4,
        Server = 5,
    }

    private void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;
        MessageUpdates = new List<MessageUpdate>();

        AccurateClock.Tick.Ticked += EveryTick;

        Server.ClientManager.ClientConnected += OnClientConnected;
    }

    private void OnClientConnected(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += OnMessageReceived;
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
                ulong SteamID;
                byte ChatChannel;
                string SentMessage;

                try
                {
                    SteamID = Reader.ReadUInt64();
                    ChatChannel = Reader.ReadByte();
                    SentMessage = Reader.ReadString();
                }
                catch (EndOfStreamException)
                {
                    e.Client.Strike($"Received malformed 'ChatMessage' packet from {e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port}. Packet Size: {Reader.Length}");
                    return;
                }

                //If the client sent more data than necccesary.
                if (Reader.Length != Reader.Position)
                {
                    e.Client.Strike($"Received malformed 'ChatMessage' packet from {e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port}. Packet Size: {Reader.Length}");
                    return;
                }

                if ((enumChatChannel)ChatChannel == enumChatChannel.Global)
                {
                    MessageUpdates.Add(new MessageUpdate(e.Client, ChatChannel, PlayerManager.SteamPlayers[SteamID].CurrentCharacterName, true, SentMessage));
                }
            }
        }
    }

    private void EveryTick(object sender, EventTimerTickedArgs e)
    {
        if (MessageUpdates.Count > 0)
        {
            foreach (MessageUpdate MessageUpdate in MessageUpdates)
            {
                if (MessageUpdate.ToAllPlayers)
                {
                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(MessageUpdate.ChatChannel);
                        Writer.Write(MessageUpdate.Name);
                        Writer.Write(MessageUpdate.Message);

                        using (Message Message = Message.Create(PacketTags.ChatMessage, Writer))
                        {
                            foreach (Player Player in PlayerManager.SteamPlayers.Values)
                            {
                                Player.Client.SendMessage(Message, SendMode.Reliable);
                            }
                        }
                    }
                }
            }
            MessageUpdates.Clear();
        }
    }
}
