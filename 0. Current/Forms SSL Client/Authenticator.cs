using System;
using System.IO;
using System.Security.Cryptography;
namespace Forms_SSL_Client
{
    class Authenticator
    {
        Aes aesKey;
        byte[] key;
        byte[] IV;
        public Authenticator()
        {
            aesKey = Aes.Create();
            key = aesKey.Key;
            IV = aesKey.IV;
            //aesKey.Padding = PaddingMode.PKCS7;
        }
        public string encryptMessage(string message)
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
        public string decryptMessage(string message)
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
        public byte[] getEncryptedAesKey(string key)
        {
            var pubCSP = new RSACryptoServiceProvider();
            pubCSP.FromXmlString(key);
            byte[] byteEncryptedAES = pubCSP.Encrypt(aesKey.Key, RSAEncryptionPadding.Pkcs1);
            return byteEncryptedAES;
        }
        public byte[] getEncryptedAesIV(string key)
        {
            string iv;
            var pubCSP = new RSACryptoServiceProvider();
            pubCSP.FromXmlString(key);
            byte[] byteEncryptedAES = pubCSP.Encrypt(aesKey.IV, RSAEncryptionPadding.Pkcs1);
            return byteEncryptedAES;
        }
    }
}
