using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Advanced_Pet_Grooming.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePetAndPetPhotoModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Customers_customer_id",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "vaccination_record",
                table: "Pets");

            migrationBuilder.RenameColumn(
                name: "breed",
                table: "Pets",
                newName: "Breed");

            migrationBuilder.RenameColumn(
                name: "size_category",
                table: "Pets",
                newName: "SizeCategory");

            migrationBuilder.RenameColumn(
                name: "pet_type",
                table: "Pets",
                newName: "PetType");

            migrationBuilder.RenameColumn(
                name: "pet_remark",
                table: "Pets",
                newName: "PetRemark");

            migrationBuilder.RenameColumn(
                name: "pet_name",
                table: "Pets",
                newName: "PetName");

            migrationBuilder.RenameColumn(
                name: "medical_record",
                table: "Pets",
                newName: "MedicalNotes");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "Pets",
                newName: "CustomerID");

            migrationBuilder.RenameColumn(
                name: "pet_id",
                table: "Pets",
                newName: "PetID");

            migrationBuilder.RenameIndex(
                name: "IX_Pets_customer_id",
                table: "Pets",
                newName: "IX_Pets_CustomerID");

            migrationBuilder.AlterColumn<string>(
                name: "Breed",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MedicalNotes",
                table: "Pets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Pets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Pets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsVaccinated",
                table: "Pets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "VaccinationDate",
                table: "Pets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VaccinationExpiryDate",
                table: "Pets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VaccineName",
                table: "Pets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Pets",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PetPhotos",
                columns: table => new
                {
                    PetPhotoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetID = table.Column<int>(type: "int", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetPhotos", x => x.PetPhotoID);
                    table.ForeignKey(
                        name: "FK_PetPhotos_Pets_PetID",
                        column: x => x.PetID,
                        principalTable: "Pets",
                        principalColumn: "PetID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PetPhotos_PetID",
                table: "PetPhotos",
                column: "PetID");

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Customers_CustomerID",
                table: "Pets",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Customers_CustomerID",
                table: "Pets");

            migrationBuilder.DropTable(
                name: "PetPhotos");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "IsVaccinated",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "VaccinationDate",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "VaccinationExpiryDate",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "VaccineName",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Pets");

            migrationBuilder.RenameColumn(
                name: "Breed",
                table: "Pets",
                newName: "breed");

            migrationBuilder.RenameColumn(
                name: "SizeCategory",
                table: "Pets",
                newName: "size_category");

            migrationBuilder.RenameColumn(
                name: "PetType",
                table: "Pets",
                newName: "pet_type");

            migrationBuilder.RenameColumn(
                name: "PetRemark",
                table: "Pets",
                newName: "pet_remark");

            migrationBuilder.RenameColumn(
                name: "PetName",
                table: "Pets",
                newName: "pet_name");

            migrationBuilder.RenameColumn(
                name: "MedicalNotes",
                table: "Pets",
                newName: "medical_record");

            migrationBuilder.RenameColumn(
                name: "CustomerID",
                table: "Pets",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "PetID",
                table: "Pets",
                newName: "pet_id");

            migrationBuilder.RenameIndex(
                name: "IX_Pets_CustomerID",
                table: "Pets",
                newName: "IX_Pets_customer_id");

            migrationBuilder.AlterColumn<string>(
                name: "breed",
                table: "Pets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "medical_record",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vaccination_record",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Customers_customer_id",
                table: "Pets",
                column: "customer_id",
                principalTable: "Customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
