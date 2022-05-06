using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;

namespace Forms_SSL_Client
{
    class InputValidation
    {
        //Validates that the username matches the expected values
        //names are only allowed to contain lower and upper case A-Z and underscores 
        //Returns true if username matches this and false if not
        public bool NameValidation(string username) //Only allows names with
        {
            if (Regex.Match(username, "^[a-zA-Z0-9_]+$").Success)
            {
                Console.WriteLine(username);
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
                    UInt16 totalLower = 0, totalUpper = 0, totalSpecial = 0, totalNumber = 0;
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
        //This function validates the pattern for an email address and returns true if the pattern is recognises
        //For an email address: *******@*****.*** Where it goes account username@mailserver.domain
        public bool EmailValidation(string email)
        {
            if (Regex.Match(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Success) { return true; }
            return false;
        }

        //This validates whether an input IP address is acceptable
        public bool IPValidation(string IPAddress)
        {
                IPAddress ipAddress;
                if (IPAddress.Count(c=>c =='.')==3)
                {
                    if (System.Net.IPAddress.TryParse(IPAddress, out ipAddress)){ return true;}
                    else { return false; }
                }
                else { return false; }
        }
    }
}
