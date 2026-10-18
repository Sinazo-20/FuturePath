using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using FuturePath.Data;
using FuturePath.Entities;

namespace FuturePath.Business
{
    public static class ExportService
    {
        // The disclaimer that must appear on every advisory output.
        public const string Disclaimer =
            "Future Path provides preliminary guidance only and is not an official university admission decision. " +
            "Sample programme requirements and funding information must be verified with the institution or funder.";

        // ---------- Small building blocks ----------

        // Makes text safe to place in a web page (so characters like < or & cannot break the page).
        private static string Enc(string text)
        {
            return WebUtility.HtmlEncode(text);
        }

        // The top of every page: title and a very plain style (just borders and spacing).
        private static string PageStart(string title)
        {
            return "<!DOCTYPE html><html><head><meta charset='utf-8'><title>" + Enc(title) + "</title>" +
                   "<style>body{font-family:Arial,sans-serif;margin:30px;}" +
                   "table{border-collapse:collapse;margin-bottom:15px;}" +
                   "th,td{border:1px solid #888;padding:4px 8px;text-align:left;}" +
                   "th{background:#eee;}</style></head><body><h1>" + Enc(title) + "</h1>";
        }

        private const string PageEnd = "</body></html>";

        // Builds an HTML table from column headings and rows of text.
        private static string SimpleTable(string[] headers, List<string[]> rows)
        {
            if (rows.Count == 0) return "<p>None</p>";
            var sb = new StringBuilder("<table><tr>");
            foreach (string header in headers) sb.Append("<th>" + Enc(header) + "</th>");
            sb.Append("</tr>");
            foreach (string[] row in rows)
            {
                sb.Append("<tr>");
                foreach (string cell in row) sb.Append("<td>" + Enc(cell) + "</td>");
                sb.Append("</tr>");
            }
            return sb.Append("</table>").ToString();
        }

        // Builds an HTML table from a DataTable (used for the report tables).
        private static string TableHtml(DataTable table)
        {
            var headers = new string[table.Columns.Count];
            for (int i = 0; i < headers.Length; i++) headers[i] = table.Columns[i].ColumnName;

            var rows = new List<string[]>();
            foreach (DataRow dataRow in table.Rows)
            {
                var cells = new string[headers.Length];
                for (int i = 0; i < cells.Length; i++) cells[i] = Row.Text(dataRow, headers[i]);
                rows.Add(cells);
            }
            return SimpleTable(headers, rows);
        }

        // A two-column "label / value" table (used for the learner's profile).
        private static string LabelValueTable(List<string[]> rows)
        {
            var sb = new StringBuilder("<table>");
            foreach (string[] row in rows)
                sb.Append("<tr><th>" + Enc(row[0]) + "</th><td>" + Enc(row[1]) + "</td></tr>");
            return sb.Append("</table>").ToString();
        }

        // ---------- Report 1: Learner Advisory Summary ----------
        public static string BuildAdvisoryHtml(AdvisorySummary summary)
        {
            AdvisorySession session = summary.Session;
            Learner learner = session.Learner;
            var sb = new StringBuilder(PageStart("Learner Advisory Summary Report"));
            sb.Append("<p>Date generated: " + DateTime.Now.ToString("dd MMM yyyy HH:mm") + "</p>");

            // 1. Learner profile
            sb.Append("<h2>1. Learner Profile</h2>");
            var profile = new List<string[]>
            {
                new[] { "Name", learner.DisplayName },
                new[] { "Grade", "Grade " + learner.GradeLevelID },
                new[] { "School", learner.SchoolName },
                new[] { "Province", learner.Province }
            };
            if (learner.ContactDetails != "") profile.Add(new[] { "Contact details", learner.ContactDetails });
            sb.Append(LabelValueTable(profile));

            // 2. Subjects and marks
            sb.Append("<h2>2. Academic Subjects and Marks</h2>");
            var marks = new List<string[]>();
            foreach (LearnerSubject s in session.Subjects) marks.Add(new[] { s.SubjectName, s.Mark + "%" });
            marks.Add(new[] { "Overall average", Math.Round(summary.OverallAverage) + "%" });
            sb.Append(SimpleTable(new[] { "Subject", "Mark" }, marks));

            // 3. Selected degree and outcome
            sb.Append("<h2>3. Selected Degree and Eligibility Outcome</h2>");
            sb.Append(LabelValueTable(new List<string[]>
            {
                new[] { "Faculty", session.FacultyName },
                new[] { "Degree", session.DegreeName },
                new[] { "Outcome", summary.Eligibility.Outcome },
                new[] { "Reason", summary.Eligibility.Reason }
            }));
            var checks = new List<string[]>();
            foreach (RequirementCheck c in summary.Eligibility.Checks)
                checks.Add(new[] { c.Requirement, c.Needed, c.YourResult, c.Status });
            sb.Append(SimpleTable(new[] { "Requirement", "Needed", "Your result", "Status" }, checks));

            // 4. Interest results
            sb.Append("<h2>4. Interest Results</h2>");
            var interests = new List<string[]>();
            foreach (KeyValuePair<string, double> item in summary.InterestResults)
                interests.Add(new[] { item.Key, Math.Round(item.Value * 100) + "% Yes" });
            sb.Append(SimpleTable(new[] { "Interest area", "Match" }, interests));

            // 5. Alternatives
            sb.Append("<h2>5. Recommended Alternative Degrees</h2>");
            if (summary.Alternatives.Count == 0)
            {
                sb.Append("<p>No suitable alternatives were found.</p>");
            }
            else
            {
                var alternatives = new List<string[]>();
                foreach (RecommendationResult a in summary.Alternatives)
                    alternatives.Add(new[] { a.ProgrammeName, a.FacultyName, Math.Round(a.OverallScore * 100) + "%", a.RecommendationOutcome });
                sb.Append(SimpleTable(new[] { "Degree", "Faculty", "Overall score", "Outcome" }, alternatives));
            }

            // 6. Funding
            sb.Append("<h2>6. Funding Options</h2>");
            var funding = new List<string[]>();
            foreach (FundingOption f in summary.Funding)
                funding.Add(new[] { f.FundingName, f.FundingType, f.EligibilityDescription, f.RequiredDocuments,
                                    f.ClosingDate.ToString("dd MMM yyyy"), f.ContactApplicationInfo,
                                    f.IsSample ? "Sample information - verify with the funder" : "Verified" });
            sb.Append(SimpleTable(new[] { "Funding", "Type", "Eligibility", "Documents", "Closing date", "Contact", "Verification" }, funding));

            // Disclaimer and signatures
            sb.Append("<p><i>" + Enc(Disclaimer) + "</i></p>");
            sb.Append("<p>Learner signature: ____________________ &nbsp; Parent signature: ____________________ &nbsp; Advisor signature: ____________________</p>");
            return sb.Append(PageEnd).ToString();
        }

