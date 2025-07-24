using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NOL.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactUsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactUs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WhatsApp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Facebook = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Instagram = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    X = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TikTok = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactUs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactUs");
        }
    }
}
