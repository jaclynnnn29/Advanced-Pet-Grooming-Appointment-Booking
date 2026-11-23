using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Advanced_Pet_Grooming.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    account_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    staff_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    account_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staffs", x => x.staff_id);
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    pet_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    pet_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    pet_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    breed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    size_category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    vaccination_record = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    medical_record = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pet_remark = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.pet_id);
                    table.ForeignKey(
                        name: "FK_Pets_Customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "Customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    announcement_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    publish_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.announcement_id);
                    table.ForeignKey(
                        name: "FK_Announcements_Staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Staffs",
                        principalColumn: "staff_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    appointment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    pet_id = table.Column<int>(type: "int", nullable: false),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    booking_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    service_package = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    appointment_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    time_slot = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    appointment_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.appointment_id);
                    table.ForeignKey(
                        name: "FK_Appointments_Customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "Customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Pets_pet_id",
                        column: x => x.pet_id,
                        principalTable: "Pets",
                        principalColumn: "pet_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Staffs",
                        principalColumn: "staff_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    appointment_id = table.Column<int>(type: "int", nullable: false),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    invoice_details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    transaction_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    payment_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_Payments_Appointments_appointment_id",
                        column: x => x.appointment_id,
                        principalTable: "Appointments",
                        principalColumn: "appointment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Staffs_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Staffs",
                        principalColumn: "staff_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_staff_id",
                table: "Announcements",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_customer_id",
                table: "Appointments",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_pet_id",
                table: "Appointments",
                column: "pet_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_staff_id",
                table: "Appointments",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_appointment_id",
                table: "Payments",
                column: "appointment_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_staff_id",
                table: "Payments",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_customer_id",
                table: "Pets",
                column: "customer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
