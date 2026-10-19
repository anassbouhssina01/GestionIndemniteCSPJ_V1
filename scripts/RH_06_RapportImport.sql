BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019123646_RH_06_RapportImport'
)
BEGIN
    CREATE TABLE [rh].[RapportImport] (
        [Id] uniqueidentifier NOT NULL,
        [NomFichier] nvarchar(260) NOT NULL,
        [ModeDryRun] bit NOT NULL,
        [NombreLignes] int NOT NULL,
        [NombreErreurs] int NOT NULL,
        [ContenuRapport] NVARCHAR(MAX) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_RapportImport] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019123646_RH_06_RapportImport'
)
BEGIN
    CREATE INDEX [IX_RapportImport_CreatedAt] ON [rh].[RapportImport] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019123646_RH_06_RapportImport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019123646_RH_06_RapportImport', N'10.0.13');
END;

COMMIT;
GO

