namespace FuturePath.Entities
{
    // Learners do not have accounts. 
    public class UserAccount
    {
        public int UserAccountID { get; set; }
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";   // Scrambled password (see PasswordHelper)
        public string UserRole { get; set; } = "";       // "Administrator" or "CareerAdvisor"
        public bool IsActive { get; set; } = true;       // false = account switched off
    }
}