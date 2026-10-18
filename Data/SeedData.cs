using System;
using System.Text;
using FuturePath.Helpers;

namespace FuturePath.Data
{
    public static class SeedData
    {
        // One test learner + their advising session. Used in the list further down.
        private class TestSession
        {
            public string First, Last, School, Province;
            public int Grade, FacultyId, DegreeId;
            public string Outcome, Reason;
            public bool Funding, Export;
            public string Marks;   // "subjectID:mark,subjectID:mark" e.g. "1:72,3:70"
            public string Answers; // one digit per question: 1 = Yes, 0 = No ("" = questionnaire not done)
        }

        public static void Run()
        {
            var sql = new StringBuilder();
            sql.Append("BEGIN TRANSACTION;"); // everything below is saved together or not at all

            // ---------- Lookup data ----------
            sql.Append("INSERT INTO GradeLevel VALUES (11,'Grade 11'),(12,'Grade 12');");

            // 16 subjects (IDs 1-16 are used by the marks lists further down)
            sql.Append(@"INSERT INTO Subject (SubjectID, SubjectName) VALUES
                (1,'English'),(2,'Afrikaans'),(3,'Mathematics'),(4,'Mathematical Literacy'),
                (5,'Physical Sciences'),(6,'Life Sciences'),(7,'Accounting'),(8,'Business Studies'),
                (9,'Economics'),(10,'Geography'),(11,'History'),(12,'Computer Applications Technology'),
                (13,'Life Orientation'),(14,'Tourism'),(15,'Engineering Graphics and Design'),(16,'Visual Arts');");

            // 5 faculties
            sql.Append(@"INSERT INTO Faculty (FacultyID, FacultyName) VALUES
                (1,'Commerce'),(2,'Humanities'),(3,'Science'),(4,'Engineering'),(5,'Health Sciences');");

            // 10 sample degree programmes (ID, faculty, name)
            sql.Append(@"INSERT INTO DegreeProgramme (DegreeProgrammeID, FacultyID, ProgrammeName) VALUES
                (1,1,'BCom Information Systems'),(2,1,'BCom Accounting'),(3,1,'BCom General'),
                (4,3,'BSc Computer Science'),(5,3,'BSc Biological Sciences'),
                (6,4,'BSc Engineering (Mechanical Engineering)'),
                (7,2,'BA Humanities'),(8,2,'BSocSci'),(9,2,'LLB Law'),
                (10,5,'Bachelor of Health Sciences');");

            // ---------- Programme requirements ----------
            // Columns: degree, required subject, alternative subject, minimum mark, minimum overall average, advisory note
            // Subject rules:
            sql.Append(@"INSERT INTO ProgrammeRequirement
                (DegreeProgrammeID, RequiredSubjectID, AlternativeSubjectID, MinimumMark, MinimumOverallAverage, AdvisoryNote) VALUES
                (1,3,NULL,60,NULL,NULL),(1,1,NULL,60,NULL,NULL),
                (2,3,NULL,65,NULL,NULL),(2,7,NULL,60,NULL,NULL),
                (3,3,4,55,NULL,'Mathematical Literacy is accepted instead of Mathematics'),(3,1,NULL,50,NULL,NULL),
                (4,3,NULL,70,NULL,NULL),(4,1,NULL,60,NULL,NULL),
                (5,3,NULL,60,NULL,NULL),(5,5,NULL,60,NULL,NULL),(5,6,NULL,60,NULL,NULL),
                (6,3,NULL,70,NULL,NULL),(6,5,NULL,70,NULL,'Engineering Graphics and Design is recommended'),
                (7,1,NULL,60,NULL,NULL),
                (8,1,NULL,65,NULL,NULL),(8,3,4,50,NULL,'Mathematical Literacy is accepted instead of Mathematics'),
                (9,1,NULL,70,NULL,NULL),
                (10,3,NULL,60,NULL,NULL),(10,5,NULL,60,NULL,NULL),(10,6,NULL,60,NULL,NULL);");
            // Overall-average rules (no specific subject):
            sql.Append(@"INSERT INTO ProgrammeRequirement (DegreeProgrammeID, MinimumOverallAverage) VALUES
                (7,55),(8,60),(9,65);");

            // ---------- Interest categories and yes/no questions ----------
            sql.Append(@"INSERT INTO InterestCategory (InterestCategoryID, CategoryName) VALUES
                (1,'Business and Numbers'),(2,'Technology and Systems'),(3,'Science and Research'),
                (4,'People and Society'),(5,'Language and Ideas');");

            // Question IDs 1-10, two per category (1-2 -> category 1, 3-4 -> category 2, and so on)
            sql.Append(@"INSERT INTO InterestQuestion (InterestQuestionID, InterestCategoryID, QuestionText) VALUES
                (1,1,'I enjoy solving problems using numbers or data.'),
                (2,1,'I enjoy business, entrepreneurship and finance.'),
                (3,2,'I enjoy working with technology.'),
                (4,2,'I enjoy designing or building practical solutions.'),
                (5,3,'I enjoy science experiments and research.'),
                (6,3,'I enjoy learning about nature, health or the human body.'),
                (7,4,'I enjoy helping people and communities.'),
                (8,4,'I enjoy understanding society, law or politics.'),
                (9,5,'I enjoy reading, writing and analysing ideas.'),
                (10,5,'I enjoy debating and sharing my views.');");

            // Which interest categories suit which degree (degree, category)
            sql.Append(@"INSERT INTO DegreeInterestMapping VALUES
                (1,1),(1,2),(2,1),(3,1),(4,2),(4,1),(5,3),(6,2),(6,3),
                (7,5),(7,4),(8,4),(8,5),(9,5),(9,4),(10,3),(10,4);");

            // ---------- Funding options (all SAMPLE data) ----------
            // Column order: ID, name, type, eligibility, documents, closing date, contact, active
            sql.Append(@"INSERT INTO FundingOption
                (FundingOptionID, FundingName, FundingType, EligibilityDescription, RequiredDocuments, ClosingDate, ContactApplicationInfo, IsActive) VALUES
                (1,'Faculty Merit Award','Scholarship','Based on academic merit','ID, results','2026-11-30','Sample only - verify with the funder',1),
                (2,'Commerce Financial Need Bursary','Bursary','Based on financial need','ID, results, proof of income','2026-10-31','Sample only - verify with the funder',1),
                (3,'Science Research Scholarship','Scholarship','Based on academic merit','ID, results','2026-12-15','Sample only - verify with the funder',1),
                (4,'STEM Opportunity Fund','External Funding','Financial need and strong Mathematics','ID, results, proof of income','2026-11-15','Sample only - verify with the funder',1),
                (5,'Engineering Excellence Bursary','Bursary','Based on academic merit','ID, results','2026-11-20','Sample only - verify with the funder',1),
                (6,'Engineering Financial Aid','Financial Aid','Based on financial need','ID, results, proof of income','2026-12-01','Sample only - verify with the funder',1),
                (7,'Health Sciences Bursary','Bursary','Based on academic merit','ID, results','2026-11-25','Sample only - verify with the funder',1),
                (8,'National Financial Aid Support','Financial Aid','Based on financial need','ID, results, proof of income','2026-12-31','Sample only - verify with the funder',1),
                (9,'Humanities Merit Scholarship','Scholarship','Based on academic merit','ID, results','2026-11-10','Sample only - verify with the funder',1),
                (10,'Legacy Transport Bursary','Bursary','Outdated option - should never be shown','ID','2024-01-31','Sample only - verify with the funder',0);");

            // Which faculties each funding option is linked to (funding, faculty)
            sql.Append(@"INSERT INTO FundingFaculty VALUES
                (1,1),(2,1),(3,3),(4,3),(4,4),(5,4),(6,4),(7,5),
                (8,1),(8,2),(8,3),(8,4),(8,5),(9,2),(10,1);");
            // Funding linked to one specific degree (funding, degree)
            sql.Append("INSERT INTO FundingProgramme VALUES (5,6),(7,10);");

            // ---------- Login accounts ----------
            // Administrator: admin / Admin@123    Career Advisor: advisor / Advisor@123
            sql.Append("INSERT INTO UserAccount (Username, PasswordHash, UserRole) VALUES ('admin','"
                       + PasswordHelper.Hash("Admin@123") + "','Administrator'),('advisor','"
                       + PasswordHelper.Hash("Advisor@123") + "','CareerAdvisor');");

            // ---------- 10 test learners, each with one advising session ----------
            // The first five are the learners required by the Part B specification.
            var sessions = new[]
            {
                new TestSession { First="Gadija", Last="Jacobs", Grade=12, School="Wynberg High", Province="Western Cape",
                    FacultyId=1, DegreeId=1, Outcome="Strong Match",
                    Reason="You meet all subject and mark requirements and your interests align well.",
                    Funding=true, Export=true, Marks="1:72,3:70,7:74,8:79,9:70,13:80", Answers="1110001000" },

                new TestSession { First="Aadiel-Raboo", Last="Magwa", Grade=12, School="Lansdowne High", Province="Western Cape",
                    FacultyId=3, DegreeId=4, Outcome="Not Currently Eligible",
                    Reason="Mathematics is required but was not taken. Mathematical Literacy is not accepted for this degree.",
                    Funding=false, Export=false, Marks="1:65,4:78,8:71,11:69,14:75,13:82", Answers="1011001000" },

                new TestSession { First="Labeeqah", Last="Rademeyer", Grade=11, School="Rondebosch High", Province="Western Cape",
                    FacultyId=3, DegreeId=5, Outcome="Possible Match",
                    Reason="Your Mathematics mark (58%) is below the minimum of 60%. Improving it would make you a strong match.",
                    Funding=false, Export=false, Marks="1:80,3:58,5:62,6:70,10:66,13:85", Answers="0000111000" },

                new TestSession { First="Siyabonga", Last="Dickson", Grade=12, School="Khayelitsha High", Province="Western Cape",
                    FacultyId=4, DegreeId=6, Outcome="Strong Match",
                    Reason="You meet all subject and mark requirements and your interests align well.",
                    Funding=false, Export=false, Marks="1:70,3:75,5:78,15:73,13:80", Answers="1011100000" },

                new TestSession { First="Derek Vin", Last="Diesel", Grade=12, School="Plumstead High", Province="Western Cape",
                    FacultyId=5, DegreeId=10, Outcome="Not Currently Eligible",
                    Reason="Mathematics and Physical Sciences are required but were not taken.",
                    Funding=false, Export=false, Marks="1:82,4:76,11:79,6:68,16:85,13:90", Answers="0000011101" },

                // The remaining five were created by our team to cover the other scenarios.
                new TestSession { First="Thando", Last="Nkosi", Grade=12, School="Gugulethu High", Province="Western Cape",
                    FacultyId=1, DegreeId=2, Outcome="Not Currently Eligible",
                    Reason="Mathematics (48% vs 65% needed) and Accounting (55% vs 60% needed) are below the minimum.",
                    Funding=true, Export=false, Marks="1:60,3:48,7:55,8:62,9:58,2:57,13:70", Answers="0100001000" },

                new TestSession { First="Lindiwe", Last="Dube", Grade=11, School="Mitchells Plain High", Province="Western Cape",
                    FacultyId=3, DegreeId=4, Outcome="Possible Match",
                    Reason="You meet the requirements, but your interests align more closely with another faculty (Humanities).",
                    Funding=false, Export=false, Marks="1:78,3:72,11:80,10:70,2:68,13:75", Answers="0000001111" },

                new TestSession { First="Sipho", Last="Mokoena", Grade=11, School="Bellville High", Province="Western Cape",
                    FacultyId=1, DegreeId=1, Outcome="More Information Needed",
                    Reason="The interest questionnaire was not completed, so the check could not be finished.",
                    Funding=false, Export=false, Marks="1:66,3:64,8:70,12:72,13:77", Answers="" },

                new TestSession { First="Amahle", Last="Zulu", Grade=12, School="Pinelands High", Province="Western Cape",
                    FacultyId=5, DegreeId=10, Outcome="Strong Match",
                    Reason="You meet all subject and mark requirements and your interests align well.",
                    Funding=true, Export=true, Marks="1:75,3:72,5:70,6:75,10:68,13:80", Answers="0000111100" },

                new TestSession { First="Neo", Last="Pillay", Grade=12, School="Claremont High", Province="Western Cape",
                    FacultyId=2, DegreeId=7, Outcome="Strong Match",
                    Reason="You meet all subject and mark requirements and your interests align well.",
                    Funding=false, Export=false, Marks="1:80,4:70,11:78,2:72,14:69,13:74", Answers="0000001111" }
            };

            int id = 0; // counts 1, 2, 3... because the database gives IDs in the order we insert
            foreach (var s in sessions)
            {
                id++; // this is both the LearnerID and the AdvisorySessionID for this learner
                string date = "2026-09-" + (9 + id).ToString("00") + " 10:00"; // a different date for each session

                sql.Append("INSERT INTO Learner (FirstName, LastName, GradeLevelID, SchoolName, Province) VALUES ('"
                           + s.First + "','" + s.Last + "'," + s.Grade + ",'" + s.School + "','" + s.Province + "');");

                sql.Append("INSERT INTO AdvisorySession (LearnerID, PreferredFacultyID, SelectedDegreeProgrammeID, SessionDateTime, Outcome, OutcomeReason, FundingRequested) VALUES ("
                           + id + "," + s.FacultyId + "," + s.DegreeId + ",'" + date + "','" + s.Outcome + "','"
                           + s.Reason + "'," + (s.Funding ? 1 : 0) + ");");

                // Marks: split "1:72,3:70" into pairs and insert one row per subject.
                foreach (string pair in s.Marks.Split(','))
                {
                    string[] parts = pair.Split(':');
                    sql.Append("INSERT INTO LearnerSubject VALUES (" + id + "," + parts[0] + "," + parts[1] + ");");
                }

                // Yes/No answers: character 0 is question 1, character 1 is question 2, and so on.
                for (int q = 0; q < s.Answers.Length; q++)
                {
                    sql.Append("INSERT INTO LearnerInterestResponse (AdvisorySessionID, InterestQuestionID, ResponseValue) VALUES ("
                               + id + "," + (q + 1) + "," + s.Answers[q] + ");");
                }

                // If this learner exported a summary, record it.
                if (s.Export)
                {
                    sql.Append("INSERT INTO ExportReport (AdvisorySessionID, ExportDateTime, FileName) VALUES ("
                               + id + ",'" + date + "','" + s.First + "_" + s.Last + "_Summary.html');");
                }
            }

            sql.Append("COMMIT;"); // save everything
            DatabaseHelper.Execute(sql.ToString());
        }
    }
}