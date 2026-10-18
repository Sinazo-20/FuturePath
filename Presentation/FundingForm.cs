using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    // PURPOSE: Screen 7: shows the funding options (bursaries, scholarships...) linked to the
    // learner's chosen faculty or degree. Funding that has been switched off is never shown.
    // Sample information is clearly labelled so the learner knows to verify it.
    public class FundingForm : BaseForm
    {
        public FundingForm() : base("Funding Options")
        {
            List<FundingOption> funding = AdvisingController.GetFunding();

            if (funding.Count == 0)
            {
                AddWide(new Label { Text = "No funding options are currently linked to your chosen faculty or degree.", AutoSize = true });
            }
            else
            {
                var table = new DataTable();
                table.Columns.Add("Funding");
                table.Columns.Add("Type");
                table.Columns.Add("Eligibility");
                table.Columns.Add("Documents");
                table.Columns.Add("Closing Date");
                table.Columns.Add("Status");
                foreach (FundingOption option in funding)
                    table.Rows.Add(option.FundingName, option.FundingType, option.EligibilityDescription,
                                   option.RequiredDocuments, option.ClosingDate.ToString("dd MMM yyyy"),
                                   option.IsSample ? "Sample - verify with the funder" : "Verified");
                AddWide(NewGrid(table, 280));
            }

            AddWide(new Label { Text = "Important: funding information shown here is locally stored guidance and may need to be verified with the funder.", AutoSize = true });

            AddButton("Next >", (s, e) => GoTo(new AdvisorySummaryForm()));
            AddButton("Back", (s, e) => GoBack());
        }
    }
}
