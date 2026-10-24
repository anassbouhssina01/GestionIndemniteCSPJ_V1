using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RH_11_RestaurerDescriptionStatutAdministratif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "rh",
                table: "StatutAdministratif",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.DropColumn(
                name: "MatriculeCspj",
                schema: "rh",
                table: "StatutAdministratif");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatriculeCspj",
                schema: "rh",
                table: "StatutAdministratif",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "rh",
                table: "StatutAdministratif");
        }
    }
}
