using System;
using System.Collections.Generic;

namespace FuturePath.Entities
{
    public class AdvisorySession
    {
        public int AdvisorySessionID { get; set; }
        public int LearnerID { get; set; }
        public int? PreferredFacultyID { get; set; }           // Faculty the learner picked
        public int? SelectedDegreeProgrammeID { get; set; }    // The one degree being assessed
        public DateTime SessionDateTime { get; set; } = DateTime.Now;

        public string Outcome { get; set; } = "";              // Final advisory outcome
        public string OutcomeReason { get; set; } = "";        // Explanation of the outcome
        public bool FundingRequested { get; set; }             // Did the learner ask to see funding?

        // Not separate database columns - these are loaded from related tables:
        public Learner Learner { get; set; } = new Learner();
        public List<LearnerSubject> Subjects { get; set; } = new List<LearnerSubject>();

        // The questionnaire answers. Key = question number, Value = true for Yes, false for No.
        public Dictionary<int, bool> Answers { get; set; } = new Dictionary<int, bool>();

        // Names copied in for showing on screen and in reports.
        public string FacultyName { get; set; } = "";
        public string DegreeName { get; set; } = "";
    }
}