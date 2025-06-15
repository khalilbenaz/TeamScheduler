using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanningPresenceBlazor.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Prenom",
                table: "Employes",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Projets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstActif = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projets_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AffectationsEquipeProjet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ProjetId1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffectationsEquipeProjet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AffectationsEquipeProjet_Equipes_EquipeId",
                        column: x => x.EquipeId,
                        principalTable: "Equipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AffectationsEquipeProjet_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AffectationsEquipeProjet_Projets_ProjetId1",
                        column: x => x.ProjetId1,
                        principalTable: "Projets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AffectationEquipeProjet_EquipeProjet",
                table: "AffectationsEquipeProjet",
                columns: new[] { "EquipeId", "ProjetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsEquipeProjet_ProjetId",
                table: "AffectationsEquipeProjet",
                column: "ProjetId");

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsEquipeProjet_ProjetId1",
                table: "AffectationsEquipeProjet",
                column: "ProjetId1");

            migrationBuilder.CreateIndex(
                name: "IX_Projets_ClientId",
                table: "Projets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Projets_Nom",
                table: "Projets",
                column: "Nom");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AffectationsEquipeProjet");

            migrationBuilder.DropTable(
                name: "Projets");

            migrationBuilder.DropColumn(
                name: "Prenom",
                table: "Employes");
        }
    }
}
