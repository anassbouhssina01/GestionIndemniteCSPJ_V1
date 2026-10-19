BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE TABLE [rh].[Tribunal] (
        [Id] uniqueidentifier NOT NULL,
        [NomTribunal] nvarchar(200) NOT NULL,
        [TypeTribunal] int NOT NULL,
        [NiveauJuridiction] int NOT NULL,
        [IdVille] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Tribunal] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Tribunal_CassationGenerale] CHECK ([NiveauJuridiction] <> 3 OR [TypeTribunal] = 0),
        CONSTRAINT [FK_Tribunal_Ville] FOREIGN KEY ([IdVille]) REFERENCES [rh].[Ville] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE TABLE [rh].[UniteOrganisationnelle] (
        [Id] uniqueidentifier NOT NULL,
        [NomUnite] nvarchar(200) NOT NULL,
        [TypeUnite] int NOT NULL,
        [IdVille] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UniteOrganisationnelle] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UniteOrganisationnelle_Ville] FOREIGN KEY ([IdVille]) REFERENCES [rh].[Ville] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE TABLE [rh].[HistoriqueOrganigrammeTribunal] (
        [Id] uniqueidentifier NOT NULL,
        [IdTribunal] uniqueidentifier NOT NULL,
        [IdTribunalParent] uniqueidentifier NULL,
        [DateDebut] date NOT NULL,
        [DateFin] date NULL,
        [IdDocument] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_HistoriqueOrganigrammeTribunal] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_HistOrgTribunal_Periode] CHECK ([DateFin] IS NULL OR [DateDebut] <= [DateFin]),
        CONSTRAINT [FK_HistOrgTribunal_Document] FOREIGN KEY ([IdDocument]) REFERENCES [rh].[PieceJointe] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistOrgTribunal_Parent] FOREIGN KEY ([IdTribunalParent]) REFERENCES [rh].[Tribunal] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistOrgTribunal_Tribunal] FOREIGN KEY ([IdTribunal]) REFERENCES [rh].[Tribunal] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE TABLE [rh].[HistoriqueOrganigrammeUnite] (
        [Id] uniqueidentifier NOT NULL,
        [IdUnite] uniqueidentifier NOT NULL,
        [IdUniteParent] uniqueidentifier NULL,
        [DateDebut] date NOT NULL,
        [DateFin] date NULL,
        [IdDocument] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_HistoriqueOrganigrammeUnite] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_HistOrgUnite_Periode] CHECK ([DateFin] IS NULL OR [DateDebut] <= [DateFin]),
        CONSTRAINT [FK_HistOrgUnite_Document] FOREIGN KEY ([IdDocument]) REFERENCES [rh].[PieceJointe] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistOrgUnite_Parent] FOREIGN KEY ([IdUniteParent]) REFERENCES [rh].[UniteOrganisationnelle] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistOrgUnite_Unite] FOREIGN KEY ([IdUnite]) REFERENCES [rh].[UniteOrganisationnelle] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE INDEX [IX_HistOrgTribunal_Cle_Periode] ON [rh].[HistoriqueOrganigrammeTribunal] ([IdTribunal], [DateDebut], [DateFin]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE INDEX [IX_HistoriqueOrganigrammeTribunal_IdDocument] ON [rh].[HistoriqueOrganigrammeTribunal] ([IdDocument]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE INDEX [IX_HistoriqueOrganigrammeTribunal_IdTribunalParent] ON [rh].[HistoriqueOrganigrammeTribunal] ([IdTribunalParent]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_HistOrgTribunal_Ouvert] ON [rh].[HistoriqueOrganigrammeTribunal] ([IdTribunal], [DateFin]) WHERE [DateFin] IS NULL AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE INDEX [IX_HistOrgUnite_Cle_Periode] ON [rh].[HistoriqueOrganigrammeUnite] ([IdUnite], [DateDebut], [DateFin]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE INDEX [IX_HistoriqueOrganigrammeUnite_IdDocument] ON [rh].[HistoriqueOrganigrammeUnite] ([IdDocument]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE INDEX [IX_HistoriqueOrganigrammeUnite_IdUniteParent] ON [rh].[HistoriqueOrganigrammeUnite] ([IdUniteParent]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_HistOrgUnite_Ouvert] ON [rh].[HistoriqueOrganigrammeUnite] ([IdUnite], [DateFin]) WHERE [DateFin] IS NULL AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE INDEX [IX_Tribunal_IdVille] ON [rh].[Tribunal] ([IdVille]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    CREATE INDEX [IX_UniteOrganisationnelle_IdVille] ON [rh].[UniteOrganisationnelle] ([IdVille]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019104054_RH_04_Organigramme'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019104054_RH_04_Organigramme', N'10.0.13');
END;

COMMIT;
GO

