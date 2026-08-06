using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeploymentConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeploymentConfigurations",
                columns: table => new
                {
                    DeploymentConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContainerPort = table.Column<int>(type: "integer", nullable: false),
                    Cpu = table.Column<int>(type: "integer", nullable: false),
                    Memory = table.Column<int>(type: "integer", nullable: false),
                    MinimumInstances = table.Column<int>(type: "integer", nullable: false),
                    MaximumInstances = table.Column<int>(type: "integer", nullable: false),
                    HealthCheckPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HealthCheckInterval = table.Column<int>(type: "integer", nullable: false),
                    HealthCheckTimeout = table.Column<int>(type: "integer", nullable: false),
                    HealthyThreshold = table.Column<int>(type: "integer", nullable: false),
                    UnhealthyThreshold = table.Column<int>(type: "integer", nullable: false),
                    Subdomain = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EnableHttps = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeploymentConfigurations", x => x.DeploymentConfigurationId);
                    table.ForeignKey(
                        name: "FK_DeploymentConfigurations_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeploymentConfigurations_ApplicationId",
                table: "DeploymentConfigurations",
                column: "ApplicationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeploymentConfigurations");
        }
    }
}
