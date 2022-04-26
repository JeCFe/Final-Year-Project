using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace New_SSL_Server
{
    /*
     This class is responsible for manageing and hosting a unique client socket
     With this class managing AES encryption and decryption
     Message handling, logging in, registering, logging out, and the inital handshake
     This class is spawned from the server with this being storage in a clienthandler list in the server class
    */

    class ClientHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("ClientHandler.cs");
        readonly Server server;
        public TcpClient client;
        public NetworkStream netStream;
        public SslStream sslStream;
        private StreamWriter writer;
        private StreamReader reader;
        private readonly byte[] sessionSalt;
        readonly Aes aesKey = Aes.Create(); //Creates and AES key and IV


        //Class storage for an account once its logged in
        AccountData ClientUser = null;
        /*
         This constructor is used to set the clientSocket, server and the salt to be stored within the class
         The thread SetupConn is also started which is the main process for this class
        */
        public ClientHandler(TcpClient clientSocket, Server s, byte[] sSalt)
        {
            server = s;
            client = clientSocket;
            sessionSalt = sSalt;

            (new Thread(new ThreadStart(SetupConn))).Start();
        }

        /*
         This function is responsible for the initial connection for the client and operates on a seperate thread
         This function sets up the SSL connection for the client 
         Once the connection is authenticated a log is made and the stream reader and writers are configured 
         Finally the message thread is began, this is responsible for reading message sent from the client
        */
        void SetupConn()
        {
            try
            {
                log.Info("Client connection");
                netStream = client.GetStream();
                sslStream = new SslStream(netStream, false);
                sslStream.AuthenticateAsServer(server.cert, false, SslProtocols.Tls, true);
                log.Info("Client SSL authenticated");
                reader = new StreamReader(sslStream, Encoding.Unicode);
                writer = new StreamWriter(sslStream, Encoding.Unicode);
                Thread messageThread = new Thread(ReadMessage);
                messageThread.Start();
                InitaliseHandshake();
            }
            catch (Exception e) { log.Error(e.ToString()); }
        }

        /*
         This function is responsible for sending a basic message to a client
         M ID 3 refers to the ID of basic message 
        */
        public void SendBasicMessage(string message)
        {
            Message M = new Message();
            StandardMessage SM = new StandardMessage();
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            SM.message = message;
            string SMstring = Serializer.Serialize(SM);
            M.message = SMstring;
            M.id = "3";
            string messageToSend = Serializer.Serialize(M);
            SendMessage(messageToSend);
        }

        /*
         When a basic message is recieved from the client this function is used
         The message is appended with the clients name
         Server broadcast is used to dispatch the message to all connected clients
        */
        private void CommonCommunications(Message M)
        {
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            StandardMessage SM = Deserializer.Deserialize<StandardMessage>(M.message);
            string amendedMessage = ClientUser.GetName() + ": " + SM.message;
            Server.Broadcast(amendedMessage);
        }

        /*
         This functions operates on its own thread started in the setupConn function
         This functions listens for a message from the client
         Upon recipt  the message is decrytped using AES decryption
         After the message has been deserialised the Message ID is used to determine the function to use
        */
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
                    decryptedMessage = DecryptMessage(receivedMessage);

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
                catch (Exception e) //When the client disconnects without warning this is when exceptions are usually thrown
                {
                    log.Error(e.ToString());
                    clientConnected = false; //Ends the infinte loop
                    Disconnect();
                    client.Close();
                }
            }
        }

        #region Handshake
        /*
         This function is responsible for initalising the encryption handshake between server and client with this function being called in SetupConn
         This function will use the object HandShakeMessage and Message to send the servers RSA public key this the client
         This function does not use the SendMessage function to avoid needing to encrypt the message 
        */
        private void InitaliseHandshake()
        {
            HandShakeMessage HSM = new HandShakeMessage();
            Message M = new Message { id = "0" };//ID tells client this message concerns handshaking
            string rsa = server.GetPublicKey();
            HSM.stage = "1";
            HSM.RSAPublicKey = rsa;
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string HSMstring = Serializer.Serialize(HSM);
            M.message = HSMstring;
            string Mmessage = Serializer.Serialize(M);
            writer.WriteLine(Mmessage);
            writer.Flush();
        }

        /*
         This function handles the return message from the client
         Client side would have encrypted their AES key and IV using the servers RSA key
         Decrytping these using the Auth DecryptAESKey functions 
            Allows for this class to store the AES Key and IV of the client to correctly continue with secure messaging
        */
        private void HandshakeManager(Message M)
        {
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            HandShakeMessage HSM = Deserializer.Deserialize<HandShakeMessage>(M.message);
            if (HSM.stage == "1")
            {
                aesKey.Key = server.DecryptAESKey(HSM.EncryptedAESKey);
                aesKey.IV = server.DecryptAESKey(HSM.EncryptedAESIV);
                aesKey.Padding = PaddingMode.PKCS7;
            }
        }
        #endregion

        #region Registration
        /*
         This functions decides what function the relevent RINFO stage refers to
        */
        private void RegistrationManager(Message M)
        {
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            RegistrationInformation RINFO = Deserializer.Deserialize<RegistrationInformation>(M.message);
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

        /*
         At the first stage of registration the server will send the client a message containing a salt that the client can has their password with
         This is to ensure that the password is never send in plain text and the server can never know what the inital password was
        */
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

        /*
         This function is where the actual registration occurs  
         First the functions will ensure that there is no current account sharing the same email address as the one recieved        
            This is done by server.AccountLookup that eventually reaches Ainfo and uses ClientLookupRequest function
            If not other account has the same email address then this function will call server.RegNewAccount
                This functions tunnels through the Auth class to Ainfo class that uses class RegNewUser functions to add the new account to the database and list
         Whether the registration was successful or not this is send back to the client as a bool
        */
        private void RegistrationFinalStage(RegistrationInformation RINFO)
        {
            AccountData ad = new AccountData(RINFO);
            if (server.AccountLookup(ad.GetEmail()) == null) //If no match for email
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
        #endregion

        #region Login and logout

        //This function is used to decide what the LINFO stage code refers too and then calls that function
        private void LoginManager(Message M)
        {
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            LoginInformation LINFO = Deserializer.Deserialize<LoginInformation>(M.message);
            switch (LINFO.stage)
            {
                case "1": //Recieved lookup email
                    LookupEmail(LINFO);
                    break;
                case "2":
                    HashComparision(LINFO);
                    break;
                case "5":
                    Logout(true);
                    break;
                default:
                    break;
            }
        }

        /*
         This function is used to compare the password hash value from the client logging to the account on record
         A temperary auth object is used to run the hash on the passwords
         After hashing the account on record password with the session salt will give us an authorisation token
         If this token is the same as the hashed password recieved from the client then the client can log in
         Whether the client password matched is returned back to the client as a bool
        */
        private void HashComparision(LoginInformation LINFO)
        {
            //determine whether the use has logged in first
            Message M = new Message();
            if (ClientUser != null) //Ensures clienthandler has a current user being queried 
            {
                //Should have authentication token from client in LINFO
                Authenticator auth = new Authenticator();
                string authenticationToken = auth.PreformPBKDF2Hash(ClientUser.GetHash(), sessionSalt);
                if (authenticationToken == LINFO.passwordHash) //Compare login tokens
                {
                    if (server.LogUserIn(ClientUser)) //If no client already logged in under that name
                    {
                        LINFO.confirmation = "true"; //will tell client that they have succcessfully logged in
                        LINFO.name = ClientUser.GetName();
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

        /*
         This function is used to determine whether an account with that the given email address exists 
         If an account does exists then a session salt is generated
            THis with the bool true is returned to the client
        Else false is returns to the client
        */
        private void LookupEmail(LoginInformation LINFO)
        {
            Message M = new Message();
            ClientUser = server.AccountLookup(LINFO.Email); //returns null if no account found
            if (ClientUser != null) //Account found
            {
                LINFO.accountSalt = ClientUser.GetSalt();
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

        /*
         This function is used to log the client out 
         IF clientSide is true this mean the client requested to be logged out 
            In this case the loggedIn flag is set to false using server.LogClientOut that tunnels via Auth to Ainfo 
         If the server is self is disconnecting the client e.g server being turned off 
            then a message is sent to the client informaing them that they are being logged out
                this action will result in the client side being closed
        */
        private void Logout(bool clientSide, LoginInformation logout = null)
        {
            if (clientSide)
            {
                if (ClientUser != null)
                {
                    if (ClientUser.GetLoggedIn() == true)
                    {
                        server.LogClientOut(ClientUser);
                    }
                }
                //This means client closed their application and to remove everything
                if(logout.name == "close")
                {
                    log.Info("Client closed application and disconnected");
                    server.RemoveClientFromClientList(this); //Ensures removal of client from client list
                }
            }
            else
            {
                LoginInformation disconnect = new LoginInformation();
                Message M = new Message();
                JavaScriptSerializer Serializer = new JavaScriptSerializer();
                disconnect.stage = "5";
                M.message = Serializer.Serialize(disconnect);
                M.id = "1";
                string messageToSend = Serializer.Serialize(M);
                SendMessage(messageToSend);
            }
        }
        #endregion

        /*
         This functions handles the sending of messages to the client 
         All messages are encrypted using the Clients AES via encryptMessage
         This uses the StreamWriter object writer
         Them writer is flused 
         If any errors occur e.g if client has been disconencted this a log of this will be made
        */
        public void SendMessage(string message)
        {
            try
            {
                writer.WriteLine(EncryptMessage(message));
                writer.Flush();
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                log.Error("Client already disconnected");
            }

        }

        //This function is used when the server is being shut down and needs to disconnect all clients
        //First the user is logged out and then is removed from the client handler list
        public void Disconnect()
        {
            log.Info("Client disconnected");
            Logout(false);
            server.RemoveClientFromClientList(this); //Ensures removal of client from client list
        }

        /*
         This function is responsible for handling AES encryption
         Using AESManaged, memorystream, cryptostream and streamwrither the message is able to be encrypted 
         The encrypter is set up using the AES key and IV on record for this client
         The cipher text is converted from a byte[] to a string and returned
        */
        public string EncryptMessage(string message)
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
                return null;
            }
        }

        /*
         THis function handles message AES decryption 
         This follows a similar process to that of the encrypter 
            But with a decrypter being created 
         The plain text is returned
        */
        public string DecryptMessage(string message)
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
