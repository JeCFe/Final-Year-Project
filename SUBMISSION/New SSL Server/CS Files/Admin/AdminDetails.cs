namespace New_SSL_Server
{
    //This class is responsible for managing the logged in admin details 
    class AdminDetails
    {
        private string Name;
        private string AuthToken;
        public string getName() { return Name; }
        public string getAuthToken() { return AuthToken; }
        public void setAdminDetails(string name, string token) { Name = name; AuthToken = token; }
    }
}
