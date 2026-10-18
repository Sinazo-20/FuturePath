using System;
using System.Data;
using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    // The Administrator adds funding options and switches them on or off.
    // Switched-off (outdated) funding is never shown to learners.
    // The funding type must come from the fixed list, and the closing date must be a real date.
    public class FundingAdminForm : BaseForm
    {
        private readonly DataGridView grid;
        private readonly TextBox nameBox = new TextBox();
        private readonly ComboBox typeBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox eligibilityBox = new TextBox();
        private readonly TextBox documentsBox = new TextBox();
        private readonly TextBox closingBox = new TextBox();
        private readonly TextBox contactBox = new TextBox();
        private readonly ComboBox facultyBox = new ComboBox();

        public FundingAdminForm() : base("Funding Options")
        {
            grid = NewGrid(BuildTable(), 150);
            AddWide(grid);

            typeBox.Items.AddRange(Validator.FundingTypes);
            Bind(facultyBox, AdvisingController.GetFaculties(), "FacultyName", "FacultyID");
            closingBox.Text = DateTime.Today.AddYears(1).ToString("yyyy-MM-dd");   // an example date to show the format

            AddRow("Funding name *", nameBox);
            AddRow("Funding type *", typeBox);
            AddRow("Eligibility", eligibilityBox);
            AddRow("Required documents", documentsBox);
            AddRow("Closing date (yyyy-mm-dd) *", closingBox);
            AddRow("Contact / application", contactBox);
            AddRow("Linked faculty *", facultyBox);
            AddWide(NewButton("Add funding", (s, e) => Add(), 130));

            AddButton("Back", (s, e) => GoBack());
            AddButton("Activate", (s, e) => SetActive(true));
            AddButton("Deactivate", (s, e) => SetActive(false));
        }

        // Builds the table of all funding options (including switched-off ones).
        private static DataTable BuildTable()
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Funding");
            table.Columns.Add("Type");
            table.Columns.Add("Closing Date");
            table.Columns.Add("Status");
            foreach (FundingOption f in AdminController.GetAllFunding())
                table.Rows.Add(f.FundingOptionID, f.FundingName, f.FundingType, f.ClosingDate.ToString("dd MMM yyyy"), f.IsActive ? "Active" : "Inactive");
            return table;
        }

        // Refreshes the table after a change.
        private void Reload()
        {
            grid.DataSource = BuildTable();
        }

        // Runs when the Administrator clicks "Add funding".
        private void Add()
        {
            string error = AdminController.AddFunding(nameBox.Text, typeBox.Text, eligibilityBox.Text, documentsBox.Text,
                                                      closingBox.Text, contactBox.Text, SelectedId(facultyBox));
            ShowError(error);
            if (error == "")
            {
                nameBox.Clear();
                eligibilityBox.Clear();
                documentsBox.Clear();
                contactBox.Clear();
                Reload();
            }
        }

        // Switches the selected funding option on or off (after asking "are you sure?").
        private void SetActive(bool active)
        {
            if (grid.CurrentRow == null)
            {
                ShowError("Please click a funding option in the table first.");
                return;
            }
            string name = Convert.ToString(grid.CurrentRow.Cells["Funding"].Value) ?? "";
            if (!Confirm((active ? "Activate " : "Deactivate ") + name + "?")) return;

            string error = AdminController.SetFundingActive(Convert.ToInt32(grid.CurrentRow.Cells["ID"].Value), active);
            ShowError(error);
            if (error == "") Reload();
        }
    }
}
