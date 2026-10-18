using System.Collections.Generic;
using FuturePath.Entities;

namespace FuturePath.Business
{
    public static class SessionState
    {
        // The staff member who is logged in (null = nobody logged in).
        public static UserAccount? CurrentUser { get; set; }

        // The learner's current visit, filled in screen by screen.
        public static AdvisorySession CurrentSession { get; set; } = new AdvisorySession();

        // The eligibility answer for the chosen degree (filled in at the results screen).
        public static EligibilityResult? Eligibility { get; set; }

        // The alternative degrees suggested for the learner.
        public static List<RecommendationResult> Alternatives { get; set; } = new List<RecommendationResult>();

        // Role checks - used to hide or block things people are not allowed to use.
        public static bool IsAdministrator
        {
            get { return CurrentUser != null && CurrentUser.UserRole == Outcomes.RoleAdministrator; }
        }

        public static bool IsAdvisor
        {
            get { return CurrentUser != null && CurrentUser.UserRole == Outcomes.RoleAdvisor; }
        }

        // Starts a fresh visit (wipes the previous learner's information).
        public static void StartNewSession()
        {
            CurrentSession = new AdvisorySession();
            Eligibility = null;
            Alternatives = new List<RecommendationResult>();
        }
    }
}