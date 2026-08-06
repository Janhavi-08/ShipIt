using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnvironmentVariables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnvironmentVariables",
                columns: table => new
                {
                    EnvironmentVariableId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeploymentConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentVariables", x => x.EnvironmentVariableId);
                    table.ForeignKey(
                        name: "FK_EnvironmentVariables_DeploymentConfigurations_DeploymentCon~",
                        column: x => x.DeploymentConfigurationId,
                        principalTable: "DeploymentConfigurations",
                        principalColumn: "DeploymentConfigurationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentVariables_DeploymentConfigurationId_Key",
                table: "EnvironmentVariables",
                columns: new[] { "DeploymentConfigurationId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnvironmentVariables");
        }
    }
}
