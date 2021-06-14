using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;

public class PositionUpdate : MonoBehaviour
{
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;
    private List<GameUpdate> GameUpdates;

    public AccurateClock AccurateClock; //Event Based Timer.

    private class GameUpdate
    {
        public IClient Client { get; set; }
        public Vector3 Position { get; set; }
        public float RotationY { get; set; }
        public long Appearance { get; set; }
        public string Name { get; set; }

        public GameUpdate(IClient client, Vector3 position, float rotationY, string name)
        {
            Client = client;
            Position = position;
            RotationY = rotationY;
            Appearance = PlayerManager.IClientPlayers[client].Appearance;
            Name = name;
        }
    }

    void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;
        GameUpdates = new List<GameUpdate>();

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
            if (Message.Tag == PacketTags.PlayerPosition)
            {
                PlayerPosition(e);
            }
        }
    }

    private void PlayerPosition(MessageReceivedEventArgs e)
    {
        using (Message PlayerPosition = e.GetMessage())
        {
            using (DarkRiftReader Reader = PlayerPosition.GetReader())
            {
                PlayerPosition Player;

                try
                {
                    Player = PlayerPosition.Deserialize<PlayerPosition>();
                }
                catch (EndOfStreamException)
                {
                    e.Client.Strike($"Received malformed 'PlayerPosition' packet from {PlayerManager.IClientPlayers[e.Client].SteamID}. Packet Size: {Reader.Length}");
                    return;
                }

                //Debug.Log(Vector3.Distance(new Vector3(PosX, PosY, PosZ), PlayerManager.Players[e.Client].Position)); //Remove

                //Check if the position is within a collider. (Attempts to prevent noclipping).
                if (Physics.CheckSphere(new Vector3(Player.PositionX, Player.PositionY - 0.681f, Player.PositionZ), 0.4f))
                {
                    e.Client.Strike($"SteamID {PlayerManager.IClientPlayers[e.Client].SteamID} striked for their last position update being within a collider.");
                    return;
                }

                //Check the distance between last two updates. (Attempts to prevent speed and teleportation hacks).
                if (Vector3.Distance(PlayerManager.IClientPlayers[e.Client].Position, new Vector3(Player.PositionX, Player.PositionY, Player.PositionZ)) > 3.25f)
                {
                    e.Client.Strike($"SteamID {PlayerManager.IClientPlayers[e.Client].SteamID} striked for their difference between the last two position updates.");
                    return;
                }

                //Checks if the player is currently grounded. (Attempts to prevent flying).
                if (Physics.Raycast(new Vector3(Player.PositionX, Player.PositionY, Player.PositionZ), -Vector3.up, 2f))
                {
                    Debug.DrawRay(new Vector3(Player.PositionX, Player.PositionY, Player.PositionZ), -Vector3.up * 2f, Color.green, 10f); //Remove

                    PlayerManager.IClientPlayers[e.Client].LastGroundedPosition = new Vector3(Player.PositionX, Player.PositionY, Player.PositionZ);
                    PlayerManager.IClientPlayers[e.Client].RisingUpdates = 0;
                    PlayerManager.IClientPlayers[e.Client].FallingUpdates = 0;

                }
                else
                {
                    Debug.DrawRay(new Vector3(Player.PositionX, Player.PositionY, Player.PositionZ), -Vector3.up * 2f, Color.red, 10f); //Remove

                    if (Player.PositionY - PlayerManager.IClientPlayers[e.Client].Position.y > 0) //Player is rising
                    {
                        PlayerManager.IClientPlayers[e.Client].RisingUpdates += 1;
                        if (PlayerManager.IClientPlayers[e.Client].RisingUpdates >= 20)
                        {
                            //Triggered after a player rises for 2 seconds.
                            e.Client.Strike($"SteamID {PlayerManager.IClientPlayers[e.Client].SteamID} striked for rising for more than 2 seconds.");
                            return;
                        }
                    }
                    else //Player is falling
                    {
                        //If the distance between the players current position and the last grounded position is less than 2, ignore the player "falling".
                        //This will allow players to hover 2 above or below where they were last standing. If their head will slightly poke out.
                        if (Vector3.Distance(PlayerManager.IClientPlayers[e.Client].LastGroundedPosition, PlayerManager.IClientPlayers[e.Client].Position) < 2)
                        {
                        }
                        else if (PlayerManager.IClientPlayers[e.Client].FallingUpdates >= 100)
                        {
                            //Strike player after 10 seconds of falling.
                            e.Client.Strike($"SteamID {PlayerManager.IClientPlayers[e.Client].SteamID} striked for falling for longer than 10 seconds.");
                            return;
                        }
                        else
                        {
                            PlayerManager.IClientPlayers[e.Client].FallingUpdates += 1;
                        }
                    }
                }

                PlayerManager.IClientPlayers[e.Client].Position = new Vector3(Player.PositionX, Player.PositionY, Player.PositionZ); //Update Player Position.
                PlayerManager.IClientPlayers[e.Client].RotationY = Player.RotationY; //Update Player Rotation.

                GameUpdates.Add(new GameUpdate(e.Client, new Vector3(Player.PositionX, Player.PositionY, Player.PositionZ), Player.RotationY, PlayerManager.IClientPlayers[e.Client].Name)); //Add Game Update.
            }
        }
    }


    private void EveryTick(object sender, EventTimerTickedArgs e)
    {
        if (GameUpdates.Count > 0)
        {
            foreach (GameUpdate GameUpdate in GameUpdates)
            {
                foreach (Player Player in PlayerManager.IClientPlayers.Values.Where(x => x.Client != GameUpdate.Client))
                {
                    if (Server.ClientManager.GetAllClients().Contains(Player.Client) && PlayerManager.IClientPlayers.ContainsKey(GameUpdate.Client))
                    {
                        //Calculate the distance between the client and the update position client to determine whether to send a message.
                        if (Vector3.Distance(Player.Position, PlayerManager.IClientPlayers[GameUpdate.Client].Position)
                            <= ServerGlobals.RenderDistance)
                        {
                            if (Player.NearbyPlayers.ContainsKey(GameUpdate.Client))
                            {
                                if (Player.NearbyPlayers[GameUpdate.Client].Position != GameUpdate.Position || Player.NearbyPlayers[GameUpdate.Client].RotationY != GameUpdate.RotationY)
                                {
                                    PlayerWithinRenderDistance PlayerData = new PlayerWithinRenderDistance(GameUpdate.Client.ID, GameUpdate.Position.x, GameUpdate.Position.y, GameUpdate.Position.z, GameUpdate.RotationY);
                                    Player.Client.SendMessage(Message.Create(PacketTags.PlayerPosition, PlayerData), SendMode.Reliable);
                                    Player.NearbyPlayers[GameUpdate.Client].Position = GameUpdate.Position;
                                    Player.NearbyPlayers[GameUpdate.Client].RotationY = GameUpdate.RotationY;
                                }
                            }
                            else
                            {
                                PlayerEnteredRenderDistance PlayerData = new PlayerEnteredRenderDistance(GameUpdate.Client.ID, GameUpdate.Position.x, GameUpdate.Position.y, GameUpdate.Position.z, GameUpdate.RotationY, GameUpdate.Appearance, GameUpdate.Name);
                                Player.Client.SendMessage(Message.Create(PacketTags.PlayerEnteredRenderDistance, PlayerData), SendMode.Reliable);
                                Player.NearbyPlayers.Add(GameUpdate.Client, new PlayerCurrent(GameUpdate.Position, GameUpdate.RotationY));

                                PlayerData = new PlayerEnteredRenderDistance(Player.Client.ID, Player.Position.x, Player.Position.y, Player.Position.z, Player.RotationY, Player.Appearance, Player.Name);
                                GameUpdate.Client.SendMessage(Message.Create(PacketTags.PlayerEnteredRenderDistance, PlayerData), SendMode.Reliable);
                                PlayerManager.IClientPlayers[GameUpdate.Client].NearbyPlayers.Add(Player.Client, new PlayerCurrent(Player.Position, Player.RotationY));
                            }
                        }
                        //Check if the player was within the render distance last update. Tell player to destroy client.
                        else if (Player.NearbyPlayers.ContainsKey(GameUpdate.Client))
                        {
                            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                            {
                                Writer.Write(GameUpdate.Client.ID);

                                using (Message DespawnPlayer = Message.Create(PacketTags.DespawnPlayer, Writer))
                                {
                                    Player.Client.SendMessage(DespawnPlayer, SendMode.Reliable);
                                    Player.NearbyPlayers.Remove(GameUpdate.Client);
                                }
                            }

                            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                            {
                                Writer.Write(Player.Client.ID);

                                using (Message DespawnPlayer = Message.Create(PacketTags.DespawnPlayer, Writer))
                                {
                                    GameUpdate.Client.SendMessage(DespawnPlayer, SendMode.Reliable);
                                    PlayerManager.IClientPlayers[GameUpdate.Client].NearbyPlayers.Remove(Player.Client);
                                }
                            }
                        }
                    }
                }

            }
            GameUpdates.Clear();
        }
    }
}
