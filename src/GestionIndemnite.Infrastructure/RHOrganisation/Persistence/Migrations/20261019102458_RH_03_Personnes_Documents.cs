using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RH_03_Personnes_Documents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fonctionnaire",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Matricule = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NomAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrenomAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NomFr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrenomFr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateNaissance = table.Column<DateOnly>(type: "date", nullable: false),
                    DateRecrutement = table.Column<DateOnly>(type: "date", nullable: false),
                    Genre = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fonctionnaire", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PieceJointe",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroDocument = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroDocumentNormalise = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateDocument = table.Column<DateOnly>(type: "date", nullable: false),
                    AnneeDocument = table.Column<int>(type: "int", nullable: false),
                    Emetteur = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EmetteurNormalise = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TypeDocument = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Objet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NomFichier = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CheminFichier = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HashFichier = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DateAjout = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PieceJointe", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rib",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdFonctionnaire = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodeBanque = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CodeVille = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    NumeroCompte = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Cle = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    EstActif = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rib", x => x.Id);
                    table.CheckConstraint("CK_Rib_Cle", "[Cle] NOT LIKE '%[^0-9]%' AND LEN([Cle]) = 2");
                    table.CheckConstraint("CK_Rib_CodeBanque", "[CodeBanque] NOT LIKE '%[^0-9]%' AND LEN([CodeBanque]) = 3");
                    table.CheckConstraint("CK_Rib_CodeVille", "[CodeVille] NOT LIKE '%[^0-9]%' AND LEN([CodeVille]) = 3");
                    table.CheckConstraint("CK_Rib_NumeroCompte", "[NumeroCompte] NOT LIKE '%[^0-9]%' AND LEN([NumeroCompte]) = 16");
                    table.ForeignKey(
                        name: "FK_Rib_Fonctionnaire",
                        column: x => x.IdFonctionnaire,
                        principalSchema: "rh",
                        principalTable: "Fonctionnaire",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Fonctionnaire_Cin",
                schema: "rh",
                table: "Fonctionnaire",
                column: "Cin",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Fonctionnaire_Matricule",
                schema: "rh",
                table: "Fonctionnaire",
                column: "Matricule",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_PieceJointe_Cle",
                schema: "rh",
                table: "PieceJointe",
                columns: new[] { "NumeroDocumentNormalise", "AnneeDocument", "EmetteurNormalise" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Rib_Actif",
                schema: "rh",
                table: "Rib",
                column: "IdFonctionnaire",
                unique: true,
                filter: "[EstActif] = 1 AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PieceJointe",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "Rib",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "Fonctionnaire",
                schema: "rh");
        }
    }
}
