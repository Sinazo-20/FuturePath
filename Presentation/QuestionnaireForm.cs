using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FuturePath.Business;
using FuturePath.Entities;

namespace FuturePath.Presentation
{
    // PURPOSE: Screen 4: the Yes/No interest questionnaire.
    // The questions come from the database, so if the Administrator adds a question it appears here
    // automatically. Every question must be answered.
    // When the learner submits, the system runs the eligibility check, saves the visit,
    // and opens the Eligibility Result screen.
    public class QuestionnaireForm : BaseForm
    {
        // For every question we keep its "Yes" and "No" buttons, so we can read the answers later.
        private readonly Dictionary<int, RadioButton> yesButtons = new Dictionary<int, RadioButton>();
        private readonly Dictionary<int, RadioButton> noButtons = new Dictionary<int, RadioButton>();

        public QuestionnaireForm() : base("Interest Questionnaire")
        {
            AddWide(new Label { Text = "Tell us what interests you. Answer every question with Yes or No.", AutoSize = true });

            // One question at a time: the question text, then a Yes/No pair underneath it.
            foreach (InterestQuestion question in AdvisingController.GetQuestions())
            {
                var yes = new RadioButton { Text = "Yes", AutoSize = true };
                var no = new RadioButton { Text = "No", AutoSize = true };

                // Each pair sits in its own little panel, so choosing Yes for one question
                // does not un-choose the answers to the other questions.
                // The panel has a fixed small size (just big enough for "Yes" and "No"),
                // so it never covers the questions below it.
                var choice = new FlowLayoutPanel { Size = new Size(160, 30) };
                choice.Controls.Add(yes);
                choice.Controls.Add(no);

                yesButtons[question.InterestQuestionID] = yes;
                noButtons[question.InterestQuestionID] = no;

                AddWide(new Label { Text = question.QuestionText, AutoSize = true });
                AddWide(choice);
            }

            // Buttons are added from the right: Submit, then Back, then Cancel.
            AddButton("Submit", (s, e) => Submit());
            AddButton("Back", (s, e) => GoBack());
            AddButton("Cancel", (s, e) => GoToWelcome());
        }

        // Runs when the learner clicks Submit.
        private void Submit()
        {
            // Read the answers: Yes = true, No = false. A question with neither chosen is left out.
            var answers = new Dictionary<int, bool>();
            foreach (KeyValuePair<int, RadioButton> item in yesButtons)
            {
                if (item.Value.Checked) answers[item.Key] = true;
                else if (noButtons[item.Key].Checked) answers[item.Key] = false;
            }

            // The controller refuses if any question was left unanswered.
            string error = AdvisingController.SubmitQuestionnaire(answers);
            if (error != "")
            {
                ShowError(error);
                return;
            }
            ShowError("");

            // Run the eligibility check, find alternatives, and save the whole visit.
            AdvisingController.RunAdvisory();

            // Show the result screen.
            GoTo(new EligibilityResultForm());
        }
    }
}