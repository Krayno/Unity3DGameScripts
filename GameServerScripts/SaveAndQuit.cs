using DarkRift.Client.Unity;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class SaveAndQuit : MonoBehaviour
{
    public UnityClient LoginServer;

    private void Start()
    {
        LoginServer.MessageReceived += OnLoginChatServerMessageReceived;
    }

    private void OnLoginChatServerMessageReceived(object sender, DarkRift.Client.MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.ShutdownServer)
            {
                SendAllDataThenShutdown();
            }
        }
    }

    private void SendAllDataThenShutdown()
    {
        Debug.Log("Sending Player Data to Login Server then shutting down.");
        using (DarkRiftWriter Writer = DarkRiftWriter.Create())
        {
            Writer.Write(LoginServerConnecter.EncryptedSessionID);

            foreach (Player Player in PlayerManager.IClientPlayers.Values)
            {
                Writer.Write(new SaveDataFromWorldServer(Player.SteamID, Player.CharacterSlot, Player.Appearance, Player.LastGroundedPosition.x, Player.LastGroundedPosition.y, Player.LastGroundedPosition.z, Player.RotationY, Player.Name, Player.TotalLevel));

                using (Message PlayerUpdate = Message.Create(PacketTags.SavePlayerData, Writer))
                {
                    LoginServer.SendMessage(PlayerUpdate, SendMode.Reliable);
                }
                //Disconnect the player.
                Player.Client.Disconnect();
            }
        }
        Debug.Log("Shutting Server Down.");
        Application.Quit();
    }
}
