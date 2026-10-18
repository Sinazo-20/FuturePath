using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    // PURPOSE: Screen 6: shows alternative degrees that suit the learner's marks and interests,
    // best match first. Then asks whether the learner wants to see funding options.
    // The alternatives were already worked out when the visit was saved.
    public class AlternativesForm : BaseForm
    {
        public AlternativesForm() : base("Alternative Programmes")
        {
            List<RecommendationResult> alternatives = SessionState.Alternatives;

            if (alternatives.Count == 0)
            {
                AddWide(new Label { Text = "No suitable alternative degrees were found based on your subjects, marks and interests.", AutoSize = true });
            }
            else
            {
                AddWide(new Label { Text = "Suggested alternatives based on your marks and interests (best match first):", AutoSize = true });

                var table = new DataTable();
                table.Columns.Add("Programme");
                table.Columns.Add("Faculty");
                table.Columns.Add("Score");
                table.Columns.Add("Outcome");
                table.Columns.Add("Why Recommended");
                foreach (RecommendationResult alternative in alternatives)
                    table.Rows.Add(alternative.ProgrammeName, alternative.FacultyName,
                                   Math.Round(alternative.OverallScore * 100) + "%", alternative.RecommendationOutcome, alternative.Reason);
                AddWide(NewGrid(table, 240));
            }

            AddWide(new Label { Text = "Would you like to view funding options for your chosen faculty or degree?", AutoSize = true });

            // "View funding" also records that the learner asked for funding (used in Report 2).
            AddButton("View funding", (s, e) =>
            {
                AdvisingController.RecordFundingRequested();
                GoTo(new FundingForm());
            });
            AddButton("Skip funding", (s, e) => GoTo(new AdvisorySummaryForm()));
            AddButton("Back", (s, e) => GoBack());
        }
    }
}