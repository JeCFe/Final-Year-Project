using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace New_SSL_Server
{
    class AdminLoginAuthentication
    {
        protected string hashedPassword;
        protected string adminSalt;
        protected string adminUser;
        protected string adminName;
        public AdminLoginAuthentication() { }
        public AdminLoginAuthentication(BsonDocument data)
        {
            hashedPassword = data["adminPassword"].AsString;
            adminSalt = data["adminSalt"].AsString;
            adminUser = data["adminUser"].AsString;
            adminName = data["adminName"].AsString;
        }
        public string getHashedPassword() { return hashedPassword; }
        public string getAdminSalt() { return adminSalt; }
        public string getAdminUser() { return adminUser; }
        public string getAdminName() { return adminName; }
        public void setHashedPassword(string hashed) { hashedPassword = hashed; }
        public void setAdminSalt(string salt) { adminSalt = salt; }
        public void setAdminName(string name) { adminName = name; }
        public void setAdminUser(string user) { adminUser = user; }
    }
}
