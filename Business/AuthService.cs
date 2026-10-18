using FuturePath.Data;
using FuturePath.Entities;
using FuturePath.Helpers;

namespace FuturePath.Business
{
    public static class AuthService
    {
        // Tries to log in. Returns "" if it worked, otherwise a message to show the user.
        public static string Login(string username, string password)
        {
            if (username.Trim() == "" || password == "")
                return "Please enter your username and password.";

            UserAccount? user = UserRepository.GetByUsername(username.Trim());

            // We give ONE message for every kind of failure, so nobody can use the
            // message to guess which usernames exist.
            if (user == null || !user.IsActive || !PasswordHelper.Verify(password, user.PasswordHash))
                return "Incorrect username or password.";

            SessionState.CurrentUser = user;   // remember who is logged in
            return "";
        }

        // Logs the current staff member out.
        public static void Logout()
        {
            SessionState.CurrentUser = null;
        }
    }
}