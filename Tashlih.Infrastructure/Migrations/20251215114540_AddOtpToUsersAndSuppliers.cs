using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tashlih.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpToUsersAndSuppliers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "active",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "active");

            migrationBuilder.AddColumn<string>(
                name: "OtpCode",
                table: "users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiresAt",
                table: "users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtpCode",
                table: "supplier_profile",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiresAt",
                table: "supplier_profile",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtpCode",
                table: "users");

            migrationBuilder.DropColumn(
                name: "OtpExpiresAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "OtpCode",
                table: "supplier_profile");

            migrationBuilder.DropColumn(
                name: "OtpExpiresAt",
                table: "supplier_profile");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "active",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldDefaultValue: "active");
        }
    }
}
