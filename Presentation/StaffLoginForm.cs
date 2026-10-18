using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    // PURPOSE: The login screen for staff. One screen serves both roles:
    // it is told which role the person says they are (Career Advisor or Administrator).
    // Logging in with the wrong kind of account is refused, so a Career Advisor can never
    // enter the Administrator area.
    public class StaffLoginForm : BaseForm
    {
        private readonly string role;                         // the role this login screen is for
        private readonly TextBox username = new TextBox();
        private readonly TextBox password = new TextBox { UseSystemPasswordChar = true };   // shows dots instead of letters

        public StaffLoginForm(string role)
            : base(role == Outcomes.RoleAdministrator ? "Administrator Login" : "Career Advisor Login")
        {
            this.role = role;

            AddRow("Username *", username);
            AddRow("Password *", password);

            Button loginButton = AddButton("Login", (s, e) => Login());
            AcceptButton = loginButton;                       // pressing Enter does the same as clicking Login
            AddButton("Back", (s, e) => GoBack());
        }

        // Runs when the user clicks Login.
        private void Login()
        {
            // The Business layer checks the username and password.
            string error = AuthService.Login(username.Text, password.Text);

            // A correct login for the WRONG kind of account is also refused.
            if (error == "" && SessionState.CurrentUser!.UserRole != role)
            {
                AuthService.Logout();
                error = "These login details do not belong to a " +
                        (role == Outcomes.RoleAdministrator ? "System Administrator" : "Career Advisor") + " account.";
            }

            if (error != "")
            {
                ShowError(error);
                password.Clear();
                return;
            }
            ShowError("");

            // Open the right home screen for this role.
            if (role == Outcomes.RoleAdministrator) GoTo(new AdminMaintenanceForm());
            else GoTo(new AdvisorDashboardForm());

            // We only get back to this point when the staff member has logged out
            // (or closed that screen), so make sure nobody stays logged in.
            AuthService.Logout();
            password.Clear();
        }
    }
}