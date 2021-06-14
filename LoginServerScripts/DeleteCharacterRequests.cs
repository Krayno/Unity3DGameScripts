using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DeleteCharacterRequests : MonoBehaviour
{
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;
    private List<DeleteCharacterRequest> Requests;

    public AccurateClock AccurateClock; //Event Based Timer.
    public MongoDBConnecter MongoDBConnecter;

    string NamesPath;

    public class DeleteCharacterRequest
    {
        public IClient Client { get; set; }
        public bool DeletedCharacter { get; set; }

        public DeleteCharacterRequest(IClient client, bool deletedCharacter)
        {
            Client = client;
            DeletedCharacter = deletedCharacter;
        }
    }

    void Start()
    {
        NamesPath = Application.persistentDataPath + "/Names.txt";

        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;
        Requests = new List<DeleteCharacterRequest>();

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
            if (Message.Tag == PacketTags.RequestDeleteCharacter)
            {
                ClientDeleteCharacterRequest(e);
            }
        }
    }

    private void ClientDeleteCharacterRequest(MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            using (DarkRiftReader Reader = Message.GetReader())
            {
                ulong SteamID;
                string Name;

                try
                {
                    SteamID = Reader.ReadUInt64();
                    Name = Reader.ReadString();
                }
                catch (EndOfStreamException)
                {
                    e.Client.Strike($"Received malformed 'RequestDeleteCharacter' packet from {e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port}. Packet Size: {Reader.Length}");
                    return;
                }
               
                //If the client sent more data than necccesary.
                if (Reader.Length != Reader.Position)
                {
                    e.Client.Strike($"Received malformed 'RequestDeleteCharacter' packet from {e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port}. Packet Size: {Reader.Length}");
                    return;
                }

                //Check if the name is present.
                if (MongoDBConnecter.StoredNames.Contains(Name))
                {
                    //Check if the player is authenticated.
                    if (PlayerManager.SteamPlayers.ContainsKey(SteamID))
                    {
                        //Delete name in the Database and List.
                        MongoDBConnecter.StoredNames.Remove(Name);

                        var NamesFilter = Builders<BsonDocument>.Filter.Exists("Names");
                        BsonDocument Document = MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("GroupedData").Find(NamesFilter).FirstOrDefault();
                        BsonArray BsonNames = Document.GetValue("Names").AsBsonArray;
                        if (!BsonNames.Remove(BsonValue.Create(Name).AsString))
                        {
                            Requests.Add(new DeleteCharacterRequest(e.Client, false)); //Add the result of the request.
                            e.Client.Strike($"{SteamID} requested to delete a name that doesn't exist in the database.");
                            return;
                        }
                        Document.Set("Names", BsonNames);
                        MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("GroupedData").ReplaceOne(NamesFilter, Document);

                        //Delete character in the Database.
                        var SteamIDFilter = Builders<BsonDocument>.Filter.Eq("_id", (long)SteamID);
                        var document = MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("Accounts").Find(SteamIDFilter).FirstOrDefault();

                        bool DeletedCharacter = false;
                        BsonDocument BsonCharacters = document.GetValue("Characters").AsBsonDocument;
                        for (byte index = 0; index < 8; index++)
                        {
                            if (BsonCharacters.GetValue(index).AsBsonDocument.GetValue("Name") == BsonValue.Create(Name).AsString)
                            {
                                BsonCharacters.GetValue(index).AsBsonDocument.Set("Name", BsonNull.Value);
                                BsonCharacters.GetValue(index).AsBsonDocument.Set("TotalLevel", BsonNull.Value);
                                BsonCharacters.GetValue(index).AsBsonDocument.Set("ZoneID", BsonNull.Value);
                                BsonCharacters.GetValue(index).AsBsonDocument.Set("Appearance", BsonNull.Value);

                                document.Set("Characters", BsonCharacters);

                                MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("Accounts").ReplaceOne(SteamIDFilter, document);
                                DeletedCharacter = true;

                                Requests.Add(new DeleteCharacterRequest(e.Client, true)); //Add the result of the request.
                                break;
                            }
                        }

                        if (!DeletedCharacter)
                        {
                            Requests.Add(new DeleteCharacterRequest(e.Client, false)); //Add the result of the request.
                            e.Client.Strike($"{SteamID} requested to delete a character that doesn't exist in the database.");
                            return;
                        }

                        //Create a temporary text file then writes all the names except the delete request name. Then overwrite with temp.
                        string TempFile = Path.GetTempFileName();
                        using (StreamWriter sw = new StreamWriter(TempFile))
                        {
                            foreach (string line in File.ReadLines(NamesPath))
                            {
                                if (line != Name)
                                {
                                    sw.WriteLine(line);
                                }

                            }
                        }
                        File.Copy(TempFile, NamesPath, true);
                        File.Delete(TempFile);
                    }
                }
                else
                {
                    e.Client.Strike($"{SteamID} requested to delete a character that doesn't exist in the Names.txt file.");
                }
            }
        }
    }

    private void EveryTick(object sender, EventTimerTickedArgs e)
    {
        if (Requests.Count > 0)
        {
            foreach (DeleteCharacterRequest Request in Requests)
            {
                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                {
                    Writer.Write(Request.DeletedCharacter);

                    if (Server.ClientManager.GetAllClients().Contains(Request.Client)) //Ensure server sends it to someone who hasn't disconnected.
                    {
                        using (Message Message = Message.Create(PacketTags.RequestDeleteCharacter, Writer))
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
