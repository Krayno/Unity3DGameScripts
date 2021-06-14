using DarkRift.Server;
using DarkRift.Server.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Text;

public class AuthenticateWorldServer : MonoBehaviour
{
    private XmlUnityServer XmlServer;
    private DarkRiftServer Server;

    public TextAsset[] PublicKeys;
    public static Dictionary<IClient, AuthenticatedWorldServer> AuthenticatedWorldServers;

    public class AuthenticatedWorldServer
    {
        public string WorldNumber;
        public Aes WorldAES { get; set; }
        public string SessionID { get; set; }
        public byte[] EncryptedSessionID { get; set; }
        public IClient Client { get; set; }

        public AuthenticatedWorldServer(string worldNumber, Aes worldAES, string sessionID, byte[] encryptedSessionID, IClient client)
        {
            WorldNumber = worldNumber;
            WorldAES = worldAES;
            SessionID = sessionID;
            EncryptedSessionID = encryptedSessionID;
            Client = client;
        }
    }

    void Start()
    {
        XmlServer = GetComponentInParent<XmlUnityServer>();
        Server = XmlServer.Server;

        Server.ClientManager.ClientConnected += OnClientConnected;
        Server.ClientManager.ClientDisconnected += OnClientDisconnected;

        //Initialise AuthenticatedWorldServers Dictionary.
        AuthenticatedWorldServers = new Dictionary<IClient, AuthenticatedWorldServer>();
    }

    private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        if (AuthenticatedWorldServers.ContainsKey(e.Client))
        {
            //Set clients that were connected to the world, not to be in a world.
            foreach (Player Player in PlayerManager.SteamPlayers.Values)
            {
                if (Player.ConnectedToWorld == e.Client)
                {
                    Player.ConnectedToWorld = null;
                }
            }

            Debug.Log($"World {AuthenticatedWorldServers[e.Client].WorldNumber} has gone offline.");

            AuthenticatedWorldServers.Remove(e.Client);
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
            if (Message.Tag == PacketTags.LoginServerAuthentication)
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
                        e.Client.Strike($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} attempted to authenticate themselves as a World Server but their packet size was incorrect. Packet Size: {Reader.Length}");
                        return;
                    }

                    foreach (TextAsset Key in PublicKeys)
                    {
                        if (VerifySignedHash(RandomMessage, SignedRandomMessage, Key.ToString()))
                        {
                            string WorldServer = Key.name.Replace("World", "").Replace("ServerPublicKey", "");
                            Debug.Log($"Authenticated Server: World {WorldServer}.");

                            //Add the WorldServer Connection, AES Key and the SessionID.
                            Aes MyAes = Aes.Create();
                            
                            //Generate a SessionID for the WorldServer and Encrypt it with AES.
                            string SessionID = Path.GetRandomFileName();
                            byte[] EncryptedSessionID = AESEncrypt(SessionID, MyAes.Key, MyAes.IV);

                            AuthenticatedWorldServers.Add(e.Client, new AuthenticatedWorldServer(WorldServer, MyAes, SessionID, EncryptedSessionID, e.Client));

                            //Encrypt the AES Key and the AES IV;
                            byte[] AESKey = RSAEncrypt(Key.ToString(), AuthenticatedWorldServers[e.Client].WorldAES.Key);
                            byte[] AESIV = RSAEncrypt(Key.ToString(), AuthenticatedWorldServers[e.Client].WorldAES.IV);

                            //Send the AES Key to the World Server and encrypt the SessionID with RSA.
                            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                            {
                                Writer.Write(true);
                                Writer.Write(RSAEncrypt(Key.ToString(), Encoding.UTF8.GetBytes(AuthenticatedWorldServers[e.Client].SessionID)));
                                Writer.Write(AESKey);
                                Writer.Write(AESIV);

                                using (Message AuthenticationResult = Message.Create(PacketTags.LoginServerAuthentication, Writer))
                                {
                                    e.Client.SendMessage(AuthenticationResult, SendMode.Reliable);
                                }
                            }

                            return;
                        }
                    }
                    using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                    {
                        Writer.Write(false);

                        using (Message AuthenticationResult = Message.Create(PacketTags.LoginServerAuthentication, Writer))
                        {
                            e.Client.SendMessage(AuthenticationResult, SendMode.Reliable);
                        }

                        Debug.Log($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} attempted to authenticate themselves with the LoginServer.");
                        e.Client.Disconnect();
                    }
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
