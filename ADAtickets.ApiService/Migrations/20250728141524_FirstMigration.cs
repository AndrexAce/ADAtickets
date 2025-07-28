﻿using ADAtickets.Shared.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADAtickets.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class FirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:priority", "high,low,medium")
                .Annotation("Npgsql:Enum:status", "closed,unassigned,waiting_operator,waiting_user")
                .Annotation("Npgsql:Enum:ticket_type", "bug,feature")
                .Annotation("Npgsql:Enum:user_type", "admin,operator,user");

            _ = migrationBuilder.CreateTable(
                name: "platforms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    repository_url = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("pk_platforms", x => x.id);
                });

            _ = migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    surname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    are_email_notifications_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    type = table.Column<UserType>(type: "user_type", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("pk_users", x => x.id);
                });

            _ = migrationBuilder.CreateTable(
                name: "platform_user",
                columns: table => new
                {
                    preferred_platforms_id = table.Column<Guid>(type: "uuid", nullable: false),
                    users_preferred_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("pk_platform_user", x => new { x.preferred_platforms_id, x.users_preferred_id });
                    _ = table.ForeignKey(
                        name: "fk_platform_user_platforms_preferred_platforms_id",
                        column: x => x.preferred_platforms_id,
                        principalTable: "platforms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "fk_platform_user_users_users_preferred_id",
                        column: x => x.users_preferred_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<TicketType>(type: "ticket_type", nullable: false),
                    creation_date_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    priority = table.Column<Priority>(type: "priority", nullable: false),
                    status = table.Column<Status>(type: "status", nullable: false),
                    work_item_id = table.Column<int>(type: "integer", nullable: false),
                    platform_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creator_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operator_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("pk_tickets", x => x.id);
                    _ = table.ForeignKey(
                        name: "fk_tickets_platforms_platform_id",
                        column: x => x.platform_id,
                        principalTable: "platforms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "fk_tickets_users_creator_user_id",
                        column: x => x.creator_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "fk_tickets_users_operator_user_id",
                        column: x => x.operator_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            _ = migrationBuilder.CreateTable(
                name: "user_platforms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_id = table.Column<Guid>(type: "uuid", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("pk_user_platforms", x => x.id);
                    _ = table.ForeignKey(
                        name: "fk_user_platforms_platforms_platform_id",
                        column: x => x.platform_id,
                        principalTable: "platforms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "fk_user_platforms_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    path = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("pk_attachments", x => x.id);
                    _ = table.ForeignKey(
                        name: "fk_attachments_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "edits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    edit_date_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    old_status = table.Column<Status>(type: "status", nullable: false),
                    new_status = table.Column<Status>(type: "status", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("pk_edits", x => x.id);
                    _ = table.ForeignKey(
                        name: "fk_edits_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "fk_edits_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    send_date_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("pk_notifications", x => x.id);
                    _ = table.ForeignKey(
                        name: "fk_notifications_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "replies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reply_date_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    message = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    author_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("pk_replies", x => x.id);
                    _ = table.ForeignKey(
                        name: "fk_replies_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "fk_replies_users_author_user_id",
                        column: x => x.author_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "notification_user",
                columns: table => new
                {
                    received_notifications_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipients_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("pk_notification_user", x => new { x.received_notifications_id, x.recipients_id });
                    _ = table.ForeignKey(
                        name: "fk_notification_user_notifications_received_notifications_id",
                        column: x => x.received_notifications_id,
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "fk_notification_user_users_recipients_id",
                        column: x => x.recipients_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "user_notifications",
                columns: table => new
                {
                    receiver_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("pk_user_notifications", x => new { x.receiver_user_id, x.notification_id });
                    _ = table.ForeignKey(
                        name: "fk_user_notifications_notifications_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "fk_user_notifications_users_receiver_user_id",
                        column: x => x.receiver_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateIndex(
                name: "ix_attachments_path_ticket_id",
                table: "attachments",
                columns: new[] { "path", "ticket_id" },
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "ix_attachments_ticket_id",
                table: "attachments",
                column: "ticket_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_edits_ticket_id",
                table: "edits",
                column: "ticket_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_edits_user_id",
                table: "edits",
                column: "user_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_notification_user_recipients_id",
                table: "notification_user",
                column: "recipients_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_notifications_ticket_id",
                table: "notifications",
                column: "ticket_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_platform_user_users_preferred_id",
                table: "platform_user",
                column: "users_preferred_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_platforms_repository_url",
                table: "platforms",
                column: "repository_url",
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "ix_replies_author_user_id",
                table: "replies",
                column: "author_user_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_replies_ticket_id",
                table: "replies",
                column: "ticket_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_tickets_creator_user_id",
                table: "tickets",
                column: "creator_user_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_tickets_operator_user_id",
                table: "tickets",
                column: "operator_user_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_tickets_platform_id",
                table: "tickets",
                column: "platform_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_user_notifications_notification_id",
                table: "user_notifications",
                column: "notification_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_user_platforms_platform_id",
                table: "user_platforms",
                column: "platform_id");

            _ = migrationBuilder.CreateIndex(
                name: "ix_user_platforms_user_id_platform_id",
                table: "user_platforms",
                columns: new[] { "user_id", "platform_id" },
                unique: true);

            _ = migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "attachments");

            _ = migrationBuilder.DropTable(
                name: "edits");

            _ = migrationBuilder.DropTable(
                name: "notification_user");

            _ = migrationBuilder.DropTable(
                name: "platform_user");

            _ = migrationBuilder.DropTable(
                name: "replies");

            _ = migrationBuilder.DropTable(
                name: "user_notifications");

            _ = migrationBuilder.DropTable(
                name: "user_platforms");

            _ = migrationBuilder.DropTable(
                name: "notifications");

            _ = migrationBuilder.DropTable(
                name: "tickets");

            _ = migrationBuilder.DropTable(
                name: "platforms");

            _ = migrationBuilder.DropTable(
                name: "users");
        }
    }
}
