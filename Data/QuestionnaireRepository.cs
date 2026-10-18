using System.Collections.Generic;
using System.Data;
using FuturePath.Entities;

namespace FuturePath.Data
{
    public static class QuestionnaireRepository
    {
        // Returns the questions. activeOnly = true hides switched-off questions (what learners see).
        public static List<InterestQuestion> GetQuestions(bool activeOnly)
        {
            var list = new List<InterestQuestion>();
            DataTable table = DatabaseHelper.Query(
                "SELECT q.*, c.CategoryName FROM InterestQuestion q " +
                "JOIN InterestCategory c ON c.InterestCategoryID = q.InterestCategoryID " +
                "WHERE (@p0 = 0 OR q.IsActive = 1) ORDER BY q.InterestQuestionID", activeOnly ? 1 : 0);
            foreach (DataRow row in table.Rows)
            {
                list.Add(new InterestQuestion
                {
                    InterestQuestionID = Row.Int(row, "InterestQuestionID"),
                    InterestCategoryID = Row.Int(row, "InterestCategoryID"),
                    CategoryName = Row.Text(row, "CategoryName"),
                    QuestionText = Row.Text(row, "QuestionText"),
                    IsActive = Row.Bool(row, "IsActive")
                });
            }
            return list;
        }

        // Returns all the interest categories (for the Administrator's drop-down).
        public static List<InterestCategory> GetCategories()
        {
            var list = new List<InterestCategory>();
            DataTable table = DatabaseHelper.Query("SELECT * FROM InterestCategory ORDER BY CategoryName");
            foreach (DataRow row in table.Rows)
            {
                list.Add(new InterestCategory
                {
                    InterestCategoryID = Row.Int(row, "InterestCategoryID"),
                    CategoryName = Row.Text(row, "CategoryName")
                });
            }
            return list;
        }

        // Adds a new question (Administrator).
        public static void AddQuestion(InterestQuestion question)
        {
            DatabaseHelper.Execute(
                "INSERT INTO InterestQuestion (InterestCategoryID, QuestionText) VALUES (@p0, @p1)",
                question.InterestCategoryID, question.QuestionText);
        }

        // Switches a question on or off (Administrator).
        public static void SetQuestionActive(int questionId, bool isActive)
        {
            DatabaseHelper.Execute("UPDATE InterestQuestion SET IsActive = @p0 WHERE InterestQuestionID = @p1",
                isActive ? 1 : 0, questionId);
        }
    }
}