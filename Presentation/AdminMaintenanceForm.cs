using System.Windows.Forms;
using FuturePath.Business;

namespace FuturePath.Presentation
{
    // PURPOSE: The Administrator's home screen: a menu of everything the Administrator can maintain.
    // Only an Administrator can get here (the login screen refuses other accounts), and the
    // Business layer checks the role again for every change.
    public class AdminMaintenanceForm : BaseForm
    {
        public AdminMaintenanceForm() : base("Administrator Maintenance")
        {
            AddWide(new Label { Text = "Choose what you want to maintain.", AutoSize = true });
            AddWide(NewButton("Degree Programmes", (s, e) => GoTo(new ProgrammesAdminForm()), 280));
            AddWide(NewButton("Programme Requirements", (s, e) => GoTo(new RequirementsAdminForm()), 280));
            AddWide(NewButton("Questionnaire Questions", (s, e) => GoTo(new QuestionsAdminForm()), 280));
            AddWide(NewButton("Funding Options", (s, e) => GoTo(new FundingAdminForm()), 280));
            AddWide(NewButton("Report Centre", (s, e) => GoTo(new ReportCentreForm()), 280));

            AddButton("Logout", (s, e) => { AuthService.Logout(); GoToWelcome(); });
        }
    }
}
