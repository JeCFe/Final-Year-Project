using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HashTesting
{
    class Program
    {
        public static byte[] GenSalt()
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            byte[] salt = new byte[128];
            provider.GetBytes(salt);
            return salt;
           
        }
        public static string Hasher(string message, byte[] salt)
        {
            Rfc2898DeriveBytes hash = new Rfc2898DeriveBytes(message, salt, 10000);
            return Convert.ToBase64String(hash.GetBytes(128));
        }

        static void Main(string[] args)
        {
            byte[] accountSalt = GenSalt();
            byte[] sessionSalt = GenSalt();
            Console.WriteLine("Enter password");
            string password = Console.ReadLine();
            string persistantFirstHash = Hasher(password, accountSalt);
            string persistantFinalHash = Hasher(persistantFirstHash, sessionSalt);
            Console.WriteLine("Password hash: " + persistantFinalHash);
            Console.WriteLine("Enter password");
             string loginPassword =  Console.ReadLine();
            string loginFirstHash = Hasher(loginPassword, accountSalt);
            string loginFinalHash = Hasher(loginFirstHash, sessionSalt);
            Console.WriteLine("Test hash: " + loginFinalHash);
            if (persistantFinalHash == loginFinalHash){ Console.WriteLine("Match"); }
            else{ Console.WriteLine("Non Match"); }
            Console.ReadKey();
        }
    }
}
