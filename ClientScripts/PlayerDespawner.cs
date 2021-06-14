using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;
using System.Collections.Generic;
using System.IO;

public class PlayerDespawner : MonoBehaviour
{
    public PlayerManager PlayerManager;

    private UnityClient Client; //The client that connects to the server.
    private List<ushort> DespawnPlayers;

    void Awake()
    {
        DespawnPlayers = new List<ushort>();

        Client = transform.parent.GetComponent<UnityClient>();
        Client.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.DespawnPlayer)
            {
                DespawnPlayer(Message);
            }
        }
    }

    private void DespawnPlayer(Message Message) //Player disconnected from server.
    {
        using (DarkRiftReader Reader = Message.GetReader())
        {
            ushort ID;

            try
            {
                ID = Reader.ReadUInt16();
            }
            catch (EndOfStreamException)
            {
                Debug.Log($"{ClientGlobals.WorldServer.Address} sent an invalid 'DespawnPlayer' Packet.");
                return;
            }

            if (PlayerManager.Players.ContainsKey(ID))
            {
                Destroy(PlayerManager.Players[ID].GameObject);
                PlayerManager.Players.Remove(ID);
            }
        }
        
    }

    private void OnDisable()
    {
        //Disconnected, so reset everything.
        DespawnPlayers = new List<ushort>();
    }
}
