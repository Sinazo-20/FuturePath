namespace FuturePath.Entities
{
    public class Learner
    {
        public int LearnerID { get; set; }            // Unique number the database gives this learner
        public string FirstName { get; set; } = "";   // Optional - a learner may stay anonymous
        public string LastName { get; set; } = "";    // Optional
        public int GradeLevelID { get; set; }         // 11 or 12 (the learner's school grade)
        public string SchoolName { get; set; } = "";
        public string Province { get; set; } = "";
        public string ContactDetails { get; set; } = "";

        // Gives a name to show on screen and in reports.
        // If the learner gave no name, we show "Anonymous learner" instead of a blank.
        public string DisplayName
        {
            get
            {
                string name = (FirstName + " " + LastName).Trim();
                return name == "" ? "Anonymous learner" : name;
            }
        }
    }
}