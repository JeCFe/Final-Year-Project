using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace New_SSL_Server
{
    class ClientHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("ClientHandler.cs");
        Server server;
        public TcpClient client;
        public NetworkStream netStream;
        public SslStream sslStream;
        private StreamWriter writer;
        private StreamReader reader;
        private byte[] sessionSalt;
        Aes aesKey = Aes.Create();

        //All types of messages that can be recieved
        AccountData ClientUser = null;
        public ClientHandler(TcpClient clientSocket, Server s, byte[] sSalt)
        {
            server = s;
            client = clientSocket;
            sessionSalt = sSalt;

            (new Thread(new ThreadStart(SetupConn))).Start();
        }
        void SetupConn()
        {
            try
            {
                Console.WriteLine("New client");
                netStream = client.GetStream();
                sslStream = new SslStream(netStream, false);
                sslStream.AuthenticateAsServer(server.cert, false, SslProtocols.Tls, true);
                Console.WriteLine("Connection Authenticated");
                reader = new StreamReader(sslStream, Encoding.Unicode);
                writer = new StreamWriter(sslStream, Encoding.Unicode);
                Thread messageThread = new Thread(ReadMessage);
                messageThread.Start();
                InitaliseHandshake();

            }
            catch (Exception e){ log.Error(e.ToString()); }
        }
        public void SendBasicMessage(string message)
        {
            Message M = new Message();
            StandardMessage SM = new StandardMessage();
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            SM.message = message;
            string SMstring = Serializer.Serialize(SM);

            M.message = SMstring;
            M.id = "3";
            if (M == null)
            {
                Console.WriteLine("Error");
            }
            string messageToSend = Serializer.Serialize(M);
            SendMessage(messageToSend);
        }
        private void CommonCommunications(Message M)
        {
            StandardMessage SM = new StandardMessage();

            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            SM = Deserializer.Deserialize<StandardMessage>(M.message);
            string amendedMessage = ClientUser.getName() + ": " + SM.message;
            Server.broadcast(amendedMessage);

        }
        private void ReadMessage()
        {
            bool clientConnected = true;
            while (clientConnected)
            {
                try
                {
                    Message M = new Message();
                    string receivedMessage = reader.ReadLine();
                    string decryptedMessage = receivedMessage;
                    decryptedMessage = decryptMessage(receivedMessage);

                    JavaScriptSerializer Deserializer = new JavaScriptSerializer();
                    M = Deserializer.Deserialize<Message>(decryptedMessage);
                    switch (M.id)
                    {
                        case "0": //HandShake
                            HandshakeManager(M);
                            break;
                        case "1": //Login
                            LoginManager(M);
                            break;
                        case "2"://Registration
                            RegistrationManager(M);
                            break;
                        case "3": //Standard Message
                            CommonCommunications(M);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    log.Info("Client disconnected");
                    clientConnected = false; //Ends the infinte loop
                    Logout();
                    server.removeClientFromClientList(this); //Ensures removal of client from client list
                    client.Close(); //Closes client connection
                }
            }
        }
        private void InitaliseHandshake()
        {
            HandShakeMessage HSM = new HandShakeMessage();
            Message M = new Message();
            M.id = "0";

            string rsa = server.getPublicKey();
            HSM.stage = "1";
            HSM.RSAPublicKey = rsa;
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string HSMstring = Serializer.Serialize(HSM);
            M.message = HSMstring;
            string Mmessage = Serializer.Serialize(M);
            if (Mmessage == "")
            {
                Console.WriteLine("Error");
                log.Error(client.ToString() + " handshake issue");
            }
            writer.WriteLine(Mmessage);
            writer.Flush();

        }
        private void HandshakeManager(Message M)
        {
            HandShakeMessage HSM = new HandShakeMessage();
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            HSM = Deserializer.Deserialize<HandShakeMessage>(M.message);
            if (HSM.stage == "1")
            {
                aesKey.Key = server.decryptAESKey(HSM.EncryptedAESKey);
                aesKey.IV = server.decryptAESKey(HSM.EncryptedAESIV);
                aesKey.Padding = PaddingMode.PKCS7;
            }
        }
        private void RegistrationManager(Message M)
        {
            RegistrationInformation RINFO = new RegistrationInformation();
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            RINFO = Deserializer.Deserialize<RegistrationInformation>(M.message);
            switch (RINFO.stage)
            {
                case "1":
                    RegistraionStageOne(RINFO); //Checking to make sure email hasnt been taken
                    break;
                case "2":
                    RegistrationFinalStage(RINFO);
                    break;
                default:
                    break;
            }
        }
        private void RegistraionStageOne(RegistrationInformation RINFO) //Send client salt
        {
            Message M = new Message();
            JavaScriptSerializer Serializer = new JavaScriptSerializer();

            RINFO.stage = "1";
            RINFO.AccountSalt = server.GenerateAccountSalt();
            RINFO.confirmation = "true";

            string RINFOmessage = Serializer.Serialize(RINFO);
            M.id = "2";
            M.message = RINFOmessage;
            string message = Serializer.Serialize(M);
            SendMessage(message);
        }
        private void RegistrationFinalStage(RegistrationInformation RINFO)
        {
            AccountData ad = new AccountData(RINFO);
            if (server.AccountLookup(ad.getEmail()) == null) //If no match for email
            {
                RINFO.confirmation = "true";
                server.RegNewAccount(ad);
            }
            else
            {
                RINFO.confirmation = "false";
            }
            Message M = new Message();
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            RINFO.stage = "2";
            string RINFOmessage = Serializer.Serialize(RINFO);
            M.id = "2";
            M.message = RINFOmessage;
            string message = Serializer.Serialize(M);
            SendMessage(message);
        }
        private void LoginManager(Message M)
        {
            LoginInformation LINFO = new LoginInformation();
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            LINFO = Deserializer.Deserialize<LoginInformation>(M.message);
            switch (LINFO.stage)
            {
                case "1": //Recieved lookup email
                    LookupEmail(LINFO);
                    break;
                case "2":
                    HashComparision(LINFO);
                    break;
                case "5":
                    Logout();
                    break;
                default:
                    break;
            }
        }
        private void HashComparision(LoginInformation LINFO)
        {
            //determine whether the use has logged in first
            Message M = new Message();
            if (ClientUser != null) //Ensures clienthandler has a current user being queried 
            {
                //Should have authentication token from client in LINFO
                Authenticator auth = new Authenticator();
                string authenticationToken = auth.PreformPBKDF2Hash(ClientUser.getHash(), sessionSalt);
                if (authenticationToken == LINFO.passwordHash) //Compare login tokens
                {
                    if (server.LogUserIn(ClientUser)) //If no client already logged in under that name
                    {
                        LINFO.confirmation = "true"; //will tell client that they have succcessfully logged in
                        LINFO.name = ClientUser.getName();
                    }
                    else
                    {
                        LINFO.confirmation = "false"; //will tell client they havent logged in
                        ClientUser = null; //deletes old clientUser data
                    }
                }
                else
                {
                    LINFO.confirmation = "false"; //Incorrect password
                    ClientUser = null;
                }
                LINFO.stage = "3";

                JavaScriptSerializer Serializer = new JavaScriptSerializer();
                string LINFOmessage = Serializer.Serialize(LINFO);
                M.id = "1";
                M.message = LINFOmessage;
                string message = Serializer.Serialize(M);
                SendMessage(message);
            }

        }
        private void LookupEmail(LoginInformation LINFO)
        {
            Message M = new Message();
            ClientUser = server.AccountLookup(LINFO.Email); //returns null if no account found
            if (ClientUser != null) //Account found
            {

                LINFO.accountSalt = ClientUser.getSalt();
                LINFO.sessionSalt = Convert.ToBase64String(sessionSalt);
                LINFO.confirmation = "true";
            }
            else //Account not found
            {
                LINFO.confirmation = "False";

            }
            LINFO.stage = "2";
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string LINFOmessage = Serializer.Serialize(LINFO);
            M.id = "1";
            M.message = LINFOmessage;
            string message = Serializer.Serialize(M);
            SendMessage(message);
        }
        private void Logout()
        {
            if (ClientUser != null)
            {
                if (ClientUser.getLoggedIn() == true)
                {
                    server.LogClientOut(ClientUser);
                }
            }
        }
        public void SendMessage(string message)
        {
            writer.WriteLine(encryptMessage(message));
            writer.Flush();
        }
        public string encryptMessage(string message)
        {
            try
            {
                byte[] encryptedData;
                string cipherText = "";
                using (AesManaged aes = new AesManaged())
                {
                    ICryptoTransform encryptor = aes.CreateEncryptor(aesKey.Key, aesKey.IV);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs))
                            {
                                sw.Write(message);
                            }
                            encryptedData = ms.ToArray();
                            cipherText = Convert.ToBase64String(encryptedData);
                        }
                    }
                }
                return cipherText;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                throw;
            }
        }
        public string decryptMessage(string message)
        {
            try
            {
                byte[] encryptedData = Convert.FromBase64String(message);
                string plainText = "";
                using (AesManaged aes = new AesManaged())
                {
                    ICryptoTransform decryptor = aes.CreateDecryptor(aesKey.Key, aesKey.IV);
                    using (MemoryStream ms = new MemoryStream(encryptedData))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cs))
                            {
                                plainText = reader.ReadToEnd();
                            }
                        }
                    }
                }
                return plainText;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                return message;
            }
        }
    }
}
