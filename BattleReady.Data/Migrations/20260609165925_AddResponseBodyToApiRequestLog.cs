using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleReady.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResponseBodyToApiRequestLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponseBody",
                table: "ApiRequestLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponseBody",
                table: "ApiRequestLogs");
        }
    }
}
