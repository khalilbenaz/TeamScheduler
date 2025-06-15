using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanningPresenceBlazor.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", nullable: true),
                    AccentColor = table.Column<string>(type: "TEXT", nullable: true),
                    FontSize = table.Column<string>(type: "TEXT", nullable: true),
                    NotifEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifPlanning = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifConge = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifReport = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifReminder = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifDaily = table.Column<string>(type: "TEXT", nullable: true),
                    NotifWeekly = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");
        }
    }
}
