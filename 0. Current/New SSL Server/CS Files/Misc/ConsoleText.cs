using System;
using System.IO;

namespace New_SSL_Server
{
    //This class handles admin view and main displaying to console
    class ConsoleText
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("ConsoleText.cs");
        private static AdminDetails AdminDets = new AdminDetails();
        private static string lastAction = "";

        //This function is used to set the current admin detials
        public ConsoleText(AdminDetails ad) { AdminDets = ad; }

        //This function is used to set the last action preformed by the admin
        public void SetLastAction(string action) { lastAction = action; }

        //This function is used to display the header above the menu options displaing the admin name and last action
        public void Header()
        {
            Console.WriteLine("You are currently Logged in as: " + AdminDets.getName());
            if (lastAction != "")
            {
                Console.WriteLine("Last Action commited: " + lastAction);
            }
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
        public void ShowLogList()
        {
            Header();
            string[] fileEntries = Directory.GetFiles("..\\Debug\\logs");
            int count = 1;
            int input = 0;
            foreach (string file in fileEntries)
            {
                //DirectorInfo.Name will show the name of the file rather than the entire path
                Console.WriteLine("[" + count + "] " + new DirectoryInfo(file).Name);
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
            ShowLogs(fileEntries[input - 1]);
        }

        /*
         This function shows the contents of the chosen log
         Using FileStream and setting the FileShare to write means that the logger is able to write to the file while reading 
         In testing this proved to invoke no race condition errors
         Each line is displayed onto the screen when the admin has read they can press anykey and be returned to the main menu
        */
        private void ShowLogs(string path)
        {
            try
            {
                Clear();
                Header();
                Console.WriteLine();
                using (FileStream fileStream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Write))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        SetLastAction("View logs");
                        Console.WriteLine(streamReader.ReadToEnd());
                        Console.WriteLine("Press any key to continue");
                        Console.Read();
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Error displaying logs. Error message: " + e.ToString());
            }
        }

        //This function simply clears the screen
        public void Clear() { Console.Clear(); }
    }
}
