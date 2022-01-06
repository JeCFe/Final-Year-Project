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

        public AccountData(BsonDocument data)
        {
            Name = data["Name"].AsString;
            Email = data["Email"].AsString;
            Hash = data["Hash"].AsString;
            Salt = data["Salt"].AsString;
        }
        public string getName() { return Name; }
        public string getEmail() { return Email; }
        public string getHash() { return Hash; }
        public string getSalt() { return Salt; }
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
    }

    class Authenticator
    {
        private AuthenticationInformation Ainfo;
        public Authenticator()
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
        public AccountData UserLookupRequest(string email)
        {
            //This will need to be mutex locked
            AccountData ad = Ainfo.ClientLookupRequest(email);
            return ad;
        }
        public string GenerateSessionSalt()
        {
            string sessionSalt = "";
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[1024];
            random.GetBytes(buffer);
            sessionSalt = BitConverter.ToString(buffer);
            return sessionSalt;
        }
    }
    

    class Program
    {
        public IPAddress ip = IPAddress.Parse("192.168.0.23");
        public int port = 2000;
        public bool running = true;
        public TcpListener server;
        public X509Certificate2 cert = new X509Certificate2("C:\\Users\\Jessi\\OneDrive\\Documents\\GitHub\\Final-Year-Project\\SSL Server\\server.pfx", "SecureChat");
        public static List<ClientHandler> clientHandlers = new List<ClientHandler>();
        public static Authenticator auth;
        static void Main(string[] args)
        {
            InitaliseRSA();
            auth = new Authenticator();
            auth.validateAdmin();
            Program p = new Program();

        }
        public static string GenerateSessionSalt()
        {
            string sessionSalt = auth.GenerateSessionSalt();
            return sessionSalt;
        }
        public static void InitaliseRSA()
        {
            var CSP = new RSACryptoServiceProvider(2048);
        }

        public Program()
        {
            Console.WriteLine("Server Started");
            server = new TcpListener(ip, port);
            server.Start();
            Listen();
        }
        void Listen()
        {
            while (running)
            {
                TcpClient tcpClient = server.AcceptTcpClient();

                ClientHandler client = new ClientHandler(tcpClient, this, auth);
                clientHandlers.Add(client);
            }

        }
        public static void broadcast(string msg)
        {
            foreach (ClientHandler client in clientHandlers)
            {
                client.SendMessage(msg);
            }

        }
        public static void AccessMongoDB()
        {
            

        }

    }

    //Class that contains a users email and hash value for comparison at a later point


    class ClientHandler
    {
        Program prog;
        Authenticator auth;
        public TcpClient client;
        public NetworkStream netStream;
        public SslStream sslStream;
        private StreamWriter writer;
        private StreamReader reader;
        private string sessionSalt;
        //All types of messages that can be recieved

        HandShakeMessage HSM = new HandShakeMessage();

        RegistrationInformation RINFO = new RegistrationInformation();

        AccountData ClientUser = null;

        public ClientHandler(TcpClient clientSocket, Program p, Authenticator authenticate)
        {
            prog = p;
            auth = authenticate;
            client = clientSocket;
            sessionSalt = auth.GenerateSessionSalt();
            (new Thread(new ThreadStart(SetupConn))).Start();
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

                Thread messageThread = new Thread(ReadMessage);
                messageThread.Start();
                Console.WriteLine("Test");
            }
            catch { }
        }

        private void CommonCommunications(Message M)
        {
            JavaScriptSerializer Serializer = new JavaScriptSerializer();
            string messageToSend = Serializer.Serialize(M);
            Program.broadcast(messageToSend);
        }
        private void ReadMessage()
        {
            while (true)
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
                catch (Exception)
                {

                    Console.WriteLine("Connection closed");
                    client.Close();
                    //Remove Client from client vector
                    break;
                }

            }

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
                default:
                    break;
            }
        }
        private void LookupEmail(LoginInformation LINFO)
        {
            Message M = new Message();
            ClientUser = auth.UserLookupRequest(LINFO.Email);
            if (ClientUser != null) //Account found
            {

                LINFO.accountSalt = ClientUser.getSalt();
                LINFO.sessionSalt = sessionSalt;
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
            writer.WriteLine(message);
            writer.Flush();
        }
        public void SendMessage(string message)
        {
            writer.WriteLine(message);
            writer.Flush();
            /* byte[] messageToSend = Encoding.UTF8.GetBytes(message);
             sslStream.Write(messageToSend);
             sslStream.Flush();*/
        }
    }
}
