using System;
using System.Data;
using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    // PURPOSE: The Administrator adds degree programmes and switches them on or off.
    // A switched-off ("Inactive") degree is never shown to learners or recommended.
    // Degrees are switched off instead of deleted, so old advising sessions stay valid.
    public class ProgrammesAdminForm : BaseForm
    {
        private readonly DataGridView grid;
        private readonly ComboBox facultyBox = new ComboBox();
        private readonly TextBox nameBox = new TextBox();
        private readonly ComboBox categoryBox = new ComboBox();

        public ProgrammesAdminForm() : base("Degree Programmes")
        {
            grid = NewGrid(BuildTable(), 190);
            grid.Columns["ID"]!.FillWeight = 15;   // make the ID column narrow
            AddWide(grid);

            Bind(facultyBox, AdvisingController.GetFaculties(), "FacultyName", "FacultyID");
            Bind(categoryBox, AdminController.GetCategories(), "CategoryName", "InterestCategoryID");
            AddRow("Faculty *", facultyBox);
            AddRow("Programme name *", nameBox);
            AddRow("Interest category *", categoryBox);
            AddWide(NewButton("Add programme", (s, e) => Add(), 130));

            AddButton("Back", (s, e) => GoBack());
            AddButton("Activate", (s, e) => SetActive(true));
            AddButton("Deactivate", (s, e) => SetActive(false));
        }

        // Builds the table of all degrees (including inactive ones).
        private static DataTable BuildTable()
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Programme");
            table.Columns.Add("Faculty");
            table.Columns.Add("Status");
            foreach (DegreeProgramme p in AdminController.GetAllProgrammes())
                table.Rows.Add(p.DegreeProgrammeID, p.ProgrammeName, p.FacultyName, p.IsActive ? "Active" : "Inactive");
            return table;
        }

        // Refreshes the table after a change.
        private void Reload()
        {
            grid.DataSource = BuildTable();
            grid.Columns["ID"]!.FillWeight = 15;
        }

        // Runs when the Administrator clicks "Add programme".
        private void Add()
        {
            string error = AdminController.AddProgramme(SelectedId(facultyBox), nameBox.Text, SelectedId(categoryBox));
            ShowError(error);
            if (error == "")
            {
                nameBox.Clear();
                Reload();
            }
        }

        // Switches the selected degree on or off (after asking "are you sure?").
        private void SetActive(bool active)
        {
            if (grid.CurrentRow == null)
            {
                ShowError("Please click a programme in the table first.");
                return;
            }
            int id = Convert.ToInt32(grid.CurrentRow.Cells["ID"].Value);
            string name = Convert.ToString(grid.CurrentRow.Cells["Programme"].Value) ?? "";

            if (!Confirm((active ? "Activate " : "Deactivate ") + name + "?")) return;

            string error = AdminController.SetProgrammeActive(id, active);
            ShowError(error);
            if (error == "") Reload();
        }
    }
}