BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019102458_RH_03_Personnes_Documents'
)
BEGIN
    CREATE TABLE [rh].[Fonctionnaire] (
        [Id] uniqueidentifier NOT NULL,
        [Matricule] nvarchar(20) NOT NULL,
        [Cin] nvarchar(20) NOT NULL,
        [NomAr] nvarchar(100) NOT NULL,
        [PrenomAr] nvarchar(100) NOT NULL,
        [NomFr] nvarchar(100) NOT NULL,
        [PrenomFr] nvarchar(100) NOT NULL,
        [DateNaissance] date NOT NULL,
        [DateRecrutement] date NOT NULL,
        [Genre] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Fonctionnaire] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019102458_RH_03_Personnes_Documents'
)
BEGIN
    CREATE TABLE [rh].[PieceJointe] (
        [Id] uniqueidentifier NOT NULL,
        [NumeroDocument] nvarchar(50) NOT NULL,
        [NumeroDocumentNormalise] nvarchar(50) NOT NULL,
        [DateDocument] date NOT NULL,
        [AnneeDocument] int NOT NULL,
        [Emetteur] nvarchar(300) NOT NULL,
        [EmetteurNormalise] nvarchar(300) NOT NULL,
        [TypeDocument] nvarchar(100) NOT NULL,
        [Objet] nvarchar(500) NOT NULL,
        [NomFichier] nvarchar(255) NULL,
        [CheminFichier] nvarchar(500) NULL,
        [HashFichier] nvarchar(64) NULL,
        [DateAjout] date NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_PieceJointe] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019102458_RH_03_Personnes_Documents'
)
BEGIN
    CREATE TABLE [rh].[Rib] (
        [Id] uniqueidentifier NOT NULL,
        [IdFonctionnaire] uniqueidentifier NOT NULL,
        [CodeBanque] nvarchar(3) NOT NULL,
        [CodeVille] nvarchar(3) NOT NULL,
        [NumeroCompte] nvarchar(16) NOT NULL,
        [Cle] nvarchar(2) NOT NULL,
        [EstActif] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Rib] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Rib_Cle] CHECK ([Cle] NOT LIKE '%[^0-9]%' AND LEN([Cle]) = 2),
        CONSTRAINT [CK_Rib_CodeBanque] CHECK ([CodeBanque] NOT LIKE '%[^0-9]%' AND LEN([CodeBanque]) = 3),
        CONSTRAINT [CK_Rib_CodeVille] CHECK ([CodeVille] NOT LIKE '%[^0-9]%' AND LEN([CodeVille]) = 3),
        CONSTRAINT [CK_Rib_NumeroCompte] CHECK ([NumeroCompte] NOT LIKE '%[^0-9]%' AND LEN([NumeroCompte]) = 16),
        CONSTRAINT [FK_Rib_Fonctionnaire] FOREIGN KEY ([IdFonctionnaire]) REFERENCES [rh].[Fonctionnaire] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019102458_RH_03_Personnes_Documents'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Fonctionnaire_Cin] ON [rh].[Fonctionnaire] ([Cin]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019102458_RH_03_Personnes_Documents'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Fonctionnaire_Matricule] ON [rh].[Fonctionnaire] ([Matricule]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019102458_RH_03_Personnes_Documents'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_PieceJointe_Cle] ON [rh].[PieceJointe] ([NumeroDocumentNormalise], [AnneeDocument], [EmetteurNormalise]) WHERE [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019102458_RH_03_Personnes_Documents'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Rib_Actif] ON [rh].[Rib] ([IdFonctionnaire]) WHERE [EstActif] = 1 AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019102458_RH_03_Personnes_Documents'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019102458_RH_03_Personnes_Documents', N'10.0.13');
END;

COMMIT;
GO

