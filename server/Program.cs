using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Web.Script.Serialization;
using System.Net;

namespace server
{
    class information
    {
        //take in username
        //take in password
        //decryption data
        public string message;
    }


    class Program
    {

        public static Hashtable clientsList = new Hashtable();
        static void Main(string[] args)
        {
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


                string dataFromClient = null;
                NetworkStream networkStream = clientSocket.GetStream();

                byte[] buffer = new byte[256];
                int num_bytes = networkStream.Read(buffer, 0, 256);
                dataFromClient = Encoding.ASCII.GetString(buffer, 0, num_bytes);
                //DecryptData
                JavaScriptSerializer Deserializer = new JavaScriptSerializer();
                information informationupdate = Deserializer.Deserialize<information>(dataFromClient);
                //Log in here

                information informationupdater = new information();
                informationupdater.message = "Recieved";
                JavaScriptSerializer Serializer = new JavaScriptSerializer();
                string jsonString = Serializer.Serialize(informationupdater);

                //finish this
                //Start to allow client to recieve messages 

                TcpClient broadCastSocket;
                broadCastSocket = clientSocket;
                NetworkStream broadcastStream = broadCastSocket.GetStream();
                byte[] broadcastBytes = Encoding.ASCII.GetBytes(jsonString);

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
                    

                //if login fails return message to client

                //if log in is successful then skip this section
                //clientsList.Add(dataFromClient, clientSocket);
                //Broadcast
                //HandleClient 
                //Start Client handling
                Console.WriteLine("Connection successful \n");
                Console.WriteLine(informationupdate.message);

     
            }
            clientSocket.Close();
            serverSocket.Stop();
            Console.WriteLine("exit");
            Console.Read();
        }

    }
}
