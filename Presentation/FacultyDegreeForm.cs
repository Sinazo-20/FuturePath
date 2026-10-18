using System.Windows.Forms;
using FuturePath.Business;

namespace FuturePath.Presentation
{
    // PURPOSE: Screen 3: the learner picks a faculty, then one degree inside it.
    // The degree list is filled again each time the faculty changes, so only degrees
    // from the chosen faculty (and only active ones) can be picked.
    public class FacultyDegreeForm : BaseForm
    {
        private readonly ComboBox facultyBox = new ComboBox();
        private readonly ComboBox programmeBox = new ComboBox();

        public FacultyDegreeForm() : base("Faculty and Programme Selection")
        {
            Bind(facultyBox, AdvisingController.GetFaculties(), "FacultyName", "FacultyID");
            Bind(programmeBox, new object[0], "ProgrammeName", "DegreeProgrammeID");   // empty until a faculty is chosen

            // When a different faculty is chosen, refill the degree list for that faculty.
            // (This is hooked up AFTER the first fill, so it does not fire too early.)
            facultyBox.SelectedIndexChanged += (s, e) =>
                Bind(programmeBox, AdvisingController.GetProgrammes(SelectedId(facultyBox)), "ProgrammeName", "DegreeProgrammeID");

            AddWide(new System.Windows.Forms.Label { Text = "Choose the degree you want more information about.", AutoSize = true });
            AddRow("Faculty *", facultyBox);
            AddRow("Degree programme *", programmeBox);

            AddButton("Next >", (s, e) => Next());
            AddButton("Back", (s, e) => GoBack());
            AddButton("Cancel", (s, e) => GoToWelcome());
        }

        // Runs when the learner clicks Next.
        private void Next()
        {
            string error = AdvisingController.SelectDegree(SelectedId(facultyBox), SelectedId(programmeBox));
            if (error != "")
            {
                ShowError(error);
                return;
            }
            ShowError("");
            GoTo(new QuestionnaireForm());
        }
    }
}