using System;
using System.Data;
using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    public class QuestionsAdminForm : BaseForm
    {
        private readonly DataGridView grid;
        private readonly ComboBox categoryBox = new ComboBox();
        private readonly TextBox questionBox = new TextBox();

        public QuestionsAdminForm() : base("Questionnaire Questions")
        {
            grid = NewGrid(BuildTable(), 220);
            grid.Columns["ID"]!.FillWeight = 12;
            AddWide(grid);

            Bind(categoryBox, AdminController.GetCategories(), "CategoryName", "InterestCategoryID");
            AddRow("Interest category *", categoryBox);
            AddRow("Question *", questionBox);
            AddWide(NewButton("Add question", (s, e) => Add(), 130));

            AddButton("Back", (s, e) => GoBack());
            AddButton("Activate", (s, e) => SetActive(true));
            AddButton("Deactivate", (s, e) => SetActive(false));
        }

        // Builds the table of all questions (including switched-off ones).
        private static DataTable BuildTable()
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Category");
            table.Columns.Add("Question");
            table.Columns.Add("Status");
            foreach (InterestQuestion q in AdminController.GetAllQuestions())
                table.Rows.Add(q.InterestQuestionID, q.CategoryName, q.QuestionText, q.IsActive ? "Active" : "Inactive");
            return table;
        }

        private void Reload()
        {
            grid.DataSource = BuildTable();
            grid.Columns["ID"]!.FillWeight = 12;
        }

        // Runs when the Administrator clicks "Add question".
        private void Add()
        {
            string error = AdminController.AddQuestion(SelectedId(categoryBox), questionBox.Text);
            ShowError(error);
            if (error == "")
            {
                questionBox.Clear();
                Reload();
            }
        }

        // Switches the selected question on or off (after asking "are you sure?").
        private void SetActive(bool active)
        {
            if (grid.CurrentRow == null)
            {
                ShowError("Please click a question in the table first.");
                return;
            }
            if (!Confirm((active ? "Activate" : "Deactivate") + " the selected question?")) return;

            string error = AdminController.SetQuestionActive(Convert.ToInt32(grid.CurrentRow.Cells["ID"].Value), active);
            ShowError(error);
            if (error == "") Reload();
        }
    }
}