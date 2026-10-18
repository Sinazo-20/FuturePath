using System;
using System.Collections.Generic;
using System.Linq;
using FuturePath.Entities;

namespace FuturePath.Business
{
    public static class Validator
    {
        // A learner may enter at most 7 subjects (a design assumption in our specification).
        public const int MaxSubjects = 7;

        // A learner must enter at least 5 subjects before they can continue.
        public const int MinSubjects = 5;

        // The kinds of funding the system recognises.
        public static readonly string[] FundingTypes = { "Scholarship", "Bursary", "Financial Aid", "External Funding" };

        // Checks a mark typed into a text box. If it is valid, "mark" receives the number.
        public static string CheckMark(string text, out int mark)
        {
            mark = 0;
            text = text.Trim();
            if (text == "") return "Please enter a mark.";
            if (!int.TryParse(text, out mark)) return "The mark must be a whole number, for example 72.";
            if (mark < 0 || mark > 100) return "The mark must be between 0 and 100.";
            return "";
        }

        // Checks that a required text box is not empty. fieldName is used in the message.
        public static string CheckRequired(string value, string fieldName)
        {
            return value.Trim() == "" ? "Please fill in: " + fieldName + "." : "";
        }

        // Checks the learner profile. Names are optional (a learner may stay anonymous),
        // but the grade must be 11 or 12 and the text must not be too long.
        public static string CheckProfile(Learner learner)
        {
            if (learner.GradeLevelID != 11 && learner.GradeLevelID != 12)
                return "Please choose Grade 11 or Grade 12.";
            if (learner.FirstName.Length > 50 || learner.LastName.Length > 50)
                return "Names must be 50 characters or fewer.";
            if (learner.SchoolName.Length > 100 || learner.ContactDetails.Length > 100)
                return "School and contact details must be 100 characters or fewer.";
            return "";
        }

        // Checks that a subject can be added to the learner's list:
        // it must not already be in the list, and the list must not be full.
        public static string CheckSubjectAdd(List<LearnerSubject> current, int subjectId)
        {
            if (current.Any(s => s.SubjectID == subjectId))
                return "That subject has already been added. Each subject can only be entered once.";
            if (current.Count >= MaxSubjects)
                return "You can add a maximum of " + MaxSubjects + " subjects.";
            return "";
        }
        // Checks the learner has entered enough subjects. The marks screen uses this when
        // the learner clicks "Next", so they cannot continue with too few subjects.
        public static string CheckSubjectCount(List<LearnerSubject> current)
        {
            if (current.Count < MinSubjects)
                return "Please add at least " + MinSubjects + " subjects (you have added " + current.Count + ").";
            return "";
        }

        // Checks that typed text is a real date (used for funding closing dates).
        public static string CheckDate(string text, out DateTime date)
        {
            if (!DateTime.TryParse(text, out date)) return "Please enter a valid date, for example 2026-11-30.";
            return "";
        }

        // Checks that the funding type is one of the recognised types.
        public static string CheckFundingType(string type)
        {
            return FundingTypes.Contains(type) ? "" : "Please choose a valid funding type.";
        }
    }
}