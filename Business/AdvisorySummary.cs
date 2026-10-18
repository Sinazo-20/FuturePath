using System.Collections.Generic;
using FuturePath.Entities;

namespace FuturePath.Business
{
    public class AdvisorySummary
    {
        public AdvisorySession Session { get; set; } = new AdvisorySession();              // learner, marks, answers
        public EligibilityResult Eligibility { get; set; } = new EligibilityResult();      // outcome, reason, requirement checks
        public List<RecommendationResult> Alternatives { get; set; } = new List<RecommendationResult>();  // alternative degrees
        public List<FundingOption> Funding { get; set; } = new List<FundingOption>();      // relevant funding
        public Dictionary<string, double> InterestResults { get; set; } = new Dictionary<string, double>(); // interest area -> 0.0 to 1.0
        public double OverallAverage { get; set; }                                          // average of all the learner's marks
    }
}