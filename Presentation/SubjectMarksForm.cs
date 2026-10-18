using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    // PURPOSE: Screen 2: the learner adds their subjects and marks one at a time.
    // The controller refuses duplicate subjects, marks outside 0-100, and more than 7 subjects.
    // The learner cannot continue until they have at least 5 subjects.
    public class SubjectMarksForm : BaseForm
    {
        private readonly ComboBox subjectBox = new ComboBox();
        private readonly TextBox markBox = new TextBox();
        private readonly ListBox subjectList = new ListBox { Height = 170, Width = 500 };   // shows the subjects added so far
        private readonly Label countLabel = new Label { AutoSize = true };

        public SubjectMarksForm() : base("Subjects and Marks")
        {
            // Fill the subject drop-down from the database (via the controller).
            Bind(subjectBox, AdvisingController.GetSubjects(), "SubjectName", "SubjectID");

            AddRow("Subject *", subjectBox);
            AddRow("Mark (%) *", markBox);
            AddWide(NewButton("Add subject", (s, e) => AddSubject()));
            AddWide(subjectList);
            AddWide(countLabel);
            AddWide(NewButton("Remove selected subject", (s, e) => RemoveSubject(), 180));

            AddButton("Next >", (s, e) => Next());
            AddButton("Back", (s, e) => GoBack());
            AddButton("Cancel", (s, e) => GoToWelcome());

            ShowSubjects();   // in case the learner came back to this screen: show the subjects already added
        }

        // Redraws the list of subjects and the "Subjects added" count.
        private void ShowSubjects()
        {
            subjectList.Items.Clear();
            foreach (LearnerSubject subject in SessionState.CurrentSession.Subjects)
                subjectList.Items.Add(subject.SubjectName + " - " + subject.Mark + "%");

            countLabel.Text = "Subjects added: " + SessionState.CurrentSession.Subjects.Count +
                              "  (you need between " + Validator.MinSubjects + " and " + Validator.MaxSubjects + ")";
        }

        // Runs when the learner clicks "Add subject".
        private void AddSubject()
        {
            string error = AdvisingController.AddSubject(SelectedId(subjectBox), subjectBox.Text, markBox.Text);
            if (error != "")
            {
                ShowError(error);
                return;
            }
            ShowError("");
            markBox.Clear();                // get ready for the next subject
            subjectBox.SelectedIndex = -1;
            ShowSubjects();
        }

        // Runs when the learner clicks "Remove selected subject".
        private void RemoveSubject()
        {
            if (subjectList.SelectedIndex < 0)
            {
                ShowError("Please click a subject in the list first.");
                return;
            }
            ShowError("");
            // The list shows subjects in the same order as the learner's saved list, so the clicked position matches.
            AdvisingController.RemoveSubject(SessionState.CurrentSession.Subjects[subjectList.SelectedIndex].SubjectID);
            ShowSubjects();
        }

        // Runs when the learner clicks Next: are there enough subjects?
        private void Next()
        {
            string error = AdvisingController.CheckMarksComplete();
            if (error != "")
            {
                ShowError(error);
                return;
            }
            ShowError("");
            GoTo(new FacultyDegreeForm());
        }
    }
}