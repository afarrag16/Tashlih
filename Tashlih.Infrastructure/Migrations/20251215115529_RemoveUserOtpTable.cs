using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tashlih.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserOtpTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_otp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_otp",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    attempts = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_used = table.Column<bool>(type: "bit", nullable: false),
                    otp_code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    purpose = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "login"),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__user_otp__3213E83F9BF32B49", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_otp_expires",
                table: "user_otp",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_otp_phone",
                table: "user_otp",
                column: "phone");
        }
    }
}
