-- ============================================================================
-- StoryVerse Database Migration: Add Hindi Language Options
-- Description: Adds "Hindi - Written in English", "Pure Hindi", and "Hindi + English"
--              to the DI_MST_DropdownOptions master table.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM DI_MST_DropdownOptions WHERE Category = 'Language' AND Value = 'HindiInEnglish')
BEGIN
    INSERT INTO DI_MST_DropdownOptions (Id, Category, Value, Text, Description, DisplayOrder, IsActive)
    VALUES (NEWID(), 'Language', 'HindiInEnglish', 'Hindi - Written in English', 'Hindi written using Roman / English script.', 3, 1);
END;

IF NOT EXISTS (SELECT 1 FROM DI_MST_DropdownOptions WHERE Category = 'Language' AND Value = 'PureHindi')
BEGIN
    INSERT INTO DI_MST_DropdownOptions (Id, Category, Value, Text, Description, DisplayOrder, IsActive)
    VALUES (NEWID(), 'Language', 'PureHindi', 'Pure Hindi', 'Pure Hindi language written in Devanagari script.', 4, 1);
END;

IF NOT EXISTS (SELECT 1 FROM DI_MST_DropdownOptions WHERE Category = 'Language' AND Value = 'HindiAndEnglish')
BEGIN
    INSERT INTO DI_MST_DropdownOptions (Id, Category, Value, Text, Description, DisplayOrder, IsActive)
    VALUES (NEWID(), 'Language', 'HindiAndEnglish', 'Hindi + English', 'Combination of Hindi and English language.', 5, 1);
END;

-- Verification Query
SELECT Id, Category, Value, Text, Description, DisplayOrder, IsActive
FROM DI_MST_DropdownOptions 
WHERE Category = 'Language'
ORDER BY DisplayOrder;
