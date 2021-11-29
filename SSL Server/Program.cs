using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace SSL_Server
{
    class information
    {
        public string identifier;
        //take in username
        //take in password
        //decryption data
        public string message;
    }
    class Program
    {
        public  IPAddress ip = IPAddress.Parse("192.168.0.23");
        public  int port = 2000;
        public  bool running = true;
        public  TcpListener server;
        public  X509Certificate2 cert = new X509Certificate2("C:\\Users\\Jessi\\OneDrive\\Documents\\GitHub\\Final-Year-Project\\SSL Server\\server.pfx", "SecureChat");
        public static List<ClientHandler> clientHandlers = new List<ClientHandler>();
        static void Main(string[] args)
        {
            Program p = new Program();

        }
        public Program()
        {
            server = new TcpListener(ip, port);
            server.Start();
            Listen();
        }
        void Listen()
        {
            while (running)
            {
                TcpClient tcpClient = server.AcceptTcpClient();

                ClientHandler client = new ClientHandler(tcpClient, this);
                clientHandlers.Add(client);
            }

        }
        public static void broadcast(string msg)
        {

            foreach (ClientHandler client in clientHandlers)
            {
/*                information informationupdater = new information();
                informationupdater.message = msg;
                JavaScriptSerializer Serializer = new JavaScriptSerializer();
                string jsonString = Serializer.Serialize(informationupdater);*/

                client.SendMessage(msg);

            }

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
        string name = "";
        public ClientHandler(TcpClient clientSocket, Program p)
        {
            prog = p;
            client = clientSocket;
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
            }
            catch { }
        }
        private void ReadMessage()
        {
            while (true)
            {


                    /*                    byte[] buffer = new byte[2048];
                                        StringBuilder message = new StringBuilder();
                                        int bytes = -1;
                                        sslStream.ReadTimeout = 5000;
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
                                        string recievedMessage = message.ToString();*/
                    string receivedMessage = reader.ReadLine();
                    Console.WriteLine(receivedMessage);

                    Program.broadcast(receivedMessage);
                }



                /*                JavaScriptSerializer Deserializer = new JavaScriptSerializer();
                                information informationupdate = Deserializer.Deserialize<information>(recievedMessage);
                                if (informationupdate.identifier == "1")
                                {
                                    name = informationupdate.message;
                                }
                                if (informationupdate.identifier == "2")
                                {
                                    Program.broadcast("Client " + name + " says hi");
                                }*/
            
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
