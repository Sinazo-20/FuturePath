using System.Drawing;
using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    // PURPOSE: Screen 1 of the learner's journey: basic profile details.
    // Only the grade is required. Name, school, province and contact are optional,
    // because a learner may continue without giving personal information (privacy).
    public class LearnerProfileForm : BaseForm
    {
        private readonly TextBox firstName = new TextBox();
        private readonly TextBox lastName = new TextBox();
        private readonly ComboBox grade = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox school = new TextBox();
        private readonly ComboBox province = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox contact = new TextBox();

        public LearnerProfileForm() : base("Learner Profile")
        {
            // The choices in the two drop-down lists.
            grade.Items.AddRange(new object[] { "Grade 11", "Grade 12" });
            province.Items.AddRange(new object[] { "", "Eastern Cape", "Free State", "Gauteng", "KwaZulu-Natal", "Limpopo",
                                                    "Mpumalanga", "Northern Cape", "North West", "Western Cape" });

            AddWide(new Label
            {
                Text = "Tell us a little about yourself. Your name, school and contact details are optional. Fields marked * are required.",
                AutoSize = true,
                MaximumSize = new Size(720, 0)
            });
            AddRow("First name", firstName);
            AddRow("Last name", lastName);
            AddRow("Current grade *", grade);
            AddRow("School", school);
            AddRow("Province", province);
            AddRow("Email / contact details", contact);

            AddButton("Next >", (s, e) => Next());
            AddButton("Cancel", (s, e) => GoToWelcome());
        }

        // Runs when the learner clicks Next.
        private void Next()
        {
            // Put what was typed into a Learner object.
            // Grade: the first list item (index 0) is Grade 11 and the second (index 1) is Grade 12,
            // so index + 11 gives 11 or 12. Nothing chosen (index -1) gives 10, which the Validator rejects.
            var learner = new Learner
            {
                FirstName = firstName.Text,
                LastName = lastName.Text,
                GradeLevelID = grade.SelectedIndex + 11,
                SchoolName = school.Text,
                Province = province.Text,
                ContactDetails = contact.Text
            };

            // The controller validates it and keeps it for the next screens.
            string error = AdvisingController.SaveProfile(learner);
            if (error != "")
            {
                ShowError(error);
                return;
            }
            ShowError("");
            GoTo(new SubjectMarksForm());
        }
    }
}