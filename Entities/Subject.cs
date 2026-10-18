namespace FuturePath.Entities
{
    public class Subject
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = "";
        public bool IsActive { get; set; } = true;   // false = switched off, hidden from learners
    }
}