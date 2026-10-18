using System;

namespace FuturePath.Entities
{
    // A record that a learner's advisory summary was exported to a file.
    public class ExportReport
    {
        public int ExportReportID { get; set; }
        public int AdvisorySessionID { get; set; }
        public DateTime ExportDateTime { get; set; }
        public string FileName { get; set; } = "";
    }
}