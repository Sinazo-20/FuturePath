using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    public class AdvisorySummaryForm : BaseForm
    {
        private readonly AdvisorySummary summary;   // everything about the visit being shown

        // Used at the end of the learner's journey: shows the CURRENT learner's summary.
        public AdvisorySummaryForm() : this(AdvisingController.BuildSummary(SessionState.CurrentSession), true)
        {
        }

        // Shows the summary that is passed in.
        public AdvisorySummaryForm(AdvisorySummary summary, bool learnerJourney) : base("Advisory Summary")
        {
            this.summary = summary;

            // A read-only text box showing the summary. It scrolls if the text is long.
            AddWide(new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Width = 580,
                Height = 390,
                Text = BuildText()
            });

            // Buttons are added from the right: Finish, Print, Export, then Back.
            if (learnerJourney) AddButton("Finish", (s, e) => GoToWelcome());
            AddButton("Print", (s, e) => Print());
            AddButton("Export", (s, e) => Export());
            AddButton("Back", (s, e) => GoBack());
        }

        // Writes the summary as plain text for the on-screen box.
        private string BuildText()
        {
            var text = new StringBuilder();
            Learner learner = summary.Session.Learner;

            text.AppendLine("LEARNER");
            text.AppendLine("  Name: " + learner.DisplayName + "  (Grade " + learner.GradeLevelID + ")");
            if (learner.SchoolName != "") text.AppendLine("  School: " + learner.SchoolName);
            text.AppendLine();

            text.AppendLine("SELECTED PROGRAMME");
            text.AppendLine("  " + summary.Session.DegreeName + " (" + summary.Session.FacultyName + ")");
            text.AppendLine("  Outcome: " + summary.Eligibility.Outcome);
            text.AppendLine("  Reason: " + summary.Eligibility.Reason);
            text.AppendLine();

            text.AppendLine("SUBJECTS AND MARKS");
            foreach (LearnerSubject subject in summary.Session.Subjects)
                text.AppendLine("  " + subject.SubjectName + ": " + subject.Mark + "%");
            text.AppendLine("  Overall average: " + Math.Round(summary.OverallAverage) + "%");
            text.AppendLine();

            text.AppendLine("INTEREST RESULTS");
            foreach (var item in summary.InterestResults)
                text.AppendLine("  " + item.Key + ": " + Math.Round(item.Value * 100) + "% Yes");
            text.AppendLine();

            text.AppendLine("ALTERNATIVE DEGREES");
            if (summary.Alternatives.Count == 0) text.AppendLine("  None found");
            foreach (RecommendationResult alternative in summary.Alternatives)
                text.AppendLine("  " + alternative.ProgrammeName + " (" + alternative.RecommendationOutcome + ")");
            text.AppendLine();

            text.AppendLine("FUNDING OPTIONS");
            if (summary.Funding.Count == 0) text.AppendLine("  None linked to the chosen faculty or degree");
            foreach (FundingOption option in summary.Funding)
                text.AppendLine("  " + option.FundingName + " - " + option.FundingType + ", closes " + option.ClosingDate.ToString("dd MMM yyyy"));
            text.AppendLine();

            text.AppendLine("Date generated: " + DateTime.Now.ToString("dd MMM yyyy HH:mm"));
            text.AppendLine(ExportService.Disclaimer);
            return text.ToString();
        }

        // Runs when the user clicks Export: asks where to save the file, then saves it.
        private void Export()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Web page (*.html)|*.html";
                dialog.FileName = ExportService.SuggestedFileName(summary);
                if (dialog.ShowDialog() != DialogResult.OK) return;   // the user cancelled

                try
                {
                    ExportService.ExportSummary(summary, dialog.FileName);
                    MessageBox.Show("The advisory summary was saved to:\n" + dialog.FileName);   // confirmation message
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    ShowError("The file could not be saved. Please choose a different folder.");
                }
            }
        }

        // Runs when the user clicks Print: opens the summary in the web browser.
        private void Print()
        {
            ExportService.OpenInBrowser(ExportService.BuildAdvisoryHtml(summary));
            MessageBox.Show("The summary has opened in your web browser. Press Ctrl+P there to print it or save it as a PDF.");
        }
    }
}