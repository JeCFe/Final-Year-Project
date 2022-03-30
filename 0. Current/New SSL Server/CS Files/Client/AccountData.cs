using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_SSL_Server
{
    class AccountData
    {
        protected string Name;
        protected string Email;
        protected string Hash;
        protected string Salt;
        protected bool loggedIn;

        public AccountData(BsonDocument data)
        {
            Name = data["Name"].AsString;
            Email = data["Email"].AsString;
            Hash = data["Hash"].AsString;
            Salt = data["Salt"].AsString;
            loggedIn = false;
        }
        public AccountData(RegistrationInformation RINFO)
        {
            Name = RINFO.Name;
            Email = RINFO.Email;
            Hash = RINFO.PasswordHash;
            Salt = RINFO.AccountSalt;
            loggedIn = false;
        }
        public string getName() { return Name; }
        public string getEmail() { return Email; }
        public string getHash() { return Hash; }
        public string getSalt() { return Salt; }
        public bool getLoggedIn() { return loggedIn; }
        public void setLoggedIn(bool log) { loggedIn = log; }
    }
}
