IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019095415_RH_01_Fondations'
)
BEGIN
    IF SCHEMA_ID(N'rh') IS NULL EXEC(N'CREATE SCHEMA [rh];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019095415_RH_01_Fondations'
)
BEGIN
    CREATE TABLE [rh].[Cadre] (
        [Id] uniqueidentifier NOT NULL,
        [CodeCadre] nvarchar(30) NOT NULL,
        [LibelleCadreAr] nvarchar(200) NOT NULL,
        [LibelleCadreFr] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [EstActif] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Cadre] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019095415_RH_01_Fondations'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Cadre_CodeCadre] ON [rh].[Cadre] ([CodeCadre]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019095415_RH_01_Fondations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019095415_RH_01_Fondations', N'10.0.13');
END;

COMMIT;
GO

