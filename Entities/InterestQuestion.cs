namespace FuturePath.Entities
{
    //Yes/No answer in the interest questionnaire,
    
    public class InterestQuestion
    {
        public int InterestQuestionID { get; set; }
        public int InterestCategoryID { get; set; }       // Which interest area this question measures
        public string CategoryName { get; set; } = "";    // Copied in for showing on screen
        public string QuestionText { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}