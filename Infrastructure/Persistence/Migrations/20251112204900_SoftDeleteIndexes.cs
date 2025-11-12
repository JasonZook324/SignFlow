using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SignFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Proposals_OrganizationId_Status",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Clients_OrganizationId_Name",
                table: "Clients");

            migrationBuilder.CreateTable(
                name: "ProposalTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    JsonDefinition = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_OrganizationId_Status_IsDeleted",
                table: "Proposals",
                columns: new[] { "OrganizationId", "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OrganizationId_Name_IsDeleted",
                table: "Clients",
                columns: new[] { "OrganizationId", "Name", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProposalTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_OrganizationId_Status_IsDeleted",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Clients_OrganizationId_Name_IsDeleted",
                table: "Clients");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_OrganizationId_Status",
                table: "Proposals",
                columns: new[] { "OrganizationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OrganizationId_Name",
                table: "Clients",
                columns: new[] { "OrganizationId", "Name" });
        }
    }
}
