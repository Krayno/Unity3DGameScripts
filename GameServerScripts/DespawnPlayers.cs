using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System.Linq;
using System.Collections.Generic;
using System;
using DarkRift.Client.Unity;

public class DespawnPlayers : MonoBehaviour
{
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;
    private List<GameUpdate> GameUpdates;

    public AccurateClock AccurateClock; //Event Based Timer.
    public UnityClient LoginServer;

    private class GameUpdate
    {
        public IClient Client { get; set; }

        public GameUpdate(IClient client)
        {
            Client = client;
        }
    }

    private void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;
        GameUpdates = new List<GameUpdate>();

        AccurateClock.Tick.Ticked += EveryTick;

        Server.ClientManager.ClientDisconnected += OnClientDisconnected;
    }

    private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        if (PlayerManager.IClientPlayers.ContainsKey(e.Client))
        {
            GameUpdates.Add(new GameUpdate(e.Client));
        }
    }

    private void EveryTick(object sender, EventTimerTickedArgs e)
    {
        if (GameUpdates.Count > 0)
        {
            foreach (GameUpdate GameUpdate in GameUpdates)
            {
                Player Player;
                if (PlayerManager.IClientPlayers.TryGetValue(GameUpdate.Client, out Player Value))
                {
                    Player = Value;
                }
                else
                {
                    continue;
                }

                using (DarkRiftWriter Writer = DarkRiftWriter.Create()) //Tells everyone to despawn the player.
                {
                    Writer.Write(GameUpdate.Client.ID);

                    using (Message PlayerDisconnected = Message.Create(PacketTags.DespawnPlayer, Writer))
                    {
                        foreach (IClient NearbyPlayer in Player.NearbyPlayers.Keys)
                        {
                           NearbyPlayer.SendMessage(PlayerDisconnected, SendMode.Reliable);
                        }
                    }
                }

                PlayerManager.IClientPlayers.Remove(GameUpdate.Client);

                //Request login server to save the player data.
                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                {
                    Writer.Write(LoginServerConnecter.EncryptedSessionID);
                    Writer.Write(new SaveDataFromWorldServer(Player.SteamID, Player.CharacterSlot, Player.Appearance, Player.LastGroundedPosition.x, Player.LastGroundedPosition.y, Player.LastGroundedPosition.z, Player.RotationY, Player.Name, Player.TotalLevel));

                    using (Message SaveData = Message.Create(PacketTags.SavePlayerData, Writer))
                    {
                        LoginServer.SendMessage(SaveData, SendMode.Reliable);
                    }

                }

                //Tell the login player to set the player to not be in a world.
                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                {
                    Writer.Write(LoginServerConnecter.EncryptedSessionID);
                    Writer.Write(Player.SteamID);

                    using (Message PlayerDisconnected = Message.Create(PacketTags.DespawnPlayer, Writer))
                    {
                        //Send the SteamID of the player to the loginserver so they know the player isn't in a world.
                        LoginServerConnecter.LoginServer.SendMessage(PlayerDisconnected, SendMode.Reliable);
                    }
                }
            }
            GameUpdates.Clear();
        }
    }
}
