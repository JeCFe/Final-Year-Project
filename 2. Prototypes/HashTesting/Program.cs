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
            Console.WriteLine("Enter password");
            string password = Console.ReadLine();
            byte[] accountSalt = GenSalt();
            byte[] sessionSalt = GenSalt();
            string firstHash = Hasher(password, accountSalt);
            string finalHash = Hasher(firstHash, sessionSalt);
            Console.WriteLine("Password hash: " + finalHash);

             string test =  Console.ReadLine();
            string fHash = Hasher(test, accountSalt);
            string fiHash = Hasher(fHash, sessionSalt);
            Console.WriteLine("Test hash: " + fiHash);

            if (finalHash == fiHash)
            {
                Console.WriteLine("Match");
            }
            else
            {
                Console.WriteLine("Non Match");
            }
            Console.ReadKey();
        }
    }
}
