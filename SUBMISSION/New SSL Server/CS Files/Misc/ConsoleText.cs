using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace New_SSL_Server
{
    //This class handles admin view and main displaying to console
    class ConsoleText
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("ConsoleText.cs");
        private static AdminDetails AdminDets = new AdminDetails();
        private static FileHandler FileHandler = new FileHandler();
        private static string lastAction = "";
        private static string lastError = "";

        //This function is used to set the current admin detials
        public ConsoleText(AdminDetails ad) { AdminDets = ad; }



        //This function is used to set the last action preformed by the admin
        public void SetLastAction(string action) { lastAction = action; }


        //This function is used to display the header above the menu options displaing the admin name and last action
        public void Header()
        {
            Console.WriteLine("You are currently Logged in as: " + AdminDets.getName());
            Console.WriteLine("Last Action committed: " + lastAction);
            Console.WriteLine("Last error: " + FileHandler.LastError());
        }

        //This function displays an intro to the server
        public void Intro()
        {
            Console.WriteLine("Welcome to the chat server!");
            Console.WriteLine("An admin is required to further access these systems");
            Console.WriteLine("Your admin details will now be taken");
        }

        /*
         This function is used to display to display menu options to the admin
         Error checks are made to ensure admin makes a valid choice
         Returns choice
        */
        public int Menu()
        {
            bool endLoop = false;
            int choice;
            do
            {
                Clear();
                Header();
                Console.WriteLine("1. Start Server");
                Console.WriteLine("2. End Server");
                Console.WriteLine("3. Add Admin");
                Console.WriteLine("4. Remove Admin");
                Console.WriteLine("5. Remove User");
                Console.WriteLine("6. View Logs");
                Console.Write("Enter option: ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out choice))
                {
                    if (choice >= 1 && choice <= 7) { endLoop = true; }
                }
                else { endLoop = false; }
            } while (!endLoop);
            return choice;

        }

        /*
         This function is used to display all logs on record by date order
         Displaing with an index for admin to choose
         Error checks to ensure admin made a valid choice
         When admin has made valid choice will call ShowLogs passing the chosen path
        */
        public FileInfo ShowLogList()
        {
            Header();
            List<FileInfo> fileEntries = FileHandler.LogEntries();
            int count = 1;
            int input = 0;
            foreach (FileInfo file in fileEntries)
            {
                //DirectorInfo.Name will show the name of the file rather than the entire path
                Console.WriteLine("[" + count + "] " + file.Name);
                count++;
            }
            do
            {
                Console.Write("Enter index of log to view: ");
                string strInput = Console.ReadLine();
                if (int.TryParse(strInput, out input)) { input = int.Parse(strInput); }
            } while (input < 1 || input > count);
            Clear();
            SetLastAction("Show Log List");
            return fileEntries[input - 1];
        }

        public string ShowLogMenu(string path)
        {
            Clear();
            Header();
            Console.WriteLine("Viewing log: " + new DirectoryInfo(path).Name);
            Console.WriteLine("Please select an option from below: ");
            Console.WriteLine("[1] All logs");
            Console.WriteLine("[2] Info Logs");
            Console.WriteLine("[3] Error Logs");
            Console.WriteLine("[4] Fatal Logs");
            Console.Write("Please enter choice: ");
            return Console.ReadLine();
        }

        /*
         This function is used to call use FileHandler to retrieve the correct logs
         Then display them one by one
        */
        public void ShowLogMenuResult(int choice, string path)
        {
            List<string> logsToShow = FileHandler.ReteriveSpecificLogs(choice, path);
            Clear();
            Header();
            foreach (string log in logsToShow){ Console.WriteLine(log); }
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }

        //This function simply clears the screen
        public void Clear() { Console.Clear(); }
    }
}
