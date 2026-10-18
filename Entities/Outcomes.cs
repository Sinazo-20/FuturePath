namespace FuturePath.Entities
{
    public static class Outcomes
    {
        public const string StrongMatch = "Strong Match";
        public const string PossibleMatch = "Possible Match";
        public const string NotCurrentlyEligible = "Not Currently Eligible";
        public const string MoreInformationNeeded = "More Information Needed";

        // The two kinds of staff account (learners do not log in).
        public const string RoleAdministrator = "Administrator";
        public const string RoleAdvisor = "CareerAdvisor";
    }
}