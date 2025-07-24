using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NOL.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LocalizeCarColors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Color",
                table: "Cars",
                newName: "ColorEn");

            migrationBuilder.AddColumn<string>(
                name: "ColorAr",
                table: "Cars",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Migrate existing color data from English to Arabic
            migrationBuilder.Sql(@"
                UPDATE Cars 
                SET ColorAr = CASE 
                    WHEN ColorEn = 'White' THEN N'أبيض'
                    WHEN ColorEn = 'Black' THEN N'أسود'
                    WHEN ColorEn = 'Silver' THEN N'فضي'
                    WHEN ColorEn = 'Blue' THEN N'أزرق'
                    WHEN ColorEn = 'Red' THEN N'أحمر'
                    WHEN ColorEn = 'Gray' THEN N'رمادي'
                    WHEN ColorEn = 'Pearl White' THEN N'أبيض لؤلؤي'
                    WHEN ColorEn = 'Metallic Gray' THEN N'رمادي معدني'
                    ELSE N'غير محدد'  -- Default to 'غير محدد' (Not specified) for unknown colors
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorAr",
                table: "Cars");

            migrationBuilder.RenameColumn(
                name: "ColorEn",
                table: "Cars",
                newName: "Color");
        }
    }
}
