using System;
using System.Security.Cryptography;
using System.Text;

namespace FuturePath.Helpers
{
    public static class PasswordHelper
    {
        //password such as "Admin@123" 
        public static string Hash(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password)); 
            return Convert.ToHexString(bytes);                                
        }

        // Returns true if the typed password matches the scrambled one stored in the database.
        public static bool Verify(string typedPassword, string storedHash)
        {
            return Hash(typedPassword) == storedHash;
        }
    }
}