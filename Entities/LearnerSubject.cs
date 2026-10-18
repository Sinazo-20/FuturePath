namespace FuturePath.Entities
{
    // for example "Mathematics - 70". A learner has several of these (7 in our design).
    public class LearnerSubject
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = "";  // Copied in so we can show it without another lookup
        public int Mark { get; set; }                  // A percentage from 0 to 100
    }
}