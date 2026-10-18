namespace FuturePath.Entities
{
    public class ProgrammeRequirement
    {
        public int ProgrammeRequirementID { get; set; }
        public int DegreeProgrammeID { get; set; }       // Which degree this rule belongs to

        public int? RequiredSubjectID { get; set; }      // The subject that is needed
        public string RequiredSubjectName { get; set; } = "";
        public int? AlternativeSubjectID { get; set; }   // Another subject accepted instead (e.g. Maths Literacy)
        public string AlternativeSubjectName { get; set; } = "";
        public int? MinimumMark { get; set; }            // The lowest mark allowed for that subject

        public int? MinimumOverallAverage { get; set; }  // For average rules: lowest overall average
        public string AdvisoryNote { get; set; } = "";   // A helpful note shown to the learner

        // True when this is a subject rule, false when it is an overall-average rule.
        public bool IsSubjectRule
        {
            get { return RequiredSubjectID.HasValue; }
        }
    }
}