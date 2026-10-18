using System.Collections.Generic;
using System.Linq;
using FuturePath.Data;
using FuturePath.Entities;

namespace FuturePath.Business
{
    public static class InterestScorer
    {
        // True when the learner answered EVERY active question.
        public static bool IsComplete(Dictionary<int, bool> answers, List<InterestQuestion> questions)
        {
            return questions.All(q => answers.ContainsKey(q.InterestQuestionID));
        }

        // Works out the learner's score (0.0 to 1.0) for each interest category.
        // Result example: { category 1 -> 1.0, category 2 -> 0.5, ... }
        public static Dictionary<int, double> CategoryScores(Dictionary<int, bool> answers, List<InterestQuestion> questions)
        {
            var yesCount = new Dictionary<int, int>();    // how many Yes answers per category
            var totalCount = new Dictionary<int, int>();  // how many questions were answered per category

            foreach (InterestQuestion q in questions)
            {
                if (!answers.ContainsKey(q.InterestQuestionID)) continue;   // skip unanswered questions

                int category = q.InterestCategoryID;
                totalCount[category] = totalCount.GetValueOrDefault(category) + 1;
                if (answers[q.InterestQuestionID])
                {
                    yesCount[category] = yesCount.GetValueOrDefault(category) + 1;
                }
            }

            var scores = new Dictionary<int, double>();
            foreach (KeyValuePair<int, int> item in totalCount)
            {
                scores[item.Key] = (double)yesCount.GetValueOrDefault(item.Key) / item.Value;
            }
            return scores;
        }

        // How well the learner's interests suit ONE degree (0.0 = not at all, 1.0 = perfectly).
        // It is the average of the learner's scores for the categories linked to that degree.
        public static double ForProgramme(int programmeId, Dictionary<int, double> categoryScores)
        {
            List<int> categories = ProgrammeRepository.GetInterestCategoryIds(programmeId);
            if (categories.Count == 0) return 0;

            double total = 0;
            foreach (int category in categories)
            {
                total += categoryScores.GetValueOrDefault(category);   // a category never answered counts as 0
            }
            return total / categories.Count;
        }
    }
}