using System.Data;
using FuturePath.Entities;

namespace FuturePath.Data
{
    public static class UserRepository
    {
        // Finds an account by username. Returns null if there is no such account.
        // (Checking the password is done by the Business layer, not here.)
        public static UserAccount? GetByUsername(string username)
        {
            DataTable table = DatabaseHelper.Query("SELECT * FROM UserAccount WHERE Username = @p0", username);
            if (table.Rows.Count == 0) return null;

            DataRow row = table.Rows[0];
            return new UserAccount
            {
                UserAccountID = Row.Int(row, "UserAccountID"),
                Username = Row.Text(row, "Username"),
                PasswordHash = Row.Text(row, "PasswordHash"),
                UserRole = Row.Text(row, "UserRole"),
                IsActive = Row.Bool(row, "IsActive")
            };
        }
    }
}