namespace New_SSL_Server
{
    //This class is used for user reg serialisation and deserialisation
    class RegistrationInformation //ID CODE 2
    {
        public string Name;
        public string Email;
        public string PasswordHash;
        public string AccountSalt;
        public string stage;
        public string confirmation;
    }
}
