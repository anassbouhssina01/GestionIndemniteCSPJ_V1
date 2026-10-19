BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    CREATE TABLE [rh].[Grade] (
        [Id] uniqueidentifier NOT NULL,
        [IdCadre] uniqueidentifier NOT NULL,
        [NumeroReferencie] int NOT NULL,
        [LibelleGradeAr] nvarchar(200) NOT NULL,
        [LibelleGradeFr] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [EstActif] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Grade] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Grade_Cadre] FOREIGN KEY ([IdCadre]) REFERENCES [rh].[Cadre] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    CREATE TABLE [rh].[Pays] (
        [Id] uniqueidentifier NOT NULL,
        [CodePays] nvarchar(30) NOT NULL,
        [NomPaysAr] nvarchar(200) NOT NULL,
        [NomPaysFr] nvarchar(200) NOT NULL,
        [EstLocal] bit NOT NULL,
        [EstActif] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Pays] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    CREATE TABLE [rh].[Responsabilite] (
        [Id] uniqueidentifier NOT NULL,
        [CodeResponsabilite] nvarchar(30) NOT NULL,
        [LibelleResponsabiliteAr] nvarchar(200) NOT NULL,
        [LibelleResponsabiliteFr] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [EstActif] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Responsabilite] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    CREATE TABLE [rh].[StatutAdministratif] (
        [Id] uniqueidentifier NOT NULL,
        [CodeStatut] nvarchar(30) NOT NULL,
        [LibelleStatutAr] nvarchar(200) NOT NULL,
        [LibelleStatutFr] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [EstActif] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_StatutAdministratif] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    CREATE TABLE [rh].[Ville] (
        [Id] uniqueidentifier NOT NULL,
        [NomVille] nvarchar(100) NOT NULL,
        [IdPays] uniqueidentifier NOT NULL,
        [Latitude] decimal(9,6) NULL,
        [Longitude] decimal(9,6) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Ville] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Ville_Latitude] CHECK ([Latitude] IS NULL OR [Latitude] BETWEEN -90 AND 90),
        CONSTRAINT [CK_Ville_Longitude] CHECK ([Longitude] IS NULL OR [Longitude] BETWEEN -180 AND 180),
        CONSTRAINT [FK_Ville_Pays] FOREIGN KEY ([IdPays]) REFERENCES [rh].[Pays] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Grade_IdCadre_NumeroReferencie] ON [rh].[Grade] ([IdCadre], [NumeroReferencie]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Pays_CodePays] ON [rh].[Pays] ([CodePays]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Pays_EstLocal] ON [rh].[Pays] ([EstLocal]) WHERE [EstLocal] = 1 AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Responsabilite_Code] ON [rh].[Responsabilite] ([CodeResponsabilite]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_StatutAdministratif_Code] ON [rh].[StatutAdministratif] ([CodeStatut]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    CREATE INDEX [IX_Ville_IdPays] ON [rh].[Ville] ([IdPays]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019101431_RH_02_Referentiels'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019101431_RH_02_Referentiels', N'10.0.13');
END;

COMMIT;
GO

