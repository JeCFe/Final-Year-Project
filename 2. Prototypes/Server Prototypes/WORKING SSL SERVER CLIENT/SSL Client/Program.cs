using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace SSL_Client
{
    class Program
    {
        public Thread tcpThread;
        public string Server { get { return "192.168.0.23"; } }
        public int Port { get { return 2000; } }
        static void Main(string[] args)
        {
            Program p = new Program();
        }
        public Program()
        {
            Connect();
        }
        void Connect()
        {
            tcpThread = new Thread(new ThreadStart(SetupConn));
            tcpThread.Start();
        }

        TcpClient client;
        NetworkStream netStream;
        SslStream sslStream;
        Thread listeningThread;
        Thread writeThread;
        StreamReader reader;
        StreamWriter writer; 
        
        void SetupConn()
        {
            client = new TcpClient(Server, Port);
            netStream = client.GetStream();
            sslStream = new SslStream(netStream, false, new RemoteCertificateValidationCallback(ValidateCert));
            sslStream.AuthenticateAsClient("InstantMessengerServer");
            reader = new StreamReader(sslStream, Encoding.Unicode);
            writer = new StreamWriter(sslStream, Encoding.Unicode);
            listeningThread = new Thread(new ThreadStart(Listen));
            listeningThread.Start();
            Write();
        }
        void Write()
        {
            while (true)
            {
                string message = Console.ReadLine();
                writer.WriteLine(message);
                writer.Flush();
               /* string strMessage = Console.ReadLine();
                byte[] message = Encoding.UTF8.GetBytes(strMessage +"<EOF>");
                Console.WriteLine("Sending hello message.");
                sslStream.Write(message);
                sslStream.Flush();*/
            }
        }
        void Listen()
        {
            while (true)
            {
                string receivedMessage = reader.ReadLine();
                Console.WriteLine(receivedMessage);
                //byte[] buffer = new byte[2048];
                //StringBuilder message = new StringBuilder();
                //int bytes = -1;
                //do
                //{
                //    bytes = sslStream.Read(buffer, 0, buffer.Length);

                //    Decoder decoder = Encoding.UTF8.GetDecoder();
                //    char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                //    decoder.GetChars(buffer, 0, bytes, chars, 0);
                //    message.Append(chars);
                //    // Check for EOF.
                //    if (message.ToString().IndexOf("<EOF>") != -1)
                //    {
                //        break;
                //    }

                //} while (bytes != 0);
                //Console.WriteLine(message.ToString());


            }
        }

        public static bool ValidateCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Uncomment this lines to disallow untrusted certificates.
            //if (sslPolicyErrors == SslPolicyErrors.None)
            //    return true;
            //else
            //    return false;

            return true; // Allow untrusted certificates.
        }
    }

}
