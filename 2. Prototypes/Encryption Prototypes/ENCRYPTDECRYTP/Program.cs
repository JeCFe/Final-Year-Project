using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ENCRYPTDECRYTP
{
    class Program
    {

        //ON THE SERVER THE PUBLIC KEY 
        public static string encryptMessage(string cypherText, RSAParameters RSAKeyInfo ) //This requires servers private key
        {
            UnicodeEncoding ByteConverter = new UnicodeEncoding();
            byte[] decryptedByte = null;
            byte[] messageToEncrypt = ByteConverter.GetBytes(cypherText);
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.ImportParameters(RSAKeyInfo);
            decryptedByte = RSA.Encrypt(messageToEncrypt, false);
            return ByteConverter.GetString(decryptedByte);
        }
        public static void GenerateKey(string ContainerName)
        {
            new RSACryptoServiceProvider(new CspParameters { KeyContainerName = ContainerName }) ;
        }
        public static RSAParameters GetPublicKey(CspParameters cp)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp);
            RSAParameters publicRSA = rsa.ExportParameters(false);
            return publicRSA;
        }
        public static RSAParameters GetPrivateKey(CspParameters cp)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp);
            RSAParameters privateKey = rsa.ExportParameters(true);
            return privateKey;
        }
        public static string DecryptMessage(string message, RSAParameters RSAKeyInfo) //Message recieved is encrypted from server using Clients Public key
        {
            UnicodeEncoding ByteConverter = new UnicodeEncoding();
            byte[] messageToEncrypt = ByteConverter.GetBytes(message);
            //byte[] encryptedMessage;
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.ImportParameters(RSAKeyInfo);
            var encryptedMessage = RSA.Decrypt(messageToEncrypt, false); //Used private key to Decrypt Message
            return ByteConverter.GetString(encryptedMessage);
        }
        static void Main(string[] args)
        {
            CspParameters cp = new CspParameters();
            cp.KeyContainerName = "ClientKey";
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(cp);
            
            string Emessage = encryptMessage("testing", RSA.ExportParameters(false));
            Console.WriteLine("E: " + Emessage + "\n");
            string Dmessage = DecryptMessage(Emessage, RSA.ExportParameters(true));

            
            Console.WriteLine("D: " + Dmessage + "\n");

            Console.ReadLine();
        }


        public static void GenerateKeyWithContainer(string ConName)
        {
            RSA rsa = new RSACryptoServiceProvider(new CspParameters {KeyContainerName = ConName });
        }
        public static void DecryptMessage(string ConName)
        {
            RSA rsa = new RSACryptoServiceProvider(new CspParameters { KeyContainerName = ConName });
        }
        public static void EncryptMessage()
        {

        }
    }
}
