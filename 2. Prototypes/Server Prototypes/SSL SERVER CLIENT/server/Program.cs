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
using System.Collections.Generic;

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

            File.WriteAllBytes("C:\\Users\\Jessi\\OneDrive\\Documents\\GitHub\\Final-Year-Project\\2. Prototypes\\Server Prototypes\\SSL SERVER CLIENT\\server\\serverCertificate.pfx", certificate.Export(X509ContentType.Pfx, "password"));
            File.WriteAllText("C:\\Users\\Jessi\\OneDrive\\Documents\\GitHub\\Final-Year-Project\\2. Prototypes\\Server Prototypes\\SSL SERVER CLIENT\\server\\serverCertificate.cer",
                 "-----BEGIN CERTIFICATE-----\r\n" + Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
                 + "\r\n-----END CERTIFICATE-----");
        }
    }


    class Program
    {


        public static Hashtable clientsList = new Hashtable();
        public static List<ClientHandler> clientHandlers = new List<ClientHandler>();
        static X509Certificate2 serverCertificate = null;
        static void Main(string[] args)
        {
            //ServerEncryptionManager SEM = new ServerEncryptionManager();
            serverCertificate = new X509Certificate2("C:\\Users\\Jessi\\OneDrive\\Documents\\GitHub\\Final-Year-Project\\2. Prototypes\\Server Prototypes\\SSL SERVER CLIENT\\server\\server.pfx", "instant");
           // serverCertificate = X509Certificate2.CreateFromCertFile("C:\\Users\\Jessi\\OneDrive\\Documents\\GitHub\\Final-Year-Project\\2. Prototypes\\Server Prototypes\\SSL SERVER CLIENT\\server\\serverCertificate.Pfx");
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
                clientHandlers.Add(client);
                client.startClient(clientSocket, serverCertificate);
                //ProcessClient(clientSocket);

            }
            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("exit");
            Console.Read();
        }
       
        public static void broadcast(string msg)
        {

            foreach (ClientHandler client in clientHandlers)
            {
                information informationupdater = new information();
                informationupdater.message = msg;
                JavaScriptSerializer Serializer = new JavaScriptSerializer();
                string jsonString = Serializer.Serialize(informationupdater);

                client.SendMessage(jsonString);

            }

        }

        public class ClientHandler
        {

            TcpClient clientSocket;
            X509Certificate cert;
            SslStream sslStream;
  
            string name = "";
            public void startClient(TcpClient inClientSocket, X509Certificate x509Certificate)
            {
                this.clientSocket = inClientSocket;
                this.cert = x509Certificate;
                ProcessClient();
                Thread clientThread = new Thread(ProcessClient);
                clientThread.Start();
            }
            private void ProcessClient()
            {
                sslStream = new SslStream(clientSocket.GetStream(), false);
                try
                {
                    sslStream.AuthenticateAsServer(cert);
                   DisplaySecurityLevel(sslStream);
                    DisplaySecurityServices(sslStream);
                    DisplayCertificateInformation(sslStream);
                    DisplayStreamProperties(sslStream);
                    sslStream.ReadTimeout = 5000;
                    sslStream.WriteTimeout = 5000;
                    Thread messageThread = new Thread(ReadMessage);
                    messageThread.Start();

                }
                catch (AuthenticationException e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                    if (e.InnerException != null)
                    {
                        Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                    }
                    Console.WriteLine("Authentication failed - closing the connection.");
                    sslStream.Close();
                    clientSocket.Close();
                    return;
                }
            }

            private void ReadMessage()
            {
                while (true)
                {
                    byte [] buffer = new byte[2048];
                    StringBuilder message = new StringBuilder();
                    int bytes = -1;
                    do
                    {
                        bytes = sslStream.Read(buffer, 0, buffer.Length);
                        Decoder decoder = Encoding.UTF8.GetDecoder();
                        char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                        decoder.GetChars(buffer, 0, bytes, chars, 0);
                        message.Append(chars);
                        if (message.ToString().IndexOf("<EOF>") != -1)
                        {
                            break;
                        }
                    } while (bytes != 0);
                    string recievedMessage = message.ToString();
                    JavaScriptSerializer Deserializer = new JavaScriptSerializer();
                    information informationupdate = Deserializer.Deserialize<information>(recievedMessage);
                    if (informationupdate.identifier == "1")
                    {
                        name = informationupdate.message;
                    }
                    if (informationupdate.identifier == "2")
                    {
                        Program.broadcast("Client " + name + " says hi");
                    }
                }
            }
            public void SendMessage(string message)
            {
                byte[] messageToSend = Encoding.UTF8.GetBytes(message);
                sslStream.Write(messageToSend);
            }

/*            private void doChat()
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
            }*/
        }
        static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }
        static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }
        static void DisplayStreamProperties(SslStream stream)
        {
            Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
            Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
        }
        static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }
            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
            }
        }
        private static void DisplayUsage()
        {
            Console.WriteLine("To start the server specify:");
            Console.WriteLine("serverSync certificateFile.cer");
            Environment.Exit(1);
        }
    }
}
