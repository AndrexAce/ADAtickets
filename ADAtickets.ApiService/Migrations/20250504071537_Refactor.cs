using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAtickets.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Refactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_user",
                columns: table => new
                {
                    received_notifications_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipients_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_user", x => new { x.received_notifications_id, x.recipients_id });
                    table.ForeignKey(
                        name: "fk_notification_user_app_users_recipients_id",
                        column: x => x.recipients_id,
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notification_user_notifications_received_notifications_id",
                        column: x => x.received_notifications_id,
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "platform_user",
                columns: table => new
                {
                    preferred_platforms_id = table.Column<Guid>(type: "uuid", nullable: false),
                    users_preferred_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_platform_user", x => new { x.preferred_platforms_id, x.users_preferred_id });
                    table.ForeignKey(
                        name: "fk_platform_user_app_users_users_preferred_id",
                        column: x => x.users_preferred_id,
                        principalTable: "app_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_platform_user_platforms_preferred_platforms_id",
                        column: x => x.preferred_platforms_id,
                        principalTable: "platforms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notification_user_recipients_id",
                table: "notification_user",
                column: "recipients_id");

            migrationBuilder.CreateIndex(
                name: "ix_platform_user_users_preferred_id",
                table: "platform_user",
                column: "users_preferred_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_user");

            migrationBuilder.DropTable(
                name: "platform_user");
        }
    }
}
