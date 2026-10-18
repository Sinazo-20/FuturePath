using System;
using System.Collections.Generic;
using System.Linq;
using FuturePath.Data;
using FuturePath.Entities;

namespace FuturePath.Business
{
    public static class AdvisingController
    {
        // A short name for "the learner's current visit".
        private static AdvisorySession Current
        {
            get { return SessionState.CurrentSession; }
        }

        // ---------- Lists for the drop-down boxes ----------
        public static List<Subject> GetSubjects() { return SubjectRepository.GetActive(); }
        public static List<Faculty> GetFaculties() { return ProgrammeRepository.GetActiveFaculties(); }
        public static List<DegreeProgramme> GetProgrammes(int facultyId) { return ProgrammeRepository.GetProgrammes(facultyId, true); }
        public static List<InterestQuestion> GetQuestions() { return QuestionnaireRepository.GetQuestions(true); }

        // Wipes everything and starts a brand new visit (used by the "Start over" button).
        public static void StartOver()
        {
            SessionState.StartNewSession();
        }

        // ---------- Screen 1: Learner profile ----------
        public static string SaveProfile(Learner learner)
        {
            // Remove accidental spaces at the start and end of what was typed.
            learner.FirstName = learner.FirstName.Trim();
            learner.LastName = learner.LastName.Trim();
            learner.SchoolName = learner.SchoolName.Trim();
            learner.ContactDetails = learner.ContactDetails.Trim();

            string error = Validator.CheckProfile(learner);
            if (error != "") return error;

            Current.Learner = learner;   // remember the profile for the next screens
            return "";
        }

        // ---------- Screen 2: Subjects and marks ----------
        // Adds one subject with its mark to the learner's list.
        public static string AddSubject(int subjectId, string subjectName, string markText)
        {
            if (subjectId == 0) return "Please choose a subject.";

            string error = Validator.CheckSubjectAdd(Current.Subjects, subjectId);   // duplicate / list-full checks
            if (error != "") return error;

            error = Validator.CheckMark(markText, out int mark);                      // mark must be 0-100
            if (error != "") return error;

            Current.Subjects.Add(new LearnerSubject { SubjectID = subjectId, SubjectName = subjectName, Mark = mark });
            return "";
        }

        // Removes a subject from the learner's list.
        public static void RemoveSubject(int subjectId)
        {
            Current.Subjects.RemoveAll(s => s.SubjectID == subjectId);
        }

        // Called when the learner clicks Next on the marks screen: are there enough subjects?
        public static string CheckMarksComplete()
        {
            return Validator.CheckSubjectCount(Current.Subjects);
        }

        // ---------- Screen 3: Faculty and degree ----------
        public static string SelectDegree(int facultyId, int programmeId)
        {
            if (facultyId == 0) return "Please choose a faculty.";
            if (programmeId == 0) return "Please choose a degree programme.";

            // The degree must really exist, be switched on, and belong to the chosen faculty.
            DegreeProgramme? degree = ProgrammeRepository.GetById(programmeId);
            if (degree == null || !degree.IsActive || degree.FacultyID != facultyId)
                return "That degree is not available for the chosen faculty.";

            Current.PreferredFacultyID = facultyId;
            Current.SelectedDegreeProgrammeID = programmeId;
            Current.FacultyName = degree.FacultyName;
            Current.DegreeName = degree.ProgrammeName;
            return "";
        }

        // ---------- Screen 4: Questionnaire ----------
        // answers: question number -> true (Yes) or false (No).
        public static string SubmitQuestionnaire(Dictionary<int, bool> answers)
        {
            if (!InterestScorer.IsComplete(answers, GetQuestions()))
                return "Please answer every question with Yes or No.";

            Current.Answers = answers;
            return "";
        }

        // ---------- Screen 5: Run the check and save the visit ----------
        // Works out the outcome and the alternatives, then saves the whole visit to the database.
        public static string RunAdvisory()
        {
            // If this visit was already run and saved, do not save it a second time.
            if (Current.AdvisorySessionID != 0) return "";

            EligibilityResult result = EligibilityChecker.Check(Current, Current.SelectedDegreeProgrammeID ?? 0);
            Current.Outcome = result.Outcome;
            Current.OutcomeReason = result.Reason;
            Current.SessionDateTime = DateTime.Now;

            SessionState.Eligibility = result;
            SessionState.Alternatives = RecommendationEngine.Generate(Current);

            SessionRepository.Save(Current);                                                       // learner, visit, marks, answers
            SessionRepository.SaveRecommendations(Current.AdvisorySessionID, SessionState.Alternatives);
            return "";
        }

        // ---------- Funding ----------
        // Funding linked to the learner's chosen faculty or degree. Outdated funding is never included.
        public static List<FundingOption> GetFunding()
        {
            return FundingRepository.GetForDegree(Current.PreferredFacultyID ?? 0, Current.SelectedDegreeProgrammeID ?? 0);
        }

        // Records that the learner asked to see funding (used by Report 2).
        public static void RecordFundingRequested()
        {
            Current.FundingRequested = true;
            if (Current.AdvisorySessionID != 0) SessionRepository.SetFundingRequested(Current.AdvisorySessionID);
        }

        // ---------- The complete summary ----------
        // Collects everything about a visit into one AdvisorySummary parcel.
        // Used for the on-screen summary, the exported file and the Career Advisor's "View Summary".
        public static AdvisorySummary BuildSummary(AdvisorySession session)
        {
            var summary = new AdvisorySummary { Session = session };

            summary.Eligibility = EligibilityChecker.Check(session, session.SelectedDegreeProgrammeID ?? 0);
            summary.Alternatives = RecommendationEngine.Generate(session);
            summary.Funding = FundingRepository.GetForDegree(session.PreferredFacultyID ?? 0, session.SelectedDegreeProgrammeID ?? 0);
            summary.OverallAverage = session.Subjects.Count == 0 ? 0 : session.Subjects.Average(s => s.Mark);

            // Interest results: one percentage for each interest area the learner answered about.
            Dictionary<int, double> scores = InterestScorer.CategoryScores(session.Answers, GetQuestions());
            foreach (InterestCategory category in QuestionnaireRepository.GetCategories())
            {
                if (scores.ContainsKey(category.InterestCategoryID))
                    summary.InterestResults[category.CategoryName] = scores[category.InterestCategoryID];
            }
            return summary;
        }
    }
}