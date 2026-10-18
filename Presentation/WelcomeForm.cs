using System.Drawing;
using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    // PURPOSE: The first screen. The user says who they are: Learner, Career Advisor or Administrator.
    // A Learner goes straight in. Staff must log in first.
    public class WelcomeForm : BaseForm
    {
        public WelcomeForm() : base("Welcome")
        {
            AddWide(new Label
            {
                Text = "Future Path helps Grade 11 and Grade 12 learners check whether their subjects and marks " +
                       "match the degree they are interested in, find alternatives, and see funding options.",
                AutoSize = true,
                MaximumSize = new Size(720, 0)   // wrap the text instead of running off the screen
            });

            // Learner: start a brand new visit (wipes any previous learner's information), then open the profile screen.
            AddWide(NewButton("I am a Learner", (s, e) =>
            {
                AdvisingController.StartOver();
                GoTo(new LearnerProfileForm());
            }, 280));

            // Staff: open the login screen for the chosen role.
            AddWide(NewButton("I am a Career Advisor", (s, e) => GoTo(new StaffLoginForm(Outcomes.RoleAdvisor)), 280));
            AddWide(NewButton("I am a System Administrator", (s, e) => GoTo(new StaffLoginForm(Outcomes.RoleAdministrator)), 280));

            

            AddButton("Exit", (s, e) => Close());   // closing this screen ends the program
        }

       
    }
}