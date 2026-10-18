namespace FuturePath.Entities
{
    // One faculty, for example "Commerce" or "Science" (one row of the Faculty table).
    public class Faculty
    {
        public int FacultyID { get; set; }
        public string FacultyName { get; set; } = "";
        public bool IsActive { get; set; } = true;   // false = switched off, hidden from learners
    }
}