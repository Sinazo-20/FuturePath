using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using FuturePath.Entities;

namespace FuturePath.Data
{
    public static class SessionRepository
    {
        // Saves a finished session: the learner, the session, their marks and their answers.
        // The marks and answers are saved together in one go: either ALL of them are saved or none.
        public static int Save(AdvisorySession session)
        {
            // 1) Make sure the learner is saved first, because the session points to the learner.
            session.LearnerID = LearnerRepository.Save(session.Learner);

            // 2) Save the session itself. An empty outcome must be stored as "nothing" (null).
            string? outcome = session.Outcome == "" ? null : session.Outcome;
            session.AdvisorySessionID = DatabaseHelper.Insert(
                "INSERT INTO AdvisorySession (LearnerID, PreferredFacultyID, SelectedDegreeProgrammeID, " +
                "SessionDateTime, Outcome, OutcomeReason, FundingRequested) VALUES (@p0,@p1,@p2,@p3,@p4,@p5,@p6)",
                session.LearnerID, session.PreferredFacultyID, session.SelectedDegreeProgrammeID,
                Row.ToDbDateTime(session.SessionDateTime), outcome, session.OutcomeReason,
                session.FundingRequested ? 1 : 0);

            // 3) Save every mark and every answer inside one transaction (all-or-nothing).
            var sql = new StringBuilder("BEGIN TRANSACTION;");
            var values = new List<object>();

            // Small helper: stores a value in the list and returns its placeholder name (@p0, @p1...).
            string P(object value)
            {
                values.Add(value);
                return "@p" + (values.Count - 1);
            }

            foreach (LearnerSubject subject in session.Subjects)
            {
                sql.Append("INSERT INTO LearnerSubject VALUES (" + P(session.AdvisorySessionID) + "," +
                           P(subject.SubjectID) + "," + P(subject.Mark) + ");");
            }
            foreach (KeyValuePair<int, bool> answer in session.Answers)
            {
                sql.Append("INSERT INTO LearnerInterestResponse (AdvisorySessionID, InterestQuestionID, ResponseValue) VALUES (" +
                           P(session.AdvisorySessionID) + "," + P(answer.Key) + "," + P(answer.Value ? 1 : 0) + ");");
            }
            sql.Append("COMMIT;");

            if (values.Count > 0)
            {
                DatabaseHelper.Execute(sql.ToString(), values.ToArray());
            }
            return session.AdvisorySessionID;
        }

        // Saves the recommendation scores for a session (replaces any saved earlier).
        public static void SaveRecommendations(int sessionId, List<RecommendationResult> results)
        {
            var sql = new StringBuilder("BEGIN TRANSACTION;");
            var values = new List<object>();
            string P(object value)
            {
                values.Add(value);
                return "@p" + (values.Count - 1);
            }

            sql.Append("DELETE FROM RecommendationResult WHERE AdvisorySessionID = " + P(sessionId) + ";");
            foreach (RecommendationResult r in results)
            {
                sql.Append("INSERT INTO RecommendationResult (AdvisorySessionID, DegreeProgrammeID, SubjectMatchScore, " +
                           "MarkMatchScore, InterestMatchScore, FacultyPreferenceScore, OverallScore, RecommendationOutcome) VALUES (" +
                           P(sessionId) + "," + P(r.DegreeProgrammeID) + "," + P(r.SubjectMatchScore) + "," +
                           P(r.MarkMatchScore) + "," + P(r.InterestMatchScore) + "," + P(r.FacultyPreferenceScore) + "," +
                           P(r.OverallScore) + "," + P(r.RecommendationOutcome) + ");");
            }
            sql.Append("COMMIT;");
            DatabaseHelper.Execute(sql.ToString(), values.ToArray());
        }

        // Records that the learner asked to see funding options.
        public static void SetFundingRequested(int sessionId)
        {
            DatabaseHelper.Execute("UPDATE AdvisorySession SET FundingRequested = 1 WHERE AdvisorySessionID = @p0", sessionId);
        }

