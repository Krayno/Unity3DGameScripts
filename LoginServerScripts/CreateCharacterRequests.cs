using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CreateCharacterRequests : MonoBehaviour
{
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;
    private List<CreateCharacterRequest> Requests;

    public AccurateClock AccurateClock; //Event Based Timer.
    public MongoDBConnecter MongoDBConnecter;

    private char[] BannedCharacters;

    public class CreateCharacterRequest
    {
        public IClient Client { get; set; }
        public bool UsernameTaken { get; set; }

        public CreateCharacterRequest(IClient client, bool usernameTaken)
        {
            Client = client;
            UsernameTaken = usernameTaken;
        }
    }

    void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;
        Requests = new List<CreateCharacterRequest>();

        AccurateClock.Tick.Ticked += EveryTick;

        Server.ClientManager.ClientConnected += OnClientConnected;

        //Setup BannedCharacters.
        BannedCharacters = new char[] {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
                                       '!', '@', '#', '$', '%', '^', '&', '*', '(', ')',
                                       '-', '_', '+', '=', '\'', '\\', '|', '[', '{', ']', '}',
                                       ';', ':', '"', '<', ',', '>', '.', '/', '?', '`', '~'};
    }

    private void OnClientConnected(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.RequestCreateCharacter)
            {
                ClientCreateCharacterRequest(e);
            }
        }
    }

    private void ClientCreateCharacterRequest(MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            using (DarkRiftReader Reader = Message.GetReader())
            {
                ulong SteamID;
                long Appearance;
                string Name;

                try
                {
                    SteamID = Reader.ReadUInt64();
                    Appearance = Reader.ReadInt64();
                    Name = Reader.ReadString();
                }
                catch (EndOfStreamException)
                {
                    e.Client.Strike($"Received malformed 'RequestCreateCharacter' packet from {e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port}. Packet Size: {Reader.Length}");
                    return;
                }

                //If the client sent more data than necccesary.
                if (Reader.Length != Reader.Position)
                {
                    e.Client.Strike($"Received malformed 'RequestCreateCharacter' packet from {e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port}. Packet Size: {Reader.Length}");
                    return;
                }

                //Check if the player is authenticated.
                if (PlayerManager.SteamPlayers.ContainsKey(SteamID))
                {
                    //Check if the name is valid.
                    if (Name.Contains(" ")) //Does the name contain spaces?
                    {
                        Debug.Log($"{SteamID} sent a name with spaces.");
                        return;
                    }

                    if (Name == string.Empty) //Is the name empty?
                    {
                        Debug.Log($"{SteamID} sent a name that was empty.");
                        return;
                    }

                    foreach (char Character in Name) //Does the name contain any special characters or numbers?
                    {
                        foreach (char BannedCharacter in BannedCharacters)
                        {
                            if (Character == BannedCharacter)
                            {
                                Debug.Log($"{SteamID} sent a name with special characters or numbers.");
                                return;
                            }
                        }
                    }

                    //Check if the name is already taken.
                    foreach (string StoredName in MongoDBConnecter.StoredNames)
                    {
                        if (Name == StoredName)
                        {
                            Debug.Log($"{SteamID} sent a name that was already taken.");
                            Requests.Add(new CreateCharacterRequest(e.Client, true));
                            return;
                        }
                    }

                    //Check if the appearance ID is valid.
                    int Gender = int.Parse(Appearance.ToString().Substring(0, 1));
                    int SkinColour = int.Parse(Appearance.ToString().Substring(1, 2));
                    int UnderwearColour = int.Parse(Appearance.ToString().Substring(3, 2));
                    int ScleraColour = int.Parse(Appearance.ToString().Substring(5, 2));
                    int LeftEyeColour = int.Parse(Appearance.ToString().Substring(7, 2));
                    int RightEyeColour = int.Parse(Appearance.ToString().Substring(9, 2));
                    int BraColour = int.Parse(Appearance.ToString().Substring(11, 2));
                    int MouthColour = int.Parse(Appearance.ToString().Substring(13, 2));
                    int EyebrowColour = int.Parse(Appearance.ToString().Substring(15, 2));
                    int HairStyle = int.Parse(Appearance.ToString().Substring(17, 2));

                    if (Gender != 1 && Gender != 2)
                    {
                        Debug.Log($"{SteamID} sent an appearance ID with an invalid gender.");
                        return;
                    }
                    if (SkinColour < 18 || SkinColour > 23)
                    {
                        Debug.Log($"{SteamID} sent an appearance ID with an invalid skin colour.");
                        return;
                    }
                    if (UnderwearColour < 0 || UnderwearColour > 17)
                    {
                        Debug.Log($"{SteamID} sent an appearance ID with an invalid underwear colour.");
                        return;
                    }
                    if (ScleraColour != 13)
                    {
                        Debug.Log($"{SteamID} sent an appearance ID with an invalid sclera colour.");
                        return;
                    }
                    if (LeftEyeColour < 0 || LeftEyeColour > 17)
                    {
                        Debug.Log($"{SteamID} sent an appearance ID with an invalid left eye colour.");
                        return;
                    }
                    if (BraColour < 0 || BraColour > 23)
                    {
                        Debug.Log($"{SteamID} sent an appearance ID with an invalid bra colour.");
                        return;
                    }
                    if (MouthColour != 12)
                    {
                        Debug.Log($"{SteamID} sent an appearance ID with an invalid mouth colour.");
                        return;
                    }
                    if (EyebrowColour < 0 || EyebrowColour > 23)
                    {
                        Debug.Log($"{SteamID} sent an appearance ID with an invalid eyebrow colour.");
                        return;
                    }
                    if (HairStyle < 0 || HairStyle > 1)
                    {
                        Debug.Log($"{SteamID} sent an appearance ID with an invalid hairstyle colour.");
                        return;
                    }

                    //Name hasn't been taken so add the name to the List.
                    MongoDBConnecter.StoredNames.Add(Name);

                    //Name hasn't been taken.
                    Requests.Add(new CreateCharacterRequest(e.Client, false));

                    //Add name to GroupData database collection.
                    var NamesFilter = Builders<BsonDocument>.Filter.Exists("Names");
                    BsonDocument Document = MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("GroupedData").Find(NamesFilter).FirstOrDefault();
                    BsonArray BsonNames = Document.GetValue("Names").AsBsonArray;
                    BsonNames.Add(BsonValue.Create(Name).AsString);
                    Document.Set("Names", BsonNames);
                    MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("GroupedData").ReplaceOne(NamesFilter, Document);

                    //Add character to the database.
                    var SteamIDFilter = Builders<BsonDocument>.Filter.Eq("_id", (long)SteamID);
                    var document = MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("Accounts").Find(SteamIDFilter).FirstOrDefault();

                    BsonDocument BsonCharacters = document.GetValue("Characters").AsBsonDocument;
                    for (byte index = 0; index < 8; index++)
                    {
                        if (BsonCharacters.GetValue(index).AsBsonDocument.GetValue("Name") == BsonNull.Value)
                        {
                            BsonCharacters.GetValue(index).AsBsonDocument.Set("Name", Name);
                            BsonCharacters.GetValue(index).AsBsonDocument.Set("Guild", string.Empty);
                            BsonCharacters.GetValue(index).AsBsonDocument.Set("TotalLevel", 0);
                            BsonCharacters.GetValue(index).AsBsonDocument.Set("ZoneID", 0);
                            BsonCharacters.GetValue(index).AsBsonDocument.Set("Appearance", Appearance);

                            //Set new player position.
                            BsonDocument CurrentPosition = BsonCharacters.GetValue(index).AsBsonDocument.GetValue("CurrentPosition").AsBsonDocument;
                            CurrentPosition.Set("PositionX", BsonValue.Create(32d).AsDouble);
                            CurrentPosition.Set("PositionY", BsonValue.Create(1.58d).AsDouble);
                            CurrentPosition.Set("PositionZ", BsonValue.Create(32d).AsDouble);
                            CurrentPosition.Set("RotationY", BsonValue.Create(0d).AsDouble);

                            BsonCharacters.GetValue(index).AsBsonDocument.Set("CurrentPosition", CurrentPosition);
                            document.Set("Characters", BsonCharacters);

                            MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("Accounts").ReplaceOne(SteamIDFilter, document);

                            return;
                        }
                    }
                    Debug.Log($"{SteamID} tried making a new character when they already have 8.");
                }
                Debug.Log($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} tried creating a character when it wasn't authenticated with the SteamID:{SteamID}.");
            }
        }
    }

    private void EveryTick(object sender, EventTimerTickedArgs e)
    {
        if (Requests.Count > 0)
        {
            foreach (CreateCharacterRequest Request in Requests)
            {
                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                {
                    Writer.Write(Request.UsernameTaken);

                    if (Server.ClientManager.GetAllClients().Contains(Request.Client)) //Ensure server sends it to someone who hasn't disconnected.
                    {
                        using (Message Message = Message.Create(PacketTags.RequestCreateCharacter, Writer))
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
