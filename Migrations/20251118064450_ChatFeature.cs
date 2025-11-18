using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastPMS.Migrations
{
    /// <inheritdoc />
    public partial class ChatFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsersId1",
                table: "AspNetUserRoles",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LiveChats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiverId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveChats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveChats_AspNetUsers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LiveChats_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UsersId1",
                table: "AspNetUserRoles",
                column: "UsersId1");

            migrationBuilder.CreateIndex(
                name: "IX_LiveChats_ReceiverId",
                table: "LiveChats",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveChats_SenderId",
                table: "LiveChats",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UsersId1",
                table: "AspNetUserRoles",
                column: "UsersId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UsersId1",
                table: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "LiveChats");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UsersId1",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "UsersId1",
                table: "AspNetUserRoles");
        }
    }
}
