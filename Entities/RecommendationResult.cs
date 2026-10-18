namespace FuturePath.Entities
{
    public class RecommendationResult
    {
        public int DegreeProgrammeID { get; set; }
        public string ProgrammeName { get; set; } = "";
        public string FacultyName { get; set; } = "";

        public double SubjectMatchScore { get; set; }       // Does the learner have the required subjects?
        public double MarkMatchScore { get; set; }          // Do the marks reach the minimum requirements?
        public double InterestMatchScore { get; set; }      // Do the learner's interests suit this degree?
        public double FacultyPreferenceScore { get; set; }  // Is it in the faculty the learner preferred?
        public double OverallScore { get; set; }            // All of the above combined

        public string RecommendationOutcome { get; set; } = "";  // Strong Match, Possible Match, etc.
        public string Reason { get; set; } = "";                 // Why this degree is suggested
    }
}