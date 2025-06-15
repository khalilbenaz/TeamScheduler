using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PlanningPresenceBlazor.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlanningSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CodeClient = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Adresse = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Ville = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CodePostal = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Pays = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ContactPrincipal = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EmailContact = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    TelephoneContact = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EstActif = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateDebutContrat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateFinContrat = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HeuresOuverture = table.Column<string>(type: "TEXT", nullable: true),
                    DistanceKm = table.Column<int>(type: "INTEGER", nullable: false),
                    TempsDeplacement = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Competences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Categorie = table.Column<int>(type: "INTEGER", nullable: false),
                    EstActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationsPlanning",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EstActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    PresencesMinParPersonne = table.Column<int>(type: "INTEGER", nullable: false),
                    PresencesMaxParPersonne = table.Column<int>(type: "INTEGER", nullable: false),
                    PresencesMinParJour = table.Column<int>(type: "INTEGER", nullable: false),
                    PresencesMaxParJour = table.Column<int>(type: "INTEGER", nullable: false),
                    JoursCritiques = table.Column<string>(type: "TEXT", nullable: false),
                    PresencesMinJoursCritiques = table.Column<int>(type: "INTEGER", nullable: false),
                    PresencesMaxJoursCritiques = table.Column<int>(type: "INTEGER", nullable: false),
                    JoursFlexibles = table.Column<string>(type: "TEXT", nullable: false),
                    PresencesMinJoursFlexibles = table.Column<int>(type: "INTEGER", nullable: false),
                    PresencesMaxJoursFlexibles = table.Column<int>(type: "INTEGER", nullable: false),
                    RotationEquitable = table.Column<bool>(type: "INTEGER", nullable: false),
                    RespectCompetences = table.Column<bool>(type: "INTEGER", nullable: false),
                    OptimiserDeplacements = table.Column<bool>(type: "INTEGER", nullable: false),
                    DelaiNotificationJours = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateModification = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiePar = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationsPlanning", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientCompetencesRequises",
                columns: table => new
                {
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    CompetenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    NiveauMinimum = table.Column<int>(type: "INTEGER", nullable: false),
                    EstObligatoire = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCompetencesRequises", x => new { x.ClientId, x.CompetenceId });
                    table.ForeignKey(
                        name: "FK_ClientCompetencesRequises_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientCompetencesRequises_Competences_CompetenceId",
                        column: x => x.CompetenceId,
                        principalTable: "Competences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AffectationsEquipeClient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EquipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    JoursPresence = table.Column<string>(type: "TEXT", nullable: true),
                    NombreMinPersonnes = table.Column<int>(type: "INTEGER", nullable: false),
                    NombreMaxPersonnes = table.Column<int>(type: "INTEGER", nullable: false),
                    TypeMission = table.Column<int>(type: "INTEGER", nullable: false),
                    FrequenceMission = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffectationsEquipeClient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AffectationsEquipeClient_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanningsEquipeClient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AffectationId = table.Column<int>(type: "INTEGER", nullable: false),
                    DatePlanning = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SemaineNumero = table.Column<int>(type: "INTEGER", nullable: false),
                    Annee = table.Column<int>(type: "INTEGER", nullable: false),
                    EmployesAssignes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValidePar = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NombrePresencesReelles = table.Column<int>(type: "INTEGER", nullable: false),
                    NombrePresencesPrevues = table.Column<int>(type: "INTEGER", nullable: false),
                    TauxPresence = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningsEquipeClient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanningsEquipeClient_AffectationsEquipeClient_AffectationId",
                        column: x => x.AffectationId,
                        principalTable: "AffectationsEquipeClient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DateDebut = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Raison = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    EmployeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DateDemande = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValidePar = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CommentaireValidation = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeCompetences",
                columns: table => new
                {
                    EmployeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CompetenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Niveau = table.Column<int>(type: "INTEGER", nullable: false),
                    DateAcquisition = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateExpiration = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstCertifie = table.Column<bool>(type: "INTEGER", nullable: false),
                    NumeroCertification = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeCompetences", x => new { x.EmployeId, x.CompetenceId });
                    table.ForeignKey(
                        name: "FK_EmployeCompetences_Competences_CompetenceId",
                        column: x => x.CompetenceId,
                        principalTable: "Competences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Telephone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TeamsId = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    EstActif = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateEmbauche = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NotificationEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotificationSMS = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotificationTeams = table.Column<bool>(type: "INTEGER", nullable: false),
                    EquipeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Poste = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    NiveauCompetence = table.Column<int>(type: "INTEGER", nullable: false),
                    PresencesMinSemaine = table.Column<int>(type: "INTEGER", nullable: false),
                    PresencesMaxSemaine = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CodeEquipe = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EstActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChefEquipeId = table.Column<int>(type: "INTEGER", nullable: true),
                    PresencesMinParJour = table.Column<int>(type: "INTEGER", nullable: false),
                    PresencesMaxParJour = table.Column<int>(type: "INTEGER", nullable: false),
                    PresencesMinParPersonne = table.Column<int>(type: "INTEGER", nullable: false),
                    PresencesMaxParPersonne = table.Column<int>(type: "INTEGER", nullable: false),
                    JoursCritiques = table.Column<string>(type: "TEXT", nullable: true),
                    PresencesMinJoursCritiques = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipes_Employes_ChefEquipeId",
                        column: x => x.ChefEquipeId,
                        principalTable: "Employes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Presences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Presences_Employes_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "Employes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoriquesAffectation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeId = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipeId = table.Column<int>(type: "INTEGER", nullable: true),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: true),
                    DateDebut = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TypeAffectation = table.Column<int>(type: "INTEGER", nullable: false),
                    Motif = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreePar = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoriquesAffectation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoriquesAffectation_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HistoriquesAffectation_Employes_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "Employes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoriquesAffectation_Equipes_EquipeId",
                        column: x => x.EquipeId,
                        principalTable: "Equipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Competences",
                columns: new[] { "Id", "Categorie", "Description", "EstActive", "Nom" },
                values: new object[,]
                {
                    { 1, 0, null, true, "Développement Web" },
                    { 2, 0, null, true, "Support Technique" },
                    { 3, 1, null, true, "Gestion de Projet" },
                    { 4, 2, null, true, "Anglais" },
                    { 5, 4, null, true, "Permis B" },
                    { 6, 3, null, true, "ITIL" },
                    { 7, 0, null, true, "Sécurité Informatique" },
                    { 8, 0, null, true, "Administration Système" }
                });

            migrationBuilder.InsertData(
                table: "ConfigurationsPlanning",
                columns: new[] { "Id", "DateCreation", "DateModification", "DelaiNotificationJours", "EstActive", "JoursCritiques", "JoursFlexibles", "ModifiePar", "Nom", "OptimiserDeplacements", "PresencesMaxJoursCritiques", "PresencesMaxJoursFlexibles", "PresencesMaxParJour", "PresencesMaxParPersonne", "PresencesMinJoursCritiques", "PresencesMinJoursFlexibles", "PresencesMinParJour", "PresencesMinParPersonne", "RespectCompetences", "RotationEquitable" },
                values: new object[] { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 7, true, "[\"Lundi\",\"Mardi\",\"Vendredi\"]", "[\"Mercredi\",\"Jeudi\"]", null, "Configuration par défaut", true, 3, 3, 4, 5, 2, 0, 2, 3, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsEquipeClient_ClientId",
                table: "AffectationsEquipeClient",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsEquipeClient_EquipeId_ClientId_DateDebut",
                table: "AffectationsEquipeClient",
                columns: new[] { "EquipeId", "ClientId", "DateDebut" });

            migrationBuilder.CreateIndex(
                name: "IX_AffectationsEquipeClient_EstActive",
                table: "AffectationsEquipeClient",
                column: "EstActive");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCompetencesRequises_CompetenceId",
                table: "ClientCompetencesRequises",
                column: "CompetenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CodeClient",
                table: "Clients",
                column: "CodeClient",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_EstActif",
                table: "Clients",
                column: "EstActif");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Nom",
                table: "Clients",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_Competences_Categorie",
                table: "Competences",
                column: "Categorie");

            migrationBuilder.CreateIndex(
                name: "IX_Competences_Nom",
                table: "Competences",
                column: "Nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationsPlanning_EstActive",
                table: "ConfigurationsPlanning",
                column: "EstActive");

            migrationBuilder.CreateIndex(
                name: "IX_Conges_DateDebut_DateFin",
                table: "Conges",
                columns: new[] { "DateDebut", "DateFin" });

            migrationBuilder.CreateIndex(
                name: "IX_Conges_EmployeId",
                table: "Conges",
                column: "EmployeId");

            migrationBuilder.CreateIndex(
                name: "IX_Conges_Status",
                table: "Conges",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeCompetences_CompetenceId",
                table: "EmployeCompetences",
                column: "CompetenceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeCompetences_DateExpiration",
                table: "EmployeCompetences",
                column: "DateExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeCompetences_Niveau",
                table: "EmployeCompetences",
                column: "Niveau");

            migrationBuilder.CreateIndex(
                name: "IX_Employes_Email",
                table: "Employes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employes_EquipeId",
                table: "Employes",
                column: "EquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employes_Nom",
                table: "Employes",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_Equipes_ChefEquipeId",
                table: "Equipes",
                column: "ChefEquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipes_CodeEquipe",
                table: "Equipes",
                column: "CodeEquipe",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipes_Nom",
                table: "Equipes",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriquesAffectation_ClientId",
                table: "HistoriquesAffectation",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriquesAffectation_DateDebut",
                table: "HistoriquesAffectation",
                column: "DateDebut");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriquesAffectation_EmployeId",
                table: "HistoriquesAffectation",
                column: "EmployeId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoriquesAffectation_EquipeId",
                table: "HistoriquesAffectation",
                column: "EquipeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningsEquipeClient_AffectationId_DatePlanning",
                table: "PlanningsEquipeClient",
                columns: new[] { "AffectationId", "DatePlanning" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningsEquipeClient_Annee_SemaineNumero",
                table: "PlanningsEquipeClient",
                columns: new[] { "Annee", "SemaineNumero" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanningsEquipeClient_Status",
                table: "PlanningsEquipeClient",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Presences_EmployeId",
                table: "Presences",
                column: "EmployeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AffectationsEquipeClient_Equipes_EquipeId",
                table: "AffectationsEquipeClient",
                column: "EquipeId",
                principalTable: "Equipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Conges_Employes_EmployeId",
                table: "Conges",
                column: "EmployeId",
                principalTable: "Employes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeCompetences_Employes_EmployeId",
                table: "EmployeCompetences",
                column: "EmployeId",
                principalTable: "Employes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employes_Equipes_EquipeId",
                table: "Employes",
                column: "EquipeId",
                principalTable: "Equipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employes_Equipes_EquipeId",
                table: "Employes");

            migrationBuilder.DropTable(
                name: "ClientCompetencesRequises");

            migrationBuilder.DropTable(
                name: "ConfigurationsPlanning");

            migrationBuilder.DropTable(
                name: "Conges");

            migrationBuilder.DropTable(
                name: "EmployeCompetences");

            migrationBuilder.DropTable(
                name: "HistoriquesAffectation");

            migrationBuilder.DropTable(
                name: "PlanningsEquipeClient");

            migrationBuilder.DropTable(
                name: "Presences");

            migrationBuilder.DropTable(
                name: "Competences");

            migrationBuilder.DropTable(
                name: "AffectationsEquipeClient");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Equipes");

            migrationBuilder.DropTable(
                name: "Employes");
        }
    }
}
