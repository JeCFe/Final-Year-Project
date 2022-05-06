using System;
using System.IO;
using System.Threading;


[assembly: log4net.Config.XmlConfigurator(Watch = true)] //Watch ensure if the config is changed then the program will react in realtime

namespace New_SSL_Server
{
    /* 
        This class has the responible of being where the program will initally boot into 
        First the authenticator and thus the Authenticator info classes are initalised 
        - This means log in details for both user and admin are brought into the system
        Second an admin is required to log into the system
        Finally when an admin is logged in theyre presented with various options, including starting the server thread
    */

    class Program
    {
        //Initalises the logger for this file
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Program.cs");

        //Creates variious objects required for program operation
        private static Authenticator auth;
        private static int ServerPort;
        private static readonly InputValidation valid = new InputValidation();
        private static AdminDetails adminDetails;

        private static ConsoleText View = new ConsoleText(adminDetails);

        //Variables required to start the server thread passing the Authenticator object as an argument
        private static Server server = new Server();
        private static Thread serverThread = new Thread(delegate () {  server.ServerStarter(auth, ServerPort);  });
        static void Main()
        {

            auth = new Authenticator();
            auth.Initalise();
            bool endLoop = true;

            //This loop ensure that a correct admin is logged into the system
            //the loop continue until a correct admin and a mathcing auth token are presented 
            //After correct admin has logged in the primary admin window will present
            do
            {
                AdminLogin();
                if (adminDetails.getAuthToken() == auth.ValidateAuthToken()) { View = new ConsoleText(adminDetails); AdminMenu(); }
                else { log.Error("Admin auth token error"); endLoop = false; }
            } while (!endLoop);

        }

        /*
            Primary menu for the admin 
            Using the View object of ConsoleText the menu option are displayed on console 
            Admin can select a menu option and the following switch case will be used to deploy the correct function
         */
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

        /*
         When admin choses to start the server the server thread is started and last action updated
         */
        private static void ServerStart()
        {
            try
            {
                bool isCorrectPort = false;
                do
                {
                    Console.Write("Please Enter Server Port: ");
                    string port = Console.ReadLine();
                    if(int.TryParse(port, out ServerPort)) { isCorrectPort = true; }

                } while (!isCorrectPort);
                serverThread.Start();
                View.SetLastAction("Server Started");
            }
            catch (Exception)
            {
                log.Error("Attempted to deploy server with server instance already running");
            }
        }

        /*
            When admin choses to close the server the following happens
        */
        private static void ServerAbort()
        {
            try
            {
                server.ServerKill(); //Server killer function inside the running server is called, this kills all internal processes
                log.Info("Server killed"); //Log event update
                View.SetLastAction("Server Killed"); //Change last action
                server = new Server(); //Define a new server for when another server instance is started
                serverThread = new Thread(delegate () { server.ServerStarter(auth, ServerPort); }); //Redefine the serverThread for when a new server instance is called
            }
            catch (Exception)
            {
                log.Error("Attempted to stop a server with no server instance running");
            }

        }

        /*
         This function is used for when an admin choses to add an additional admin account
        */
        private static void AddAdmin()
        {
            string name, password, username;
            bool endLoop;
            do
            {
                View.Clear();
                View.Header();
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
                AdminRegDetails ARD = new AdminRegDetails
                {
                    name = name,
                    username = username,
                    password = password
                };
                if (auth.RegAdmin(ARD)) { Console.WriteLine("Admin Account Added"); endLoop = true; }
                else { Console.WriteLine("Error occurred"); endLoop = false; }
                if (!endLoop)
                {
                    Console.Write("Do you wish to try again Y/N: ");
                    string input = Console.ReadLine();
                    if (input == "N") { endLoop = true; }
                }
            } while (!endLoop);
            View.SetLastAction("Admin Added");
        }

        /*
         This function uses the authentication object to delete an admin from the authentication info / databases
         Updates the last action
        */
        private static void RemoveAdmin() { auth.DeleteAdmin(); View.SetLastAction("Admin deleted"); }

        /*
         This function uses the authentication object to remove a user from the authenticaion info / databases 
         These changes are refelected when the server is restarted and will have no impact on the current running server
         The last action is then updated
        */
        private static void RemoveUser() { auth.DeleteUser(); View.SetLastAction("User deleted"); }

        /*
         Uses the ConsoleView object to display available log options 
         Take an admin choice of what log to view
         Displays that log
        */
        private static void ShowLogs()
        {
            bool isLoopEnd = false;
            int menuInput;
            FileInfo filePath = View.ShowLogList();
            do
            {
                string choice = View.ShowLogMenu(filePath.Name);
                if (int.TryParse(choice, out menuInput))
                {
                    menuInput = int.Parse(choice);
                    int menuLowerBound = 0;
                    int menuUpperBound = 4;
                    if (menuInput > menuLowerBound && menuInput <= menuUpperBound){ isLoopEnd = true; }
                }
            } while (!isLoopEnd);
            View.ShowLogMenuResult(menuInput, filePath.FullName);
        }

        /*
            This function is vital for the secure log in of an admin
            Required the use of the autheticator class and take a username and password from the admin
            Uses validation to ensure the credabilitiy of the inputted name and password to ensure they match expected input styles 
            Will validate the given details against saved details
            Will ensure the auth token for the admin matches the auth token on the server
         */
        private static void AdminLogin()
        {
            bool endLoop = false;
            bool authorise;
            do
            {
                View.Intro(); //Displays intro
                string adminUsername;
                do
                {
                    Console.Write("Enter admin username: ");
                    adminUsername = Console.ReadLine();
                    //Will use the validation class to ensure the name matches expect inputs
                    if (!valid.NameValidation(adminUsername)) { Console.WriteLine("Incorrect format entered!"); }
                    else { endLoop = true; }
                } while (!endLoop); //Loop ends if the input name matches regrex
                endLoop = false;

                string adminPassword;
                do
                {
                    Console.Write("Enter password: ");
                    adminPassword = Console.ReadLine();
                    //Will use the validation class to ensure the password follows the password policy
                    if (!valid.PasswordValidation(adminPassword)) { Console.WriteLine("Incorrect format entered!"); }
                    else { endLoop = true; }
                } while (!endLoop); //Loop ends if the input password follows the password policy

                //Using the auth class will determine if the given login details match those on record
                //Returns the adminDetails of that account
                //If returned adminDetails returns as null this means the login details were incorrect
                adminDetails = auth.ValidateAdmin(adminUsername, adminPassword);

                if (adminDetails == null) //Incorrect login details
                {
                    Console.WriteLine("Incorrect username or password");
                    Console.ReadLine(); Console.Clear();
                    authorise = false;
                }

                //Ensure the auth token match across the board
                //If they do not match then the adminDetails are set toull
                else if (adminDetails.getAuthToken() != auth.ValidateAuthToken())
                {
                    Console.WriteLine("Authorisation error occurred");
                    log.Error("Autheorisation error");
                    Console.ReadLine();
                    Console.Clear();
                    adminDetails.setAdminDetails(null, null);
                    authorise = false;
                }
                else { authorise = true; }
            } while (!authorise); //Loops ends if a valid admin has logged in
            log.Info(adminDetails.getName() + " logged in"); //Displays logged in admin name
            Console.Clear();
            View.SetLastAction("Admin Login"); //Sets last action as that admin logged in
        }
    }

}
