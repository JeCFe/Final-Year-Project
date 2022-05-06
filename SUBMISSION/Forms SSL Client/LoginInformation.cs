namespace Forms_SSL_Client
{
    //This class is used for the data in Login serialisation and deserialisation
    class LoginInformation //ID CODE 1
    {
        public string confirmation;
        public string stage;
        public string Email;
        public string accountSalt;
        public string sessionSalt;
        public string name;
        public string passwordHash;
    }
}
