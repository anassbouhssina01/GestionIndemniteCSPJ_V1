BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE TABLE [rh].[HistoriqueAffectation] (
        [Id] uniqueidentifier NOT NULL,
        [IdFonctionnaire] uniqueidentifier NOT NULL,
        [IdTribunalAffectation] uniqueidentifier NULL,
        [IdUniteAffectation] uniqueidentifier NULL,
        [IdUniteTravail] uniqueidentifier NULL,
        [SectionTribunal] int NULL,
        [DateDebut] date NOT NULL,
        [DateFin] date NULL,
        [IdDocument] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_HistoriqueAffectation] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_HistAffectation_ExactementUn] CHECK (([IdTribunalAffectation] IS NOT NULL AND [IdUniteAffectation] IS NULL) OR ([IdTribunalAffectation] IS NULL AND [IdUniteAffectation] IS NOT NULL)),
        CONSTRAINT [CK_HistAffectation_Periode] CHECK ([DateFin] IS NULL OR [DateDebut] <= [DateFin]),
        CONSTRAINT [CK_HistAffectation_SectionTribunal] CHECK ([SectionTribunal] IS NULL OR [IdTribunalAffectation] IS NOT NULL),
        CONSTRAINT [CK_HistAffectation_UniteTravail] CHECK ([IdUniteTravail] IS NULL OR [IdTribunalAffectation] IS NOT NULL),
        CONSTRAINT [FK_HistAffectation_Document] FOREIGN KEY ([IdDocument]) REFERENCES [rh].[PieceJointe] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistAffectation_Fonctionnaire] FOREIGN KEY ([IdFonctionnaire]) REFERENCES [rh].[Fonctionnaire] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistAffectation_Tribunal] FOREIGN KEY ([IdTribunalAffectation]) REFERENCES [rh].[Tribunal] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistAffectation_UniteAffectation] FOREIGN KEY ([IdUniteAffectation]) REFERENCES [rh].[UniteOrganisationnelle] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistAffectation_UniteTravail] FOREIGN KEY ([IdUniteTravail]) REFERENCES [rh].[UniteOrganisationnelle] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE TABLE [rh].[HistoriqueCadreGrade] (
        [Id] uniqueidentifier NOT NULL,
        [IdFonctionnaire] uniqueidentifier NOT NULL,
        [IdGrade] uniqueidentifier NOT NULL,
        [DateDebut] date NOT NULL,
        [DateFin] date NULL,
        [IdDocument] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_HistoriqueCadreGrade] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_HistCadreGrade_Periode] CHECK ([DateFin] IS NULL OR [DateDebut] <= [DateFin]),
        CONSTRAINT [FK_HistCadreGrade_Document] FOREIGN KEY ([IdDocument]) REFERENCES [rh].[PieceJointe] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistCadreGrade_Fonctionnaire] FOREIGN KEY ([IdFonctionnaire]) REFERENCES [rh].[Fonctionnaire] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistCadreGrade_Grade] FOREIGN KEY ([IdGrade]) REFERENCES [rh].[Grade] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE TABLE [rh].[HistoriqueResponsabilite] (
        [Id] uniqueidentifier NOT NULL,
        [IdFonctionnaire] uniqueidentifier NOT NULL,
        [IdResponsabilite] uniqueidentifier NOT NULL,
        [IdTribunal] uniqueidentifier NULL,
        [IdUniteOrganisationnelle] uniqueidentifier NULL,
        [DateDebut] date NOT NULL,
        [DateFin] date NULL,
        [IdDocument] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_HistoriqueResponsabilite] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_HistResp_ExactementUn] CHECK (([IdTribunal] IS NOT NULL AND [IdUniteOrganisationnelle] IS NULL) OR ([IdTribunal] IS NULL AND [IdUniteOrganisationnelle] IS NOT NULL)),
        CONSTRAINT [CK_HistResp_Periode] CHECK ([DateFin] IS NULL OR [DateDebut] <= [DateFin]),
        CONSTRAINT [FK_HistResp_Document] FOREIGN KEY ([IdDocument]) REFERENCES [rh].[PieceJointe] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistResp_Fonctionnaire] FOREIGN KEY ([IdFonctionnaire]) REFERENCES [rh].[Fonctionnaire] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistResp_Responsabilite] FOREIGN KEY ([IdResponsabilite]) REFERENCES [rh].[Responsabilite] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistResp_Tribunal] FOREIGN KEY ([IdTribunal]) REFERENCES [rh].[Tribunal] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistResp_Unite] FOREIGN KEY ([IdUniteOrganisationnelle]) REFERENCES [rh].[UniteOrganisationnelle] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE TABLE [rh].[SituationAdministrative] (
        [Id] uniqueidentifier NOT NULL,
        [IdFonctionnaire] uniqueidentifier NOT NULL,
        [IdStatut] uniqueidentifier NOT NULL,
        [DateDebut] date NOT NULL,
        [DateFin] date NULL,
        [IdDocument] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SituationAdministrative] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_SituationAdmin_Periode] CHECK ([DateFin] IS NULL OR [DateDebut] <= [DateFin]),
        CONSTRAINT [FK_SituationAdmin_Document] FOREIGN KEY ([IdDocument]) REFERENCES [rh].[PieceJointe] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SituationAdmin_Fonctionnaire] FOREIGN KEY ([IdFonctionnaire]) REFERENCES [rh].[Fonctionnaire] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SituationAdmin_Statut] FOREIGN KEY ([IdStatut]) REFERENCES [rh].[StatutAdministratif] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistAffectation_Cle_Periode] ON [rh].[HistoriqueAffectation] ([IdFonctionnaire], [DateDebut], [DateFin]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistoriqueAffectation_IdDocument] ON [rh].[HistoriqueAffectation] ([IdDocument]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistoriqueAffectation_IdTribunalAffectation] ON [rh].[HistoriqueAffectation] ([IdTribunalAffectation]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistoriqueAffectation_IdUniteAffectation] ON [rh].[HistoriqueAffectation] ([IdUniteAffectation]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistoriqueAffectation_IdUniteTravail] ON [rh].[HistoriqueAffectation] ([IdUniteTravail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_HistAffectation_Ouvert] ON [rh].[HistoriqueAffectation] ([IdFonctionnaire], [DateFin]) WHERE [DateFin] IS NULL AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistCadreGrade_Cle_Periode] ON [rh].[HistoriqueCadreGrade] ([IdFonctionnaire], [DateDebut], [DateFin]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistoriqueCadreGrade_IdDocument] ON [rh].[HistoriqueCadreGrade] ([IdDocument]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistoriqueCadreGrade_IdGrade] ON [rh].[HistoriqueCadreGrade] ([IdGrade]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_HistCadreGrade_Ouvert] ON [rh].[HistoriqueCadreGrade] ([IdFonctionnaire], [DateFin]) WHERE [DateFin] IS NULL AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistoriqueResponsabilite_IdDocument] ON [rh].[HistoriqueResponsabilite] ([IdDocument]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistoriqueResponsabilite_IdResponsabilite] ON [rh].[HistoriqueResponsabilite] ([IdResponsabilite]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistoriqueResponsabilite_IdTribunal] ON [rh].[HistoriqueResponsabilite] ([IdTribunal]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistoriqueResponsabilite_IdUniteOrganisationnelle] ON [rh].[HistoriqueResponsabilite] ([IdUniteOrganisationnelle]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_HistResp_Cle_Periode] ON [rh].[HistoriqueResponsabilite] ([IdFonctionnaire], [DateDebut], [DateFin]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_HistResp_Ouvert] ON [rh].[HistoriqueResponsabilite] ([IdFonctionnaire], [IdResponsabilite], [IdTribunal], [IdUniteOrganisationnelle], [DateFin]) WHERE [DateFin] IS NULL AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_SituationAdmin_Cle_Periode] ON [rh].[SituationAdministrative] ([IdFonctionnaire], [DateDebut], [DateFin]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_SituationAdministrative_IdDocument] ON [rh].[SituationAdministrative] ([IdDocument]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    CREATE INDEX [IX_SituationAdministrative_IdStatut] ON [rh].[SituationAdministrative] ([IdStatut]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_SituationAdmin_Ouvert] ON [rh].[SituationAdministrative] ([IdFonctionnaire], [DateFin]) WHERE [DateFin] IS NULL AND [IsDeleted] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019111916_RH_05_Historiques'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019111916_RH_05_Historiques', N'10.0.13');
END;

COMMIT;
GO

