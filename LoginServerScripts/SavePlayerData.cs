using DarkRift.Server;
using DarkRift.Server.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;

public class SavePlayerData : MonoBehaviour
{
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;
    public MongoDBConnecter MongoDBConnecter;

    void Start()
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
        if (AuthenticateWorldServer.AuthenticatedWorldServers.ContainsKey(e.Client))
        {
            using (Message Message = e.GetMessage())
            {
                if (Message.Tag == PacketTags.SavePlayerData)
                {
                    SaveDataToMongoDB(e);
                }
            }
        }
    }

    private void SaveDataToMongoDB(MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            AuthenticateWorldServer.AuthenticatedWorldServer WorldServerEncryption = AuthenticateWorldServer.AuthenticatedWorldServers[e.Client];
            using (DarkRiftReader Reader = Message.GetReader())
            {
                byte[] EncryptedSessionID;

                try
                {
                    EncryptedSessionID = Reader.ReadBytes();
                }
                catch (EndOfStreamException)
                {
                    e.Client.Strike($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} World Server attempted save data to the Database but their packet size was incorrect. Packet Size: {Reader.Length}");
                    return;
                }

                if (WorldServerEncryption.SessionID == AuthenticateWorldServer.AESDecrypt(EncryptedSessionID, WorldServerEncryption.WorldAES.Key, WorldServerEncryption.WorldAES.IV))
                {
                    while (Reader.Position < Reader.Length) //Loops through the data until it reaches the end.
                    {
                        SaveDataFromWorldServer PlayerData = Reader.ReadSerializable<SaveDataFromWorldServer>();

                        var SteamIDFilter = Builders<BsonDocument>.Filter.Eq("_id", (long)PlayerData.SteamID);
                        var document = MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("Accounts").Find(SteamIDFilter).FirstOrDefault();

                        if (document != null)
                        {
                            //Get the CharacterSlot Position document.
                            BsonDocument CharacterPosition = document.GetValue("Characters").AsBsonDocument.GetValue(PlayerData.CharacterSlot.ToString()).AsBsonDocument.GetValue("CurrentPosition").AsBsonDocument;

                            //Set the values in the document.
                            CharacterPosition.Set("PositionX", PlayerData.PositionX);
                            CharacterPosition.Set("PositionY", PlayerData.PositionY);
                            CharacterPosition.Set("PositionZ", PlayerData.PositionZ);
                            CharacterPosition.Set("RotationY", PlayerData.RotationY);

                            //Update the Character Slot Position, then update the Character Slot, then finally update the Characters.
                            BsonDocument CharacterSlotDoc = document.GetValue("Characters").AsBsonDocument.GetValue(PlayerData.CharacterSlot.ToString()).AsBsonDocument.Set("CurrentPosition", CharacterPosition);
                            BsonDocument CharactersDoc = document.GetValue("Characters").AsBsonDocument.Set(PlayerData.CharacterSlot.ToString(), CharacterSlotDoc);
                            document.Set("Characters", CharactersDoc);

                            MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("Accounts").ReplaceOne(SteamIDFilter, document);

                            if (ServerAdminVerifyAndCommands.WaitingToShutdown || ServerAdminVerifyAndCommands.AllWorlddShutdown)
                            {
                                //Set the player that was proccessed to not be in a World.
                                PlayerManager.SteamPlayers[PlayerData.SteamID].ConnectedToWorld = null;
                            }
                        }
                        else
                        {
                            Debug.Log("WorldServer sent an update about a player that doesn't exist.");
                        }
                    }

                    //Check if the Login Server has initiated a shutdown.
                    if (ServerAdminVerifyAndCommands.WaitingToShutdown)
                    {
                        //Update the World that sent data to true.
                        AuthenticateWorldServer.AuthenticatedWorldServers.Remove(e.Client);

                        //Check if all authenticated WorldServers have sent data.
                        if (AuthenticateWorldServer.AuthenticatedWorldServers.Count == 0)
                        {
                            Debug.Log("Sending final data to Database before shutting down login server.");

                            //Update the BsonNames Array.
                            var NamesFilter = Builders<BsonDocument>.Filter.Exists("Names");
                            BsonDocument Document = MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("GroupedData").Find(NamesFilter).FirstOrDefault();
                            Document.Set("Names", new BsonArray(MongoDBConnecter.StoredNames));
                            MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("GroupedData").ReplaceOne(NamesFilter, Document);

                            Application.Quit();
                        }
                    }
                }
            }
        }
    }
}
