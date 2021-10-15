using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace newRSA
{
    class RSAClient
    {
        protected RSAParameters privKey;
        protected RSAParameters pubKey;
        public RSAClient()
        {
            RSA rsa = RSA.Create(2048);
            privKey = rsa.ExportParameters(true);
            pubKey = rsa.ExportParameters(false);
        }
        public string EncryptMessage(string message)
        {
            UnicodeEncoding byteConverter = new UnicodeEncoding();
            byte[] messageToEncrypt = byteConverter.GetBytes(message);
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.ImportParameters(pubKey);
            byte[] cypherKey = RSA.Encrypt(messageToEncrypt, false);
            return byteConverter.GetString(cypherKey);
        }
        public string DecryptMessage(string message)
        {
            UnicodeEncoding byteConverter = new UnicodeEncoding();
            byte[] messageToDecrypt = byteConverter.GetBytes(message);
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.ImportParameters(privKey);
            byte[] plainText = RSA.Encrypt(messageToDecrypt, false);
            return byteConverter.GetString(plainText);
        }
        public RSAParameters GetPublicKey()
        {
            return pubKey;
        }
    }
    class Program
    {
        public static RSAClient clientKeys = new RSAClient();
        static void Main(string[] args)
        {
            string message = clientKeys.EncryptMessage("Hello");
            Console.WriteLine(message);
            string dMessage = clientKeys.DecryptMessage(message);
            Console.WriteLine(dMessage);
        }

        
    }
}
