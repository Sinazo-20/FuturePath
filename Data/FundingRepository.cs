using System;
using System.Collections.Generic;
using System.Data;
using FuturePath.Entities;

namespace FuturePath.Data
{
    public static class FundingRepository
    {
        // Returns the funding that applies to a learner's chosen degree: funding linked to the
        // degree's faculty OR linked to the degree itself. Switched-off (outdated) funding is never returned.
        public static List<FundingOption> GetForDegree(int facultyId, int programmeId)
        {
            DataTable table = DatabaseHelper.Query(
                "SELECT * FROM FundingOption f WHERE f.IsActive = 1 AND (" +
                "f.FundingOptionID IN (SELECT FundingOptionID FROM FundingFaculty WHERE FacultyID = @p0) OR " +
                "f.FundingOptionID IN (SELECT FundingOptionID FROM FundingProgramme WHERE DegreeProgrammeID = @p1)) " +
                "ORDER BY f.ClosingDate", facultyId, programmeId);
            return MapList(table);
        }

        // Returns ALL funding options, including switched-off ones (for the Administrator).
        public static List<FundingOption> GetAll()
        {
            return MapList(DatabaseHelper.Query("SELECT * FROM FundingOption ORDER BY FundingName"));
        }

        // Turns the rows of a table into a list of FundingOption objects.
        private static List<FundingOption> MapList(DataTable table)
        {
            var list = new List<FundingOption>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(new FundingOption
                {
                    FundingOptionID = Row.Int(row, "FundingOptionID"),
                    FundingName = Row.Text(row, "FundingName"),
                    FundingType = Row.Text(row, "FundingType"),
                    EligibilityDescription = Row.Text(row, "EligibilityDescription"),
                    RequiredDocuments = Row.Text(row, "RequiredDocuments"),
                    ClosingDate = Row.Date(row, "ClosingDate"),
                    ContactApplicationInfo = Row.Text(row, "ContactApplicationInfo"),
                    IsActive = Row.Bool(row, "IsActive"),
                    IsSample = Row.Bool(row, "IsSample")
                });
            }
            return list;
        }

        // Adds a new funding option and links it to a faculty (Administrator).
        public static void Add(FundingOption option, int facultyId)
        {
            option.FundingOptionID = DatabaseHelper.Insert(
                "INSERT INTO FundingOption (FundingName, FundingType, EligibilityDescription, RequiredDocuments, " +
                "ClosingDate, ContactApplicationInfo) VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
                option.FundingName, option.FundingType, option.EligibilityDescription,
                option.RequiredDocuments, Row.ToDbDate(option.ClosingDate), option.ContactApplicationInfo);
            DatabaseHelper.Execute("INSERT INTO FundingFaculty VALUES (@p0, @p1)", option.FundingOptionID, facultyId);
        }

        // Switches a funding option on or off (Administrator).
        public static void SetActive(int fundingOptionId, bool isActive)
        {
            DatabaseHelper.Execute("UPDATE FundingOption SET IsActive = @p0 WHERE FundingOptionID = @p1",
                isActive ? 1 : 0, fundingOptionId);
        }
    }
}