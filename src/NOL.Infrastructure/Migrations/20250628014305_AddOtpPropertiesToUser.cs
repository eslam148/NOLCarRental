using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NOL.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpPropertiesToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationOtp",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationOtpExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetOtp",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetOtpExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationOtp",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationOtpExpiry",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetOtp",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetOtpExpiry",
                table: "AspNetUsers");
        }
    }
}
