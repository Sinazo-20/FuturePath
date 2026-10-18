using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    // PURPOSE: The Administrator views, adds and removes the requirements (rules) of a degree.
    // Two kinds of rule:
    //   - Subject rule: choose a required subject (and optionally an accepted alternative) and the minimum mark.
    //   - Overall-average rule: leave the subject as "(none)" and enter the minimum average.
    public class RequirementsAdminForm : BaseForm
    {
        private readonly DataGridView grid;
        private readonly ComboBox programmeBox = new ComboBox();
        private readonly ComboBox subjectBox = new ComboBox();
        private readonly ComboBox alternativeBox = new ComboBox();
        private readonly TextBox minimumBox = new TextBox();
        private readonly TextBox noteBox = new TextBox();

        public RequirementsAdminForm() : base("Programme Requirements")
        {
            grid = NewGrid(BuildTable(), 150);
            grid.Columns["ID"]!.FillWeight = 15;

            Bind(programmeBox, AdminController.GetAllProgrammes(), "ProgrammeName", "DegreeProgrammeID");
            programmeBox.SelectedIndexChanged += (s, e) => Reload();   // show the rules of whichever degree is chosen

            // The subject lists start with "(none)" (ID 0) so a rule can have no subject / no alternative.
            Bind(subjectBox, SubjectList(), "SubjectName", "SubjectID");
            subjectBox.SelectedIndex = 0;
            Bind(alternativeBox, SubjectList(), "SubjectName", "SubjectID");
            alternativeBox.SelectedIndex = 0;

            AddRow("Programme *", programmeBox);
            AddWide(grid);
            AddWide(new Label { Text = "Add a rule. Leave the required subject as (none) for an overall-average rule.", AutoSize = true });
            AddRow("Required subject", subjectBox);
            AddRow("Alternative subject", alternativeBox);
            AddRow("Minimum (%) *", minimumBox);
            AddRow("Advisory note", noteBox);
            AddWide(NewButton("Add requirement", (s, e) => Add(), 140));

            AddButton("Back", (s, e) => GoBack());
            AddButton("Delete selected", (s, e) => Delete(), 130);
        }

        // The subject drop-down items: "(none)" first, then all active subjects.
        private static List<Subject> SubjectList()
        {
            var list = new List<Subject> { new Subject { SubjectID = 0, SubjectName = "(none)" } };
            list.AddRange(AdvisingController.GetSubjects());
            return list;
        }

        // Builds the table of rules for the chosen degree (empty if no degree is chosen yet).
        private DataTable BuildTable()
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Requirement");
            table.Columns.Add("Alternative");
            table.Columns.Add("Minimum");
            table.Columns.Add("Note");
            foreach (ProgrammeRequirement r in AdminController.GetRequirements(SelectedId(programmeBox)))
                table.Rows.Add(r.ProgrammeRequirementID, r.IsSubjectRule ? r.RequiredSubjectName : "Overall average",
                               r.AlternativeSubjectName, (r.MinimumMark ?? r.MinimumOverallAverage) + "%", r.AdvisoryNote);
            return table;
        }

        // Refreshes the table after a change or after a different degree is chosen.
        private void Reload()
        {
            grid.DataSource = BuildTable();
            grid.Columns["ID"]!.FillWeight = 15;
        }

        // Runs when the Administrator clicks "Add requirement".
        private void Add()
        {
            string error = AdminController.AddRequirement(SelectedId(programmeBox), SelectedId(subjectBox),
                                                          SelectedId(alternativeBox), minimumBox.Text, noteBox.Text);
            ShowError(error);
            if (error == "")
            {
                minimumBox.Clear();
                noteBox.Clear();
                subjectBox.SelectedIndex = 0;
                alternativeBox.SelectedIndex = 0;
                Reload();
            }
        }

        // Removes the selected rule (after asking "are you sure?").
        private void Delete()
        {
            if (grid.CurrentRow == null)
            {
                ShowError("Please click a requirement in the table first.");
                return;
            }
            if (!Confirm("Delete the selected requirement?")) return;

            string error = AdminController.DeleteRequirement(Convert.ToInt32(grid.CurrentRow.Cells["ID"].Value));
            ShowError(error);
            if (error == "") Reload();
        }
    }
}