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
        protected List<AdminLoginAuthentication> ALAUTH = new List<AdminLoginAuthentication>();
        protected List<AccountData> accountData = new List<AccountData>();
        public AuthenticationInformation()
        {
            UserAuthInfo();
            ServerAuthInfo();
        }
        private void UserAuthInfo()
        {
            MongoClient dsClient = new MongoClient("mongodb://ServerAccess:k2KNmJpNUlvGtNJG@accounts-shard-00-00.rhuha.mongodb.net:27017," +
             "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");

            IMongoDatabase dab = dsClient.GetDatabase("accountsDB");
            var AccountDetails = dab.GetCollection<BsonDocument>("account");
            var documents = AccountDetails.Find(new BsonDocument()).ToList();
            foreach (BsonDocument item in documents)
            {
                // BsonSerializer.Deserialize<AccountData>(item);
                AccountData ad = new AccountData(item);
                accountData.Add(ad);
            }
        }
        private void ServerAuthInfo()
        {
            MongoClient dbClient = new MongoClient("mongodb://ServerAccess:k2KNmJpNUlvGtNJG@accounts-shard-00-00.rhuha.mongodb.net:27017," +
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
        public bool validateUser(string email, string hashedPassword, string sessionSalt)
        {
            return true;
        }
        public AccountData ClientLookupRequest(string email)
        {
            AccountData ad = null;
            foreach (AccountData item in accountData)
            {
                if (item.getEmail() == email)
                {
                    ad = item;
                }
            }
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
                }
            }
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
                return true;
            }
            else //if account already logged in
            {
                return false;
            }
        }
        private void UpdateUserDatabase(AccountData ad)
        {
            MongoClient dsClient = new MongoClient("mongodb://ServerAccess:k2KNmJpNUlvGtNJG@accounts-shard-00-00.rhuha.mongodb.net:27017," +
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
                if (account.getAdminUser() == aLog.getAdminUser()){ return false; }
            }

            MongoClient dbClient = new MongoClient("mongodb://ServerAccess:k2KNmJpNUlvGtNJG@accounts-shard-00-00.rhuha.mongodb.net:27017," +
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
            ServerDetails.InsertOneAsync(document);
            ALAUTH.Add(aLog);
            return true;
        }
    }
}