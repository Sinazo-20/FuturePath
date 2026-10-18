namespace FuturePath.Entities
{
    // One broad area of interest, for example "Science and Research".
    // Each questionnaire question belongs to one category.
    public class InterestCategory
    {
        public int InterestCategoryID { get; set; }
        public string CategoryName { get; set; } = "";
    }
}