using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RH_05_Historiques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoriqueAffectation",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFonctionnaire = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTribunalAffectation = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdUniteAffectation = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdUniteTravail = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SectionTribunal = table.Column<int>(type: "int", nullable: true),
                    DateDebut = table.Column<DateOnly>(type: "date", nullable: false),
                    DateFin = table.Column<DateOnly>(type: "date", nullable: true),
                    IdDocument = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriqueAffectation", x => x.Id);
                    table.CheckConstraint("CK_HistAffectation_ExactementUn", "([IdTribunalAffectation] IS NOT NULL AND [IdUniteAffectation] IS NULL) OR ([IdTribunalAffectation] IS NULL AND [IdUniteAffectation] IS NOT NULL)");
                    table.CheckConstraint("CK_HistAffectation_Periode", "[DateFin] IS NULL OR [DateDebut] <= [DateFin]");
                    table.CheckConstraint("CK_HistAffectation_SectionTribunal", "[SectionTribunal] IS NULL OR [IdTribunalAffectation] IS NOT NULL");
                    table.CheckConstraint("CK_HistAffectation_UniteTravail", "[IdUniteTravail] IS NULL OR [IdTribunalAffectation] IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_HistAffectation_Document",
                        column: x => x.IdDocument,
                        principalSchema: "rh",
                        principalTable: "PieceJointe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistAffectation_Fonctionnaire",
                        column: x => x.IdFonctionnaire,
                        principalSchema: "rh",
                        principalTable: "Fonctionnaire",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistAffectation_Tribunal",
                        column: x => x.IdTribunalAffectation,
                        principalSchema: "rh",
                        principalTable: "Tribunal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistAffectation_UniteAffectation",
                        column: x => x.IdUniteAffectation,
                        principalSchema: "rh",
                        principalTable: "UniteOrganisationnelle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistAffectation_UniteTravail",
                        column: x => x.IdUniteTravail,
                        principalSchema: "rh",
                        principalTable: "UniteOrganisationnelle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoriqueCadreGrade",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFonctionnaire = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdGrade = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateDebut = table.Column<DateOnly>(type: "date", nullable: false),
                    DateFin = table.Column<DateOnly>(type: "date", nullable: true),
                    IdDocument = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriqueCadreGrade", x => x.Id);
                    table.CheckConstraint("CK_HistCadreGrade_Periode", "[DateFin] IS NULL OR [DateDebut] <= [DateFin]");
                    table.ForeignKey(
                        name: "FK_HistCadreGrade_Document",
                        column: x => x.IdDocument,
                        principalSchema: "rh",
                        principalTable: "PieceJointe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistCadreGrade_Fonctionnaire",
                        column: x => x.IdFonctionnaire,
                        principalSchema: "rh",
                        principalTable: "Fonctionnaire",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistCadreGrade_Grade",
                        column: x => x.IdGrade,
                        principalSchema: "rh",
                        principalTable: "Grade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoriqueResponsabilite",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFonctionnaire = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdResponsabilite = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTribunal = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdUniteOrganisationnelle = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateDebut = table.Column<DateOnly>(type: "date", nullable: false),
                    DateFin = table.Column<DateOnly>(type: "date", nullable: true),
                    IdDocument = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriqueResponsabilite", x => x.Id);
                    table.CheckConstraint("CK_HistResp_ExactementUn", "([IdTribunal] IS NOT NULL AND [IdUniteOrganisationnelle] IS NULL) OR ([IdTribunal] IS NULL AND [IdUniteOrganisationnelle] IS NOT NULL)");
                    table.CheckConstraint("CK_HistResp_Periode", "[DateFin] IS NULL OR [DateDebut] <= [DateFin]");
                    table.ForeignKey(
                        name: "FK_HistResp_Document",
                        column: x => x.IdDocument,
                        principalSchema: "rh",
                        principalTable: "PieceJointe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistResp_Fonctionnaire",
                        column: x => x.IdFonctionnaire,
                        principalSchema: "rh",
                        principalTable: "Fonctionnaire",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistResp_Responsabilite",
                        column: x => x.IdResponsabilite,
                        principalSchema: "rh",
                        principalTable: "Responsabilite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistResp_Tribunal",
                        column: x => x.IdTribunal,
                        principalSchema: "rh",
                        principalTable: "Tribunal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistResp_Unite",
                        column: x => x.IdUniteOrganisationnelle,
                        principalSchema: "rh",
                        principalTable: "UniteOrganisationnelle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SituationAdministrative",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFonctionnaire = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdStatut = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateDebut = table.Column<DateOnly>(type: "date", nullable: false),
                    DateFin = table.Column<DateOnly>(type: "date", nullable: true),
                    IdDocument = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SituationAdministrative", x => x.Id);
                    table.CheckConstraint("CK_SituationAdmin_Periode", "[DateFin] IS NULL OR [DateDebut] <= [DateFin]");
                    table.ForeignKey(
                        name: "FK_SituationAdmin_Document",
                        column: x => x.IdDocument,
                        principalSchema: "rh",
                        principalTable: "PieceJointe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SituationAdmin_Fonctionnaire",
                        column: x => x.IdFonctionnaire,
                        principalSchema: "rh",
                        principalTable: "Fonctionnaire",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SituationAdmin_Statut",
                        column: x => x.IdStatut,
                        principalSchema: "rh",
                        principalTable: "StatutAdministratif",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistAffectation_Cle_Periode",
                schema: "rh",
                table: "HistoriqueAffectation",
                columns: new[] { "IdFonctionnaire", "DateDebut", "DateFin" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueAffectation_IdDocument",
                schema: "rh",
                table: "HistoriqueAffectation",
                column: "IdDocument");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueAffectation_IdTribunalAffectation",
                schema: "rh",
                table: "HistoriqueAffectation",
                column: "IdTribunalAffectation");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueAffectation_IdUniteAffectation",
                schema: "rh",
                table: "HistoriqueAffectation",
                column: "IdUniteAffectation");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueAffectation_IdUniteTravail",
                schema: "rh",
                table: "HistoriqueAffectation",
                column: "IdUniteTravail");

            migrationBuilder.CreateIndex(
                name: "UX_HistAffectation_Ouvert",
                schema: "rh",
                table: "HistoriqueAffectation",
                columns: new[] { "IdFonctionnaire", "DateFin" },
                unique: true,
                filter: "[DateFin] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HistCadreGrade_Cle_Periode",
                schema: "rh",
                table: "HistoriqueCadreGrade",
                columns: new[] { "IdFonctionnaire", "DateDebut", "DateFin" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueCadreGrade_IdDocument",
                schema: "rh",
                table: "HistoriqueCadreGrade",
                column: "IdDocument");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueCadreGrade_IdGrade",
                schema: "rh",
                table: "HistoriqueCadreGrade",
                column: "IdGrade");

            migrationBuilder.CreateIndex(
                name: "UX_HistCadreGrade_Ouvert",
                schema: "rh",
                table: "HistoriqueCadreGrade",
                columns: new[] { "IdFonctionnaire", "DateFin" },
                unique: true,
                filter: "[DateFin] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueResponsabilite_IdDocument",
                schema: "rh",
                table: "HistoriqueResponsabilite",
                column: "IdDocument");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueResponsabilite_IdResponsabilite",
                schema: "rh",
                table: "HistoriqueResponsabilite",
                column: "IdResponsabilite");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueResponsabilite_IdTribunal",
                schema: "rh",
                table: "HistoriqueResponsabilite",
                column: "IdTribunal");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueResponsabilite_IdUniteOrganisationnelle",
                schema: "rh",
                table: "HistoriqueResponsabilite",
                column: "IdUniteOrganisationnelle");

            migrationBuilder.CreateIndex(
                name: "IX_HistResp_Cle_Periode",
                schema: "rh",
                table: "HistoriqueResponsabilite",
                columns: new[] { "IdFonctionnaire", "DateDebut", "DateFin" });

            migrationBuilder.CreateIndex(
                name: "UX_HistResp_Ouvert",
                schema: "rh",
                table: "HistoriqueResponsabilite",
                columns: new[] { "IdFonctionnaire", "IdResponsabilite", "IdTribunal", "IdUniteOrganisationnelle", "DateFin" },
                unique: true,
                filter: "[DateFin] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SituationAdmin_Cle_Periode",
                schema: "rh",
                table: "SituationAdministrative",
                columns: new[] { "IdFonctionnaire", "DateDebut", "DateFin" });

            migrationBuilder.CreateIndex(
                name: "IX_SituationAdministrative_IdDocument",
                schema: "rh",
                table: "SituationAdministrative",
                column: "IdDocument");

            migrationBuilder.CreateIndex(
                name: "IX_SituationAdministrative_IdStatut",
                schema: "rh",
                table: "SituationAdministrative",
                column: "IdStatut");

            migrationBuilder.CreateIndex(
                name: "UX_SituationAdmin_Ouvert",
                schema: "rh",
                table: "SituationAdministrative",
                columns: new[] { "IdFonctionnaire", "DateFin" },
                unique: true,
                filter: "[DateFin] IS NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoriqueAffectation",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "HistoriqueCadreGrade",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "HistoriqueResponsabilite",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "SituationAdministrative",
                schema: "rh");
        }
    }
}
