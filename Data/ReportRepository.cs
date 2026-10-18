using System;
using System.Data;

namespace FuturePath.Data
{
    public static class ReportRepository
    {
        // The filter text added to every Report 2 question.
        // (@p0..@p3 are filled in by the Run method below.)
        private const string Filter =
            " WHERE date(s.SessionDateTime) BETWEEN @p0 AND @p1 " +
            "AND (@p2 = 0 OR s.PreferredFacultyID = @p2) " +
            "AND (@p3 = '' OR s.Outcome = @p3) ";

        // Runs a question with the four filters filled in.
        private static DataTable Run(string sql, DateTime from, DateTime to, int facultyId, string outcome)
        {
            return DatabaseHelper.Query(sql, Row.ToDbDate(from), Row.ToDbDate(to), facultyId, outcome);
        }

        // How many learners chose each faculty and degree (most popular first).
        public static DataTable FacultyDegreeSelections(DateTime from, DateTime to, int facultyId, string outcome)
        {
            return Run(
                "SELECT f.FacultyName AS Faculty, d.ProgrammeName AS DegreeProgramme, COUNT(*) AS NumberOfLearners " +
                "FROM AdvisorySession s " +
                "JOIN DegreeProgramme d ON d.DegreeProgrammeID = s.SelectedDegreeProgrammeID " +
                "JOIN Faculty f ON f.FacultyID = d.FacultyID " + Filter +
                "GROUP BY f.FacultyName, d.ProgrammeName ORDER BY COUNT(*) DESC",
                from, to, facultyId, outcome);
        }

        // How many learners got each outcome (Strong Match, Possible Match, etc.).
        public static DataTable OutcomeCounts(DateTime from, DateTime to, int facultyId, string outcome)
        {
            return Run(
                "SELECT s.Outcome AS Outcome, COUNT(*) AS NumberOfLearners FROM AdvisorySession s " + Filter +
                "AND s.Outcome IS NOT NULL GROUP BY s.Outcome",
                from, to, facultyId, outcome);
        }

        // The subjects learners are MISSING most often (a required subject that was not taken).
        public static DataTable SubjectGaps(DateTime from, DateTime to, int facultyId, string outcome)
        {
            return Run(
                "SELECT sub.SubjectName AS Subject, COUNT(*) AS NumberOfLearners FROM AdvisorySession s " +
                "JOIN ProgrammeRequirement r ON r.DegreeProgrammeID = s.SelectedDegreeProgrammeID AND r.RequiredSubjectID IS NOT NULL " +
                "JOIN Subject sub ON sub.SubjectID = r.RequiredSubjectID " + Filter +
                "AND NOT EXISTS (SELECT 1 FROM LearnerSubject ls WHERE ls.AdvisorySessionID = s.AdvisorySessionID " +
                "AND (ls.SubjectID = r.RequiredSubjectID OR ls.SubjectID = r.AlternativeSubjectID)) " +
                "GROUP BY sub.SubjectName ORDER BY COUNT(*) DESC",
                from, to, facultyId, outcome);
        }

        // The subjects where learners' marks are BELOW the minimum most often.
        public static DataTable MarkGaps(DateTime from, DateTime to, int facultyId, string outcome)
        {
            return Run(
                "SELECT sub.SubjectName AS Subject, COUNT(*) AS NumberOfLearners FROM AdvisorySession s " +
                "JOIN ProgrammeRequirement r ON r.DegreeProgrammeID = s.SelectedDegreeProgrammeID AND r.RequiredSubjectID IS NOT NULL " +
                "JOIN LearnerSubject ls ON ls.AdvisorySessionID = s.AdvisorySessionID " +
                "AND (ls.SubjectID = r.RequiredSubjectID OR ls.SubjectID = r.AlternativeSubjectID) " +
                "JOIN Subject sub ON sub.SubjectID = ls.SubjectID " + Filter +
                "AND ls.Mark < r.MinimumMark GROUP BY sub.SubjectName ORDER BY COUNT(*) DESC",
                from, to, facultyId, outcome);
        }

