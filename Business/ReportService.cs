using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using FuturePath.Data;
using FuturePath.Entities;

namespace FuturePath.Business
{
    public static class ReportService
    {
        // True when a Career Advisor or an Administrator is logged in.
        public static bool CanViewReports
        {
            get { return SessionState.IsAdvisor || SessionState.IsAdministrator; }
        }

        // search learners/sessions. Returns an empty table if nobody is logged in.
        // The table has these columns: SessionID, Learner, Grade, Programme, Outcome, SessionDate.
        public static DataTable SearchSessions(string text)
        {
            return CanViewReports ? SessionRepository.Search(text) : new DataTable();
        }

        // Loads the full summary for one session
        // Returns null if the session does not exist or nobody is allowed to see it.
        public static AdvisorySummary? GetSummary(int sessionId)
        {
            if (!CanViewReports) return null;
            AdvisorySession? session = SessionRepository.GetById(sessionId);
            return session == null ? null : AdvisingController.BuildSummary(session);
        }

        // Turns column names such as "NumberOfLearners" into "Number Of Learners" for display.
        private static DataTable Prettify(DataTable table)
        {
            foreach (DataColumn column in table.Columns)
            {
                column.ColumnName = Regex.Replace(column.ColumnName, "(?<=[a-z])(?=[A-Z])", " ");
            }
            return table;
        }

        // Shows a percentage such as "42.9%" (n out of total).
        private static string Percent(int n, int total)
        {
            return total == 0 ? "0%" : (100.0 * n / total).ToString("0.0") + "%";
        }

        // ---------- Report 2: Degree Interest and Eligibility Report ----------
        // facultyId 0 = all faculties.  outcome "" = all outcomes.
        public static DegreeInterestReport? BuildDegreeInterestReport(DateTime from, DateTime to, int facultyId, string outcome)
        {
            if (!CanViewReports) return null;

            var report = new DegreeInterestReport
            {
                FromDate = from,
                ToDate = to,
                GeneratedOn = DateTime.Now,
                GeneratedBy = (SessionState.CurrentUser?.Username ?? "") + " (" + (SessionState.CurrentUser?.UserRole ?? "") + ")",
                FilterText = (facultyId == 0 ? "All faculties" :
                                 AdvisingController.GetFaculties().FirstOrDefault(f => f.FacultyID == facultyId)?.FacultyName ?? "") +
                             " / " + (outcome == "" ? "All outcomes" : outcome)
            };

            // Which faculties and degrees were chosen, most popular first.
            DataTable selections = ReportRepository.FacultyDegreeSelections(from, to, facultyId, outcome);
            if (selections.Rows.Count > 0)
            {
                report.MostSelectedDegree = Row.Text(selections.Rows[0], "DegreeProgramme");
                report.MostSelectedFaculty = selections.AsEnumerable()
                    .GroupBy(r => Row.Text(r, "Faculty"))
                    .OrderByDescending(g => g.Sum(r => Row.Int(r, "NumberOfLearners")))
                    .First().Key;
            }

            // Outcome table: ALWAYS shows all four outcomes (even when the number is 0), with percentages.
            var found = new Dictionary<string, int>();
            foreach (DataRow r in ReportRepository.OutcomeCounts(from, to, facultyId, outcome).Rows)
            {
                found[Row.Text(r, "Outcome")] = Row.Int(r, "NumberOfLearners");
            }
            report.TotalLearners = found.Values.Sum();
            report.MostCommonOutcome = found.Count == 0 ? "None" : found.OrderByDescending(kv => kv.Value).First().Key;

            var outcomeTable = new DataTable();
            outcomeTable.Columns.Add("Eligibility Outcome");
            outcomeTable.Columns.Add("Number Of Learners", typeof(int));
            outcomeTable.Columns.Add("Percentage");
            foreach (string name in new[] { Outcomes.StrongMatch, Outcomes.PossibleMatch, Outcomes.NotCurrentlyEligible, Outcomes.MoreInformationNeeded })
            {
                int n = found.GetValueOrDefault(name);
                outcomeTable.Rows.Add(name, n, Percent(n, report.TotalLearners));
            }

            report.FacultyDegree = Prettify(selections);
            report.OutcomeSummary = outcomeTable;
            report.SubjectGaps = Prettify(ReportRepository.SubjectGaps(from, to, facultyId, outcome));
            report.MarkGaps = Prettify(ReportRepository.MarkGaps(from, to, facultyId, outcome));
            report.FundingCategories = Prettify(ReportRepository.FundingCategories(from, to, facultyId, outcome));
            return report;
        }

        // ---------- Report 3: Learners Not Currently Eligible Exception Report ----------
        public static ExceptionReportData? BuildExceptionReport()
        {
            if (!CanViewReports) return null;

            DataTable details = ReportRepository.ExceptionReport();
            int total = details.Rows.Count;

            // Summary 1: how many learners per outcome.
            var outcomeTable = new DataTable();
            outcomeTable.Columns.Add("Advisory Outcome");
            outcomeTable.Columns.Add("Learners", typeof(int));
            outcomeTable.Columns.Add("% Of Total");
            foreach (string name in new[] { Outcomes.MoreInformationNeeded, Outcomes.NotCurrentlyEligible })
            {
                int n = details.AsEnumerable().Count(r => Row.Text(r, "Outcome") == name);
                outcomeTable.Rows.Add(name, n, Percent(n, total));
            }
            outcomeTable.Rows.Add("Total exceptions", total, total == 0 ? "0%" : "100%");

            // Summary 2: how many learners per kind of reason.
            var reasonTable = new DataTable();
            reasonTable.Columns.Add("Reason For Exception");
            reasonTable.Columns.Add("Learners", typeof(int));
            reasonTable.Rows.Add("Required subject not taken", details.AsEnumerable().Count(r => Row.Text(r, "SubjectGap") != ""));
            reasonTable.Rows.Add("Mark below minimum", details.AsEnumerable().Count(r => Row.Text(r, "MarkGap") != ""));
            reasonTable.Rows.Add("More information needed", details.AsEnumerable().Count(r => Row.Text(r, "Outcome") == Outcomes.MoreInformationNeeded));

            return new ExceptionReportData
            {
                GeneratedOn = DateTime.Now,
                TotalExceptions = total,
                Details = Prettify(details),
                OutcomeSummary = outcomeTable,
                ReasonSummary = reasonTable
            };
        }
    }

    // The finished Report 2, ready for the screen or for export.
    public class DegreeInterestReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime GeneratedOn { get; set; }
        public string GeneratedBy { get; set; } = "";
        public string FilterText { get; set; } = "";
        public int TotalLearners { get; set; }
        public string MostSelectedFaculty { get; set; } = "None";
        public string MostSelectedDegree { get; set; } = "None";
        public string MostCommonOutcome { get; set; } = "None";
        public DataTable FacultyDegree { get; set; } = new DataTable();
        public DataTable OutcomeSummary { get; set; } = new DataTable();
        public DataTable SubjectGaps { get; set; } = new DataTable();
        public DataTable MarkGaps { get; set; } = new DataTable();
        public DataTable FundingCategories { get; set; } = new DataTable();
    }

    // The finished Report 3, ready for the screen or for export.
    public class ExceptionReportData
    {
        public DateTime GeneratedOn { get; set; }
        public int TotalExceptions { get; set; }
        public DataTable Details { get; set; } = new DataTable();
        public DataTable OutcomeSummary { get; set; } = new DataTable();
        public DataTable ReasonSummary { get; set; } = new DataTable();
    }
}