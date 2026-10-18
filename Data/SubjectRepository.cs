using System.Collections.Generic;
using System.Data;
using FuturePath.Entities;

namespace FuturePath.Data
{
    public static class SubjectRepository
    {
        // Returns every subject that is switched on, in alphabetical order.
        public static List<Subject> GetActive()
        {
            var list = new List<Subject>();
            DataTable table = DatabaseHelper.Query("SELECT * FROM Subject WHERE IsActive = 1 ORDER BY SubjectName");
            foreach (DataRow row in table.Rows)
            {
                list.Add(new Subject
                {
                    SubjectID = Row.Int(row, "SubjectID"),
                    SubjectName = Row.Text(row, "SubjectName"),
                    IsActive = Row.Bool(row, "IsActive")
                });
            }
            return list;
        }
    }
}