using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_SSL_Server
{
    class ConsoleText
    {
        private static AdminDetails AdminDets = new AdminDetails();
        private static string lastAction = "";

        public ConsoleText(AdminDetails ad) { AdminDets = ad; }
        public void setLastAction(string action) { lastAction = action; }
        private static void Header()
        {
            Console.WriteLine("You are currently Logged in as: " + AdminDets.getName());
            if (lastAction !="")
            {
                Console.WriteLine("Last Action commited: " + lastAction);
            }
        }
        public void Intro()
        {
            Console.WriteLine("Welcome to the chat server!");
            Console.WriteLine("An admin is required to further access these systems");
            Console.WriteLine("Your admin details will now be taken");
        }
        public int Menu()
        {
            int choice = 0;
            string input = "";
            bool endLoop = false;
            do
            {
                Header();
                Console.WriteLine("1. Start Server");
                Console.WriteLine("2. End Server");
                Console.WriteLine("3. Add Admin");
                Console.WriteLine("4. Remove Admin");
                Console.WriteLine("5. Add User");
                Console.WriteLine("6. Remove User");
                Console.WriteLine("7. View Logs");
                Console.Write("Enter option: ");
                input = Console.ReadLine();
                if (int.TryParse(input, out choice))
                {
                    if (choice >= 1 && choice <= 7) { endLoop = true; }
                }
                else { endLoop = false; }
            } while (!endLoop);
            return choice;

        }
        public void Clear() { Console.Clear(); }
    }
}
