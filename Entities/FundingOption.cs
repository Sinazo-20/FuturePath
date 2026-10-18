using System;

namespace FuturePath.Entities
{
    //One funding opportunity (a bursary, scholarship, financial aid or external funding).
    public class FundingOption
    {
        public int FundingOptionID { get; set; }
        public string FundingName { get; set; } = "";
        public string FundingType { get; set; } = "";            // Scholarship, Bursary, Financial Aid or External Funding
        public string EligibilityDescription { get; set; } = ""; // Who may apply (merit, financial need, etc.)
        public string RequiredDocuments { get; set; } = "";
        public DateTime ClosingDate { get; set; }
        public string ContactApplicationInfo { get; set; } = "";
        public bool IsActive { get; set; } = true;               // false = outdated, never shown to learners
        public bool IsSample { get; set; } = true;               // true = sample data, learner must verify it
    }
}