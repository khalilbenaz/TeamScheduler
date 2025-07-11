﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanningPresenceBlazor.Migrations
{
    /// <inheritdoc />
    public partial class FixClientDescriptionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Clients",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Clients");
        }
    }
}
