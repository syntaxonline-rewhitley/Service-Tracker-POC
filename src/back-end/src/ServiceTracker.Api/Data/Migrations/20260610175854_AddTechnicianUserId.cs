using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceTracker.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Technicians",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Technicians");
        }
    }
}
