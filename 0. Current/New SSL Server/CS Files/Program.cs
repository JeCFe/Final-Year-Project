using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

[assembly: log4net.Config.XmlConfigurator(Watch = true)] //Wwatch ensure if the config is changed then the program will react in realtime

namespace New_SSL_Server
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Program.cs");
        private static Authenticator auth;
        private static InputValidation valid = new InputValidation();
        private static AdminDetails adminDetails;
        private static Server server = new Server();
        private static Thread serverThread = new Thread(delegate () { server.ServerStarter(auth); });
        private static ConsoleText View = new ConsoleText(adminDetails);

        static void Main(string[] args)
        {
            auth = new Authenticator();
            auth.Initalise();
            bool endLoop = true;
            do
            {
                AdminLogin();
                if (adminDetails.getAuthToken() == auth.ValidateAuthToken()) { View = new ConsoleText(adminDetails); AdminMenu(); }
                else { Console.WriteLine("Error"); endLoop = false; }
            } while (!endLoop);

        }
        private static void AdminMenu()
        {
            while (true)
            {
                View.Clear();
                int choice = View.Menu();

                switch (choice)
                {
                    case 1:
                        ServerStart();
                        break;
                    case 2:
                        ServerAbort();
                        //Work out how to kill a thread with an infinate loop
                        break;
                    case 3:
                        AddAdmin();
                        break;
                    case 4:
                        RemoveAdmin();
                        break;
                    case 5:
                        RemoveUser();
                        break;
                    case 6:
                        ShowLogs();
                        break;
                    default:
                        break;
                }
            }
        }
        private static void ServerStart(){ serverThread.Start(); View.setLastAction("Server Started"); }
        private static void ServerAbort() { serverThread.Abort(); Console.WriteLine("Server Killed"); View.setLastAction("Server Killed"); }
        private static void AddAdmin()
        {
            string name, password, username;
            bool endLoop;
            do
            {
                do //Intake name
                {
                    Console.Write("Enter admin name: ");
                    name = Console.ReadLine();
                } while (!valid.NameValidation(name));
                do //intake username
                {
                    Console.Write("Enter admin username: ");
                    username = Console.ReadLine();
                } while (!valid.NameValidation(username));
                do //intake password
                {
                    Console.Write("Enter admin password: ");
                    password = Console.ReadLine();
                } while (!valid.PasswordValidation(password));
                AdminRegDetails ARD = new AdminRegDetails();
                ARD.name = name;
                ARD.username = username;
                ARD.password = password;
                if (auth.RegAdmin(ARD)) { Console.WriteLine("Admin Account Added"); endLoop = true; }
                else { Console.WriteLine("Error occurred"); endLoop = false; }
                if (!endLoop)
                {
                    Console.Write("Do you wish to try again Y/N: ");
                    string input = Console.ReadLine();
                    if (input == "N"){ endLoop = true; }
                }
            } while (!endLoop);
            View.setLastAction("Admin Added");
        }
        private static void RemoveAdmin() { auth.DeleteAdmin(); }
        private static void RemoveUser() { auth.DeleteUser(); Console.WriteLine("These changes will be reflected the next time the server is run"); }
        private static void ShowLogs() { }
        private static void AdminLogin()
        {
            string adminUsername = null;
            string adminPassword = null;
            bool endLoop = false;
            bool authorise = false;
            do
            {
                View.Intro();
                do
                {
                    Console.Write("Enter admin username: ");
                    adminUsername = Console.ReadLine();
                    if (!valid.NameValidation(adminUsername)) { Console.WriteLine("Incorrect format entered!"); }
                    else { endLoop = true; }
                } while (!endLoop);
                endLoop = false;
                do
                {
                    Console.Write("Enter password: ");
                    adminPassword = Console.ReadLine();
                    if (!valid.PasswordValidation(adminPassword)) { Console.WriteLine("Incorrect format entered!"); }
                    else { endLoop = true; }
                } while (!endLoop);
                //We have the admin username and password
                adminDetails = auth.validateAdmin(adminUsername, adminPassword);
                if (adminDetails == null)
                { 
                    Console.WriteLine("Incorrect username or password"); 
                    Console.ReadLine(); Console.Clear();
                    authorise = false;
                }
                else if(adminDetails.getAuthToken() != auth.ValidateAuthToken()) 
                { 
                    Console.WriteLine("Authorisation error occurred");
                    Console.ReadLine(); Console.Clear(); 
                    adminDetails.setAdminDetails(null, null);
                    authorise = false;
                }
                else { authorise = true; }
            } while (!authorise);
            log.Info(adminDetails.getName() + " logged in");
            Console.Clear();
        }
    }
    
}
