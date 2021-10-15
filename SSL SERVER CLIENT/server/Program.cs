using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Web.Script.Serialization;

namespace server
{
    class information
    {
        public string identifier;
        //take in username
        //take in password
        //decryption data
        public string message;
    }

    class ServerEncryptionManager
    {
        protected static RSA rsaKeys = null;
        protected static Aes aesKeys = null;

        public ServerEncryptionManager()
        {
            GenerateRSAKeys();
            GenerateTestCertificate();
        }

        private static void GenerateRSAKeys()
        {
            rsaKeys = RSA.Create(2048);
        }
        private static void GenerateTestCertificate()
        {
            var request = new CertificateRequest("cn=JessicaFealy", rsaKeys, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var certificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(20));

            File.WriteAllBytes("C:\\Users\\Jessi\\OneDrive\\Documents\\GitHub\\Computer-Science-Year-2\\Proto\\SSL SERVER CLIENT\\server\\Certificate.pfx", certificate.Export(X509ContentType.Pfx, "password"));
            File.WriteAllText("C:\\Users\\Jessi\\OneDrive\\Documents\\GitHub\\Computer-Science-Year-2\\Proto\\SSL SERVER CLIENT\\server\\Certificate.cer",
                 "-----BEGIN CERTIFICATE-----\r\n" + Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
                 + "\r\n-----END CERTIFICATE-----");
        }
    }


    class Program
    {


        public static Hashtable clientsList = new Hashtable();
        static X509Certificate serverCertificate = null;
        static void Main(string[] args)
        {
            ServerEncryptionManager SEM = new ServerEncryptionManager();
            serverCertificate = X509Certificate.CreateFromCertFile("C:\\Users\\Jessi\\OneDrive\\Documents\\GitHub\\Computer-Science-Year-2\\Proto\\SSL SERVER CLIENT\\server\\Certificate.cer");
            IPAddress ip = IPAddress.Parse("192.168.0.23");
            TcpListener serverSocket = new TcpListener(ip, 8888);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;

            serverSocket.Start();
            Console.WriteLine("Chat Server Started ....");
            counter = 0;
            while (true)
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                clientsList.Add(counter, clientSocket);
                Console.WriteLine("Client added");
                ClientHandler client = new ClientHandler();
                client.startClient(clientSocket);
                //ProcessClient(clientSocket);

            }
            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("exit");
            Console.Read();
        }
        static void ProcessClient(TcpClient clientSocket)
        {
            SslStream sslStream = new SslStream(clientSocket.GetStream(), false);
            try
            {
                sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                sslStream.ReadTimeout = 5000;
                sslStream.WriteTimeout = 5000;

            }
            catch { }
        }
        public static void broadcast(string msg)
        {
            foreach (DictionaryEntry Item in clientsList)
            {
                information informationupdater = new information();
                informationupdater.message = msg;
                JavaScriptSerializer Serializer = new JavaScriptSerializer();
                string jsonString = Serializer.Serialize(informationupdater);

                TcpClient broadCastSocket;
                broadCastSocket = (TcpClient)Item.Value;
                NetworkStream broadcastStream = broadCastSocket.GetStream();
                byte[] broadcastBytes = Encoding.ASCII.GetBytes(jsonString);

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();

            }

        }

        public class ClientHandler
        {

            TcpClient clientSocket;
  
            string name = "";
            public void startClient(TcpClient inClientSocket)
            {
                this.clientSocket = inClientSocket;

                Thread clientThread = new Thread(doChat);
                clientThread.Start();
            }
            private void doChat()
            {
                int requestCount = 0;
                byte[] bytesFrom = new byte[10025];
                string dataFromClient = null;
                Byte[] sendBytes = null;
                string serverResponse = null;
                string rCount = null;
                requestCount = 0;

                while ((true))
                {
                    try
                    {
                        NetworkStream networkStream = clientSocket.GetStream();

                        byte[] buffer = new byte[256];
                        int num_bytes = networkStream.Read(buffer, 0, 256);
                        dataFromClient = Encoding.ASCII.GetString(buffer, 0, num_bytes);
                        //DecryptData
                        JavaScriptSerializer Deserializer = new JavaScriptSerializer();
                        information informationupdate = Deserializer.Deserialize<information>(dataFromClient);
                        if (informationupdate.identifier == "1")
                        {
                            name = informationupdate.message;
                        }
                        if (informationupdate.identifier == "2")
                        {
                            Program.broadcast("Client " + name + " says hi");
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
