using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Net.Sockets;

public class LoginServerConnecter : MonoBehaviour
{
    public static UnityClient LoginServer;
    public TextAsset PrivateKey;
    public TextAsset PublicKey;

    private byte[] RandomMessage;
    private byte[] SignedRandomMessage;

    public static Aes Aes;
    public static string SessionID;
    public static byte[] EncryptedSessionID;

    void Awake()
    {
        //Create the AES instance.
        Aes = Aes.Create();

        //Generate a random message that is converted into a byte array.
        RandomMessage = Encoding.UTF8.GetBytes($"{Path.GetRandomFileName()}");

        //Hash and Sign the Random Message with the PrivateKey.
        SignedRandomMessage = HashAndSignBytes(RandomMessage, PrivateKey.ToString());

        //Assing the client and subscribe.
        LoginServer = transform.GetComponent<UnityClient>();
        LoginServer.MessageReceived += OnMessageReceived;
        LoginServer.Disconnected += OnDisconnection;
    }

    private void OnDisconnection(object sender, DisconnectedEventArgs e)
    {
        //Quit the World if we lose connection to the login server.
        Application.Quit();
    }

    private void Start()
    {
        //Connect to the server.
        LoginServer.ConnectInBackground(LoginServer.Host, LoginServer.Port, true, ConnectionCallback);
    }

    private void ConnectionCallback(Exception e)
    {
        if (LoginServer.ConnectionState == ConnectionState.Connected)
        {
            //Send the RandomMessage and the SignedRandomMessage to the LoginServer.
            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(RandomMessage);
                Writer.Write(SignedRandomMessage);

                using (Message Message = Message.Create(PacketTags.LoginServerAuthentication, Writer))
                {
                    LoginServer.SendMessage(Message, SendMode.Reliable);
                }
            }
        }
        else
        {
            Debug.Log("Failed to connect to LoginServer. Restart the World Server to try again.");

            //DarkRift Bug Github Issue #81 - DarkRiftClient holds on Connecting after failed connection attempt
            try
            {
                LoginServer.Disconnect();
            }
            catch (SocketException) { };
        }
    
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message Message = e.GetMessage())
        {
            if (Message.Tag == PacketTags.LoginServerAuthentication)
            {
                using (DarkRiftReader Reader = Message.GetReader())
                {
                    bool Authenticated;
                    byte[] sessionID;
                    byte[] AESKey;
                    byte[] AESIV;

                    try
                    {
                        Authenticated = Reader.ReadBoolean();
                        if (!Authenticated)
                        {
                            Debug.Log($"Result of Authentication with LoginServer: {Authenticated}");
                            return;
                        }
                        sessionID = Reader.ReadBytes();
                        AESKey = Reader.ReadBytes();
                        AESIV = Reader.ReadBytes();
                    }
                    catch (EndOfStreamException)
                    {
                        Debug.Log($"Login Server sent an invalid packet. Packet Size: {Reader.Length}");
                        return;
                    }

                    //Decrypt the SessionID and set the public variable.
                    SessionID = Encoding.UTF8.GetString(RSADecrypt(PrivateKey.ToString(), sessionID));

                    //Decrypt the AESkey and AESiv
                    AESKey = RSADecrypt(PrivateKey.ToString(), AESKey);
                    AESIV = RSADecrypt(PrivateKey.ToString(), AESIV);

                    //Set the unencrypted AESKey and AESIV to the AES instance associated with the login server.
                    Aes.Key = AESKey;
                    Aes.IV = AESIV;

                    //Set the encrypted version of the session id to save computation for other methods.
                    EncryptedSessionID = AESEncrypt(SessionID, Aes.Key, Aes.IV);

                    Debug.Log($"Result of Authentication with LoginServer: {Authenticated}");
                }
            }
        }
    }

    private byte[] HashAndSignBytes(byte[] DataToSign, string PrivateKey)
    {
        // Create a new instance of RSACryptoServiceProvider using the
        // PrivateKeyPath.
        using (RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider())
        {
            RSAalg.PersistKeyInCsp = false;
            RSAalg.FromXmlString(PrivateKey);

            // Hash and sign the data. Pass a new instance of SHA256
            // to specify the hashing algorithm.
            return RSAalg.SignData(DataToSign, SHA256.Create());
        }

    }

    private byte[] RSADecrypt(string privateKey, byte[] encrypted)
    {
        byte[] decrypted;
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(privateKey);
            decrypted = rsa.Decrypt(encrypted, true);
        }

        return decrypted;
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
