﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace New_SSL_Server
{
    class Server
    {
        private static Mutex removeClient = new Mutex();
        private static Mutex databaseAddMutex = new Mutex();
        public IPAddress ip = IPAddress.Parse("192.168.0.23");
        public int port = 2556;
        public bool running = true;
        public TcpListener server;
        private static string certPath = AppDomain.CurrentDomain.BaseDirectory + @"server.pfx";
        public X509Certificate2 cert = new X509Certificate2(certPath, "SecureChat");
        public static List<ClientHandler> clientHandlers = new List<ClientHandler>();
        public static Authenticator auth;

        //static void Main(string[] args)
        //{
        //    auth = new Authenticator();
        //    auth.Initalise();
        //    auth.validateAdmin();
        //    Program p = new Program();
        //}

        public AccountData AccountLookup(string email)
        {
            return auth.UserLookupRequest(email);
        }
        public void ServerStarter(Authenticator authenticator)
        {
            try
            {
                auth = authenticator;
                Console.WriteLine("Server Started");
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                Listen();
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine("Can safely kill");
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
            removeClient.WaitOne();
            clientHandlers.Remove(client);
            removeClient.ReleaseMutex();
        }
        public static void broadcast(string msg)
        {
            foreach (ClientHandler client in clientHandlers)
            {
                client.SendBasicMessage(msg);
            }
        }
        public string GenerateAccountSalt()
        {
            return Convert.ToBase64String(auth.GenerateSalt());
        }
        public void RegNewAccount(AccountData ad)
        {
            //lock
            Console.WriteLine(ad.getName() + " is waiting");
            databaseAddMutex.WaitOne();
            auth.RegNewAccount(ad);
            Console.WriteLine(ad.getName() + " is Completed");
            databaseAddMutex.ReleaseMutex();
        }
        public byte[] decryptAESKey(byte[] message)
        {
            return auth.decryptAESMessage(message);
        }
        public string getPublicKey()
        {
            return auth.getPublicRSA();

        }
    }
}