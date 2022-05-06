using System.Text.RegularExpressions;

namespace New_SSL_Server
{
    class InputValidation
    {
        //Validates that the username matches the expected values
        //names are only allowed to contain lower and upper case A-Z and underscores 
        //Returns true if username matches this and false if not
        public bool NameValidation(string username)
        {
            if (Regex.Match(username, "^[a-zA-Z0-9_]+$").Success)
            {
                return true;
            }
            return false;
        }

        //Validates that the password follows the password policy
        //A password must between 8 and 16 characters
        //Includes 2 uppercase, 2 lowercase, 2 numbers, and 2 specials _ ! @ 
        //Returns true if rule followed and false if not
        public bool PasswordValidation(string password)
        {
            if (password.Length >= 8 && password.Length <= 16)
            {
                if (Regex.Match(password, "^[a-zA-Z0-9_!@]+$").Success)
                {
                    int totalLower = 0, totalUpper = 0, totalSpecial = 0, totalNumber = 0;
                    string specialCharacters = "_!@";
                    foreach (char character in password)
                    {
                        foreach (char special in specialCharacters)
                        {
                            if (character.Equals(special)) { totalSpecial++; }
                        }
                        if (char.IsUpper(character)) { totalUpper++; }
                        if (char.IsLower(character)) { totalLower++; }
                        if (int.TryParse(character.ToString(), out int parseChar)) { totalNumber++; }
                    }
                    if (totalLower >= 2 && totalUpper >= 2 && totalSpecial >= 1 && totalNumber >= 1) { return true; }
                    else { return false; }
                }
                else { return false; }
            }
            return false;
        }
    }
}
