using System.Data;
using FuturePath.Entities;

namespace FuturePath.Data
{
    public static class LearnerRepository
    {
        public static int Save(Learner learner)
        {
            if (learner.LearnerID == 0)
            {
                // New learner: add a row and remember the ID the database gave it.
                learner.LearnerID = DatabaseHelper.Insert(
                    "INSERT INTO Learner (FirstName, LastName, GradeLevelID, SchoolName, Province, ContactDetails) " +
                    "VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
                    learner.FirstName, learner.LastName, learner.GradeLevelID,
                    learner.SchoolName, learner.Province, learner.ContactDetails);
            }
            else
            {
                // Existing learner: change the row that already exists.
                DatabaseHelper.Execute(
                    "UPDATE Learner SET FirstName=@p0, LastName=@p1, GradeLevelID=@p2, " +
                    "SchoolName=@p3, Province=@p4, ContactDetails=@p5 WHERE LearnerID=@p6",
                    learner.FirstName, learner.LastName, learner.GradeLevelID,
                    learner.SchoolName, learner.Province, learner.ContactDetails, learner.LearnerID);
            }
            return learner.LearnerID;
        }

        // Loads one learner by ID. Returns null (nothing) if no such learner exists.
        public static Learner? GetById(int learnerId)
        {
            DataTable table = DatabaseHelper.Query("SELECT * FROM Learner WHERE LearnerID = @p0", learnerId);
            if (table.Rows.Count == 0) return null;

            DataRow row = table.Rows[0];
            return new Learner
            {
                LearnerID = Row.Int(row, "LearnerID"),
                FirstName = Row.Text(row, "FirstName"),
                LastName = Row.Text(row, "LastName"),
                GradeLevelID = Row.Int(row, "GradeLevelID"),
                SchoolName = Row.Text(row, "SchoolName"),
                Province = Row.Text(row, "Province"),
                ContactDetails = Row.Text(row, "ContactDetails")
            };
        }
    }
}