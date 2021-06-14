using System;
using System.Security.Cryptography;
using DarkRift;
using DarkRift.Client;
using DarkRift.Server;
using System.Net;
using System.Text;
using System.IO;

namespace ServerAdmin
{
    public class ServerAdmin : Plugin
    {
        public override bool ThreadSafe => false;
        public override Version Version => new Version(1, 0, 0);

        public override Command[] Commands => new Command[]
        {
            new Command("Shutdown", "Shutdown a server.", "Shutdown \"World\", \"ALL\" or \"WorldNumber\" OR Shutdown \"Login\".", ShutDownHandler),
        };

        private void ShutDownHandler(object sender, CommandEventArgs e)
        {
            string[] RawArguments = e.RawArguments;
            sbyte x = 0;

            if (RawArguments[0] == "World")
            {
                if (RawArguments[1] == "ALL")
                {
                    x = -1;
                }
                else
                {
                    x = sbyte.Parse(RawArguments[1]);
                }
            }
            else if (RawArguments[0] == "Login")
            {
                x = -2;
            }

            using (DarkRiftWriter Writer = DarkRiftWriter.Create())
            {
                Writer.Write(EncryptedSessionID);
                Writer.Write(x);

                using (Message Message = Message.Create(65001, Writer))
                {
                    Client.SendMessage(Message, SendMode.Reliable);
                }
            }
        }

        private string PrivateKeyFile;

        private byte[] RandomMessage;
        private byte[] SignedRandomMessage;

        private DarkRiftClient Client;

        private string SessionID;
        private byte[] EncryptedSessionID;
        private Aes Aes;

        public ServerAdmin(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            Client = new DarkRiftClient();
            Client.ConnectInBackground(IPAddress.Parse("139.180.181.62"), 4296, true, ConnectionCallback);

            Client.MessageReceived += OnMessageReceived;
            Client.Disconnected += OnDisconnection;

            PrivateKeyFile = "Plugins/Data/ServerAdminPrivateKey.xml";
            Aes = Aes.Create();

            //Generate a random message that is converted into a byte array.
            RandomMessage = Encoding.UTF8.GetBytes($"{Path.GetRandomFileName()}");

            //Hash and Sign the Random Message with the PrivateKey.
            SignedRandomMessage = HashAndSignBytes(RandomMessage, PrivateKeyFile);

            //Subscribe to ClientManager just to reject anyone trying to connect.
            ClientManager.ClientConnected += ClientConnected;
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.Disconnect();

            Console.WriteLine($"{e.Client.GetRemoteEndPoint("TCP").Address}:{e.Client.GetRemoteEndPoint("TCP").Port} tried connecting to the Server Admin Client.");
        }

        private void OnDisconnection(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine("Login Server went offline. Re-open the client when the Login Server is back online.");
        }

        private void ConnectionCallback(Exception e)
        {
            if (Client.ConnectionState == ConnectionState.Connected)
            {
                //Send the RandomMessage and the SignedRandomMessage to the LoginServer.
                using (DarkRiftWriter Writer = DarkRiftWriter.Create())
                {
                    Writer.Write(RandomMessage);
                    Writer.Write(SignedRandomMessage);

                    using (Message Message = Message.Create(65000, Writer))
                    {
                        Client.SendMessage(Message, SendMode.Reliable);
                    }
                }
            }
            else
            {
                Console.WriteLine("Failed to connect to Login Server. Restart the Server Admin Client to try again.");
            }
        }

        private void OnMessageReceived(object sender, DarkRift.Client.MessageReceivedEventArgs e)
        {
            using (Message Message = e.GetMessage())
            {
                if (Message.Tag == 65000)
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
                                Console.WriteLine($"Result of Authentication with LoginServer: {Authenticated}");
                                return;
                            }
                            sessionID = Reader.ReadBytes();
                            AESKey = Reader.ReadBytes();
                            AESIV = Reader.ReadBytes();
                        }
                        catch (EndOfStreamException)
                        {
                            Console.WriteLine($"Login Server sent an invalid packet. Packet Size: {Reader.Length}");
                            return;
                        }

                        //Decrypt the SessionID and set the public variable.
                        SessionID = Encoding.UTF8.GetString(RSADecrypt(PrivateKeyFile, sessionID));

                        //Decrypt the AESkey and AESiv
                        AESKey = RSADecrypt(PrivateKeyFile, AESKey);
                        AESIV = RSADecrypt(PrivateKeyFile, AESIV);

                        //Set the unencrypted AESKey and AESIV to the AES instance associated with the login server.
                        Aes.Key = AESKey;
                        Aes.IV = AESIV;

                        //Set the encrypted version of the session id to save computation for other methods.
                        EncryptedSessionID = AESEncrypt(SessionID, Aes.Key, Aes.IV);


                        Console.WriteLine($"Successfully Authenticated as ServerAdmin to Login Server.");
                    }
                }
            }
        }

        public static byte[] HashAndSignBytes(byte[] DataToSign, string PrivateKeyPath)
        {
            // Create a new instance of RSACryptoServiceProvider using the
            // PrivateKeyPath.
            using (RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider())
            {
                RSAalg.PersistKeyInCsp = false;
                string PrivateKey = File.ReadAllText(PrivateKeyPath);

                RSACryptoServiceProviderExtensions.RSACryptoServiceProviderExtensions.FromXmlString(RSAalg, PrivateKey);

                // Hash and sign the data. Pass a new instance of SHA256
                // to specify the hashing algorithm.
                return RSAalg.SignData(DataToSign, SHA256.Create());
            }

        }

        private byte[] RSADecrypt(string privateKeyFile, byte[] encrypted)
        {
            byte[] decrypted;
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                string privateKey = File.ReadAllText(privateKeyFile);
                RSACryptoServiceProviderExtensions.RSACryptoServiceProviderExtensions.FromXmlString(rsa, privateKey);
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
}
