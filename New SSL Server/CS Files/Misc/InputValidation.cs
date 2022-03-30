using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace New_SSL_Server
{
    class InputValidation
    {
        public bool NameValidation(string username) //Only allows names with
        {
            if (Regex.Match(username, "^[a-zA-Z0-9_]+$").Success)
            {
                Console.WriteLine(username);
                return true;
            }
            return false;
        }
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
        public bool EmailValidation(string email)
        {
            if (Regex.Match(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Success) { return true; }
            return false;
        }
    }
}
