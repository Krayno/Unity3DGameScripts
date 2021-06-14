using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using System.IO;

public class LoginRequests : MonoBehaviour
{
    private string ServerVersion;
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;
    private List<LoginRequest> Requests;

    public AccurateClock AccurateClock; //Event Based Timer.
    public MongoDBConnecter MongoDBConnecter;

    public class Character
    {
        public byte CharacterSlot { get; set; }
        public string Name { get; set; }
        public ushort TotalLevel { get; set; }
        public byte ZoneID { get; set; }
        public long Appearance { get; set; }

        public Character(byte characterSlot, string name, ushort totalLevel, byte zoneID, long appearance)
        {
            CharacterSlot = characterSlot;
            Name = name;
            TotalLevel = totalLevel;
            ZoneID = zoneID;
            Appearance = appearance;
        }
    }

    public class LoginRequest
    {
        public IClient Client { get; set; }
        public bool Authenticated { get; set; }
        public bool VersionMismatch { get; set; }
        public bool AlreadyLoggedIn { get; set; }
        public List<Character> Characters { get; set; }

        public LoginRequest(IClient client, bool authenticated, bool versionMismatch, bool alreadyLoggedIn, List<Character> characters)
        {
            Client = client;
            Authenticated = authenticated;
            VersionMismatch = versionMismatch;
            AlreadyLoggedIn = alreadyLoggedIn;
            Characters = characters;
        }

        public LoginRequest(IClient client, bool authenticated, bool versionMismatch, bool alreadyLoggedIn)
        {
            Client = client;
            Authenticated = authenticated;
            VersionMismatch = versionMismatch;
            AlreadyLoggedIn = alreadyLoggedIn;
            Characters = null;

        }
    }

    void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;
        Requests = new List<LoginRequest>();

        AccurateClock.Tick.Ticked += EveryTick;

        Server.ClientManager.ClientConnected += OnClientConnected;
        ServerVersion = "0.0.1";
    }

    private void OnClientConnected(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += OnMessageReceived;
        e.Client.StrikeOccured += OnClientStrike;
    }

    //All client strikes are handled here. Probably need to move it to it's own file.
    private void OnClientStrike(object sender, StrikeEventArgs e)
    {
        Debug.Log(e.Message);
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.RequestLogin)
            {
                ClientLoginRequest(e);
            }
        }
    }

    private void ClientLoginRequest(MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            using (DarkRiftReader Reader = Message.GetReader())
            {
                string ClientVersion;
                ulong SteamID;

                //If the client already requested a login, ignore.
                if (Requests.Exists(x => x.Client == e.Client)) 
                {
                    e.Client.Strike($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} tried to spam login requests.");
                    return;
                }

                try
                {
                    ClientVersion = Reader.ReadString();
                    SteamID = Reader.ReadUInt64();
                }
                catch (EndOfStreamException)
                {
                    e.Client.Strike($"Received malformed 'RequestLogin' packet from {e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port}. Packet Size: {Reader.Length}");
                    return;
                }

                //If the client sent more data than necccesary.
                if (Reader.Length != Reader.Position)
                {
                    e.Client.Strike($"Received malformed 'RequestLogin' packet from {e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port}. Packet Size: {Reader.Length}");
                    return;
                }

                //Check version information.
                if (ClientVersion != ServerVersion)
                {
                    Requests.Add(new LoginRequest(e.Client, false, true, false));
                    return;
                }

                //Authenticate using STEAM here. //

                //If SteamID is already logged in, refuse login.
                if (PlayerManager.SteamPlayers.ContainsKey(SteamID))
                {
                    Requests.Add(new LoginRequest(e.Client, false, false, true));
                    return;
                }

                //Add player to PlayerManager.
                PlayerManager.SteamPlayers.Add(SteamID, new Player(e.Client, SteamID, null));

                var SteamIDFilter = Builders<BsonDocument>.Filter.Eq("_id", (long)SteamID);
                var document = MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("Accounts").Find(SteamIDFilter).FirstOrDefault();

                //If the player doesn't have an account.
                if (document == null)
                {
                    MongoDBConnecter.CreateNewAccount((long)SteamID);

                    Requests.Add(new LoginRequest(e.Client, true, false, false));
                }
                else
                {
                    List<Character> Characters = new List<Character>();

                    //Cycle through the characters in the document to find if any characters exist.
                    BsonDocument BsonCharacters = document.GetValue("Characters").AsBsonDocument;
                    for (byte index = 0; index < 8; index++)
                    {
                        if (BsonCharacters.GetValue(index).AsBsonDocument.GetValue("Name") != BsonNull.Value)
                        {
                             Characters.Add(new Character(index,
                                                          BsonCharacters.GetValue(index).AsBsonDocument.GetValue("Name").AsString,
                                                          (ushort)BsonCharacters.GetValue(index).AsBsonDocument.GetValue("TotalLevel").AsInt32,
                                                          (byte)BsonCharacters.GetValue(index).AsBsonDocument.GetValue("ZoneID").AsInt32,
                                                          BsonCharacters.GetValue(index).AsBsonDocument.GetValue("Appearance").AsInt64));
                        }
                    }

                    Requests.Add(new LoginRequest(e.Client, true, false, false, Characters));
                }
            }
        }
    }

    private void EveryTick(object sender, EventTimerTickedArgs e)
    {
        if (Requests.Count > 0)
        {
            foreach (LoginRequest Request in Requests)
            {
                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                {
                    Writer.Write(Request.Authenticated);
                    Writer.Write(Request.VersionMismatch);
                    Writer.Write(Request.AlreadyLoggedIn);

                    if (Request.Characters != null)
                    {
                        foreach (Character Character in Request.Characters)
                        {
                            Writer.Write(Character.CharacterSlot);
                            Writer.Write(Character.Name);
                            Writer.Write(Character.TotalLevel);
                            Writer.Write(Character.ZoneID);
                            Writer.Write(Character.Appearance);
                        }
                    } 

                    if (Server.ClientManager.GetAllClients().Contains(Request.Client)) //Ensure server sends it to someone who hasn't disconnected.
                    {
                        using (Message Message = Message.Create(PacketTags.RequestLogin, Writer))
                        {
                            Request.Client.SendMessage(Message, SendMode.Reliable);
                        }
                    }
                }
            }
            Requests.Clear();
        }
    }
}
