using System;
using System.Collections.Generic;
using System.Linq;
using FuturePath.Data;
using FuturePath.Entities;

namespace FuturePath.Business
{
    public static class EligibilityChecker
    {
        // The interest match needed (0.5 = 50%) to be called a Strong Match.
        private const double StrongInterestThreshold = 0.5;

        // Checks the learner (inside "session") against ONE degree.
        public static EligibilityResult Check(AdvisorySession session, int programmeId)
        {
            var result = new EligibilityResult();

            // ----- Step 1: do we have enough information to do the check? -----
            if (programmeId == 0)
              
                return NeedMoreInfo(result, "No degree has been selected yet.");

            // Too few subjects means we cannot judge the learner fairly, so we ask for more information.
            if (session.Subjects.Count < Validator.MinSubjects)
                return NeedMoreInfo(result, "At least " + Validator.MinSubjects + " subjects with marks are needed to complete the check.");

            List<InterestQuestion> questions = QuestionnaireRepository.GetQuestions(true);
            if (!InterestScorer.IsComplete(session.Answers, questions))
                return NeedMoreInfo(result, "The interest questionnaire has not been completed, so the check could not be finished.");

            List<ProgrammeRequirement> requirements = ProgrammeRepository.GetRequirements(programmeId);
            if (requirements.Count == 0)
                return NeedMoreInfo(result, "No requirements are stored for this degree yet.");

            // ----- Step 2: go through every requirement of the degree -----
            double average = session.Subjects.Average(s => s.Mark);   // the learner's overall average
            var messages = new List<string>();                         // plain-English explanations

            foreach (ProgrammeRequirement req in requirements)
            {
                if (req.IsSubjectRule)
                {
                    // A subject rule, e.g. "Mathematics, at least 60%" (maybe "or Maths Literacy").
                    string label = req.RequiredSubjectName;
                    if (req.AlternativeSubjectID.HasValue) label += " (or " + req.AlternativeSubjectName + ")";
                    string needed = req.MinimumMark.HasValue ? req.MinimumMark.Value + "%" : "Any mark";

                    // Look for the required subject (or the accepted alternative) in the learner's subjects.
                    // If the learner has both, the better mark is used.
                    LearnerSubject? found = session.Subjects
                        .Where(s => s.SubjectID == req.RequiredSubjectID || s.SubjectID == req.AlternativeSubjectID)
                        .OrderByDescending(s => s.Mark)
                        .FirstOrDefault();

                    if (found == null)
                    {
                        // The learner does not take this subject at all.
                        result.MissingSubjects.Add(label);
                        messages.Add("You need " + label + ", but it is not in your subjects.");
                        result.Checks.Add(new RequirementCheck { Requirement = label, Needed = needed, YourResult = "Not taken", Status = "Gap" });
                    }
                    else if (req.MinimumMark.HasValue && found.Mark < req.MinimumMark.Value)
                    {
                        // The learner takes it, but the mark is too low.
                        string text = "Your " + found.SubjectName + " mark (" + found.Mark + "%) is below the minimum of " + req.MinimumMark.Value + "%.";
                        result.LowMarks.Add(text);
                        messages.Add(text);
                        result.Checks.Add(new RequirementCheck { Requirement = label, Needed = needed, YourResult = found.Mark + "%", Status = "Gap" });
                    }
                    else
                    {
                        // Requirement met.
                        result.Checks.Add(new RequirementCheck { Requirement = label, Needed = needed, YourResult = found.Mark + "%", Status = "Met" });
                    }
                }
                else
                {
                    // An overall-average rule, e.g. "overall average of at least 55%".
                    int minimum = req.MinimumOverallAverage ?? 0;
                    bool met = average >= minimum;
                    result.Checks.Add(new RequirementCheck
                    {
                        Requirement = "Overall average",
                        Needed = minimum + "%",
                        YourResult = Math.Round(average) + "%",
                        Status = met ? "Met" : "Gap"
                    });
                    if (!met)
                    {
                        string text = "Your overall average (" + Math.Round(average) + "%) is below the minimum of " + minimum + "%.";
                        result.LowMarks.Add(text);
                        messages.Add(text);
                    }
                }
            }

            // ----- Step 3: how well do the learner's interests suit this degree? -----
            Dictionary<int, double> categoryScores = InterestScorer.CategoryScores(session.Answers, questions);
            result.InterestAlignment = InterestScorer.ForProgramme(programmeId, categoryScores);
            double interestPercent = Math.Round(result.InterestAlignment * 100);

            // ----- Step 4: decide the outcome -----
            if (result.MissingSubjects.Count > 0 || result.LowMarks.Count >= 2)
            {
                result.Outcome = Outcomes.NotCurrentlyEligible;
                result.Reason = string.Join(" ", messages) + " Please look at the alternative degrees suggested for you.";
            }
            else if (result.LowMarks.Count == 1)
            {
                result.Outcome = Outcomes.PossibleMatch;
                result.Reason = string.Join(" ", messages) + " Improving this mark would make you a stronger match.";
            }
            else if (result.InterestAlignment >= StrongInterestThreshold)
            {
                result.Outcome = Outcomes.StrongMatch;
                result.Reason = "You meet all the subject and mark requirements and your interests suit this degree (" + interestPercent + "% interest match).";
            }
            else
            {
                result.Outcome = Outcomes.PossibleMatch;
                result.Reason = "You meet all the subject and mark requirements, but your interests only match this degree by " +
                                interestPercent + "%. You may want to explore the alternatives as well.";
            }
            return result;
        }

        // Builds a "More Information Needed" answer with the given reason.
        private static EligibilityResult NeedMoreInfo(EligibilityResult result, string reason)
        {
            result.Outcome = Outcomes.MoreInformationNeeded;
            result.Reason = reason;
            return result;
        }
    }
}