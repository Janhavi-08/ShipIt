using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipIt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecrets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Secrets",
                columns: table => new
                {
                    SecretId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeploymentConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EncryptedValue = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Secrets", x => x.SecretId);
                    table.ForeignKey(
                        name: "FK_Secrets_DeploymentConfigurations_DeploymentConfigurationId",
                        column: x => x.DeploymentConfigurationId,
                        principalTable: "DeploymentConfigurations",
                        principalColumn: "DeploymentConfigurationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Secrets_DeploymentConfigurationId_Key",
                table: "Secrets",
                columns: new[] { "DeploymentConfigurationId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Secrets");
        }
    }
}
