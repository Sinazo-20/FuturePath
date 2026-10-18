using System;
using System.Collections.Generic;
using System.Linq;
using FuturePath.Data;
using FuturePath.Entities;

namespace FuturePath.Business
{
    public static class AdminController
    {
        // Returns "" for an Administrator, otherwise an error message.
        private static string RequireAdmin()
        {
            return SessionState.IsAdministrator ? "" : "Only Administrators can make this change.";
        }

        // ---------- Lists for the maintenance screen (include switched-off items) ----------
        public static List<DegreeProgramme> GetAllProgrammes() { return ProgrammeRepository.GetProgrammes(0, false); }
        public static List<ProgrammeRequirement> GetRequirements(int programmeId) { return ProgrammeRepository.GetRequirements(programmeId); }
        public static List<InterestQuestion> GetAllQuestions() { return QuestionnaireRepository.GetQuestions(false); }
        public static List<InterestCategory> GetCategories() { return QuestionnaireRepository.GetCategories(); }
        public static List<FundingOption> GetAllFunding() { return FundingRepository.GetAll(); }

        // ---------- Degrees ----------
        public static string AddProgramme(int facultyId, string name, int interestCategoryId)
        {
            string error = RequireAdmin();
            if (error != "") return error;
            if (facultyId == 0) return "Please choose a faculty.";
            error = Validator.CheckRequired(name, "Programme name");
            if (error != "") return error;
            if (interestCategoryId == 0) return "Please choose an interest category for this degree.";

            name = name.Trim();
            // Two degrees may not have the same name.
            if (ProgrammeRepository.GetProgrammes(0, false).Any(p => p.ProgrammeName.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return "A degree with that name already exists.";

            ProgrammeRepository.AddProgramme(new DegreeProgramme { FacultyID = facultyId, ProgrammeName = name }, interestCategoryId);
            return "";
        }

        public static string SetProgrammeActive(int programmeId, bool isActive)
        {
            string error = RequireAdmin();
            if (error != "") return error;
            if (programmeId == 0) return "Please select a degree first.";
            ProgrammeRepository.SetProgrammeActive(programmeId, isActive);
            return "";
        }

        // ---------- Requirements ----------
        // If a subject is chosen -> a SUBJECT rule, and "minimumText" is the minimum mark for it.
        // If no subject is chosen (subjectId = 0) -> an OVERALL AVERAGE rule, and "minimumText" is the minimum average.
        public static string AddRequirement(int programmeId, int subjectId, int alternativeSubjectId, string minimumText, string note)
        {
            string error = RequireAdmin();
            if (error != "") return error;
            if (programmeId == 0) return "Please select a degree first.";

            error = Validator.CheckMark(minimumText, out int minimum);   // minimum mark/average must be 0-100
            if (error != "") return error;

            var requirement = new ProgrammeRequirement { DegreeProgrammeID = programmeId, AdvisoryNote = note.Trim() };
            if (subjectId != 0)
            {
                if (alternativeSubjectId == subjectId) return "The alternative subject must be different from the required subject.";
                requirement.RequiredSubjectID = subjectId;
                requirement.AlternativeSubjectID = alternativeSubjectId == 0 ? null : alternativeSubjectId;
                requirement.MinimumMark = minimum;
            }
            else
            {
                requirement.MinimumOverallAverage = minimum;
            }
            ProgrammeRepository.AddRequirement(requirement);
            return "";
        }

        public static string DeleteRequirement(int requirementId)
        {
            string error = RequireAdmin();
            if (error != "") return error;
            ProgrammeRepository.DeleteRequirement(requirementId);
            return "";
        }

        // ---------- Questionnaire ----------
        public static string AddQuestion(int categoryId, string text)
        {
            string error = RequireAdmin();
            if (error != "") return error;
            if (categoryId == 0) return "Please choose an interest category.";
            error = Validator.CheckRequired(text, "Question text");
            if (error != "") return error;

            QuestionnaireRepository.AddQuestion(new InterestQuestion { InterestCategoryID = categoryId, QuestionText = text.Trim() });
            return "";
        }

        public static string SetQuestionActive(int questionId, bool isActive)
        {
            string error = RequireAdmin();
            if (error != "") return error;
            if (questionId == 0) return "Please select a question first.";
            QuestionnaireRepository.SetQuestionActive(questionId, isActive);
            return "";
        }

        // ---------- Funding ----------
        public static string AddFunding(string name, string type, string eligibility, string documents,
                                        string closingDateText, string contact, int facultyId)
        {
            string error = RequireAdmin();
            if (error != "") return error;
            error = Validator.CheckRequired(name, "Funding name");
            if (error != "") return error;
            error = Validator.CheckFundingType(type);
            if (error != "") return error;
            error = Validator.CheckDate(closingDateText, out DateTime closingDate);
            if (error != "") return error;
            if (facultyId == 0) return "Please choose the faculty this funding is linked to.";

            FundingRepository.Add(new FundingOption
            {
                FundingName = name.Trim(),
                FundingType = type,
                EligibilityDescription = eligibility.Trim(),
                RequiredDocuments = documents.Trim(),
                ClosingDate = closingDate,
                ContactApplicationInfo = contact.Trim()
            }, facultyId);
            return "";
        }

        public static string SetFundingActive(int fundingOptionId, bool isActive)
        {
            string error = RequireAdmin();
            if (error != "") return error;
            if (fundingOptionId == 0) return "Please select a funding option first.";
            FundingRepository.SetActive(fundingOptionId, isActive);
            return "";
        }
    }
}