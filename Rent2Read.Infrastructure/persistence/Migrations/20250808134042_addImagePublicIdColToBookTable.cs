using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rent2Read.Infrastructure.persistence
{
    /// <inheritdoc />
    public partial class addImagePublicIdColToBookTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "Books",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "Books");
        }
    }
}
