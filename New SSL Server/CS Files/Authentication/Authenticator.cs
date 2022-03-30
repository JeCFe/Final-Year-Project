using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace New_SSL_Server
{
    class Authenticator
    {
        private AuthenticationInformation Ainfo;
        private RSAParameters pubKey;
        private RSAParameters privKey;
        private string stringPubKey;
        private AdminDetails adminDetails = new AdminDetails();
        public void Initalise()
        {
            Ainfo = new AuthenticationInformation();
            exportKeys();
        }
        public AdminDetails getAdminDetails() { return adminDetails; }
        private void exportKeys()
        {
            var csp = new RSACryptoServiceProvider(2048);
            privKey = csp.ExportParameters(true);
            pubKey = csp.ExportParameters(false);
            stringPubKey = csp.ToXmlString(false);
        }
        public string getPublicRSA()
        {
            return stringPubKey;
        }
        public byte[] decryptAESMessage(byte[] message)
        {
            try
            {

                var privCSP = new RSACryptoServiceProvider();
                var pubCSP = new RSACryptoServiceProvider();
                pubCSP.FromXmlString(stringPubKey);
                privCSP.ImportParameters(privKey);
                byte[] decryptedBytes = privCSP.Decrypt(message, RSAEncryptionPadding.Pkcs1);

                return decryptedBytes;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string ValidateAuthToken() { return adminDetails.getAuthToken(); }
        public AdminDetails validateAdmin(string username, string password)
        {
            string AuthToken = "";
            string adminName = "";
            adminDetails.setAdminDetails(adminName, AuthToken);
            AdminLoginAuthentication aLog = Ainfo.searchServerAuth(username);
            if (aLog != null)
            {
                string hashedPassword = PreformPBKDF2Hash(password, Convert.FromBase64String(aLog.getAdminSalt()));
                if (aLog.getHashedPassword() == hashedPassword)
                {
                    AuthToken = PreformPBKDF2Hash(hashedPassword, GenerateSalt());
                    adminDetails.setAdminDetails(aLog.getAdminName(), AuthToken);
                }
            }

            return adminDetails;
        }
        public string PreformPBKDF2Hash(string accountHashedPassword, byte[] salt)
        {
            int iterations = 10000;
            Rfc2898DeriveBytes hash = new Rfc2898DeriveBytes(accountHashedPassword, salt, iterations);
            return Convert.ToBase64String(hash.GetBytes(128));

        }
        public AccountData UserLookupRequest(string email)
        {
            //This will need to be mutex locked
            AccountData ad = Ainfo.ClientLookupRequest(email);
            return ad;
        }
        public bool LogUserIn(AccountData ad)
        {
            return Ainfo.LogInUser(ad);
        }
        public byte[] GenerateSalt()
        {
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] salt = new byte[128];
            random.GetBytes(salt);
            return salt;
        }
        public void RegNewAccount(AccountData ad)
        {
            Ainfo.RegNewAccount(ad);
        }
        public bool RegAdmin(AdminRegDetails ARD)
        {
            AdminLoginAuthentication aLog = new AdminLoginAuthentication();
            aLog.setAdminName(ARD.name);
            aLog.setAdminUser(ARD.username);
            aLog.setAdminSalt(Convert.ToBase64String(GenerateSalt()));
            aLog.setHashedPassword(PreformPBKDF2Hash(ARD.password, Convert.FromBase64String(aLog.getAdminSalt())));
            return Ainfo.RegAdminAccount(aLog);
        }
    }
}
