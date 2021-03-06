using log4net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
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


[assembly: log4net.Config.XmlConfigurator(Watch = true)] //Wwatch ensure if the config is changed then the program will react in realtime

namespace SSL_Server
{
    class Message //Default message recieved 
    {
        public string id;
        public string message; //Additional serialised Message
    }
    class HandShakeMessage //ID CODE 0
    {
        public string RSAPublicKey;
        public string EncryptedAESKey;
        public string EncryptedAESIV;
        public bool Confirmation;
        public string SessionSalt;
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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("AuthenticationInformation.cs");
        protected List<AdminLoginAuthentication> ALAUTH = new List<AdminLoginAuthentication>();
        protected List<AccountData> accountData = new List<AccountData>();
        public AuthenticationInformation()
        {
            UserAuthInfo();
            ServerAuthInfo();
        }
        private void UserAuthInfo()
        {
            try
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
            catch (Exception e)
            {
                log.Error(e);
            }
        }
        private void ServerAuthInfo()
        {
            try
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
            catch (Exception e)
            {
                log.Error(e);
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
            try
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
            catch (Exception e)
            {
                log.Error(e);
            }
        }
        public void RegNewAccount(AccountData ad)
        {
            accountData.Add(ad);
            UpdateUserDatabase(ad);
        }

    }

    class Authenticator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Authenticator.cs");
        private AuthenticationInformation Ainfo;
        int a = 0;
        public void Initalise()
        {
            Ainfo = new AuthenticationInformation();
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
            //Consider having a try here
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
            //  string sessionSalt = "";
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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Program.cs");
        public IPAddress ip = IPAddress.Parse("192.168.0.23");


        public int port = 2556;
        public bool running = true;
        public TcpListener server;
        public X509Certificate2 cert = new X509Certificate2("C:\\Users\\Jessi\\OneDrive\\Documents\\GitHub\\Final-Year-Project\\New SSL Server\\server.pfx", "SecureChat");

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

            try
            {

                server = new TcpListener(ip, port);
                server.Start();
                log.Info("Server Started");
                Listen();

            }
            catch (Exception e)
            {
                log.Error(e);
            }
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

    }

    //Class that contains a users email and hash value for comparison at a later point


    class ClientHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("ClientHandler.cs");

        Program prog;
        public TcpClient client;
        public NetworkStream netStream;
        public SslStream sslStream;
        private StreamWriter writer;
        private StreamReader reader;
        private byte[] sessionSalt;
        //All types of messages that can be recieved

        HandShakeMessage HSM = new HandShakeMessage();



        AccountData ClientUser = null;

        public ClientHandler(TcpClient clientSocket, Program p, byte[] sSalt)
        {
            prog = p;
            client = clientSocket;
            sessionSalt = sSalt;
            (new Thread(new ThreadStart(SetupConn))).Start();
        }

        void SetupConn()
        {
            try
            {
                log.Info("New client");
                netStream = client.GetStream();
                sslStream = new SslStream(netStream, false);
                sslStream.AuthenticateAsServer(prog.cert, false, SslProtocols.Tls, true);
                log.Info("Connection Authenticated");
                reader = new StreamReader(sslStream, Encoding.Unicode);
                writer = new StreamWriter(sslStream, Encoding.Unicode);

                Thread messageThread = new Thread(ReadMessage);
                messageThread.Start();

            }
            catch (Exception e)
            {
                log.Error(e);
            }
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

                    JavaScriptSerializer Deserializer = new JavaScriptSerializer();
                    M = Deserializer.Deserialize<Message>(receivedMessage);
                    switch (M.id)
                    {
                        case "0": //HandShake
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
                    //Console.WriteLine(receivedMessage);

                    //Program.broadcast(receivedMessage);
                }
                catch (Exception e)
                {
                    clientConnected = false; //Ends the infinte loop
                    prog.removeClientFromClientList(this); //Ensures removal of client from client list
                    client.Close(); //Closes client connection

                    log.Error(e);
                    log.Info("Client Disconnected");
                    Console.WriteLine("Connection closed");
                }

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
            RINFO.AccountSalt = prog.GenerateAccountSalt();
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
            if (prog.AccountLookup(ad.getEmail()) == null) //If no match for email
            {
                RINFO.confirmation = "true";
                prog.RegNewAccount(ad);
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
                        LINFO.confirmation = "true"; //will tell client that they have succcessfully logged in
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
            ClientUser = prog.AccountLookup(LINFO.Email); //returns null if no account found
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
        public void SendMessage(string message)
        {
            writer.WriteLine(message);
            writer.Flush();
        }
    }
}
