using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Advanced_Pet_Grooming.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnouncementsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Staffs_staff_id",
                table: "Announcements");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Announcements",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Announcements",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "publish_date",
                table: "Announcements",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "Announcements",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "announcement_id",
                table: "Announcements",
                newName: "AnnouncementId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Announcements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "staff_id",
                table: "Announcements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Announcements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Staffs_staff_id",
                table: "Announcements",
                column: "staff_id",
                principalTable: "Staffs",
                principalColumn: "staff_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Staffs_staff_id",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Announcements");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Announcements",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Announcements",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Announcements",
                newName: "content");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Announcements",
                newName: "publish_date");

            migrationBuilder.RenameColumn(
                name: "AnnouncementId",
                table: "Announcements",
                newName: "announcement_id");

            migrationBuilder.AlterColumn<int>(
                name: "staff_id",
                table: "Announcements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "Announcements",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Staffs_staff_id",
                table: "Announcements",
                column: "staff_id",
                principalTable: "Staffs",
                principalColumn: "staff_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
