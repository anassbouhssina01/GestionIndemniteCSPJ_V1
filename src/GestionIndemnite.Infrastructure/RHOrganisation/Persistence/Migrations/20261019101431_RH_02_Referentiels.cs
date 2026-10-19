using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RH_02_Referentiels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Grade",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCadre = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroReferencie = table.Column<int>(type: "int", nullable: false),
                    LibelleGradeAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LibelleGradeFr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstActif = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grade_Cadre",
                        column: x => x.IdCadre,
                        principalSchema: "rh",
                        principalTable: "Cadre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pays",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodePays = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NomPaysAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomPaysFr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EstLocal = table.Column<bool>(type: "bit", nullable: false),
                    EstActif = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Responsabilite",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodeResponsabilite = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LibelleResponsabiliteAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LibelleResponsabiliteFr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstActif = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsabilite", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatutAdministratif",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodeStatut = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LibelleStatutAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LibelleStatutFr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstActif = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatutAdministratif", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ville",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomVille = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdPays = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ville", x => x.Id);
                    table.CheckConstraint("CK_Ville_Latitude", "[Latitude] IS NULL OR [Latitude] BETWEEN -90 AND 90");
                    table.CheckConstraint("CK_Ville_Longitude", "[Longitude] IS NULL OR [Longitude] BETWEEN -180 AND 180");
                    table.ForeignKey(
                        name: "FK_Ville_Pays",
                        column: x => x.IdPays,
                        principalSchema: "rh",
                        principalTable: "Pays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_Grade_IdCadre_NumeroReferencie",
                schema: "rh",
                table: "Grade",
                columns: new[] { "IdCadre", "NumeroReferencie" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Pays_CodePays",
                schema: "rh",
                table: "Pays",
                column: "CodePays",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Pays_EstLocal",
                schema: "rh",
                table: "Pays",
                column: "EstLocal",
                unique: true,
                filter: "[EstLocal] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Responsabilite_Code",
                schema: "rh",
                table: "Responsabilite",
                column: "CodeResponsabilite",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_StatutAdministratif_Code",
                schema: "rh",
                table: "StatutAdministratif",
                column: "CodeStatut",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Ville_IdPays",
                schema: "rh",
                table: "Ville",
                column: "IdPays");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Grade",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "Responsabilite",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "StatutAdministratif",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "Ville",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "Pays",
                schema: "rh");
        }
    }
}
