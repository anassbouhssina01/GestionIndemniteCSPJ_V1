using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RH_04_Organigramme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tribunal",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomTribunal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TypeTribunal = table.Column<int>(type: "int", nullable: false),
                    NiveauJuridiction = table.Column<int>(type: "int", nullable: false),
                    IdVille = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tribunal", x => x.Id);
                    table.CheckConstraint("CK_Tribunal_CassationGenerale", "[NiveauJuridiction] <> 3 OR [TypeTribunal] = 0");
                    table.ForeignKey(
                        name: "FK_Tribunal_Ville",
                        column: x => x.IdVille,
                        principalSchema: "rh",
                        principalTable: "Ville",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UniteOrganisationnelle",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomUnite = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TypeUnite = table.Column<int>(type: "int", nullable: false),
                    IdVille = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniteOrganisationnelle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniteOrganisationnelle_Ville",
                        column: x => x.IdVille,
                        principalSchema: "rh",
                        principalTable: "Ville",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoriqueOrganigrammeTribunal",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTribunal = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTribunalParent = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_HistoriqueOrganigrammeTribunal", x => x.Id);
                    table.CheckConstraint("CK_HistOrgTribunal_Periode", "[DateFin] IS NULL OR [DateDebut] <= [DateFin]");
                    table.ForeignKey(
                        name: "FK_HistOrgTribunal_Document",
                        column: x => x.IdDocument,
                        principalSchema: "rh",
                        principalTable: "PieceJointe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistOrgTribunal_Parent",
                        column: x => x.IdTribunalParent,
                        principalSchema: "rh",
                        principalTable: "Tribunal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistOrgTribunal_Tribunal",
                        column: x => x.IdTribunal,
                        principalSchema: "rh",
                        principalTable: "Tribunal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoriqueOrganigrammeUnite",
                schema: "rh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUnite = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUniteParent = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_HistoriqueOrganigrammeUnite", x => x.Id);
                    table.CheckConstraint("CK_HistOrgUnite_Periode", "[DateFin] IS NULL OR [DateDebut] <= [DateFin]");
                    table.ForeignKey(
                        name: "FK_HistOrgUnite_Document",
                        column: x => x.IdDocument,
                        principalSchema: "rh",
                        principalTable: "PieceJointe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistOrgUnite_Parent",
                        column: x => x.IdUniteParent,
                        principalSchema: "rh",
                        principalTable: "UniteOrganisationnelle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistOrgUnite_Unite",
                        column: x => x.IdUnite,
                        principalSchema: "rh",
                        principalTable: "UniteOrganisationnelle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistOrgTribunal_Cle_Periode",
                schema: "rh",
                table: "HistoriqueOrganigrammeTribunal",
                columns: new[] { "IdTribunal", "DateDebut", "DateFin" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueOrganigrammeTribunal_IdDocument",
                schema: "rh",
                table: "HistoriqueOrganigrammeTribunal",
                column: "IdDocument");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueOrganigrammeTribunal_IdTribunalParent",
                schema: "rh",
                table: "HistoriqueOrganigrammeTribunal",
                column: "IdTribunalParent");

            migrationBuilder.CreateIndex(
                name: "UX_HistOrgTribunal_Ouvert",
                schema: "rh",
                table: "HistoriqueOrganigrammeTribunal",
                columns: new[] { "IdTribunal", "DateFin" },
                unique: true,
                filter: "[DateFin] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_HistOrgUnite_Cle_Periode",
                schema: "rh",
                table: "HistoriqueOrganigrammeUnite",
                columns: new[] { "IdUnite", "DateDebut", "DateFin" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueOrganigrammeUnite_IdDocument",
                schema: "rh",
                table: "HistoriqueOrganigrammeUnite",
                column: "IdDocument");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriqueOrganigrammeUnite_IdUniteParent",
                schema: "rh",
                table: "HistoriqueOrganigrammeUnite",
                column: "IdUniteParent");

            migrationBuilder.CreateIndex(
                name: "UX_HistOrgUnite_Ouvert",
                schema: "rh",
                table: "HistoriqueOrganigrammeUnite",
                columns: new[] { "IdUnite", "DateFin" },
                unique: true,
                filter: "[DateFin] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Tribunal_IdVille",
                schema: "rh",
                table: "Tribunal",
                column: "IdVille");

            migrationBuilder.CreateIndex(
                name: "IX_UniteOrganisationnelle_IdVille",
                schema: "rh",
                table: "UniteOrganisationnelle",
                column: "IdVille");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoriqueOrganigrammeTribunal",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "HistoriqueOrganigrammeUnite",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "Tribunal",
                schema: "rh");

            migrationBuilder.DropTable(
                name: "UniteOrganisationnelle",
                schema: "rh");
        }
    }
}
