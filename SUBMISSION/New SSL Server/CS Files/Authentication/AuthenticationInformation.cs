using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace New_SSL_Server
{
    /*
     This class has the primary responsbility for MongoDB connections 
     Also this class is responsble for the initilsation of the admin and user login details
     Allows for admin or user accounts to be added or removed
    */

    class AuthenticationInformation
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("AuthenticationInformation.cs");
        protected List<AdminLoginAuthentication> ALAUTH = new List<AdminLoginAuthentication>(); //List for admin details
        protected List<AccountData> accountData = new List<AccountData>(); //List for user detials
        public AuthenticationInformation() //Constructer to initalise the admin and user lists
        {
            UserAuthInfo();
            log.Info("Users setup");
            ServerAuthInfo();
            log.Info("Admins setup");
        }

        /*
         This functions connects to MongoDB and gets all the accounts for users 
         Adds each account to the accountData list
        */
        private void UserAuthInfo()
        {
            MongoClient dsClient = new MongoClient("mongodb://ServerAccess:2vLfydinhC3csldJ@accounts-shard-00-00.rhuha.mongodb.net:27017," +
             "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase dab = dsClient.GetDatabase("accountsDB");
            var AccountDetails = dab.GetCollection<BsonDocument>("account");//Defines what table to look at
            var documents = AccountDetails.Find(new BsonDocument()).ToList();//Gets each entry in the account table
            foreach (BsonDocument item in documents)
            {
                AccountData ad = new AccountData(item);
                accountData.Add(ad); //Adds account to list
            }

        }

        /*
         This functions connects to MongoDB and gets all the admin details
         Adds each admin details to the ALAUTH list
        */
        private void ServerAuthInfo()
        {
            MongoClient dbClient = new MongoClient("mongodb://ServerAccess:2vLfydinhC3csldJ@accounts-shard-00-00.rhuha.mongodb.net:27017," +
            "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase db = dbClient.GetDatabase("accountsDB");
            var ServerDetails = db.GetCollection<BsonDocument>("server"); //Defines to look at the table storing admin details
            var documents = ServerDetails.Find(new BsonDocument()).ToList(); //Gets all entires from this table
            foreach (BsonDocument item in documents)
            {
                AdminLoginAuthentication alog = new AdminLoginAuthentication(item);
                ALAUTH.Add(alog); //Adds each admin to the list
            }
        }

        /*
         This function is used to verify whether an email address exists in the accountData list
         Returns an AccountData object
         If AccountData object is null this means no account was found 
         Else the AccountData returns will contain all data relating to the email address being inspected
        */
        public AccountData ClientLookupRequest(string email)
        {
            AccountData ad = null;
            foreach (AccountData item in accountData)
            {
                if (item.GetEmail() == email)
                {
                    ad = item;
                    return ad;
                }
            }
            log.Info("ClientLookupRequest no results found");
            return ad;
        }

        /*
         This function is used to verify whether an admin username exists in the ALAUTH list
         Returns an AdminLoginAuthentication (aLog) object
         If aLog is null this mean no match account was found
         Else returns the account details relating to the admin username
        */
        public AdminLoginAuthentication SearchServerAuth(string adminUserName)
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

        /*
         This function is used to flag as logged in
         This takes a copy of the accountdetails
         If there is no current flags for this account to be logged in then the flag is change to true
            When the account has been updated to being logged in the orignial entry in the list is replaced with the new entry 
            Then returns true
         If the account already has the logged in flag as true when nothing changes and returns false
        */
        public bool LogInUser(AccountData ad)
        {
            int index = accountData.IndexOf(ad);
            AccountData reviewAccount = accountData[index];
            if (reviewAccount.GetLoggedIn() == false) //If no account logged in under that account
            {
                reviewAccount.SetLoggedIn(true); //Mark that account has logged in 
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

        /*
         Similar in function that that of LogInUser but in reverse
         This function will change the logged in flag to false
        */
        public void LogClientOut(AccountData ad)
        {
            int index = accountData.IndexOf(ad);
            AccountData review = accountData[index];
            if (review.GetLoggedIn() == true)
            {
                review.SetLoggedIn(false);
                accountData[index] = review;
                log.Info("User successfully logged out");
            }
        }

        /*
          This function is responsible for the addition of users to MongoDB
          After connection to the server and the correct table being selected 
          The AccountData contents is converted to BsonDocument format 
          The new document is inserted in to the table
        */
        private static void UpdateUserDatabase(AccountData ad)
        {
            MongoClient dsClient = new MongoClient("mongodb://ServerAccess:2vLfydinhC3csldJ@accounts-shard-00-00.rhuha.mongodb.net:27017," +
                                     "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase dab = dsClient.GetDatabase("accountsDB");
            var AccountDetails = dab.GetCollection<BsonDocument>("account");
            var document = new BsonDocument
            {
                {"Email", ad.GetEmail() },
                {"Hash", ad.GetHash() },
                {"Salt", ad.GetSalt() },
                {"Name", ad.GetName() }
            };
            AccountDetails.InsertOneAsync(document); //Update database
            log.Info("User database updated");
        }

        /*
         This function is used to register new account, this is the function call that the auth call uses
         Updates the accountData list and calls UpdateUserDatabase function to update the database
        */
        public void RegNewAccount(AccountData ad)
        {
            accountData.Add(ad);
            UpdateUserDatabase(ad);
        }

        /*
         This function is used to register admin account in both the database and ALAUTH list
         Fist a check is preformed to ensuer that the account being registered doesnt already exist
            If the account exists false is returns to end the process 
         If an account doenst exist the following happens
            Connection establised to MongoDB into the server table that hold admin details
            The aLog details are converted to BsonDocument format
            The details are inserted into the server table on the database
            The admin details are added to the ALAUTH list
            Returns true
        */
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

        /*
         This function is used to display all email addresses from the accountData list with and index starting at 1 
        */
        public void ShowUsers()
        {
            UInt64 count = 1;
            foreach (var item in accountData)
            {
                Console.WriteLine("[" + count + "] " + item.GetEmail());
                count++;
            }
        }

        /*
         This function is used to delete a user from both the accountData list and the account table in MongoDB
         First using ShowUsers function to display all users 
         Takes an input for what index to remove 
         Using try catch to validate the int.Parse 
         When correct input is given then the following happens
            Connection to MongoDB account table
            Building filter for entry to delete setting the filter to the email address of the selected index
            Deletes one entry matching the email address in the filter
            Due to accounts requiring unique email address this will delete the correct email
        */
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
                try //This try catch is used to catch when a string is given instead of int
                {
                    indexToRemove = int.Parse(strInput);
                    if (indexToRemove > 0 && indexToRemove <= acceptableInputRange)
                    {
                        correctInput = true;
                    }
                }
                catch (Exception e) { log.Error(e.ToString()); }
                if (correctInput == false)
                {
                    Console.WriteLine("Illegal input");
                }
            } while (!correctInput); //Keeps looping until correct input is given

            MongoClient dsClient = new MongoClient("mongodb://ServerAccess:2vLfydinhC3csldJ@accounts-shard-00-00.rhuha.mongodb.net:27017," +
             "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase dab = dsClient.GetDatabase("accountsDB");
            var AccountDetails = dab.GetCollection<BsonDocument>("account");
            var deleteFilter = Builders<BsonDocument>.Filter.Eq("Email", accountData[indexToRemove - 1].GetEmail());
            AccountDetails.DeleteOne(deleteFilter);
            accountData.RemoveAt(indexToRemove - 1);
            log.Info("User removed from database");
        }

        /*
         Diplays all admins in the ALAUTH list with an index starting at 1 
        */
        public void ShowAdmin()
        {
            int count = 1;
            foreach (var item in ALAUTH)
            {
                Console.WriteLine("[" + count + "] " + item.getAdminUser());
                count++;
            }
        }

        /*
         This function is used to delete a selected admin from both the ALAUTH list and the server table in MongoDB
         This function operates in a similar fashion to DeleteUser following the same process 
            With the exception of using admin usernames instread of email address
        */
        public void DeleteAdmin()
        {
            bool correctInput = false;
            int acceptableInputRange = ALAUTH.Count;
            string strInput;
            int indexToRemove = -1;
            do
            {
                ShowAdmin(); //Displays all admins with index
                Console.Write("Enter index to remove: ");
                strInput = Console.ReadLine();
                try //Try catch to ensure int.parse works correctly
                {
                    indexToRemove = int.Parse(strInput);
                    if (indexToRemove > 0 && indexToRemove <= acceptableInputRange)
                    {
                        correctInput = true;
                    }
                }
                catch (Exception e) { log.Error(e.ToString()); }
                if (correctInput == false)
                {
                    Console.WriteLine("Illegal input");
                }
            } while (!correctInput);//When input is between 1 and acceptable range
            MongoClient dbClient = new MongoClient("mongodb://ServerAccess:2vLfydinhC3csldJ@accounts-shard-00-00.rhuha.mongodb.net:27017," +
            "accounts-shard-00-01.rhuha.mongodb.net:27017,accounts-shard-00-02.rhuha.mongodb.net:27017/Accounts?ssl=true&replicaSet=atlas-18hn6b-shard-0&authSource=admin&retryWrites=true&w=majority");
            IMongoDatabase db = dbClient.GetDatabase("accountsDB");
            var ServerDetails = db.GetCollection<BsonDocument>("server");
            var DeleteFilter = Builders<BsonDocument>.Filter.Eq("adminUser", ALAUTH[indexToRemove - 1].getAdminUser());
            ServerDetails.DeleteOne(DeleteFilter);
            ALAUTH.RemoveAt(indexToRemove - 1);
            log.Info("Admin removed from database");
        }
    }
}