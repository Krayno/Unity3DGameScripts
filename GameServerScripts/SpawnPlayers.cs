using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System;
using System.Linq;
using System.Collections.Generic;
using DarkRift.Client.Unity;
using System.IO;

public class SpawnPlayers : MonoBehaviour
{
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;
    private List<GameUpdate> GameUpdates;
    private Dictionary<ulong, AuthenticationRequest> AuthenticationRequests;

    public AccurateClock AccurateClock; //Event Based Timer.
    public UnityClient LoginServer;

    private class GameUpdate
    {
        public IClient Client { get; set; }
        public Vector3 Position { get; set; }
        public float RotationY { get; set; }
        public long Appearance { get; set; }
        public bool ToAllClients { get; set; }
        public string Name { get; set; }

        public GameUpdate(IClient client, Vector3 position, float rotationY, long appearance, bool toAllClients, string name)
        {
            Client = client;
            Position = position;
            RotationY = rotationY;
            Appearance = appearance;
            ToAllClients = toAllClients;
            Name = name;
        }
    }

    private class AuthenticationRequest
    {
        public IClient IClient { get; set; }
        public ulong SteamID { get; set; }
        public byte CharacterSlot { get; set; }

        public AuthenticationRequest(IClient iClient, ulong steamID, byte characterSlot)
        {
            IClient = iClient;
            SteamID = steamID;
            CharacterSlot = characterSlot;
        }
    }

    private void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;
        GameUpdates = new List<GameUpdate>();
        AuthenticationRequests = new Dictionary<ulong, AuthenticationRequest>();

        AccurateClock.Tick.Ticked += EveryTick;

