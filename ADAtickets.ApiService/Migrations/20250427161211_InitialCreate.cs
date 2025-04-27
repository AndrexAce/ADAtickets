using ADAtickets.ApiService.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAtickets.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:priority", "high,low,medium")
                .Annotation("Npgsql:Enum:status", "closed,unassigned,waiting_operator,waiting_user")
                .Annotation("Npgsql:Enum:ticket_type", "bug,feature")
                .Annotation("Npgsql:Enum:user_type", "admin,operator,user");

            migrationBuilder.CreateTable(
                name: "platforms",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    repository_url = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_platforms", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    password = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    surname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_email2fa_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_phone2fa_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    are_email_notifications_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    are_phone_notifications_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    type = table.Column<UserType>(type: "user_type", nullable: false),
                    microsoft_account_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.email);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<TicketType>(type: "ticket_type", nullable: false),
                    title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    priority = table.Column<Priority>(type: "priority", nullable: false),
                    status = table.Column<Status>(type: "status", nullable: false),
                    work_item_id = table.Column<int>(type: "integer", nullable: false),
                    platform_name = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    creator_user_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    operator_user_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tickets", x => x.id);
                    table.ForeignKey(
                        name: "fk_tickets_platforms_platform_name",
                        column: x => x.platform_name,
                        principalTable: "platforms",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tickets_users_creator_user_email",
                        column: x => x.creator_user_email,
                        principalTable: "users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tickets_users_operator_user_email",
                        column: x => x.operator_user_email,
                        principalTable: "users",
                        principalColumn: "email");
                });

            migrationBuilder.CreateTable(
                name: "user_platforms",
                columns: table => new
                {
                    user_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    platform_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_platforms", x => new { x.user_email, x.platform_name });
                    table.ForeignKey(
                        name: "fk_user_platforms_platforms_platform_name",
                        column: x => x.platform_name,
                        principalTable: "platforms",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_platforms_users_user_email",
                        column: x => x.user_email,
                        principalTable: "users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    path = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attachments", x => new { x.path, x.ticket_id });
                    table.ForeignKey(
                        name: "fk_attachments_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "edits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    edit_date_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    old_status = table.Column<Status>(type: "status", nullable: false),
                    new_status = table.Column<Status>(type: "status", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_edits", x => x.id);
                    table.ForeignKey(
                        name: "fk_edits_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_edits_users_user_email",
                        column: x => x.user_email,
                        principalTable: "users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_email",
                        column: x => x.user_email,
                        principalTable: "users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "replies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reply_date_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    message = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    author_user_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_replies", x => x.id);
                    table.ForeignKey(
                        name: "fk_replies_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_replies_users_author_user_email",
                        column: x => x.author_user_email,
                        principalTable: "users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_notifications",
                columns: table => new
                {
                    receiver_user_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_notifications", x => new { x.receiver_user_email, x.notification_id });
                    table.ForeignKey(
                        name: "fk_user_notifications_notifications_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_notifications_users_receiver_user_email",
                        column: x => x.receiver_user_email,
                        principalTable: "users",
                        principalColumn: "email",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attachments_ticket_id",
                table: "attachments",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_edits_ticket_id",
                table: "edits",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_edits_user_email",
                table: "edits",
                column: "user_email");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_ticket_id",
                table: "notifications",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_email",
                table: "notifications",
                column: "user_email");

            migrationBuilder.CreateIndex(
                name: "ix_platforms_repository_url",
                table: "platforms",
                column: "repository_url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_replies_author_user_email",
                table: "replies",
                column: "author_user_email");

            migrationBuilder.CreateIndex(
                name: "ix_replies_ticket_id",
                table: "replies",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_creator_user_email",
                table: "tickets",
                column: "creator_user_email");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_operator_user_email",
                table: "tickets",
                column: "operator_user_email");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_platform_name",
                table: "tickets",
                column: "platform_name");

            migrationBuilder.CreateIndex(
                name: "ix_user_notifications_notification_id",
                table: "user_notifications",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_platforms_platform_name",
                table: "user_platforms",
                column: "platform_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attachments");

            migrationBuilder.DropTable(
                name: "edits");

            migrationBuilder.DropTable(
                name: "replies");

            migrationBuilder.DropTable(
                name: "user_notifications");

            migrationBuilder.DropTable(
                name: "user_platforms");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "tickets");

            migrationBuilder.DropTable(
                name: "platforms");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
