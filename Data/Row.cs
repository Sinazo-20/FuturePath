using System;
using System.Data;
using System.Globalization;

namespace FuturePath.Data
{
    public static class Row
    {
        // Reads a text cell. An empty cell becomes "" (empty text).
        public static string Text(DataRow row, string column)
        {
            return row.IsNull(column) ? "" : Convert.ToString(row[column]) ?? "";
        }

        // Reads a whole-number cell. An empty cell becomes 0.
        public static int Int(DataRow row, string column)
        {
            return row.IsNull(column) ? 0 : Convert.ToInt32(row[column]);
        }

        // Reads a whole-number cell that is ALLOWED to be empty. An empty cell stays "null" (nothing).
        public static int? NullableInt(DataRow row, string column)
        {
            if (row.IsNull(column)) return null;
            return Convert.ToInt32(row[column]);
        }

        // Reads a true/false cell. The database stores true as 1 and false as 0.
        public static bool Bool(DataRow row, string column)
        {
            return Int(row, column) == 1;
        }

        // Reads a decimal-number cell (such as a score of 0.75). An empty cell becomes 0.
        public static double Double(DataRow row, string column)
        {
            return row.IsNull(column) ? 0 : Convert.ToDouble(row[column], CultureInfo.InvariantCulture);
        }

        // Reads a date cell. The database keeps dates as text like "2026-09-10 10:00".
        public static DateTime Date(DataRow row, string column)
        {
            string text = Text(row, column);
            return text == "" ? DateTime.MinValue : DateTime.Parse(text, CultureInfo.InvariantCulture);
        }

        // Turns a date into the text format the database stores, e.g. "2026-09-10 10:00".
        public static string ToDbDateTime(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }

        // Turns a date into text WITHOUT the time, e.g. "2026-09-10".
        public static string ToDbDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}