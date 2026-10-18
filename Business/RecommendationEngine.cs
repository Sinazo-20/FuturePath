using System.Collections.Generic;
using System.Linq;
using FuturePath.Data;
using FuturePath.Entities;

namespace FuturePath.Business
{
    public static class RecommendationEngine
    {
        private const double SubjectWeight = 0.30;
        private const double MarkWeight = 0.30;
        private const double InterestWeight = 0.30;
        private const double FacultyWeight = 0.10;

        // The most alternatives we will show.
        private const int MaxAlternatives = 5;

        public static List<RecommendationResult> Generate(AdvisorySession session)
        {
            var results = new List<RecommendationResult>();

            // Look at every degree that is switched on (switched-off degrees are never recommended).
            foreach (DegreeProgramme degree in ProgrammeRepository.GetProgrammes(0, true))
            {
                // Skip the degree the learner already chose.
                if (degree.DegreeProgrammeID == session.SelectedDegreeProgrammeID) continue;

                // Check the learner against this degree using the SAME rules as the main check.
                EligibilityResult check = EligibilityChecker.Check(session, degree.DegreeProgrammeID);
                if (check.Outcome != Outcomes.StrongMatch && check.Outcome != Outcomes.PossibleMatch) continue;

                // Count the rules so we can turn the gaps into scores.
                List<ProgrammeRequirement> requirements = ProgrammeRepository.GetRequirements(degree.DegreeProgrammeID);
                int totalRules = requirements.Count;
                int subjectRules = requirements.Count(r => r.IsSubjectRule);   // how many rules are about a subject

                double subjectScore = subjectRules == 0 ? 1.0 : (double)(subjectRules - check.MissingSubjects.Count) / subjectRules;
                double markScore = totalRules == 0 ? 1.0 : (double)(totalRules - check.MissingSubjects.Count - check.LowMarks.Count) / totalRules;
                double interestScore = check.InterestAlignment;
                double facultyScore = degree.FacultyID == session.PreferredFacultyID ? 1.0 : 0.0;

                results.Add(new RecommendationResult
                {
                    DegreeProgrammeID = degree.DegreeProgrammeID,
                    ProgrammeName = degree.ProgrammeName,
                    FacultyName = degree.FacultyName,
                    SubjectMatchScore = subjectScore,
                    MarkMatchScore = markScore,
                    InterestMatchScore = interestScore,
                    FacultyPreferenceScore = facultyScore,
                    OverallScore = SubjectWeight * subjectScore + MarkWeight * markScore +
                                   InterestWeight * interestScore + FacultyWeight * facultyScore,
                    RecommendationOutcome = check.Outcome,
                    Reason = check.Reason
                });
            }

            // Rank: best overall score first (names are the tie-breaker), and keep the top few.
            return results
                .OrderByDescending(r => r.OverallScore)
                .ThenBy(r => r.ProgrammeName)
                .Take(MaxAlternatives)
                .ToList();
        }
    }
}