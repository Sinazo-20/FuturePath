using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    // PURPOSE: Screen 5: shows the advisory outcome for the degree the learner chose.
    // It always explains WHY (which subject is missing or which mark is too low) and shows the
    // advisory disclaimer, as our specification requires.
    // There is no "Back" button here: the visit was saved when the learner submitted the
    // questionnaire, so the learner cannot go back and change their answers (they start over instead).
    public class EligibilityResultForm : BaseForm
    {
        public EligibilityResultForm() : base("Eligibility Result")
        {
            AdvisorySession session = SessionState.CurrentSession;
            EligibilityResult result = SessionState.Eligibility ?? new EligibilityResult();

            AddWide(new Label { Text = "Selected programme: " + session.DegreeName + " (" + session.FacultyName + ")", AutoSize = true });
            AddWide(new Label { Text = "Outcome: " + result.Outcome, AutoSize = true, Font = new Font("Arial", 13F, FontStyle.Bold) });

            // The interest match only makes sense when the check could be completed.
            if (result.Outcome != Outcomes.MoreInformationNeeded)
                AddWide(new Label { Text = "Interest match: " + Math.Round(result.InterestAlignment * 100) + "%", AutoSize = true });

            AddWide(new Label { Text = result.Reason, AutoSize = true });   // the explanation, in plain English

            // A table with one line per requirement: what is needed, what the learner has, and Met or Gap.
            var table = new DataTable();
            table.Columns.Add("Requirement");
            table.Columns.Add("Needed");
            table.Columns.Add("Your Result");
            table.Columns.Add("Status");
            foreach (RequirementCheck check in result.Checks)
                table.Rows.Add(check.Requirement, check.Needed, check.YourResult, check.Status);
            AddWide(NewGrid(table, 170));

            AddWide(new Label { Text = ExportService.Disclaimer, AutoSize = true });

            AddButton("Next >", (s, e) => GoTo(new AlternativesForm()));
            AddButton("Finish", (s, e) => GoToWelcome());
        }

        // If the learner closes this window with the X in the corner, treat it as "Finish"
        // (go back to the Welcome screen) instead of "Back", because the visit is already saved.
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (DialogResult == DialogResult.Cancel) DialogResult = DialogResult.Abort;
        }
    }
}