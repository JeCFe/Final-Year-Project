using System;
using System.IO;
using System.Security.Cryptography;

namespace Encrypting_AES_using_RSA
{
    class Program
    {
        static void Main(string[] args)
        {
            var csp = new RSACryptoServiceProvider(2048);
            var privKey = csp.ExportParameters(true);
            var pubKey = csp.ExportParameters(false);

            var pubCSP = new RSACryptoServiceProvider();
            pubCSP.ImportParameters(pubKey);
            string test = pubKey.ToString();

            var privCSP = new RSACryptoServiceProvider();
            privCSP.ImportParameters(privKey);


            Console.WriteLine("INPUT MESSAGE");
            string message = Console.ReadLine();
            Aes aesKey = Aes.Create();
            Console.WriteLine(System.Text.Encoding.Default.GetString(aesKey.Key));
            byte[] encryptedAESKey = pubCSP.Encrypt(aesKey.Key, RSAEncryptionPadding.Pkcs1);
            byte[] encryptedAESIV = pubCSP.Encrypt(aesKey.IV, RSAEncryptionPadding.Pkcs1);
            string strEAesKey = System.Text.Encoding.Default.GetString(encryptedAESKey);
            string strEAesIV = System.Text.Encoding.Default.GetString(encryptedAESIV);
            Console.WriteLine(strEAesKey);
            Console.WriteLine(strEAesIV);

            byte[] decryptedAESKey = privCSP.Decrypt(encryptedAESKey, RSAEncryptionPadding.Pkcs1);
            byte[] decryptedAESIV = privCSP.Decrypt(encryptedAESIV, RSAEncryptionPadding.Pkcs1);
            string strDAesKey = System.Text.Encoding.Default.GetString(decryptedAESKey);
            string strDAesIV = System.Text.Encoding.Default.GetString(decryptedAESIV);
            Console.WriteLine(strDAesKey);
            Console.WriteLine(strDAesIV);
            byte[] encrypted = EncryptStringToBytes_Aes(message, aesKey.Key, aesKey.IV);

            string plainText = DecryptStringFromBytes_Aes(encrypted, aesKey.Key, aesKey.IV);
            Console.WriteLine(plainText);


        }
        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}