        Server.ClientManager.ClientConnected += OnClientConnected;
        LoginServer.MessageReceived += OnLoginServerMessageReceived;
    }

    private void OnLoginServerMessageReceived(object sender, DarkRift.Client.MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.LoginServerVerifiedWorldServerRequest)
            {
                LoginServerVerificationResult(e);
            }
        }
    }

    private void OnClientConnected(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += OnMessageReceived;
        e.Client.StrikeOccured += OnClientStriked;
    }

    //All Strike Messages are handled here. Probably need to move it to it's own seperate file.
    private void OnClientStriked(object sender, StrikeEventArgs e)
    {
        Debug.Log(e.Message);
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.ClientToWorldServerAuthentication)
            {
                ClientRequestsLogin(e);
            }
            if (Message.Tag == PacketTags.SpawnPlayer)
            {
                SpawnPlayer(e);
            }
        }
    }

    private void LoginServerVerificationResult(DarkRift.Client.MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            using (DarkRiftReader Reader = Message.GetReader())
            {
                byte[] EncryptedSessionID;

                try
                {
                    EncryptedSessionID = Reader.ReadBytes();
                }
                catch (EndOfStreamException)
                {
                    Debug.Log($"Login Server attempted to send the verification result of a player that we asked for but their packet size was incorrect. Packet Size: {Reader.Length}");
                    return;
                }
                if (LoginServerConnecter.SessionID == LoginServerConnecter.AESDecrypt(EncryptedSessionID, LoginServerConnecter.Aes.Key, LoginServerConnecter.Aes.IV))
                {
                    AuthenticationRequest Request;

                    //If verification failed, there should only be 8 bytes left in the packet.
                    if (Reader.Length - Reader.Position == 8)
                    {
                        ulong SteamID = Reader.ReadUInt64();

                        Request = AuthenticationRequests[SteamID];

                        using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                        {
                            Writer.Write(false);

                            using (Message FailedVerification = Message.Create(PacketTags.ClientToWorldServerAuthentication, Writer))
                            {
                                Request.IClient.SendMessage(FailedVerification, SendMode.Reliable);
                                AuthenticationRequests.Remove(SteamID);
                                return;
                            }
                        }
                    }

                    PlayerDataToWorldServer PlayerData = Reader.ReadSerializable<PlayerDataToWorldServer>();
                    Request = AuthenticationRequests[PlayerData.SteamID];

                    if (AuthenticationRequests.ContainsKey(Request.SteamID))
                    {
                        using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                        {
                            Writer.Write(true);

                            using (Message Verified = Message.Create(PacketTags.ClientToWorldServerAuthentication, Writer))
                            {
                                Request.IClient.SendMessage(Verified, SendMode.Reliable);

                                //Add to list of players.
                                PlayerManager.IClientPlayers.Add(Request.IClient, new Player(Request.IClient, PlayerData.SteamID,
                                                                                    Request.CharacterSlot,
                                                                                    new Vector3(PlayerData.PositionX, PlayerData.PositionY, PlayerData.PositionZ),
                                                                                    PlayerData.RotationY, PlayerData.Appearance, PlayerData.Name, PlayerData.TotalLevel));
                                AuthenticationRequests.Remove(PlayerData.SteamID);
                            }
                        }
                        return;
                    }
                    Debug.Log($"Authentication Request for {PlayerData.SteamID} is not present.");
                }
                Debug.Log($"Login Server sent an encrypted SessionID that didn't match with the stored SessionID for it.");
            }
        }
    }

    private void ClientRequestsLogin(MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            using (DarkRiftReader Reader = Message.GetReader())
            {
                ulong SteamID;
                byte CharacterSlot;

                //If the client already requested a world login authentication, ignore.
                if (AuthenticationRequests.Values.Any(x => x.IClient == e.Client))
                {
                    e.Client.Strike($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} tried to spam world login authentication requests.");
                    return;
                }

                try
                {
                    SteamID = Reader.ReadUInt64();
                    CharacterSlot = Reader.ReadByte();
                }
                catch (EndOfStreamException)
                {
                    e.Client.Strike($"Received malformed 'ClientToWorldServerAuthentication' packet from {e.Client.GetRemoteEndPoint("TCP").Address}:{ e.Client.GetRemoteEndPoint("TCP").Port}. Packet Size: {Reader.Length}");
                    return;
                }

                //Check if the CharacterSlot is valid.
                if (CharacterSlot > 8)
                {
                    PlayerManager.IClientPlayers[e.Client].Client.Strike($"{SteamID} sent a character slot byte larger than 8 to the World Server.");
                    return;
                }

                AuthenticationRequests.Add(SteamID, new AuthenticationRequest(e.Client, SteamID, CharacterSlot));

                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                {
                    Writer.Write(LoginServerConnecter.EncryptedSessionID);
                    Writer.Write(SteamID);
                    Writer.Write(CharacterSlot);

                    using (Message VerifySteamID = Message.Create(PacketTags.ClientToWorldServerAuthentication, Writer))
                    {
                        LoginServer.SendMessage(VerifySteamID, SendMode.Reliable);
                    }
                }
            }
        }
    }

    public void SpawnPlayer(MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (PlayerManager.IClientPlayers.ContainsKey(e.Client))
            {
                //Fetch player information.
                Vector3 CurrentPlayerPosition = PlayerManager.IClientPlayers[e.Client].Position;
                float CurrentYRotation = PlayerManager.IClientPlayers[e.Client].RotationY;
                long Appearance = PlayerManager.IClientPlayers[e.Client].Appearance;
                string Name = PlayerManager.IClientPlayers[e.Client].Name;

                GameUpdates.Add(new GameUpdate(e.Client, CurrentPlayerPosition, CurrentYRotation, Appearance, true, Name)); //To all clients.
                GameUpdates.Add(new GameUpdate(e.Client, CurrentPlayerPosition, CurrentYRotation, Appearance, false, Name)); //To new client.

                return;
            }
            Debug.Log("Player tried spawning without being verified.");
        }
    }

    private void EveryTick(object sender, EventTimerTickedArgs e) //Send SpawnPlayer updates every tick.
    {
        if (GameUpdates.Count > 0)
        {
            foreach (GameUpdate GameUpdate in GameUpdates)
            {
                if (GameUpdate.ToAllClients) //Send message to all clients or send message to new player.
                {
                    PlayerEnteredRenderDistance PlayerData = new PlayerEnteredRenderDistance(GameUpdate.Client.ID, GameUpdate.Position.x, GameUpdate.Position.y, GameUpdate.Position.z, GameUpdate.RotationY, GameUpdate.Appearance, GameUpdate.Name);

                    using (Message SpawnPlayer = Message.Create(PacketTags.SpawnPlayer, PlayerData))
                    {
                        foreach (Player Player in PlayerManager.IClientPlayers.Values.Where(x => x.Client != GameUpdate.Client))
                        {
                            if (Server.ClientManager.GetAllClients().Contains(Player.Client) && PlayerManager.IClientPlayers.ContainsKey(Player.Client))
                            {
                                //Calculate the distance between the client and the new player and decide whether to spawn or not.
                                if (Vector3.Distance(Player.Position, GameUpdate.Position) <= ServerGlobals.RenderDistance)
                                {
                                    Player.Client.SendMessage(SpawnPlayer, SendMode.Reliable);
                                }
                            }
                        }
                    }

                }
                else
                {
                    foreach (Player Player in PlayerManager.IClientPlayers.Values)
                    {
                        if (Server.ClientManager.GetAllClients().Contains(Player.Client) && PlayerManager.IClientPlayers.ContainsKey(Player.Client))
                        {
                            //Calculate the distance between the clients and the new player and decide whether to spawn or not.
                            if (Vector3.Distance(Player.Position, GameUpdate.Position) <= ServerGlobals.RenderDistance)
                            {
                                PlayerEnteredRenderDistance PlayerData = new PlayerEnteredRenderDistance(Player.Client.ID, Player.Position.x, Player.Position.y, Player.Position.z, Player.RotationY, Player.Appearance, Player.Name);

                                using (Message SpawnPlayers = Message.Create(PacketTags.SpawnPlayer, PlayerData))
                                {
                                    GameUpdate.Client.SendMessage(SpawnPlayers, SendMode.Reliable);
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
