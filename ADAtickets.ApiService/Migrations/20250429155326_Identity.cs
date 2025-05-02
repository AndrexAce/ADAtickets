using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ADAtickets.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Identity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_edits_users_user_email",
                table: "edits");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_users_user_email",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_replies_users_author_user_email",
                table: "replies");

            migrationBuilder.DropForeignKey(
                name: "fk_tickets_platforms_platform_name",
                table: "tickets");

            migrationBuilder.DropForeignKey(
                name: "fk_tickets_users_creator_user_email",
                table: "tickets");

            migrationBuilder.DropForeignKey(
                name: "fk_tickets_users_operator_user_email",
                table: "tickets");

            migrationBuilder.DropForeignKey(
                name: "fk_user_notifications_users_receiver_user_email",
                table: "user_notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_user_platforms_platforms_platform_name",
                table: "user_platforms");

            migrationBuilder.DropForeignKey(
                name: "fk_user_platforms_users_user_email",
                table: "user_platforms");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_platforms",
                table: "user_platforms");

            migrationBuilder.DropIndex(
                name: "ix_user_platforms_platform_name",
                table: "user_platforms");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_notifications",
                table: "user_notifications");

            migrationBuilder.DropIndex(
                name: "ix_tickets_creator_user_email",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "ix_tickets_operator_user_email",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "ix_tickets_platform_name",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "ix_replies_author_user_email",
                table: "replies");

            migrationBuilder.DropPrimaryKey(
                name: "pk_platforms",
                table: "platforms");

            migrationBuilder.DropIndex(
                name: "ix_notifications_user_email",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_edits_user_email",
                table: "edits");

            migrationBuilder.DropPrimaryKey(
                name: "pk_attachments",
                table: "attachments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropColumn(
                name: "user_email",
                table: "user_platforms");

            migrationBuilder.DropColumn(
                name: "platform_name",
                table: "user_platforms");

            migrationBuilder.DropColumn(
                name: "receiver_user_email",
                table: "user_notifications");

            migrationBuilder.DropColumn(
                name: "creator_user_email",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "operator_user_email",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "platform_name",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "author_user_email",
                table: "replies");

            migrationBuilder.DropColumn(
                name: "user_email",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "user_email",
                table: "edits");

            migrationBuilder.DropColumn(
                name: "email",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password",
                table: "users");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "users");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "app_users");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "user_platforms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "platform_id",
                table: "user_platforms",
                type: "uuid",
                maxLength: 50,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "receiver_user_id",
                table: "user_notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "creator_user_id",
                table: "tickets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "operator_user_id",
                table: "tickets",
                type: "uuid",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "platform_id",
                table: "tickets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "author_user_id",
                table: "replies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "platforms",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "edits",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "attachments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "app_users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "identity_user_id",
                table: "app_users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_platforms",
                table: "user_platforms",
                columns: new[] { "user_id", "platform_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_notifications",
                table: "user_notifications",
                columns: new[] { "receiver_user_id", "notification_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_platforms",
                table: "platforms",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_attachments",
                table: "attachments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_app_users",
                table: "app_users",
                column: "id");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_asp_net_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_platforms_platform_id",
                table: "user_platforms",
                column: "platform_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_creator_user_id",
                table: "tickets",
                column: "creator_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_operator_user_id",
                table: "tickets",
                column: "operator_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_platform_id",
                table: "tickets",
                column: "platform_id");

            migrationBuilder.CreateIndex(
                name: "ix_replies_author_user_id",
                table: "replies",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_platforms_name",
                table: "platforms",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_edits_user_id",
                table: "edits",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_attachments_path_ticket_id",
                table: "attachments",
                columns: new[] { "path", "ticket_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_app_users_identity_user_id",
                table: "app_users",
                column: "identity_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "AspNetRoleClaims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "AspNetUserClaims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "AspNetUserLogins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "AspNetUserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_app_users_users_identity_user_id",
                table: "app_users",
                column: "identity_user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_edits_app_users_user_id",
                table: "edits",
                column: "user_id",
                principalTable: "app_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_app_users_user_id",
                table: "notifications",
                column: "user_id",
                principalTable: "app_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_replies_app_users_author_user_id",
                table: "replies",
                column: "author_user_id",
                principalTable: "app_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tickets_app_users_creator_user_id",
                table: "tickets",
                column: "creator_user_id",
                principalTable: "app_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tickets_app_users_operator_user_id",
                table: "tickets",
                column: "operator_user_id",
                principalTable: "app_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_tickets_platforms_platform_id",
                table: "tickets",
                column: "platform_id",
                principalTable: "platforms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_notifications_app_users_receiver_user_id",
                table: "user_notifications",
                column: "receiver_user_id",
                principalTable: "app_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_platforms_app_users_user_id",
                table: "user_platforms",
                column: "user_id",
                principalTable: "app_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_platforms_platforms_platform_id",
                table: "user_platforms",
                column: "platform_id",
                principalTable: "platforms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_app_users_users_identity_user_id",
                table: "app_users");

            migrationBuilder.DropForeignKey(
                name: "fk_edits_app_users_user_id",
                table: "edits");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_app_users_user_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_replies_app_users_author_user_id",
                table: "replies");

            migrationBuilder.DropForeignKey(
                name: "fk_tickets_app_users_creator_user_id",
                table: "tickets");

            migrationBuilder.DropForeignKey(
                name: "fk_tickets_app_users_operator_user_id",
                table: "tickets");

            migrationBuilder.DropForeignKey(
                name: "fk_tickets_platforms_platform_id",
                table: "tickets");

            migrationBuilder.DropForeignKey(
                name: "fk_user_notifications_app_users_receiver_user_id",
                table: "user_notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_user_platforms_app_users_user_id",
                table: "user_platforms");

            migrationBuilder.DropForeignKey(
                name: "fk_user_platforms_platforms_platform_id",
                table: "user_platforms");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_platforms",
                table: "user_platforms");

            migrationBuilder.DropIndex(
                name: "ix_user_platforms_platform_id",
                table: "user_platforms");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_notifications",
                table: "user_notifications");

            migrationBuilder.DropIndex(
                name: "ix_tickets_creator_user_id",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "ix_tickets_operator_user_id",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "ix_tickets_platform_id",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "ix_replies_author_user_id",
                table: "replies");

            migrationBuilder.DropPrimaryKey(
                name: "pk_platforms",
                table: "platforms");

            migrationBuilder.DropIndex(
                name: "ix_platforms_name",
                table: "platforms");

            migrationBuilder.DropIndex(
                name: "ix_notifications_user_id",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_edits_user_id",
                table: "edits");

            migrationBuilder.DropPrimaryKey(
                name: "pk_attachments",
                table: "attachments");

            migrationBuilder.DropIndex(
                name: "ix_attachments_path_ticket_id",
                table: "attachments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_app_users",
                table: "app_users");

            migrationBuilder.DropIndex(
                name: "ix_app_users_identity_user_id",
                table: "app_users");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "user_platforms");

            migrationBuilder.DropColumn(
                name: "platform_id",
                table: "user_platforms");

            migrationBuilder.DropColumn(
                name: "receiver_user_id",
                table: "user_notifications");

            migrationBuilder.DropColumn(
                name: "creator_user_id",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "operator_user_id",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "platform_id",
                table: "tickets");

            migrationBuilder.DropColumn(
                name: "author_user_id",
                table: "replies");

            migrationBuilder.DropColumn(
                name: "id",
                table: "platforms");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "edits");

            migrationBuilder.DropColumn(
                name: "id",
                table: "attachments");

            migrationBuilder.DropColumn(
                name: "id",
                table: "app_users");

            migrationBuilder.DropColumn(
                name: "identity_user_id",
                table: "app_users");

            migrationBuilder.RenameTable(
                name: "app_users",
                newName: "users");

            migrationBuilder.AddColumn<string>(
                name: "user_email",
                table: "user_platforms",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "platform_name",
                table: "user_platforms",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "receiver_user_email",
                table: "user_notifications",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "creator_user_email",
                table: "tickets",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "operator_user_email",
                table: "tickets",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "platform_name",
                table: "tickets",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "author_user_email",
                table: "replies",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_email",
                table: "notifications",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_email",
                table: "edits",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "users",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_platforms",
                table: "user_platforms",
                columns: new[] { "user_email", "platform_name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_notifications",
                table: "user_notifications",
                columns: new[] { "receiver_user_email", "notification_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_platforms",
                table: "platforms",
                column: "name");

            migrationBuilder.AddPrimaryKey(
                name: "pk_attachments",
                table: "attachments",
                columns: new[] { "path", "ticket_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_user_platforms_platform_name",
                table: "user_platforms",
                column: "platform_name");

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
                name: "ix_replies_author_user_email",
                table: "replies",
                column: "author_user_email");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_email",
                table: "notifications",
                column: "user_email");

            migrationBuilder.CreateIndex(
                name: "ix_edits_user_email",
                table: "edits",
                column: "user_email");

            migrationBuilder.AddForeignKey(
                name: "fk_edits_users_user_email",
                table: "edits",
                column: "user_email",
                principalTable: "users",
                principalColumn: "email",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_users_user_email",
                table: "notifications",
                column: "user_email",
                principalTable: "users",
                principalColumn: "email",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_replies_users_author_user_email",
                table: "replies",
                column: "author_user_email",
                principalTable: "users",
                principalColumn: "email",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tickets_platforms_platform_name",
                table: "tickets",
                column: "platform_name",
                principalTable: "platforms",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tickets_users_creator_user_email",
                table: "tickets",
                column: "creator_user_email",
                principalTable: "users",
                principalColumn: "email",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tickets_users_operator_user_email",
                table: "tickets",
                column: "operator_user_email",
                principalTable: "users",
                principalColumn: "email");

            migrationBuilder.AddForeignKey(
                name: "fk_user_notifications_users_receiver_user_email",
                table: "user_notifications",
                column: "receiver_user_email",
                principalTable: "users",
                principalColumn: "email",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_platforms_platforms_platform_name",
                table: "user_platforms",
                column: "platform_name",
                principalTable: "platforms",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_platforms_users_user_email",
                table: "user_platforms",
                column: "user_email",
                principalTable: "users",
                principalColumn: "email",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
