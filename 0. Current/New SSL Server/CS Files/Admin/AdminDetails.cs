namespace New_SSL_Server
{
    class AdminDetails
    {
        private string Name;
        private string AuthToken;
        public string getName() { return Name; }
        public string getAuthToken() { return AuthToken; }
        public void setAdminDetails(string name, string token) { Name = name; AuthToken = token; }
    }
}
