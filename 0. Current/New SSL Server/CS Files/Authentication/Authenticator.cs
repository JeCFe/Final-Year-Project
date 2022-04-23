using System;
using System.Security.Cryptography;

namespace New_SSL_Server
{
    /*
     This class has the primary responsbility of connecting the server activities to authentication information
     Seperation of the classes is required to ensure that sensitive infomration is not exposed directly to the server 
     This function will preform RSA encryption as well as hashing
     Many functions are used to tunnel request from the server to authenticaion info
    */
    class Authenticator
    {
        //Initalises the logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Authenticator.cs");
        private AuthenticationInformation Ainfo; //Creates Authentication Info object
        private RSAParameters privKey; //Holds the private keys for RSA
        private string stringPubKey; //Holds string public rsa keys
        private readonly AdminDetails adminDetails = new AdminDetails(); //Creates adminDetails object

        /*
         This function can be seen as the sudo constructor
         The reason a constructor is not implemented is because not all instances of Auth class requires the new RSA keys or Ainfo initalisation
        */
        public void Initalise()
        {
            //Assigns and starts the constructor of the Authentication info class
            Ainfo = new AuthenticationInformation();
            ExportKeys(); //Generates RSA keys
        }

        //Used to tunnel requires to Ainfo from server or program classes
        public void DeleteUser() { Ainfo.DeleteUser(); }

        //Used to tunnel requests to Ainfo from server or program classes
        public void DeleteAdmin() { Ainfo.DeleteAdmin(); }

        //This function is used to return the public key string
        public string GetPublicRSA() { return stringPubKey; }

        /*
         This functions holds the primary responsbility of decrypting the handshake message from the client
         This message has been encrytped from the client using the server RSA public key 
         With the message contents being the clients AES key and IV 
         For the decryptions the process is standards using .Net built in CSP Decrypt function  
            With paddings of Pkcs1 
            Returning the decrypted bytes 
         If for whatever reason the decryption fails then a error log is taken and the server throws
            The entire program is closed due to the severity of this error
        */
        public byte[] DecryptAESMessage(byte[] message)
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
            catch (Exception e)
            {
                log.Error(e.ToString());
                log.Fatal("Server crash at point of RSA decrypt for AES message from client");
                Environment.Exit(-1);
                return null;
            }
        }

        //This function returns the auth token held by the logged in admin
        public string ValidateAuthToken() { return adminDetails.getAuthToken(); }

        /*
         This function holds the responsibility for logging in and validating the admin
         Taking the name and passwords as arguemnt given from the program class
            Assinging these details to the adminDetails object held as a private vairable 
         A search within Auth info to ensure that there is an admin account with the same username given as an argument 
            if the return is null this means that there is no matching admin account
            And a log is made of an failed attempted log and null is returned
         If the return is not null this means that there is a valid admin account to compare too
            The given arguemnt password is hashed using the salt helded within the saved admin account
                If the newly hashed argument password matches the password on record this means that the admin can continue to log in
                The system auth token is equal to hashed return of the admin hashed password and a newly generated salt
                This auth token is set within the adminDetails and held with the auth for later comparrision 
                The admin details are then returned
        */
        public AdminDetails ValidateAdmin(string username, string password)
        {
            string AuthToken = "";
            string adminName = "";
            adminDetails.setAdminDetails(adminName, AuthToken);
            AdminLoginAuthentication aLog = Ainfo.SearchServerAuth(username);
            if (aLog != null)
            {
                string hashedPassword = PreformPBKDF2Hash(password, Convert.FromBase64String(aLog.getAdminSalt()));
                if (aLog.getHashedPassword() == hashedPassword)
                {
                    AuthToken = PreformPBKDF2Hash(hashedPassword, GenerateSalt());
                    adminDetails.setAdminDetails(aLog.getAdminName(), AuthToken);
                    return adminDetails;
                }
            }
            log.Info("Invalid admin log in attempt");
            return null;
        }

        /*
         This function is vitally used within the program to hash passwords for either comparision to saved passwords or for storage
         This PBKDF2 uses 10000 iterations, higher iterations more secure but longer time is taken to hash the password
        */
        public string PreformPBKDF2Hash(string accountHashedPassword, byte[] salt)
        {
            int iterations = 10000;
            Rfc2898DeriveBytes hash = new Rfc2898DeriveBytes(accountHashedPassword, salt, iterations);
            return Convert.ToBase64String(hash.GetBytes(128));

        }

        //This functions looks up an email address from Ainfo and returns the related account
        public AccountData UserLookupRequest(string email)
        {
            AccountData ad = Ainfo.ClientLookupRequest(email);
            return ad;
        }

        //This function is a tunnel from another class to Auth info and is used to log in a user
        public bool LogUserIn(AccountData ad)
        {
            return Ainfo.LogInUser(ad);
        }

        //This functions generates a 128 byte salt and returns the array
        public byte[] GenerateSalt()
        {
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] salt = new byte[128];
            random.GetBytes(salt);
            return salt;
        }

        //This functions is a tunnel from Program or Server class to Ainfo to add a new user
        public void RegNewAccount(AccountData ad) { Ainfo.RegNewAccount(ad); }

        /*
          This functions is used to register a new admin into the database 
          Convering the AdminRegDetails into an AdminLoginAuthentications 
          This means adding a salt into the details and hashing the plain text password with the new salt into a hashed password
          Using Ainfo to add to relevant list and database table and returning the response from Ainfo
        */
        public bool RegAdmin(AdminRegDetails ARD)
        {
            AdminLoginAuthentication aLog = new AdminLoginAuthentication();
            aLog.setAdminName(ARD.name);
            aLog.setAdminUser(ARD.username);
            aLog.setAdminSalt(Convert.ToBase64String(GenerateSalt()));
            aLog.setHashedPassword(PreformPBKDF2Hash(ARD.password, Convert.FromBase64String(aLog.getAdminSalt())));
            return Ainfo.RegAdminAccount(aLog);
        }

        //This function uses Ainfo to update the client logged in flag to false
        public void LogClientOut(AccountData Client) { Ainfo.LogClientOut(Client); }

        /*
         This function is responsible for the generation of the server RSA 2048 bit keys
         While we can use the SSL cert RSA keys the design decision was made to make custom keys 
            This is so we can show custom RSA/AES key exchanges working
        The private key is save as default RSAParamaters
        The public key is converted to XML string for transmission to clients
        */
        private void ExportKeys()
        {
            var csp = new RSACryptoServiceProvider(2048);
            privKey = csp.ExportParameters(true);
            stringPubKey = csp.ToXmlString(false);
        }
    }
}
