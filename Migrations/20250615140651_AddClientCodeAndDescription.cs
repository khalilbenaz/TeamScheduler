using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanningPresenceBlazor.Migrations
{
    /// <inheritdoc />
    public partial class AddClientCodeAndDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeHierarchy");

            migrationBuilder.AddColumn<int>(
                name: "JoursClientObligatoires",
                table: "Employes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "JoursSiteObligatoires",
                table: "Employes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "JoursTeletravailObligatoires",
                table: "Employes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEntree",
                table: "Employee",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSortie",
                table: "Employee",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentsRH",
                table: "Employee",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "JoursClient",
                table: "Employee",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "JoursSite",
                table: "Employee",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "JoursTeletravail",
                table: "Employee",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "Employee",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlageHorairePreferee",
                table: "Employee",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeContrat",
                table: "Employee",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Client",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PresenceRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Commentaire = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresenceRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PresenceRecord_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_ManagerId",
                table: "Employee",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_PresenceRecord_EmployeeId",
                table: "PresenceRecord",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Employee_ManagerId",
                table: "Employee",
                column: "ManagerId",
                principalTable: "Employee",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Employee_ManagerId",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "PresenceRecord");

            migrationBuilder.DropIndex(
                name: "IX_Employee_ManagerId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "JoursClientObligatoires",
                table: "Employes");

            migrationBuilder.DropColumn(
                name: "JoursSiteObligatoires",
                table: "Employes");

            migrationBuilder.DropColumn(
                name: "JoursTeletravailObligatoires",
                table: "Employes");

            migrationBuilder.DropColumn(
                name: "DateEntree",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "DateSortie",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "DocumentsRH",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "JoursClient",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "JoursSite",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "JoursTeletravail",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "PlageHorairePreferee",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "TypeContrat",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Client");

            migrationBuilder.CreateTable(
                name: "EmployeeHierarchy",
                columns: table => new
                {
                    SubordonnesId = table.Column<int>(type: "INTEGER", nullable: false),
                    SuperieursId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeHierarchy", x => new { x.SubordonnesId, x.SuperieursId });
                    table.ForeignKey(
                        name: "FK_EmployeeHierarchy_Employee_SubordonnesId",
                        column: x => x.SubordonnesId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeHierarchy_Employee_SuperieursId",
                        column: x => x.SuperieursId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeHierarchy_SuperieursId",
                table: "EmployeeHierarchy",
                column: "SuperieursId");
        }
    }
}