        // ---------- Report 2: Degree Interest and Eligibility Report ----------
        public static string BuildDegreeInterestHtml(DegreeInterestReport r)
        {
            var sb = new StringBuilder(PageStart("Degree Interest and Eligibility Report"));
            sb.Append("<p>Reporting period: " + r.FromDate.ToString("dd MMM yyyy") + " to " + r.ToDate.ToString("dd MMM yyyy") +
                      "<br>Filter: " + Enc(r.FilterText) +
                      "<br>Date generated: " + r.GeneratedOn.ToString("dd MMM yyyy HH:mm") +
                      "<br>Generated by: " + Enc(r.GeneratedBy) + "</p>");
            sb.Append("<h2>Faculty and Degree Selection</h2>" + TableHtml(r.FacultyDegree));
            sb.Append("<h2>Eligibility Outcomes</h2>" + TableHtml(r.OutcomeSummary));
            sb.Append("<h2>Common Subject Gaps</h2>" + TableHtml(r.SubjectGaps));
            sb.Append("<h2>Common Mark Gaps</h2>" + TableHtml(r.MarkGaps));
            sb.Append("<h2>Most Requested Funding Categories</h2>" + TableHtml(r.FundingCategories));
            sb.Append("<h2>Report Summary</h2>" + LabelValueTable(new List<string[]>
            {
                new[] { "Total learners included", r.TotalLearners.ToString() },
                new[] { "Most selected faculty", r.MostSelectedFaculty },
                new[] { "Most selected degree", r.MostSelectedDegree },
                new[] { "Most common eligibility outcome", r.MostCommonOutcome }
            }));
            sb.Append("<p><i>" + Enc(Disclaimer) + "</i></p>");
            return sb.Append(PageEnd).ToString();
        }

        // ---------- Report 3: Learners Not Currently Eligible Exception Report ----------
        public static string BuildExceptionHtml(ExceptionReportData r)
        {
            var sb = new StringBuilder(PageStart("Learners Not Currently Eligible Exception Report"));
            sb.Append("<p>Date generated: " + r.GeneratedOn.ToString("dd MMM yyyy HH:mm") + "</p>");
            sb.Append("<h2>Exception Summary</h2>" + TableHtml(r.OutcomeSummary) + TableHtml(r.ReasonSummary));
            sb.Append("<h2>Exception Details</h2>" + TableHtml(r.Details));
            sb.Append("<p><i>" + Enc(Disclaimer) + "</i></p>");
            return sb.Append(PageEnd).ToString();
        }

        // ---------- Saving and printing ----------

        // A sensible default file name, e.g. "Gadija_Jacobs_Summary_20260910.html".
        public static string SuggestedFileName(AdvisorySummary summary)
        {
            string name = summary.Session.Learner.DisplayName.Replace(' ', '_');
            foreach (char bad in Path.GetInvalidFileNameChars()) name = name.Replace(bad, '_');   // remove characters not allowed in file names
            return name + "_Summary_" + DateTime.Now.ToString("yyyyMMdd") + ".html";
        }

        // Writes a page to a file.
        public static void SaveFile(string path, string html)
        {
            File.WriteAllText(path, html, Encoding.UTF8);
        }

        // Saves a learner's summary to a file AND records in the database that it was exported.
        public static void ExportSummary(AdvisorySummary summary, string path)
        {
            SaveFile(path, BuildAdvisoryHtml(summary));
            if (summary.Session.AdvisorySessionID != 0)
                SessionRepository.SaveExport(summary.Session.AdvisorySessionID, Path.GetFileName(path));
        }

        // The "Print" buttons: saves the page to a temporary file and opens it in the web browser.
        // The user then presses Ctrl+P in the browser to print or to save as PDF.
        public static void OpenInBrowser(string html)
        {
            string path = Path.Combine(Path.GetTempPath(), "FuturePath_" + Guid.NewGuid().ToString("N") + ".html");
            SaveFile(path, html);
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}