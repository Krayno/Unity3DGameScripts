using DarkRift.Server;
using DarkRift.Server.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.IO;

public class WorldServerVerifyPlayer : MonoBehaviour
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
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.ClientToWorldServerAuthentication)
            {
                VerifySteamID(e);
            }
            if (Message.Tag == PacketTags.DespawnPlayer)
            {
                PlayerDisconnectedFromWorld(e);
            }
        }
    }

    private void PlayerDisconnectedFromWorld(MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
           if (AuthenticateWorldServer.AuthenticatedWorldServers.ContainsKey(e.Client))
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
                        e.Client.Strike($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} World Server attempted to despawn a player but their packet size was incorrect. Packet Size: {Reader.Length}");
                        return;
                    }

                    if (WorldServerEncryption.SessionID == AuthenticateWorldServer.AESDecrypt(EncryptedSessionID, WorldServerEncryption.WorldAES.Key, WorldServerEncryption.WorldAES.IV))
                    {
                        ulong SteamID = Reader.ReadUInt64();

                        if (PlayerManager.SteamPlayers.ContainsKey(SteamID))
                        {
                            PlayerManager.SteamPlayers[SteamID].ConnectedToWorld = null;
                            PlayerManager.SteamPlayers[SteamID].CurrentCharacterName = string.Empty;
                            PlayerManager.SteamPlayers[SteamID].CurrentCharacterGuild = string.Empty;
                            PlayerManager.SteamPlayers[SteamID].CurrentCharacterTotalLevel = -1;
                            return;
                        }
                        Debug.Log($"{e.Client} World Server requested us to set a player to not be in a world when said player doesn't exist or the player quit the game.");
                        return;
                    }
                    Debug.Log($"{e.Client} World Server sent an encrypted SessionID that didn't match with the stored SessionID for his world.");
                }
           }
           Debug.Log($"{e.Client} sent a message saying a player disconnected when he isn't authenticated as a world server.");
        }
    }

    private void VerifySteamID(MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (AuthenticateWorldServer.AuthenticatedWorldServers.ContainsKey(e.Client)) 
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
                        e.Client.Strike($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} World Server attempted to verify a player but their packet size was incorrect. Packet Size: {Reader.Length}");
                        return;
                    }

                    if (WorldServerEncryption.SessionID == AuthenticateWorldServer.AESDecrypt(EncryptedSessionID, WorldServerEncryption.WorldAES.Key, WorldServerEncryption.WorldAES.IV))
                    {
                        ulong SteamID = Reader.ReadUInt64();
                        byte CharacterSlot = Reader.ReadByte();

                        if (PlayerManager.SteamPlayers.ContainsKey(SteamID))
                        {
                            if (PlayerManager.SteamPlayers[SteamID].ConnectedToWorld == null)
                            {
                                //Set the player InAWorld to true.
                                PlayerManager.SteamPlayers[SteamID].ConnectedToWorld = e.Client;

                                //Check if the CharacterSlot is valid.
                                if (CharacterSlot > 8)
                                {
                                    PlayerManager.SteamPlayers[SteamID].Client.Strike($"World {WorldServerEncryption.WorldNumber} sent a character slot byte larger than 8 to the Login Server.");
                                    return;
                                }

                                //Fetch the requested Character Data from the SteamID.
                                var SteamIDFilter = Builders<BsonDocument>.Filter.Eq("_id", (long)SteamID);
                                var document = MongoDBConnecter.ProjectX.GetCollection<BsonDocument>("Accounts").Find(SteamIDFilter).FirstOrDefault();

                                BsonDocument CharacterSlotDoc = document.GetValue("Characters").AsBsonDocument.GetValue(CharacterSlot.ToString()).AsBsonDocument;
                                BsonDocument CharacterSlotPositionDoc = CharacterSlotDoc.GetValue("CurrentPosition").AsBsonDocument;

                                //Check if the given character slot has a character in it.
                                CharacterSlotDoc.TryGetValue("Appearance", out BsonValue value);
                                if (value == BsonNull.Value)
                                {
                                    Debug.Log($"{SteamID} tried entering a world when their account has no characters.");
                                    return;
                                }

                                long Appearance = value.AsInt64;
                                double PositionX = CharacterSlotPositionDoc.GetValue("PositionX").AsDouble;
                                double PositionY = CharacterSlotPositionDoc.GetValue("PositionY").AsDouble;
                                double PositionZ = CharacterSlotPositionDoc.GetValue("PositionZ").AsDouble;
                                double RotationY = CharacterSlotPositionDoc.GetValue("RotationY").AsDouble;
                                string Name = CharacterSlotDoc.GetValue("Name").AsString;
                                string Guild = CharacterSlotDoc.GetValue("Guild").AsString;
                                int TotalLevel = CharacterSlotDoc.GetValue("TotalLevel").AsInt32;

                                PlayerManager.SteamPlayers[SteamID].CurrentCharacterName = Name;
                                PlayerManager.SteamPlayers[SteamID].CurrentCharacterGuild = Guild;
                                PlayerManager.SteamPlayers[SteamID].CurrentCharacterTotalLevel = (short)TotalLevel;

                                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                                {
                                    Writer.Write(WorldServerEncryption.EncryptedSessionID);
                                    Writer.Write(new PlayerDataToWorldServer(SteamID, Appearance, PositionX, PositionY, PositionZ, RotationY, Name, TotalLevel));
                                    using (Message Authenticated = Message.Create(PacketTags.LoginServerVerifiedWorldServerRequest, Writer))
                                    {
                                        e.Client.SendMessage(Authenticated, SendMode.Reliable);
                                        return;
                                    }
                                }

                            }
                            //Failed verification.
                            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                            {
                                Writer.Write(WorldServerEncryption.EncryptedSessionID);
                                Writer.Write(SteamID);

                                using (Message Authenticated = Message.Create(PacketTags.LoginServerVerifiedWorldServerRequest, Writer))
                                {
                                    e.Client.SendMessage(Authenticated, SendMode.Reliable);
                                }

                            }
                            Debug.Log($"WorldServer requested {SteamID} verification when {SteamID} is already in a WorldServer.");
                            return;
                        }
                        //Failed verification.
                        using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                        {
                            Writer.Write(WorldServerEncryption.EncryptedSessionID);
                            Writer.Write(SteamID);

                            using (Message Authenticated = Message.Create(PacketTags.LoginServerVerifiedWorldServerRequest, Writer))
                            {
                                e.Client.SendMessage(Authenticated, SendMode.Reliable);
                            }

                        }
                        Debug.Log($"WorldServer requested {SteamID} verification when {SteamID} isn't connected to LoginServer.");
                        return;
                    }
                    Debug.Log($"Authenticated World Server {e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} sent an encrypted SessionID that didn't match with the stored SessionID for his world.");
                }
            }
            Debug.Log($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} tried to authenticate a player when it isn't authenticated as a world server.");
        }
    }
}