        // Records that the learner exported their summary to a file.
        public static void SaveExport(int sessionId, string fileName)
        {
            DatabaseHelper.Execute(
                "INSERT INTO ExportReport (AdvisorySessionID, ExportDateTime, FileName) VALUES (@p0, @p1, @p2)",
                sessionId, Row.ToDbDateTime(DateTime.Now), fileName);
        }

        // Loads one complete session (learner, marks, answers). Returns null if not found.
        public static AdvisorySession? GetById(int sessionId)
        {
            DataTable table = DatabaseHelper.Query(
                "SELECT s.*, f.FacultyName, d.ProgrammeName FROM AdvisorySession s " +
                "LEFT JOIN Faculty f ON f.FacultyID = s.PreferredFacultyID " +
                "LEFT JOIN DegreeProgramme d ON d.DegreeProgrammeID = s.SelectedDegreeProgrammeID " +
                "WHERE s.AdvisorySessionID = @p0", sessionId);
            if (table.Rows.Count == 0) return null;

            DataRow row = table.Rows[0];
            var session = new AdvisorySession
            {
                AdvisorySessionID = Row.Int(row, "AdvisorySessionID"),
                LearnerID = Row.Int(row, "LearnerID"),
                PreferredFacultyID = Row.NullableInt(row, "PreferredFacultyID"),
                SelectedDegreeProgrammeID = Row.NullableInt(row, "SelectedDegreeProgrammeID"),
                SessionDateTime = Row.Date(row, "SessionDateTime"),
                Outcome = Row.Text(row, "Outcome"),
                OutcomeReason = Row.Text(row, "OutcomeReason"),
                FundingRequested = Row.Bool(row, "FundingRequested"),
                FacultyName = Row.Text(row, "FacultyName"),
                DegreeName = Row.Text(row, "ProgrammeName")
            };

            session.Learner = LearnerRepository.GetById(session.LearnerID) ?? new Learner();

            // The learner's subjects and marks.
            DataTable marks = DatabaseHelper.Query(
                "SELECT ls.SubjectID, sub.SubjectName, ls.Mark FROM LearnerSubject ls " +
                "JOIN Subject sub ON sub.SubjectID = ls.SubjectID " +
                "WHERE ls.AdvisorySessionID = @p0 ORDER BY sub.SubjectName", sessionId);
            foreach (DataRow m in marks.Rows)
            {
                session.Subjects.Add(new LearnerSubject
                {
                    SubjectID = Row.Int(m, "SubjectID"),
                    SubjectName = Row.Text(m, "SubjectName"),
                    Mark = Row.Int(m, "Mark")
                });
            }

            // The learner's Yes/No answers.
            DataTable answers = DatabaseHelper.Query(
                "SELECT InterestQuestionID, ResponseValue FROM LearnerInterestResponse WHERE AdvisorySessionID = @p0", sessionId);
            foreach (DataRow a in answers.Rows)
            {
                session.Answers[Row.Int(a, "InterestQuestionID")] = Row.Bool(a, "ResponseValue");
            }
            return session;
        }

        // Career Advisor search: returns a table of sessions for the dashboard grid.
        // Typing a name (or part of one) or a learner ID filters the list. Empty text shows everyone.
        public static DataTable Search(string text)
        {
            return DatabaseHelper.Query(
                "SELECT s.AdvisorySessionID AS SessionID, " +
                "COALESCE(NULLIF(TRIM(l.FirstName || ' ' || l.LastName), ''), 'Anonymous learner') AS Learner, " +
                "'Grade ' || l.GradeLevelID AS Grade, d.ProgrammeName AS Programme, " +
                "COALESCE(s.Outcome, 'Not completed') AS Outcome, s.SessionDateTime AS SessionDate " +
                "FROM AdvisorySession s JOIN Learner l ON l.LearnerID = s.LearnerID " +
                "LEFT JOIN DegreeProgramme d ON d.DegreeProgrammeID = s.SelectedDegreeProgrammeID " +
                "WHERE (@p0 = '' OR l.FirstName LIKE '%' || @p0 || '%' OR l.LastName LIKE '%' || @p0 || '%' " +
                "OR CAST(l.LearnerID AS TEXT) = @p0) ORDER BY s.SessionDateTime DESC", text.Trim());
        }
    }
}
