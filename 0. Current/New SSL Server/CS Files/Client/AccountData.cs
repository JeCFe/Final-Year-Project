using MongoDB.Bson;

namespace New_SSL_Server
{
    /*
     This class is responible for the local storage of each user account details 
     With functions required to convert from MongoDB BsonDocument to a format that JavaScript Serializer can understand
    */

    class AccountData
    {
        protected string Name;
        protected string Email;
        protected string Hash;
        protected string Salt;
        protected bool loggedIn;

        //This function converts from BsonDocument to this program standard storage method
        public AccountData(BsonDocument data)
        {
            Name = data["Name"].AsString;
            Email = data["Email"].AsString;
            Hash = data["Hash"].AsString;
            Salt = data["Salt"].AsString;
            loggedIn = false;
        }

        //This function can data from a Reg Info object and saves the content 
        public AccountData(RegistrationInformation RINFO)
        {
            Name = RINFO.Name;
            Email = RINFO.Email;
            Hash = RINFO.PasswordHash;
            Salt = RINFO.AccountSalt;
            loggedIn = false;
        }

        //The following functions are simple getters and setter functions
        public string GetName() { return Name; }
        public string GetEmail() { return Email; }
        public string GetHash() { return Hash; }
        public string GetSalt() { return Salt; }
        public bool GetLoggedIn() { return loggedIn; }
        public void SetLoggedIn(bool log) { loggedIn = log; }
    }
}
