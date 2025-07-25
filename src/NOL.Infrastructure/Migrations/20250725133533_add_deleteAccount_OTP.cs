using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NOL.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_deleteAccount_OTP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountDeletionOtp",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccountDeletionOtpExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountDeletionOtpResendCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAccountDeletionOtpResendTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountDeletionOtp",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AccountDeletionOtpExpiry",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AccountDeletionOtpResendCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastAccountDeletionOtpResendTime",
                table: "AspNetUsers");
        }
    }
}
