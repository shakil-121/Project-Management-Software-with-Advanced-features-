using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastPMS.Migrations
{
    /// <inheritdoc />
    public partial class AddImageFieldToDeveloper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "image",
                table: "Developers",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image",
                table: "Developers");
        }
    }
}
