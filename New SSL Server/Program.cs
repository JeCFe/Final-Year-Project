using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace SSL_Server
{

    class Message //Default message recieved 
    {
        public string id;
        public string message; //Additional serialised Message
    }
    class HandShakeMessage //ID CODE 0
    {
        public string stage;
        public string test;
        public string RSAPublicKey;
        public byte[] EncryptedAESKey;
        public byte[] EncryptedAESIV;
        public bool Confirmation;
    }
    class LoginInformation //ID CODE 1
    {
        public string confirmation;
        public string stage;
        public string Email;
        public string accountSalt;
        public string sessionSalt;
        public string passwordHash;
    }
    class RegistrationInformation //ID CODE 2
    {
        public string Name;
        public string Email;
        public string PasswordHash;
        public string AccountSalt;
        public string stage;
        public string confirmation;
    }
    class StandardMessage //ID CODE 3
    {
        public string message;
    }
    class AdminLoginAuthentication
    {
        protected string hashedPassword;
        protected string adminSalt;
        protected string adminUser;
        public AdminLoginAuthentication(BsonDocument data)
        {
            hashedPassword = data["hashedPassword"].AsString;
            adminSalt = data["adminSalt"].AsString;
            adminUser = data["adminUser"].AsString;
        }
        public string getHashedPassword() { return hashedPassword; }
        public string getAdminSalt() { return adminSalt; }
        public string getAdminUser() { return adminUser; }
    }
    class AccountData
    {
        protected string Name;
        protected string Email;
        protected string Hash;
        protected string Salt;
        protected bool loggedIn;

        public AccountData(BsonDocument data)
        {
            Name = data["Name"].AsString;
            Email = data["Email"].AsString;
            Hash = data["Hash"].AsString;
            Salt = data["Salt"].AsString;
            loggedIn = false;
        }
        public AccountData(RegistrationInformation RINFO)
        {
            Name = RINFO.Name;
            Email = RINFO.Email;
            Hash = RINFO.PasswordHash;
            Salt = RINFO.AccountSalt;
            loggedIn = false;
        }
        public string getName() { return Name; }
        public string getEmail() { return Email; }
        public string getHash() { return Hash; }
        public string getSalt() { return Salt; }
        public bool getLoggedIn() { return loggedIn; }
        public void setLoggedIn(bool log) { loggedIn = log; }
    }
    class AuthenticationInformation
    {
        protected List<AdminLoginAuthentication> ALAUTH = new List<AdminLoginAuthentication>();
        protected List<AccountData> accountData = new List<AccountData>();
        public AuthenticationInformation()
        {
            UserAuthInfo();
            ServerAuthInfo();
        }
        private void UserAuthInfo()
        {
            MongoClient dsClient = new MongoClient("mongodb://ServerAccess:k2KNmJpNUlvGtNJG@accounts-shard-00-00.rhuha.mongodb.net:27017," +
             "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");

            IMongoDatabase dab = dsClient.GetDatabase("accountsDB");
            var AccountDetails = dab.GetCollection<BsonDocument>("account");
            var documents = AccountDetails.Find(new BsonDocument()).ToList();
            foreach (BsonDocument item in documents)
            {
                Console.WriteLine(item.ToString());
                // BsonSerializer.Deserialize<AccountData>(item);
                AccountData ad = new AccountData(item);
                accountData.Add(ad);
            }
        }
        private void ServerAuthInfo()
        {
            MongoClient dbClient = new MongoClient("mongodb://ServerAccess:k2KNmJpNUlvGtNJG@accounts-shard-00-00.rhuha.mongodb.net:27017," +
            "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase db = dbClient.GetDatabase("accountsDB");
            var ServerDetails = db.GetCollection<BsonDocument>("server");
            var documents = ServerDetails.Find(new BsonDocument()).ToList();
            foreach (BsonDocument item in documents)
            {
                AdminLoginAuthentication alog = new AdminLoginAuthentication(item);
                ALAUTH.Add(alog);
            }
        }

        public bool validateUser(string email, string hashedPassword, string sessionSalt) 
        {
            return true;
        }
        public AccountData ClientLookupRequest(string email)
        {
            AccountData ad = null;
            foreach (AccountData item in accountData)
            {
                if (item.getEmail() == email)
                {
                    ad = item;
                }
            }
            return ad;
        }
        public AdminLoginAuthentication searchServerAuth(string adminUserName)
        {
            AdminLoginAuthentication aLog = null;
            foreach (AdminLoginAuthentication item in ALAUTH)
            {
                if (item.getAdminUser() == adminUserName)
                {
                    aLog = item;
                }
            }
            return aLog;
        }

        public bool LogInUser(AccountData ad)
        {
            int index = accountData.IndexOf(ad);
            AccountData reviewAccount = accountData[index];
            if (reviewAccount.getLoggedIn() == false) //If no account logged in under that account
            {
                reviewAccount.setLoggedIn(true); //Mark that account has logged in 
                accountData[index] = reviewAccount;
                return true;
            }
            else //if account already logged in
            {
                return false;
            }
        }
        private void UpdateUserDatabase(AccountData ad)
        {
            MongoClient dsClient = new MongoClient("mongodb://ServerAccess:k2KNmJpNUlvGtNJG@accounts-shard-00-00.rhuha.mongodb.net:27017," +
                                     "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");

            IMongoDatabase dab = dsClient.GetDatabase("accountsDB");
            var AccountDetails = dab.GetCollection<BsonDocument>("account");
            var document = new BsonDocument
            {
                {"Email", ad.getEmail() },
                {"Hash", ad.getHash() },
                {"Salt", ad.getSalt() },
                {"Name", ad.getName() }
            };
            AccountDetails.InsertOneAsync(document); //Update database
        }
        public void RegNewAccount(AccountData ad)
        {
            accountData.Add(ad);
            UpdateUserDatabase(ad);
        }
    }
    class Authenticator
    {
        private AuthenticationInformation Ainfo;
        private RSAParameters pubKey;
        private RSAParameters privKey;
        private string stringPubKey;

        public void Initalise()
        {
            Ainfo = new AuthenticationInformation();
            exportKeys();
        }
        private void exportKeys()
        {
            var csp = new RSACryptoServiceProvider(2048);
            privKey = csp.ExportParameters(true);
            pubKey = csp.ExportParameters(false);
            stringPubKey = csp.ToXmlString(false);
        }
        public string getPublicRSA()
        {
            return stringPubKey;
        }
        public byte[] decryptAESMessage(byte[] message)
        {
            try
            {

                var privCSP = new RSACryptoServiceProvider();
                var pubCSP = new RSACryptoServiceProvider();
                pubCSP.FromXmlString(stringPubKey);
                privCSP.ImportParameters(privKey);
                byte[] decryptedBytes = privCSP.Decrypt(message, RSAEncryptionPadding.Pkcs1);

                return decryptedBytes;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public bool validateAdmin()
        {
            Console.WriteLine("Enter username: ");
            string userName = Console.ReadLine();
            Console.WriteLine("Enter password");
            string plainPassword = Console.ReadLine();

            AdminLoginAuthentication aLog = Ainfo.searchServerAuth(userName);

            return true;
        }
        public string PreformPBKDF2Hash(string accountHashedPassword, byte[] salt)
        {
            int iterations = 10000;
            Rfc2898DeriveBytes hash = new Rfc2898DeriveBytes(accountHashedPassword, salt, iterations);
            return Convert.ToBase64String(hash.GetBytes(128));

        }
        public AccountData UserLookupRequest(string email)
        {
            //This will need to be mutex locked
            AccountData ad = Ainfo.ClientLookupRequest(email);
            return ad;
        }
        public bool LogUserIn(AccountData ad)
        {
            return Ainfo.LogInUser(ad);
        }
        public byte[] GenerateSalt()
        {
            string sessionSalt = "";
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] salt = new byte[128];
            random.GetBytes(salt);
            return salt;
        }
        public void RegNewAccount(AccountData ad)
        {
            Ainfo.RegNewAccount(ad);
        }

    }
    
    class Program
    {
        public IPAddress ip = IPAddress.Parse("192.168.0.23");
        public int port = 2556;
        public bool running = true;
        public TcpListener server;
        public  X509Certificate2 cert = new X509Certificate2("C:\\Users\\Jessi\\OneDrive\\Documents\\GitHub\\Final-Year-Project\\New SSL Server\\server.pfx", "SecureChat");
        public static List<ClientHandler> clientHandlers = new List<ClientHandler>();
        public static Authenticator auth;

        static void Main(string[] args)
        {

            InitaliseRSA();
            auth = new Authenticator();
            auth.Initalise();
            auth.validateAdmin();



            Program p = new Program();

        }

        public AccountData AccountLookup(string email)
        {
            //Lock
            AccountData ad = auth.UserLookupRequest(email);
            return ad;

        }
        public static void InitaliseRSA()
        {
            var CSP = new RSACryptoServiceProvider(2048);
        }

        public Program()
        {
            Console.WriteLine("Server Started");
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Listen();
        }
        public bool LogUserIn(AccountData ad)
        {
            return auth.LogUserIn(ad);
        }
        void Listen()
        {
            while (running)
            {
                TcpClient tcpClient = server.AcceptTcpClient();
                byte[] salt = auth.GenerateSalt();
                ClientHandler client = new ClientHandler(tcpClient, this, salt); //Creates new client instance, runs on seperate threads
                clientHandlers.Add(client); //Adds client to clienthandlers
            }

        }
        public void removeClientFromClientList(ClientHandler client)
        {
            //lock
            clientHandlers.Remove(client);
        }
        public static void broadcast(string msg)
        {
            foreach (ClientHandler client in clientHandlers)
            {
                client.SendMessage(msg);
            }
        }
        public string GenerateAccountSalt()
        {
            //Lock
            string salt = Convert.ToBase64String(auth.GenerateSalt());
            return salt;
        }
        public void RegNewAccount(AccountData ad)
        {
            //lock
            auth.RegNewAccount(ad);

        }
        public byte[] decryptAESKey(byte[] message)
        {
            //needs to lock
            return auth.decryptAESMessage(message);
        }
        public string getPublicKey()
        {
            return auth.getPublicRSA();

        }

    }




    class ClientHandler
    {
        Program prog;
        public TcpClient client;
        public NetworkStream netStream;
        public SslStream sslStream;
        private StreamWriter writer;
        private StreamReader reader;
        private byte[] sessionSalt;
        Aes aesKey = Aes.Create();
        RSA privateKey;
        RSA publicKey;
        //All types of messages that can be recieved

        AccountData ClientUser = null;

        public ClientHandler(TcpClient clientSocket, Program p, byte[] sSalt)
        {
            prog = p;
            client = clientSocket;
            sessionSalt = sSalt;

            (new Thread(new ThreadStart(SetupConn))).Start();
        }

        void testRSA()
        {
            privateKey = prog.cert.GetRSAPrivateKey();
            publicKey = prog.cert.GetRSAPublicKey();
        }

        void SetupConn()
        {
            try
            {
                Console.WriteLine("New client");
                netStream = client.GetStream();
                sslStream = new SslStream(netStream, false);
                sslStream.AuthenticateAsServer(prog.cert, false, SslProtocols.Tls, true);
                Console.WriteLine("Connection Authenticated");
                reader = new StreamReader(sslStream, Encoding.Unicode);
                writer = new StreamWriter(sslStream, Encoding.Unicode);
                testRSA();
                Thread messageThread = new Thread(ReadMessage);
                messageThread.Start();
                InitaliseHandshake();

            }
            catch { }
        }

        private void CommonCommunications(Message M)
        {
            StandardMessage SM = new StandardMessage();

            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            SM = Deserializer.Deserialize<StandardMessage>(M.message);
            string amendedMessage = ClientUser.getName() + ": " + SM.message;

            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            SM.message = amendedMessage;
            string SMstring = Serializer.Serialize(SM);

            M.message = SMstring;
            M.id = "3";
            if (M == null)
            {
                Console.WriteLine("Error");
            }
            string messageToSend = Serializer.Serialize(M);
            Program.broadcast(messageToSend);
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
                    try
                    {
                        //decryptedMessage = decryptMessage(receivedMessage);
                    }
                    catch (Exception)
                    {
                        //error log
                    }

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
                catch (Exception)
                {
                    clientConnected = false; //Ends the infinte loop
                    prog.removeClientFromClientList(this); //Ensures removal of client from client list
                    client.Close(); //Closes client connection
                    Console.WriteLine("Connection closed");
                }

             }

        }
        private void InitaliseHandshake()
        {
            HandShakeMessage HSM = new HandShakeMessage();
            Message M = new Message();
            M.id = "0";

            string rsa = prog.getPublicKey();
            HSM.stage = "1";
            HSM.RSAPublicKey = rsa;
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string HSMstring = Serializer.Serialize(HSM);
            M.message = HSMstring;
            string Mmessage = Serializer.Serialize(M);
            if (Mmessage == "")
            {
                Console.WriteLine("Error");
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
                aesKey.Key = prog.decryptAESKey(HSM.EncryptedAESKey);
                aesKey.IV = prog.decryptAESKey(HSM.EncryptedAESIV);

                if (decryptMessage(HSM.test) == "Test")
                {
                    Console.WriteLine("Success");
                }
            }
        }
        private void RegistrationManager(Message M)
        {
            RegistrationInformation RINFO = new RegistrationInformation();
            JavaScriptSerializer Deserializer = new JavaScriptSerializer();
            RINFO = Deserializer.Deserialize<RegistrationInformation>(M.message);
            switch (decryptMessage(RINFO.stage))
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

            RINFO.stage = encryptMessage("1");
            RINFO.AccountSalt = prog.GenerateAccountSalt();
            RINFO.confirmation = encryptMessage("true");

            string RINFOmessage = Serializer.Serialize(RINFO);
            M.id = "2";
            M.message = RINFOmessage;
            string message = Serializer.Serialize(M);
            SendMessage(message);
        }
        private void RegistrationFinalStage(RegistrationInformation RINFO)
        {
            RegistrationInformation TempRI = new RegistrationInformation();
            TempRI.AccountSalt = RINFO.AccountSalt;
            TempRI.Email = decryptMessage(RINFO.Email);
            TempRI.PasswordHash = RINFO.PasswordHash;
            TempRI.Name = decryptMessage(RINFO.Name);
            AccountData ad = new AccountData(TempRI);
            if (prog.AccountLookup(ad.getEmail()) == null) //If no match for email
            {
                RINFO.confirmation = encryptMessage("true");
                prog.RegNewAccount(ad);
            }
            else
            {
                RINFO.confirmation = encryptMessage("false");
            }
            Message M = new Message();
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            RINFO.stage = encryptMessage("2");
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
            switch (decryptMessage(LINFO.stage))
            {
                case "1": //Recieved lookup email
                    LookupEmail(LINFO);
                    break;
                case "2":
                    HashComparision(LINFO);
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
                    if (prog.LogUserIn(ClientUser)) //If no client already logged in under that name
                    {
                        LINFO.confirmation = encryptMessage("true"); //will tell client that they have succcessfully logged in
                    }
                    else
                    {
                        LINFO.confirmation = encryptMessage("false"); //will tell client they havent logged in
                        ClientUser = null; //deletes old clientUser data
                    }
                }
                else
                {
                    LINFO.confirmation = encryptMessage("false"); //Incorrect password
                    ClientUser = null;
                }
                LINFO.stage = encryptMessage("3");

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
            ClientUser = prog.AccountLookup(decryptMessage(LINFO.Email)); //returns null if no account found
            if (ClientUser != null) //Account found
            {

                LINFO.accountSalt = ClientUser.getSalt();
                LINFO.sessionSalt = Convert.ToBase64String(sessionSalt);
                LINFO.confirmation = encryptMessage("true");
            }
            else //Account not found
            {
                LINFO.confirmation = encryptMessage("False");
                
            }
            LINFO.stage = encryptMessage("2");
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string LINFOmessage = Serializer.Serialize(LINFO);
            M.id = "1";
            M.message = LINFOmessage;
            string message = Serializer.Serialize(M);
            if (message == "")
            {
                Console.WriteLine("Error");
            }
            SendMessage(message);
        }
        public void SendMessage(string message)
        {
            //string encryptedMessage = encryptMessage(message);
            writer.WriteLine(message);
            writer.Flush();
        }
        public string encryptMessage(string message)
        {
            try
            {
                byte[] encryptedData;
                string encryptedString = "";
                ICryptoTransform encryptor = aesKey.CreateEncryptor();
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(message);
                        }
                        encryptedData = msEncrypt.ToArray();
                    }
                }
                encryptedString = Encoding.Unicode.GetString(encryptedData);
                return encryptedString;
            }
            catch (Exception)
            { 
                throw;
            }

        }
        public string decryptMessage(string message)
        {
            try
            {
                string decryptedString = "";
                byte[] encryptedData = Encoding.Unicode.GetBytes(message);
                ICryptoTransform decryptor = aesKey.CreateDecryptor();
                using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            decryptedString = srDecrypt.ReadToEnd();
                        }
                    }
                }
                return decryptedString;
            }
            catch (Exception)
            {

                return message;
            }

        }
    }
}
