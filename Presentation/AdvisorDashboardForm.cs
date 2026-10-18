using System;
using System.Windows.Forms;
using FuturePath.Business;

namespace FuturePath.Presentation
{
    // PURPOSE: The Career Advisor's home screen. The advisor can search for learners and sessions,
    // open any learner's advisory summary, open the Report Centre, or log out.
    // The Business layer only returns data if a Career Advisor or Administrator is logged in.
    public class AdvisorDashboardForm : BaseForm
    {
        private readonly TextBox searchBox = new TextBox();
        private readonly DataGridView grid;   // the table of sessions

        public AdvisorDashboardForm() : base("Career Advisor Dashboard")
        {
            grid = NewGrid(ReportService.SearchSessions(""), 260);   // start by showing everyone

            AddRow("Search learner", searchBox);
            AddWide(new Label { Text = "Type part of a name or a learner number. Leave it blank to see everyone.", AutoSize = true });
            AddWide(NewButton("Search", (s, e) => Search()));
            AddWide(grid);

            // Buttons are added from the right: Logout, then Generate Report, then View Summary.
            AddButton("Logout", (s, e) => { AuthService.Logout(); GoToWelcome(); });
            AddButton("Generate Report", (s, e) => GoTo(new ReportCentreForm()), 130);
            AddButton("View Summary", (s, e) => ViewSummary(), 110);
        }

        // Fills the table with the sessions that match what was typed.
        private void Search()
        {
            grid.DataSource = ReportService.SearchSessions(searchBox.Text);
        }

        // Opens the advisory summary of the session selected in the table.
        private void ViewSummary()
        {
            if (grid.CurrentRow == null)
            {
                ShowError("Please click a session in the table first.");
                return;
            }

            int sessionId = Convert.ToInt32(grid.CurrentRow.Cells["SessionID"].Value);
            AdvisorySummary? summary = ReportService.GetSummary(sessionId);
            if (summary == null)
            {
                ShowError("That session could not be opened.");
                return;
            }
            ShowError("");
            GoTo(new AdvisorySummaryForm(summary, false));
        }
    }
}