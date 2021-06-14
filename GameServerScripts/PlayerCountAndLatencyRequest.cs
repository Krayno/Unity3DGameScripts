using DarkRift.Server;
using DarkRift.Server.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class PlayerCountAndLatencyRequest : MonoBehaviour
{
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;

    private void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;

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
            if (Message.IsPingMessage)
            {
                using (Message PingReply = Message.CreateEmpty(PacketTags.Heartbeat))
                {
                    PingReply.MakePingAcknowledgementMessage(Message);
                    e.Client.SendMessage(PingReply, SendMode.Reliable);
                }
            }
            if (Message.Tag == PacketTags.PlayerCount)
            {
                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                {
                    Writer.Write((short)PlayerManager.IClientPlayers.Count);

                    using (Message PlayerCount = Message.Create(PacketTags.PlayerCount, Writer))
                    {
                        e.Client.SendMessage(PlayerCount, SendMode.Reliable);
                    }
                }
            }
        }
    }
}
