using System.Collections.Generic;

namespace FuturePath.Entities
{
    public class EligibilityResult
    {
        public string Outcome { get; set; } = "";             // One of the four words in Outcomes.cs
        public string Reason { get; set; } = "";              // A plain-English explanation for the learner
        public double InterestAlignment { get; set; }         // 0.0 to 1.0 (0.5 means 50% interest match)

        // The detail lines shown in the results grid, one per requirement checked.
        public List<RequirementCheck> Checks { get; set; } = new List<RequirementCheck>();

        // Any subjects the learner is missing and any marks that are too low.
        public List<string> MissingSubjects { get; set; } = new List<string>();
        public List<string> LowMarks { get; set; } = new List<string>();
    }

    // One line of the results grid, for example: Mathematics | needs 60 | you have 58 | Gap
    public class RequirementCheck
    {
        public string Requirement { get; set; } = "";   // e.g. "Mathematics"
        public string Needed { get; set; } = "";        // e.g. "60%"
        public string YourResult { get; set; } = "";    // e.g. "58%" or "Not taken"
        public string Status { get; set; } = "";        // "Met" or "Gap"
    }
}