        // The funding types most requested (counts learners who asked for funding,
        // for each active funding option that matches their chosen degree).
        public static DataTable FundingCategories(DateTime from, DateTime to, int facultyId, string outcome)
        {
            return Run(
                "SELECT fo.FundingType AS FundingCategory, COUNT(DISTINCT s.AdvisorySessionID) AS NumberOfRequests " +
                "FROM AdvisorySession s " +
                "JOIN DegreeProgramme d ON d.DegreeProgrammeID = s.SelectedDegreeProgrammeID " +
                "JOIN FundingOption fo ON fo.IsActive = 1 AND (" +
                "fo.FundingOptionID IN (SELECT FundingOptionID FROM FundingFaculty WHERE FacultyID = d.FacultyID) OR " +
                "fo.FundingOptionID IN (SELECT FundingOptionID FROM FundingProgramme WHERE DegreeProgrammeID = d.DegreeProgrammeID)) " +
                Filter + "AND s.FundingRequested = 1 GROUP BY fo.FundingType ORDER BY COUNT(DISTINCT s.AdvisorySessionID) DESC",
                from, to, facultyId, outcome);
        }

        // Report 3: every learner whose outcome is "Not Currently Eligible" or "More Information Needed".
        // Sorted by outcome, then learner name. Switched-off degrees are left out.
        public static DataTable ExceptionReport()
        {
            return DatabaseHelper.Query(
                "SELECT COALESCE(NULLIF(TRIM(l.FirstName || ' ' || l.LastName), ''), 'Anonymous learner') AS Learner, " +
                "l.GradeLevelID AS Grade, d.ProgrammeName AS Qualification, f.FacultyName AS Faculty, s.Outcome AS Outcome, " +
                // Required subjects the learner did not take:
                "COALESCE((SELECT group_concat(sub.SubjectName || ' not taken', '; ') FROM ProgrammeRequirement r " +
                "JOIN Subject sub ON sub.SubjectID = r.RequiredSubjectID " +
                "WHERE r.DegreeProgrammeID = s.SelectedDegreeProgrammeID AND NOT EXISTS (SELECT 1 FROM LearnerSubject ls " +
                "WHERE ls.AdvisorySessionID = s.AdvisorySessionID AND (ls.SubjectID = r.RequiredSubjectID OR ls.SubjectID = r.AlternativeSubjectID))), '') AS SubjectGap, " +
                // Marks that are below the minimum:
                "COALESCE((SELECT group_concat(sub.SubjectName || ': ' || ls.Mark || '% vs ' || r.MinimumMark || '% required', '; ') " +
                "FROM ProgrammeRequirement r JOIN LearnerSubject ls ON ls.AdvisorySessionID = s.AdvisorySessionID " +
                "AND (ls.SubjectID = r.RequiredSubjectID OR ls.SubjectID = r.AlternativeSubjectID) " +
                "JOIN Subject sub ON sub.SubjectID = ls.SubjectID " +
                "WHERE r.DegreeProgrammeID = s.SelectedDegreeProgrammeID AND ls.Mark < r.MinimumMark), '') AS MarkGap, " +
                "s.OutcomeReason AS Reason, " +
                // The best-scoring alternative degree saved for this session (if any):
                "COALESCE((SELECT d2.ProgrammeName FROM RecommendationResult rr " +
                "JOIN DegreeProgramme d2 ON d2.DegreeProgrammeID = rr.DegreeProgrammeID " +
                "WHERE rr.AdvisorySessionID = s.AdvisorySessionID AND rr.DegreeProgrammeID <> s.SelectedDegreeProgrammeID " +
                "ORDER BY rr.OverallScore DESC LIMIT 1), 'None saved') AS RecommendedAlternative, " +
                "s.SessionDateTime AS SessionDate " +
                "FROM AdvisorySession s JOIN Learner l ON l.LearnerID = s.LearnerID " +
                "JOIN DegreeProgramme d ON d.DegreeProgrammeID = s.SelectedDegreeProgrammeID " +
                "JOIN Faculty f ON f.FacultyID = d.FacultyID " +
                "WHERE s.Outcome IN ('Not Currently Eligible', 'More Information Needed') AND d.IsActive = 1 " +
                "ORDER BY s.Outcome, Learner, s.SessionDateTime DESC");
        }
    }
}
