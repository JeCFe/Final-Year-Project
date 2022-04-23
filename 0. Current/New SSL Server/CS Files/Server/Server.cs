using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace New_SSL_Server
{
    //This class handles the server runnning 
    class Server
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Server.cs");

        //Mutex locks variables
        private static readonly Mutex removeClient = new Mutex();
        private static readonly Mutex databaseAddMutex = new Mutex();

        //Static IP and Port for the server 
        //Ideally these should be dynamic ease of convience these have been made static
        public IPAddress ip = IPAddress.Parse("192.168.0.23");
        public int port = 2556;
        public bool running = true;
        public TcpListener server;
        private static readonly string certPath = AppDomain.CurrentDomain.BaseDirectory + @"server.pfx"; //Paths for the SSL certificate
        public X509Certificate2 cert = new X509Certificate2(certPath, "SecureChat"); //Sets up public certificate with path and password
        public static List<ClientHandler> clientHandlers = new List<ClientHandler>(); //List for storing connected clients
        public static Authenticator auth; //Authenticator object variable

        /*
         This function is responsible for killing the server and safely disconnecting all connected clients 
         Stops the listening function
        */
        public void ServerKill()
        {
            running = false;
            for (int i = 0; i < clientHandlers.Count; i++)
            {
                clientHandlers[i].Disconnect();
            }
            server.Stop();
        }

        /*
         This function is responsible for starting the server
         The Auth variable is passed from the Program class and is initalised with all admins and users lists
         Starts the TCP listener with the static IP and Port variables
         Starts the message Listen thread 
         Any errors thrown starting the server are logged
        */
        public void ServerStarter(Authenticator authenticator)
        {
            try
            {
                auth = authenticator;
                Console.WriteLine("Server Started");
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                log.Info("Server instance started");
                Listen();
            }
            catch (ThreadAbortException e)
            {
                log.Error("Server Starter error + " + e.ToString());
            }
        }

        //This function is used to look up client email address
        public AccountData AccountLookup(string email){ return auth.UserLookupRequest(email); }

        //This function is responsible for setting a client as logged in
        public bool LogUserIn(AccountData ad) { return auth.LogUserIn(ad); }

        /*
         This function is used to listen for new client connections 
         When a client connects to the server they are assigned a session salt
         They are are created a ClientHandler instance with the constructor being passed teh tcpClient, the server reference, and the session salt
         The client is then added to the ClientHandlers list
         This function runs until the server is stopped by an admin
        */
        void Listen()
        {
            while (running)
            {
                try
                {
                    TcpClient tcpClient = server.AcceptTcpClient();
                    byte[] salt = auth.GenerateSalt();
                    ClientHandler client = new ClientHandler(tcpClient, this, salt); //Creates new client instance, runs on seperate threads
                    clientHandlers.Add(client); //Adds client to clienthandlers
                }
                catch (Exception e)
                {
                    log.Info("Potential server closing view error for more info");
                    log.Error(e.ToString());
                }

            }
        }

        //This function is used to flag a client as logged out
        public void LogClientOut(AccountData Client){ auth.LogClientOut(Client); }

        /*
         This function is used to remove a client from ClientHandlers list when they disconnect
         This function uses mutex locks as multiple threads can be accessing a shared resource
         When the client has been removed from the list the lock is released
        */
        public void RemoveClientFromClientList(ClientHandler client)
        {
            removeClient.WaitOne();
            clientHandlers.Remove(client);
            log.Info("Client disconnected");
            removeClient.ReleaseMutex();
        }

        //This function is used to broadcast client messages to all clients 
        public static void Broadcast(string msg){ foreach (ClientHandler client in clientHandlers) { client.SendBasicMessage(msg); } }

        //This function is used to return a salt generation from the Auth class
        public string GenerateAccountSalt() { return Convert.ToBase64String(auth.GenerateSalt()); }

        /*
          This function is used to add a new account to database and the relevant list
          This function has a mutex due to accessing a shared resource, when the account has been addedd ..
          .. the lock is released
         */
        public void RegNewAccount(AccountData ad)
        {
            databaseAddMutex.WaitOne();
            auth.RegNewAccount(ad);
            databaseAddMutex.ReleaseMutex();
        }

        //This function is used to call the Auth class to decrypt the RSA recived from the client and return the AES bytes
        public byte[] DecryptAESKey(byte[] message) { return auth.DecryptAESMessage(message); }

        //This function is used to get the servers RSA keys held in the Auth class
        public string GetPublicKey() { return auth.GetPublicRSA(); }
    }
}
