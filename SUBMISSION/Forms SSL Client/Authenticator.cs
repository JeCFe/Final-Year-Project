using System;
using System.IO;
using System.Security.Cryptography;
namespace Forms_SSL_Client
{
    /*
     This class is used for the both AES and RSA encryption and decryption
    */
    class Authenticator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Authenticator.cs");
        Aes aesKey;
        byte[] key;
        byte[] IV;

        //Constructor that auto generates creates and assigns and AES key and IV 
        public Authenticator()
        {
            aesKey = Aes.Create();
            key = aesKey.Key;
            IV = aesKey.IV;
            //aesKey.Padding = PaddingMode.PKCS7;
        }

        /*
        This function is responsible for handling AES encryption
        Using AESManaged, memorystream, cryptostream and streamwrither the message is able to be encrypted 
        The encrypter is set up using the AES key and IV on record for this client
        The cipher text is converted from a byte[] to a string and returned
        */
        public string EncryptMessage(string message)
        {
            try
            {
                byte[] encryptedData;
                string cipherText = "";
                using (AesManaged aes = new AesManaged())
                {
                    ICryptoTransform encryptor = aes.CreateEncryptor(aesKey.Key, aesKey.IV);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs))
                            {
                                sw.Write(message);
                            }
                            encryptedData = ms.ToArray();
                            cipherText = Convert.ToBase64String(encryptedData);
                        }
                    }
                }
                return cipherText;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /*
        This function handles message AES decryption 
        This follows a similar process to that of the encrypter 
        But with a decrypter being created 
        The plain text is returned
        */
        public string DecryptMessage(string message)
        {
            try
            {
                byte[] encryptedData = Convert.FromBase64String(message);
                string plainText = "";
                using (AesManaged aes = new AesManaged())
                {
                    ICryptoTransform decryptor = aes.CreateDecryptor(aesKey.Key, aesKey.IV);
                    using (MemoryStream ms = new MemoryStream(encryptedData))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cs))
                            {
                                plainText = reader.ReadToEnd();
                            }
                        }
                    }
                }
                return plainText;
            }
            catch (Exception)
            {
                return message;
                //error log
            }
        }
        //This function is used to encrypt the AES key using the server provided RSA public
        public byte[] GetEncryptedAesKey(string key)
        {
            var pubCSP = new RSACryptoServiceProvider();
            pubCSP.FromXmlString(key);
            byte[] byteEncryptedAES = pubCSP.Encrypt(aesKey.Key, RSAEncryptionPadding.Pkcs1);
            return byteEncryptedAES;
        }
        //This function is used to encrypt the AES IV using the server provided RSA public
        public byte[] GetEncryptedAesIV(string key)
        {
            string iv;
            var pubCSP = new RSACryptoServiceProvider();
            pubCSP.FromXmlString(key);
            byte[] byteEncryptedAES = pubCSP.Encrypt(aesKey.IV, RSAEncryptionPadding.Pkcs1);
            return byteEncryptedAES;
        }
    }
}
