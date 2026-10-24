using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RH_08_NumeroDatesOrganigramme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DateCreation",
                schema: "rh",
                table: "UniteOrganisationnelle",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateCreation",
                schema: "rh",
                table: "Tribunal",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroTribunal",
                schema: "rh",
                table: "Tribunal",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "UX_Tribunal_Numero",
                schema: "rh",
                table: "Tribunal",
                column: "NumeroTribunal",
                unique: true,
                filter: "[NumeroTribunal] <> '' AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Tribunal_Numero",
                schema: "rh",
                table: "Tribunal");

            migrationBuilder.DropColumn(
                name: "DateCreation",
                schema: "rh",
                table: "UniteOrganisationnelle");

            migrationBuilder.DropColumn(
                name: "DateCreation",
                schema: "rh",
                table: "Tribunal");

            migrationBuilder.DropColumn(
                name: "NumeroTribunal",
                schema: "rh",
                table: "Tribunal");
        }
    }
}
