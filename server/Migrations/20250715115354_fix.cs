using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "User",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_ManagerId",
                table: "User",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_User_ManagerId",
                table: "User",
                column: "ManagerId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_User_ManagerId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_ManagerId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "User");
        }
    }
}
