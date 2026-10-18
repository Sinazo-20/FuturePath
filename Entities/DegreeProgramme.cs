namespace FuturePath.Entities
{
   
    public class DegreeProgramme
    {
        public int DegreeProgrammeID { get; set; }
        public int FacultyID { get; set; }              // Which faculty this degree belongs to
        public string FacultyName { get; set; } = "";   // Copied in for showing on screen and in reports
        public string ProgrammeName { get; set; } = "";
        public bool IsActive { get; set; } = true;      // false = outdated, never shown to learners
    }
}