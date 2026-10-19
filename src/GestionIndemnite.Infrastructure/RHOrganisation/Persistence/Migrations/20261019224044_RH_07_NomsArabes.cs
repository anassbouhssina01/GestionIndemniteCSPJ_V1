using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RH_07_NomsArabes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomVilleAr",
                schema: "rh",
                table: "Ville",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomUniteAr",
                schema: "rh",
                table: "UniteOrganisationnelle",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomTribunalAr",
                schema: "rh",
                table: "Tribunal",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomVilleAr",
                schema: "rh",
                table: "Ville");

            migrationBuilder.DropColumn(
                name: "NomUniteAr",
                schema: "rh",
                table: "UniteOrganisationnelle");

            migrationBuilder.DropColumn(
                name: "NomTribunalAr",
                schema: "rh",
                table: "Tribunal");
        }
    }
}
