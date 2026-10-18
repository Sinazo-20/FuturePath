using System.Collections.Generic;
using System.Data;
using FuturePath.Entities;

namespace FuturePath.Data
{
    public static class ProgrammeRepository
    {
        // Returns every faculty that is switched on.
        public static List<Faculty> GetActiveFaculties()
        {
            var list = new List<Faculty>();
            DataTable table = DatabaseHelper.Query("SELECT * FROM Faculty WHERE IsActive = 1 ORDER BY FacultyName");
            foreach (DataRow row in table.Rows)
            {
                list.Add(new Faculty
                {
                    FacultyID = Row.Int(row, "FacultyID"),
                    FacultyName = Row.Text(row, "FacultyName"),
                    IsActive = Row.Bool(row, "IsActive")
                });
            }
            return list;
        }

        // Returns degree programmes.
        //   facultyId = 0  -> degrees from ALL faculties;  any other number -> only that faculty.
        //   activeOnly = true -> hide switched-off degrees (learners); false -> show all (Administrator).
        public static List<DegreeProgramme> GetProgrammes(int facultyId, bool activeOnly)
        {
            var list = new List<DegreeProgramme>();
            DataTable table = DatabaseHelper.Query(
                "SELECT d.*, f.FacultyName FROM DegreeProgramme d " +
                "JOIN Faculty f ON f.FacultyID = d.FacultyID " +
                "WHERE (@p0 = 0 OR d.FacultyID = @p0) AND (@p1 = 0 OR d.IsActive = 1) " +
                "ORDER BY d.ProgrammeName",
                facultyId, activeOnly ? 1 : 0);
            foreach (DataRow row in table.Rows)
            {
                list.Add(MapProgramme(row));
            }
            return list;
        }

        // Loads one degree by its ID. Returns null if it does not exist.
        public static DegreeProgramme? GetById(int programmeId)
        {
            DataTable table = DatabaseHelper.Query(
                "SELECT d.*, f.FacultyName FROM DegreeProgramme d " +
                "JOIN Faculty f ON f.FacultyID = d.FacultyID WHERE d.DegreeProgrammeID = @p0", programmeId);
            return table.Rows.Count == 0 ? null : MapProgramme(table.Rows[0]);
        }

        // Turns one database row into a DegreeProgramme object.
        private static DegreeProgramme MapProgramme(DataRow row)
        {
            return new DegreeProgramme
            {
                DegreeProgrammeID = Row.Int(row, "DegreeProgrammeID"),
                FacultyID = Row.Int(row, "FacultyID"),
                FacultyName = Row.Text(row, "FacultyName"),
                ProgrammeName = Row.Text(row, "ProgrammeName"),
                IsActive = Row.Bool(row, "IsActive")
            };
        }

        // Returns every rule (requirement) that one degree demands.
        public static List<ProgrammeRequirement> GetRequirements(int programmeId)
        {
            var list = new List<ProgrammeRequirement>();
            DataTable table = DatabaseHelper.Query(
                "SELECT r.*, rs.SubjectName AS RequiredSubjectName, alt.SubjectName AS AlternativeSubjectName " +
                "FROM ProgrammeRequirement r " +
                "LEFT JOIN Subject rs ON rs.SubjectID = r.RequiredSubjectID " +
                "LEFT JOIN Subject alt ON alt.SubjectID = r.AlternativeSubjectID " +
                "WHERE r.DegreeProgrammeID = @p0 ORDER BY r.ProgrammeRequirementID", programmeId);
            foreach (DataRow row in table.Rows)
            {
                list.Add(new ProgrammeRequirement
                {
                    ProgrammeRequirementID = Row.Int(row, "ProgrammeRequirementID"),
                    DegreeProgrammeID = Row.Int(row, "DegreeProgrammeID"),
                    RequiredSubjectID = Row.NullableInt(row, "RequiredSubjectID"),
                    RequiredSubjectName = Row.Text(row, "RequiredSubjectName"),
                    AlternativeSubjectID = Row.NullableInt(row, "AlternativeSubjectID"),
                    AlternativeSubjectName = Row.Text(row, "AlternativeSubjectName"),
                    MinimumMark = Row.NullableInt(row, "MinimumMark"),
                    MinimumOverallAverage = Row.NullableInt(row, "MinimumOverallAverage"),
                    AdvisoryNote = Row.Text(row, "AdvisoryNote")
                });
            }
            return list;
        }

        // Returns the ID numbers of the interest categories that suit one degree.
        public static List<int> GetInterestCategoryIds(int programmeId)
        {
            var list = new List<int>();
            DataTable table = DatabaseHelper.Query(
                "SELECT InterestCategoryID FROM DegreeInterestMapping WHERE DegreeProgrammeID = @p0", programmeId);
            foreach (DataRow row in table.Rows)
            {
                list.Add(Row.Int(row, "InterestCategoryID"));
            }
            return list;
        }

        // ---------- Administrator changes ----------

        // Adds a new degree. Also links it to an interest category so recommendations can use it.
        public static int AddProgramme(DegreeProgramme programme, int interestCategoryId)
        {
            programme.DegreeProgrammeID = DatabaseHelper.Insert(
                "INSERT INTO DegreeProgramme (FacultyID, ProgrammeName) VALUES (@p0, @p1)",
                programme.FacultyID, programme.ProgrammeName);
            DatabaseHelper.Execute(
                "INSERT INTO DegreeInterestMapping VALUES (@p0, @p1)",
                programme.DegreeProgrammeID, interestCategoryId);
            return programme.DegreeProgrammeID;
        }

        // Switches a degree on or off. Switched-off degrees are never shown to learners.
        public static void SetProgrammeActive(int programmeId, bool isActive)
        {
            DatabaseHelper.Execute("UPDATE DegreeProgramme SET IsActive = @p0 WHERE DegreeProgrammeID = @p1",
                isActive ? 1 : 0, programmeId);
        }

        // Adds a new requirement (rule) to a degree.
        public static void AddRequirement(ProgrammeRequirement r)
        {
            DatabaseHelper.Execute(
                "INSERT INTO ProgrammeRequirement (DegreeProgrammeID, RequiredSubjectID, AlternativeSubjectID, " +
                "MinimumMark, MinimumOverallAverage, AdvisoryNote) VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
                r.DegreeProgrammeID, r.RequiredSubjectID, r.AlternativeSubjectID,
                r.MinimumMark, r.MinimumOverallAverage, r.AdvisoryNote);
        }

        // Removes one requirement from a degree.
        public static void DeleteRequirement(int requirementId)
        {
            DatabaseHelper.Execute("DELETE FROM ProgrammeRequirement WHERE ProgrammeRequirementID = @p0", requirementId);
        }
    }
}