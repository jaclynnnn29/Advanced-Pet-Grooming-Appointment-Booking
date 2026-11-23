using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Advanced_Pet_Grooming.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomerSecurityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "failed_login_attempts",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "locked_until",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reset_token",
                table: "Customers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reset_token_expiry",
                table: "Customers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "failed_login_attempts",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "locked_until",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "reset_token",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "reset_token_expiry",
                table: "Customers");
        }
    }
}
