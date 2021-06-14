using DarkRift.Server;
using DarkRift.Server.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Net;
using MongoDB.Bson;
using MongoDB.Driver;

public class ServerAdminVerifyAndCommands : MonoBehaviour
{
    public MongoDBConnecter MongoDBConnecter;

    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;
    public TextAsset PublicKey;
    private IClient ServerAdmin;

    public static bool WaitingToShutdown;
    public static IClient WorldThatIsShuttingDown;
    public static bool AllWorlddShutdown;

    public Aes AdminAes;
    public string SessionID;
    public byte[] EncryptedSessionID;

    void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;

        Server.ClientManager.ClientConnected += OnClientConnected;
        Server.ClientManager.ClientDisconnected += OnClientDisconnected;

        WaitingToShutdown = false;
        WorldThatIsShuttingDown = null;
        AllWorlddShutdown = false;

        AdminAes = Aes.Create();
    }

    private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        if (e.Client == ServerAdmin)
        {
            Debug.Log($"Server Admin from {ServerAdmin.GetRemoteEndPoint("TCP").Address}:{ServerAdmin.GetRemoteEndPoint("TCP").Port} has disconnected.");
        }
    }

    private void OnClientConnected(object sender, ClientConnectedEventArgs e)
    {
        e.Client.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == 65000)
            {
                using (DarkRiftReader Reader = Message.GetReader())
                {
                    byte[] RandomMessage;
                    byte[] SignedRandomMessage;

                    try
                    {
                        RandomMessage = Reader.ReadBytes();
                        SignedRandomMessage = Reader.ReadBytes();
                    }
                    catch (EndOfStreamException)
                    {
                        e.Client.Strike($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} attempted to authenticate themselves as a Server Admin but the their packet size was incorrect. Packet Size: {Reader.Length}");
                        return;
                    }

                    if (VerifySignedHash(RandomMessage, SignedRandomMessage, PublicKey.ToString()))
                    {
                        ServerAdmin = e.Client;

                        Debug.Log($"Authenticated Admin from IP: {e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port}");

                        //Add the WorldServer Connection, AES Key and the SessionID.
                        Aes MyAes = Aes.Create();
                        
                        //Generate a SessionID for the WorldServer and Encrypt it with AES.
                        SessionID = Path.GetRandomFileName();
                        EncryptedSessionID = AESEncrypt(SessionID, MyAes.Key, MyAes.IV);

                        AdminAes.Key = MyAes.Key;
                        AdminAes.IV = MyAes.IV;
                        
                        //Encrypt the AES Key,AES IV and the sessionID with RSA.
                        byte[] AESKey = RSAEncrypt(PublicKey.ToString(), AdminAes.Key);
                        byte[] AESIV = RSAEncrypt(PublicKey.ToString(), AdminAes.IV);
                        byte[] sessionID = RSAEncrypt(PublicKey.ToString(), Encoding.UTF8.GetBytes(SessionID));

                        using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                        {
                            Writer.Write(true);
                            Writer.Write(sessionID);
                            Writer.Write(AESKey);
                            Writer.Write(AESIV);

                            using (Message AuthenticationResult = Message.Create(65000, Writer))
                            {
                                e.Client.SendMessage(AuthenticationResult, SendMode.Reliable);
                            }
                        }
                    }
                    else
                    {
                        using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                        {
                            Writer.Write(false);

                            using (Message AuthenticationResult = Message.Create(65000, Writer))
                            {
                                e.Client.SendMessage(AuthenticationResult, SendMode.Reliable);
                            }
                        }
                    }
                }
            }
            if (Message.Tag == 65001)
            {
                if (e.Client == ServerAdmin)
                {
                    Shutdown(e);
                }
            }
        }
    }

    private void Shutdown(MessageReceivedEventArgs e)
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
                    e.Client.Strike($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} Someone attempted to shutdown a world but their packet size was incorrect. Packet Size: {Reader.Length}");
                    return;
                }

                if (SessionID == AESDecrypt(EncryptedSessionID, AdminAes.Key, AdminAes.IV))
                {
                    sbyte x = Reader.ReadSByte();

                    if (x == -2)
                    {
                        foreach (IClient WorldServer in AuthenticateWorldServer.AuthenticatedWorldServers.Keys)
                        {
                            using (Message Shutdown = Message.CreateEmpty(PacketTags.ShutdownServer))
                            {
                                WorldServer.SendMessage(Shutdown, SendMode.Reliable);
                            }
                        }
                        WaitingToShutdown = true;
                        Debug.Log("Shutting Down Login Server once all World Servers have sent data.");

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
                    if (AuthenticateWorldServer.AuthenticatedWorldServers.Count > 0)
                    {
                        if (x == -1)
                        {
                            foreach (IClient WorldServer in AuthenticateWorldServer.AuthenticatedWorldServers.Keys)
                            {
                                using (Message Shutdown = Message.CreateEmpty(PacketTags.ShutdownServer))
                                {
                                    WorldServer.SendMessage(Shutdown, SendMode.Reliable);
                                }
                            }
                            AllWorlddShutdown = true;
                            Debug.Log("Shutting Down All World Servers.");
                        }
                        else if (x == -2)
                        {
                            foreach (IClient WorldServer in AuthenticateWorldServer.AuthenticatedWorldServers.Keys)
                            {
                                using (Message Shutdown = Message.CreateEmpty(PacketTags.ShutdownServer))
                                {
                                    WorldServer.SendMessage(Shutdown, SendMode.Reliable);
                                }
                            }
                            WaitingToShutdown = true;
                            Debug.Log("Shutting Down Login Server once all World Servers have sent data.");

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
                        else
                        {
                            using (Message Shutdown = Message.CreateEmpty(PacketTags.ShutdownServer))
                            {
                                IClient[] IClientArray = new IClient[AuthenticateWorldServer.AuthenticatedWorldServers.Count];
                                AuthenticateWorldServer.AuthenticatedWorldServers.Keys.CopyTo(IClientArray, 0);

                                foreach (AuthenticateWorldServer.AuthenticatedWorldServer WorldServer in AuthenticateWorldServer.AuthenticatedWorldServers.Values)
                                {
                                    if (WorldServer.WorldNumber == x.ToString())
                                    {
                                        WorldServer.Client.SendMessage(Shutdown, SendMode.Reliable);
                                        WorldThatIsShuttingDown = e.Client;
                                        Debug.Log($"Shutting down World {x}.");
                                        break;
                                    }
                                }
                            }
                        }

                    }
                    Debug.Log($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} Someone tried to send a shutdown message when there are no worlds to shutdown.");
                }
                else
                {
                    Debug.Log($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} Someone tried to send a shutdown message without the correct session id.");
                }
            }
        }
    }

    public static bool VerifySignedHash(byte[] DataToVerify, byte[] SignedData, string PublicKey)
    {
        // Create a new instance of RSACryptoServiceProvider using the
        // key from RSAParameters.
        using (RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider())
        {
            RSAalg.PersistKeyInCsp = false;
            RSAalg.FromXmlString(PublicKey);

            // Verify the data using the signature.  Pass a new instance of SHA256
            // to specify the hashing algorithm.
            return RSAalg.VerifyData(DataToVerify, SHA256.Create(), SignedData);
        }
    }

    private byte[] RSAEncrypt(string publicKey, byte[] plain)
    {
        byte[] encrypted;
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(publicKey);
            encrypted = rsa.Encrypt(plain, true);
        }

        return encrypted;
    }

    public static byte[] AESEncrypt(string plainText, byte[] Key, byte[] IV)
    {
        byte[] encrypted;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    public static string AESDecrypt(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }
}
