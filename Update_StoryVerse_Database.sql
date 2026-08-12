-- ============================================================================
-- StoryVerse Database Migration & Seed Script
-- Description: Adds StoryPart hierarchy, Story detail metadata fields,
--              UserGoal extensions, and seeds the featured story "Dil Hai Ki Maanta Nahi".
-- ============================================================================

-- 1. Add Story Detail Fields to DI_TRN_WebStories
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'StoryType')
BEGIN
    ALTER TABLE DI_TRN_WebStories ADD StoryType NVARCHAR(100) NOT NULL DEFAULT 'Novel';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'Tagline')
BEGIN
    ALTER TABLE DI_TRN_WebStories ADD Tagline NVARCHAR(500) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'Synopsis')
BEGIN
    ALTER TABLE DI_TRN_WebStories ADD Synopsis NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'PointOfView')
BEGIN
    ALTER TABLE DI_TRN_WebStories ADD PointOfView NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'TimePeriod')
BEGIN
    ALTER TABLE DI_TRN_WebStories ADD TimePeriod NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'Language')
BEGIN
    ALTER TABLE DI_TRN_WebStories ADD Language NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'TargetAudience')
BEGIN
    ALTER TABLE DI_TRN_WebStories ADD TargetAudience NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'Themes')
BEGIN
    ALTER TABLE DI_TRN_WebStories ADD Themes NVARCHAR(500) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'Tone')
BEGIN
    ALTER TABLE DI_TRN_WebStories ADD Tone NVARCHAR(200) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'HeroBannerImageUrl')
BEGIN
    ALTER TABLE DI_TRN_WebStories ADD HeroBannerImageUrl NVARCHAR(500) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebStories') AND name = 'IsBookmarked')
BEGIN
    ALTER TABLE DI_TRN_WebStories ADD IsBookmarked BIT NOT NULL DEFAULT 0;
END


-- 2. Create DI_TRN_WebStoryParts Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('DI_TRN_WebStoryParts') AND type in (N'U'))
BEGIN
    CREATE TABLE DI_TRN_WebStoryParts (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        StoryId UNIQUEIDENTIFIER NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        [Order] INT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_DI_TRN_WebStoryParts_DI_TRN_WebStories_StoryId FOREIGN KEY (StoryId) REFERENCES DI_TRN_WebStories(Id) ON DELETE CASCADE
    );
END


-- 3. Add PartId and Status to DI_TRN_WebChapters
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'PartId')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD PartId UNIQUEIDENTIFIER NULL;
    ALTER TABLE DI_TRN_WebChapters ADD CONSTRAINT FK_DI_TRN_WebChapters_DI_TRN_WebStoryParts_PartId FOREIGN KEY (PartId) REFERENCES DI_TRN_WebStoryParts(Id) ON DELETE SET NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'Status')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD Status NVARCHAR(50) NOT NULL DEFAULT 'Completed';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'Content')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD Content NVARCHAR(MAX) NULL;
END


IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'CharacterCount')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD CharacterCount INT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'Version')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD Version INT NOT NULL DEFAULT 1;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'Summary')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD Summary NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'TargetWordCount')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD TargetWordCount INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'Purpose')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD Purpose NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'Goal')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD Goal NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'KeyEvents')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD KeyEvents NVARCHAR(MAX) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'EmotionalTone')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD EmotionalTone NVARCHAR(500) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_WebChapters') AND name = 'PointOfView')
BEGIN
    ALTER TABLE DI_TRN_WebChapters ADD PointOfView NVARCHAR(200) NULL;
END


-- 4. Extend DI_TRN_UserGoals Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_UserGoals') AND name = 'WeeklyWordCountGoal')
BEGIN
    ALTER TABLE DI_TRN_UserGoals ADD WeeklyWordCountGoal INT NOT NULL DEFAULT 5000;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_UserGoals') AND name = 'WordsWrittenThisWeek')
BEGIN
    ALTER TABLE DI_TRN_UserGoals ADD WordsWrittenThisWeek INT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DI_TRN_UserGoals') AND name = 'WordsWrittenThisMonth')
BEGIN
    ALTER TABLE DI_TRN_UserGoals ADD WordsWrittenThisMonth INT NOT NULL DEFAULT 0;
END

PRINT 'StoryVerse Database Schema Migration Completed Successfully!';
