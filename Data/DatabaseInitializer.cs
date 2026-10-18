using System;

namespace FuturePath.Data
{
    // Step 1: create all the tables (the "shape" of the database).
    // Step 2: if the tables are empty, fill them with the sample/test data (SeedData).
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            DatabaseHelper.Execute(CreateTablesScript);

            // If there are no subjects yet, this is a brand new database, so add the test data.
            int subjectCount = Convert.ToInt32(DatabaseHelper.Scalar("SELECT COUNT(*) FROM Subject"));
            if (subjectCount == 0)
            {
                SeedData.Run();
            }
        }

        // The SQL that creates every table. "PRIMARY KEY" = the unique ID of a row.
        // IsActive: 1 = in use, 0 = switched off (switched-off items are hidden from learners).
        private const string CreateTablesScript = @"
CREATE TABLE IF NOT EXISTS GradeLevel (
    GradeLevelID INTEGER PRIMARY KEY,
    GradeName    TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS Learner (
    LearnerID     INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName     TEXT,                 -- optional: a learner may stay anonymous
    LastName      TEXT,                 -- optional
    GradeLevelID  INTEGER NOT NULL REFERENCES GradeLevel(GradeLevelID),
    SchoolName    TEXT,
    Province      TEXT,
    ContactDetails TEXT
);

CREATE TABLE IF NOT EXISTS Subject (
    SubjectID   INTEGER PRIMARY KEY AUTOINCREMENT,
    SubjectName TEXT NOT NULL UNIQUE,
    IsActive    INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Faculty (
    FacultyID   INTEGER PRIMARY KEY AUTOINCREMENT,
    FacultyName TEXT NOT NULL UNIQUE,
    IsActive    INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS DegreeProgramme (
    DegreeProgrammeID INTEGER PRIMARY KEY AUTOINCREMENT,
    FacultyID         INTEGER NOT NULL REFERENCES Faculty(FacultyID),
    ProgrammeName     TEXT NOT NULL UNIQUE,
    IsActive          INTEGER NOT NULL DEFAULT 1
);

-- One row = one rule for a degree. Either a subject rule (RequiredSubjectID + MinimumMark)
-- or an overall-average rule (MinimumOverallAverage). AlternativeSubjectID = another subject
-- that is accepted instead (e.g. Maths OR Maths Literacy).
CREATE TABLE IF NOT EXISTS ProgrammeRequirement (
    ProgrammeRequirementID INTEGER PRIMARY KEY AUTOINCREMENT,
    DegreeProgrammeID      INTEGER NOT NULL REFERENCES DegreeProgramme(DegreeProgrammeID),
    RequiredSubjectID      INTEGER REFERENCES Subject(SubjectID),
    AlternativeSubjectID   INTEGER REFERENCES Subject(SubjectID),
    MinimumMark            INTEGER CHECK (MinimumMark BETWEEN 0 AND 100),
    MinimumOverallAverage  INTEGER CHECK (MinimumOverallAverage BETWEEN 0 AND 100),
    FacultySpecificRule    TEXT,
    AdvisoryNote           TEXT
);

CREATE TABLE IF NOT EXISTS InterestCategory (
    InterestCategoryID INTEGER PRIMARY KEY AUTOINCREMENT,
    CategoryName       TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS InterestQuestion (
    InterestQuestionID INTEGER PRIMARY KEY AUTOINCREMENT,
    InterestCategoryID INTEGER NOT NULL REFERENCES InterestCategory(InterestCategoryID),
    QuestionText       TEXT NOT NULL,
    IsActive           INTEGER NOT NULL DEFAULT 1
);

-- Links each degree to the interest categories it suits (many-to-many).
CREATE TABLE IF NOT EXISTS DegreeInterestMapping (
    DegreeProgrammeID  INTEGER NOT NULL REFERENCES DegreeProgramme(DegreeProgrammeID),
    InterestCategoryID INTEGER NOT NULL REFERENCES InterestCategory(InterestCategoryID),
    PRIMARY KEY (DegreeProgrammeID, InterestCategoryID)
);

-- One advising visit by a learner. (DESIGN CHANGE vs ERD: Outcome, OutcomeReason and
-- FundingRequested were added so reports can show the result of each session.)
CREATE TABLE IF NOT EXISTS AdvisorySession (
    AdvisorySessionID          INTEGER PRIMARY KEY AUTOINCREMENT,
    LearnerID                  INTEGER NOT NULL REFERENCES Learner(LearnerID),
    PreferredFacultyID         INTEGER REFERENCES Faculty(FacultyID),
    SelectedDegreeProgrammeID  INTEGER REFERENCES DegreeProgramme(DegreeProgrammeID),
    SessionDateTime            TEXT NOT NULL,
    Outcome                    TEXT CHECK (Outcome IN ('Strong Match','Possible Match','Not Currently Eligible','More Information Needed')),
    OutcomeReason              TEXT,
    FundingRequested           INTEGER NOT NULL DEFAULT 0
);

-- The learner's subjects and marks for one session.
-- The two-part primary key means the SAME subject cannot be added twice in one session.
CREATE TABLE IF NOT EXISTS LearnerSubject (
    AdvisorySessionID INTEGER NOT NULL REFERENCES AdvisorySession(AdvisorySessionID),
    SubjectID         INTEGER NOT NULL REFERENCES Subject(SubjectID),
    Mark              INTEGER NOT NULL CHECK (Mark BETWEEN 0 AND 100),
    PRIMARY KEY (AdvisorySessionID, SubjectID)
);

-- ResponseValue: 1 = Yes, 0 = No.
CREATE TABLE IF NOT EXISTS LearnerInterestResponse (
    LearnerInterestResponseID INTEGER PRIMARY KEY AUTOINCREMENT,
    AdvisorySessionID         INTEGER NOT NULL REFERENCES AdvisorySession(AdvisorySessionID),
    InterestQuestionID        INTEGER NOT NULL REFERENCES InterestQuestion(InterestQuestionID),
    ResponseValue             INTEGER NOT NULL CHECK (ResponseValue IN (0,1)),
    UNIQUE (AdvisorySessionID, InterestQuestionID)
);

-- The scores the recommendation engine calculates for each degree in a session.
CREATE TABLE IF NOT EXISTS RecommendationResult (
    RecommendationResultID INTEGER PRIMARY KEY AUTOINCREMENT,
    AdvisorySessionID      INTEGER NOT NULL REFERENCES AdvisorySession(AdvisorySessionID),
    DegreeProgrammeID      INTEGER NOT NULL REFERENCES DegreeProgramme(DegreeProgrammeID),
    SubjectMatchScore      REAL NOT NULL,
    MarkMatchScore         REAL NOT NULL,
    InterestMatchScore     REAL NOT NULL,
    FacultyPreferenceScore REAL NOT NULL,
    OverallScore           REAL NOT NULL,
    RecommendationOutcome  TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS FundingOption (
    FundingOptionID        INTEGER PRIMARY KEY AUTOINCREMENT,
    FundingName            TEXT NOT NULL,
    FundingType            TEXT NOT NULL CHECK (FundingType IN ('Scholarship','Bursary','Financial Aid','External Funding')),
    EligibilityDescription TEXT,
    RequiredDocuments      TEXT,
    ClosingDate            TEXT,        -- stored as yyyy-MM-dd
    ContactApplicationInfo TEXT,
    IsActive               INTEGER NOT NULL DEFAULT 1,
    IsSample               INTEGER NOT NULL DEFAULT 1   -- 1 = sample data that needs verification
);

CREATE TABLE IF NOT EXISTS FundingFaculty (
    FundingOptionID INTEGER NOT NULL REFERENCES FundingOption(FundingOptionID),
    FacultyID       INTEGER NOT NULL REFERENCES Faculty(FacultyID),
    PRIMARY KEY (FundingOptionID, FacultyID)
);

CREATE TABLE IF NOT EXISTS FundingProgramme (
    FundingOptionID   INTEGER NOT NULL REFERENCES FundingOption(FundingOptionID),
    DegreeProgrammeID INTEGER NOT NULL REFERENCES DegreeProgramme(DegreeProgrammeID),
    PRIMARY KEY (FundingOptionID, DegreeProgrammeID)
);

CREATE TABLE IF NOT EXISTS ExportReport (
    ExportReportID    INTEGER PRIMARY KEY AUTOINCREMENT,
    AdvisorySessionID INTEGER NOT NULL REFERENCES AdvisorySession(AdvisorySessionID),
    ExportDateTime    TEXT NOT NULL,
    FileName          TEXT NOT NULL
);

-- DESIGN CHANGE vs ERD: UserAccountID is the primary key (the ERD wrongly used Password).
CREATE TABLE IF NOT EXISTS UserAccount (
    UserAccountID INTEGER PRIMARY KEY AUTOINCREMENT,
    Username      TEXT NOT NULL UNIQUE,
    PasswordHash  TEXT NOT NULL,
    UserRole      TEXT NOT NULL CHECK (UserRole IN ('Administrator','CareerAdvisor')),
    IsActive      INTEGER NOT NULL DEFAULT 1
);
";
    }
}