using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wplay.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "endpoints",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: true),
                    uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    type = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Webhook"),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    default_status_code = table.Column<int>(type: "INTEGER", nullable: true, defaultValue: 200),
                    default_response_headers = table.Column<string>(type: "json", nullable: true),
                    default_response_body = table.Column<string>(type: "TEXT", nullable: true),
                    response_delay = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0, comment: "Response delay in milliseconds"),
                    is_protected = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    auth_credentials = table.Column<string>(type: "json", nullable: true),
                    expires_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_endpoints", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "email_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    endpoint_id = table.Column<int>(type: "INTEGER", nullable: false),
                    uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    sender = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    recipient = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    subject = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    text_body = table.Column<string>(type: "TEXT", nullable: true),
                    html_body = table.Column<string>(type: "TEXT", nullable: true),
                    raw_headers = table.Column<string>(type: "json", nullable: true),
                    attachments_metadata = table.Column<string>(type: "json", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_email_requests_endpoints_endpoint_id",
                        column: x => x.endpoint_id,
                        principalTable: "endpoints",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "webhook_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    endpoint_id = table.Column<int>(type: "INTEGER", nullable: false),
                    uuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    method = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: "POST"),
                    url = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    query_parameters = table.Column<string>(type: "json", nullable: true),
                    headers = table.Column<string>(type: "json", nullable: true),
                    body = table.Column<string>(type: "TEXT", nullable: true),
                    ip_address = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    content_type = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    response_code = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 200),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webhook_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_webhook_requests_endpoints_endpoint_id",
                        column: x => x.endpoint_id,
                        principalTable: "endpoints",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "email_requests_endpoint_created_index",
                table: "email_requests",
                columns: new[] { "endpoint_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "email_requests_uuid_index",
                table: "email_requests",
                column: "uuid");

            migrationBuilder.CreateIndex(
                name: "endpoints_userid_index",
                table: "endpoints",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "endpoints_uuid_index",
                table: "endpoints",
                column: "uuid");

            migrationBuilder.CreateIndex(
                name: "webhook_requests_endpoint_created_index",
                table: "webhook_requests",
                columns: new[] { "endpoint_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "webhook_requests_uuid_index",
                table: "webhook_requests",
                column: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_requests");

            migrationBuilder.DropTable(
                name: "webhook_requests");

            migrationBuilder.DropTable(
                name: "endpoints");
        }
    }
}
