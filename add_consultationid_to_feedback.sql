-- Add ConsultationId column to Feedbacks table
-- Run this script in SSMS against your Askify database

-- Step 1: Add the ConsultationId column
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Feedbacks' AND COLUMN_NAME = 'ConsultationId'
)
BEGIN
    ALTER TABLE [Feedbacks] ADD [ConsultationId] INT NULL;
    PRINT 'Added ConsultationId column to Feedbacks table';
END
ELSE
BEGIN
    PRINT 'ConsultationId column already exists';
END

-- Step 2: Add foreign key constraint
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME = 'FK_Feedbacks_Consultations_ConsultationId'
)
BEGIN
    ALTER TABLE [Feedbacks] 
    ADD CONSTRAINT [FK_Feedbacks_Consultations_ConsultationId] 
    FOREIGN KEY ([ConsultationId]) REFERENCES [Consultations]([Id]) ON DELETE NO ACTION;
    PRINT 'Added foreign key constraint';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint already exists';
END

-- Step 3: Clear existing feedbacks (optional - to start fresh with per-consultation ratings)
-- Uncomment the next line if you want to clear existing feedbacks
-- DELETE FROM [Feedbacks];
-- PRINT 'Cleared existing feedbacks';

-- Step 4: Create index for better query performance
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Feedbacks_ConsultationId' AND object_id = OBJECT_ID('Feedbacks')
)
BEGIN
    CREATE INDEX [IX_Feedbacks_ConsultationId] ON [Feedbacks]([ConsultationId]);
    PRINT 'Created index on ConsultationId';
END
ELSE
BEGIN
    PRINT 'Index already exists';
END

PRINT 'Migration complete!';
