using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_SSL_Server
{
    class AuthenticationInformation
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("AuthenticationInformation.cs");
        protected List<AdminLoginAuthentication> ALAUTH = new List<AdminLoginAuthentication>();
        protected List<AccountData> accountData = new List<AccountData>();
        public AuthenticationInformation()
        {
            UserAuthInfo();
            log.Info("Users setup");
            ServerAuthInfo();
            log.Info("Admins setup");
        }
        private void UserAuthInfo()
        {
            MongoClient dsClient = new MongoClient("mongodb://ServerAccess:2vLfydinhC3csldJ@accounts-shard-00-00.rhuha.mongodb.net:27017," +
             "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase dab = dsClient.GetDatabase("accountsDB");
            var AccountDetails = dab.GetCollection<BsonDocument>("account");
            var documents = AccountDetails.Find(new BsonDocument()).ToList();
            foreach (BsonDocument item in documents)
            {
                AccountData ad = new AccountData(item);
                accountData.Add(ad);
            }
            
        }
        private void ServerAuthInfo()
        {
            MongoClient dbClient = new MongoClient("mongodb://ServerAccess:2vLfydinhC3csldJ@accounts-shard-00-00.rhuha.mongodb.net:27017," +
            "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase db = dbClient.GetDatabase("accountsDB");
            var ServerDetails = db.GetCollection<BsonDocument>("server");
            var documents = ServerDetails.Find(new BsonDocument()).ToList();
            foreach (BsonDocument item in documents)
            {
                AdminLoginAuthentication alog = new AdminLoginAuthentication(item);
                ALAUTH.Add(alog);
            }
        }
        public AccountData ClientLookupRequest(string email)
        {
            AccountData ad = null;
            foreach (AccountData item in accountData)
            {
                if (item.getEmail() == email)
                {
                    ad = item;
                    return ad;
                }
            }
            log.Info("ClientLookupRequest no results found");
            return ad;
        }
        public AdminLoginAuthentication searchServerAuth(string adminUserName)
        {
            AdminLoginAuthentication aLog = null;
            foreach (AdminLoginAuthentication item in ALAUTH)
            {
                if (item.getAdminUser() == adminUserName)
                {
                    aLog = item;
                    return aLog;
                }
            }
            log.Info("SearchServerAuth no results found");
            return aLog;
        }
        public bool LogInUser(AccountData ad)
        {
            int index = accountData.IndexOf(ad);
            AccountData reviewAccount = accountData[index];
            if (reviewAccount.getLoggedIn() == false) //If no account logged in under that account
            {
                reviewAccount.setLoggedIn(true); //Mark that account has logged in 
                accountData[index] = reviewAccount;
                log.Info("User successfully logged in");
                return true;
            }
            else //if account already logged in
            {
                log.Info("User already logged in");
                return false;
            }
        }
        public void LogClientOut(AccountData ad)
        {
            int index = accountData.IndexOf(ad);
            AccountData review = accountData[index];
            if (review.getLoggedIn() == true)
            {
                review.setLoggedIn(false);
                accountData[index] = review;
                log.Info("User successfully logged out");
            }
        }
        private void UpdateUserDatabase(AccountData ad)
        {
            MongoClient dsClient = new MongoClient("mongodb://ServerAccess:2vLfydinhC3csldJ@accounts-shard-00-00.rhuha.mongodb.net:27017," +
                                     "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase dab = dsClient.GetDatabase("accountsDB");
            var AccountDetails = dab.GetCollection<BsonDocument>("account");
            var document = new BsonDocument
            {
                {"Email", ad.getEmail() },
                {"Hash", ad.getHash() },
                {"Salt", ad.getSalt() },
                {"Name", ad.getName() }
            };
            AccountDetails.InsertOneAsync(document); //Update database
            log.Info("User database updated");
        }
        public void RegNewAccount(AccountData ad)
        {
            accountData.Add(ad);
            UpdateUserDatabase(ad);
        }
        public bool RegAdminAccount(AdminLoginAuthentication aLog)
        {
            foreach (var account in ALAUTH)
            {
                //if accounts exists then no account will be added
                if (account.getAdminUser() == aLog.getAdminUser())
                {
                    Console.WriteLine("Account exists already");
                    return false;
                }
            }

            MongoClient dbClient = new MongoClient("mongodb://ServerAccess:2vLfydinhC3csldJ@accounts-shard-00-00.rhuha.mongodb.net:27017," +
            "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase db = dbClient.GetDatabase("accountsDB");
            var ServerDetails = db.GetCollection<BsonDocument>("server");
            var document = new BsonDocument
            {
                {"adminPassword", aLog.getHashedPassword() },
                {"adminSalt", aLog.getAdminSalt() },
                {"adminName", aLog.getAdminName() },
                {"adminUser", aLog.getAdminUser() },
            };
            ServerDetails.InsertOne(document);
            ALAUTH.Add(aLog);
            log.Info("New admin account added");
            return true;
        }
        public void ShowUsers() //Indexs and shows all users in database
        {
            UInt64 count = 1;
            foreach (var item in accountData)
            {
                Console.WriteLine("["+count+"] "+item.getEmail());
                count++;
            }
        }
        public void DeleteUser()
        {
            bool correctInput = false;
            int acceptableInputRange = accountData.Count;
            string strInput;
            int indexToRemove = -1;
            do
            {
                ShowUsers();
                Console.Write("Enter index to remove: ");
                strInput = Console.ReadLine();
                try
                {
                    indexToRemove = int.Parse(strInput);
                    if (indexToRemove > 0 && indexToRemove <= acceptableInputRange)
                    {
                        correctInput = true;
                    }
                }
                catch (Exception){}//LOG
                if (correctInput == false)
                {
                    Console.WriteLine("Illegal input");
                }
            } while (!correctInput);

            MongoClient dsClient = new MongoClient("mongodb://ServerAccess:2vLfydinhC3csldJ@accounts-shard-00-00.rhuha.mongodb.net:27017," +
             "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase dab = dsClient.GetDatabase("accountsDB");
            var AccountDetails = dab.GetCollection<BsonDocument>("account");
            var deleteFilter = Builders<BsonDocument>.Filter.Eq("Email", accountData[indexToRemove - 1].getEmail());
            AccountDetails.DeleteOne(deleteFilter);
            log.Info("User removed from database");
            accountData.RemoveAt(indexToRemove - 1);
        }
        public void ShowAdmin()
        {
            int count = 1;
            foreach (var item in ALAUTH)
            {
                Console.WriteLine("[" + count + "] " + item.getAdminUser());
                count++;
            }
        }
        public void DeleteAdmin() 
        {
            bool correctInput = false;
            int acceptableInputRange = ALAUTH.Count;
            string strInput;
            int indexToRemove = -1;
            do
            {
                ShowAdmin();
                Console.Write("Enter index to remove: ");
                strInput = Console.ReadLine();
                try
                {
                    indexToRemove = int.Parse(strInput);
                    if (indexToRemove > 0 && indexToRemove <= acceptableInputRange)
                    {
                        correctInput = true;
                    }
                }
                catch (Exception) { }//LOG
                if (correctInput == false)
                {
                    Console.WriteLine("Illegal input");
                }
            } while (!correctInput);
            MongoClient dbClient = new MongoClient("mongodb://ServerAccess:2vLfydinhC3csldJ@accounts-shard-00-00.rhuha.mongodb.net:27017," +
            "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase db = dbClient.GetDatabase("accountsDB");
            var ServerDetails = db.GetCollection<BsonDocument>("server");
            var DeleteFilter = Builders<BsonDocument>.Filter.Eq("adminUser", ALAUTH[indexToRemove - 1].getAdminUser());
            ServerDetails.DeleteOne(DeleteFilter);
            log.Info("Admin removed from database");
            ALAUTH.RemoveAt(indexToRemove - 1);
        }
    }
}