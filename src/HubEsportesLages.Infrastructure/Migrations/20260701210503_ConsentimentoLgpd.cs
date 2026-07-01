using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HubEsportesLages.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsentimentoLgpd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConsentimentoLgpdEm",
                table: "AspNetUsers",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsentimentoVersao",
                table: "AspNetUsers",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsentimentoLgpdEm",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ConsentimentoVersao",
                table: "AspNetUsers");
        }
    }
}
