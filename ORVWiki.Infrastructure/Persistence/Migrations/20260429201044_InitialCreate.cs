using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ORVWiki.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attribute_rarity", "common,rare,epic,legend,mythic")
                .Annotation("Npgsql:Enum:character_constellation_rel", "main_sponsor,patron,subscriber,opposed")
                .Annotation("Npgsql:Enum:character_status", "alive,dead,unknown,transcended")
                .Annotation("Npgsql:Enum:comment_reaction_type", "like,dislike,heart,laugh,sad,star")
                .Annotation("Npgsql:Enum:concept_impact", "low,medium,high,core")
                .Annotation("Npgsql:Enum:constellation_grade", "historical,fable,myth,star_stream")
                .Annotation("Npgsql:Enum:dokkaebi_rank", "low,intermediate,great,grand,zen")
                .Annotation("Npgsql:Enum:edit_suggestion_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:entity_type", "character,constellation,nebula,dokkaebi,demon_king,outer_god,worldline,fable,stigma,attribute,skill,item,location,scenario,arc,event,concept")
                .Annotation("Npgsql:Enum:event_character_role", "participant,observer,victim,perpetrator,mentor")
                .Annotation("Npgsql:Enum:event_connection_type", "regression,migration,causality,prophecy,parallel")
                .Annotation("Npgsql:Enum:event_importance", "minor,major,pivotal")
                .Annotation("Npgsql:Enum:fable_grade", "legendary,mythical,great,divine")
                .Annotation("Npgsql:Enum:gender", "male,female,unknown")
                .Annotation("Npgsql:Enum:item_grade", "common,uncommon,rare,epic,legendary,mythic,divine")
                .Annotation("Npgsql:Enum:notification_type", "comment_reply,edit_approved,edit_rejected,mention,system")
                .Annotation("Npgsql:Enum:scenario_difficulty", "f,e,d,c,b,a,s,ss,sss,unknown")
                .Annotation("Npgsql:Enum:scenario_outcome", "succeeded,failed,withdrew,pending")
                .Annotation("Npgsql:Enum:scenario_type", "main,sub,hidden,bounty,disaster")
                .Annotation("Npgsql:Enum:skill_type", "active,passive,general");

            migrationBuilder.CreateTable(
                name: "pages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    entity_type = table.Column<int>(type: "integer", nullable: false),
                    discovery_chapter = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    spoiler_map = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb"),
                    short_description = table.Column<string>(type: "text", nullable: true),
                    view_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pages", x => x.id);
                    table.CheckConstraint("ck_page_discovery_chapter", "discovery_chapter >= 1");
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permissions = table.Column<JsonDocument>(type: "jsonb", nullable: true, defaultValueSql: "'{}'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                    table.CheckConstraint("ck_tag_color_hex", "color IS NULL OR color ~ '^#[0-9A-Fa-f]{6}$'");
                });

            migrationBuilder.CreateTable(
                name: "arcs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    order_number = table.Column<short>(type: "smallint", nullable: false),
                    chapter_start = table.Column<int>(type: "integer", nullable: false),
                    chapter_end = table.Column<int>(type: "integer", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_arcs", x => x.id);
                    table.CheckConstraint("ck_arc_chapter_range", "chapter_end >= chapter_start AND chapter_start >= 1");
                    table.ForeignKey(
                        name: "fk_arcs_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "attributes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    rarity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    effect = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attributes", x => x.id);
                    table.ForeignKey(
                        name: "fk_attributes_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "concepts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    definition = table.Column<string>(type: "text", nullable: false),
                    impact_level = table.Column<int>(type: "integer", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_concepts", x => x.id);
                    table.ForeignKey(
                        name: "fk_concepts_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "demon_kings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    ranking = table.Column<short>(type: "smallint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    demon_realm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_demon_kings", x => x.id);
                    table.CheckConstraint("ck_demon_king_ranking", "ranking BETWEEN 1 AND 72");
                    table.ForeignKey(
                        name: "fk_demon_kings_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dokkaebi",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    channel_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    rank = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    speciality = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dokkaebi", x => x.id);
                    table.ForeignKey(
                        name: "fk_dokkaebi_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    item_grade = table.Column<int>(type: "integer", nullable: false),
                    is_star_relic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_items_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "outer_gods",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    god_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outer_gods", x => x.id);
                    table.ForeignKey(
                        name: "fk_outer_gods_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scenarios",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    conditions = table.Column<string>(type: "text", nullable: true),
                    rewards = table.Column<string>(type: "text", nullable: true),
                    penalty = table.Column<string>(type: "text", nullable: true),
                    chapter_start = table.Column<int>(type: "integer", nullable: false),
                    chapter_end = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scenarios", x => x.id);
                    table.CheckConstraint("ck_scenario_chapter_range", "chapter_end IS NULL OR chapter_end >= chapter_start");
                    table.CheckConstraint("ck_scenario_chapter_start", "chapter_start >= 1");
                    table.ForeignKey(
                        name: "fk_scenarios_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    skill_type = table.Column<int>(type: "integer", nullable: false),
                    cost_in_coins = table.Column<int>(type: "integer", nullable: true),
                    max_level = table.Column<short>(type: "smallint", nullable: true, defaultValue: (short)10),
                    effect = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_skills", x => x.id);
                    table.ForeignKey(
                        name: "fk_skills_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worldlines",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    line_number = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    parent_worldline_id = table.Column<long>(type: "bigint", nullable: true),
                    is_main = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worldlines", x => x.id);
                    table.ForeignKey(
                        name: "fk_worldlines_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_worldlines_worldlines_parent_worldline_id",
                        column: x => x.parent_worldline_id,
                        principalTable: "worldlines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    current_chapter = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    bio = table.Column<string>(type: "text", nullable: true),
                    role_id = table.Column<short>(type: "smallint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("ck_user_current_chapter", "current_chapter >= 0");
                    table.ForeignKey(
                        name: "fk_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "page_tags",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    tag_id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_page_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_page_tags_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_page_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chapters",
                columns: table => new
                {
                    chapter_number = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    arc_id = table.Column<long>(type: "bigint", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: true),
                    release_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chapters", x => x.chapter_number);
                    table.ForeignKey(
                        name: "fk_chapters_arcs_arc_id",
                        column: x => x.arc_id,
                        principalTable: "arcs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    dimension = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    worldline_id = table.Column<long>(type: "bigint", nullable: true),
                    parent_location_id = table.Column<long>(type: "bigint", nullable: true),
                    coordinates = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_locations", x => x.id);
                    table.ForeignKey(
                        name: "fk_locations_locations_parent_location_id",
                        column: x => x.parent_location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_locations_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_locations_worldlines_worldline_id",
                        column: x => x.worldline_id,
                        principalTable: "worldlines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "bookmarks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bookmarks", x => x.id);
                    table.ForeignKey(
                        name: "fk_bookmarks_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bookmarks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    parent_comment_id = table.Column<long>(type: "bigint", nullable: true),
                    body = table.Column<string>(type: "text", nullable: false),
                    chapter_at_post = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comments", x => x.id);
                    table.CheckConstraint("ck_comment_chapter_at_post", "chapter_at_post >= 0");
                    table.ForeignKey(
                        name: "fk_comments_comments_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_comments_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_comments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "edit_suggestions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    proposed_changes = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    reviewed_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_edit_suggestions", x => x.id);
                    table.ForeignKey(
                        name: "fk_edit_suggestions_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_edit_suggestions_users_reviewed_by_user_id",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_edit_suggestions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    alt_text = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    mime_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    uploaded_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media", x => x.id);
                    table.ForeignKey(
                        name: "fk_media_users_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    payload = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    chapter_number = table.Column<int>(type: "integer", nullable: false),
                    location_id = table.Column<long>(type: "bigint", nullable: true),
                    worldline_id = table.Column<long>(type: "bigint", nullable: true),
                    importance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    event_order = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_events_chapters_chapter_number",
                        column: x => x.chapter_number,
                        principalTable: "chapters",
                        principalColumn: "chapter_number",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_events_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_events_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_events_worldlines_worldline_id",
                        column: x => x.worldline_id,
                        principalTable: "worldlines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "scenario_locations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scenario_id = table.Column<long>(type: "bigint", nullable: false),
                    location_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scenario_locations", x => x.id);
                    table.ForeignKey(
                        name: "fk_scenario_locations_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_scenario_locations_scenarios_scenario_id",
                        column: x => x.scenario_id,
                        principalTable: "scenarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comment_reactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    comment_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    reaction_type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comment_reactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_comment_reactions_comments_comment_id",
                        column: x => x.comment_id,
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_comment_reactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    alias = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    species = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, defaultValue: "human"),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    birth_chapter = table.Column<int>(type: "integer", nullable: true),
                    death_chapter = table.Column<int>(type: "integer", nullable: true),
                    biography = table.Column<string>(type: "text", nullable: true),
                    portrait_media_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_characters", x => x.id);
                    table.CheckConstraint("ck_character_death_after_birth", "death_chapter IS NULL OR birth_chapter IS NULL OR death_chapter >= birth_chapter");
                    table.ForeignKey(
                        name: "fk_characters_media_portrait_media_id",
                        column: x => x.portrait_media_id,
                        principalTable: "media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_characters_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_attributes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    character_id = table.Column<long>(type: "bigint", nullable: false),
                    attribute_id = table.Column<long>(type: "bigint", nullable: false),
                    acquired_chapter = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_attributes", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_attributes_attributes_attribute_id",
                        column: x => x.attribute_id,
                        principalTable: "attributes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_character_attributes_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    character_id = table.Column<long>(type: "bigint", nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    acquired_chapter = table.Column<int>(type: "integer", nullable: true),
                    lost_chapter = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_items_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_character_items_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_skills",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    character_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_id = table.Column<long>(type: "bigint", nullable: false),
                    level = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1),
                    acquired_chapter = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_skills", x => x.id);
                    table.CheckConstraint("ck_character_skill_level", "level BETWEEN 1 AND 10");
                    table.ForeignKey(
                        name: "fk_character_skills_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_character_skills_skills_skill_id",
                        column: x => x.skill_id,
                        principalTable: "skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_characters",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<long>(type: "bigint", nullable: false),
                    character_id = table.Column<long>(type: "bigint", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_characters", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_characters_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_event_characters_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_connections",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_event_id = table.Column<long>(type: "bigint", nullable: false),
                    target_event_id = table.Column<long>(type: "bigint", nullable: false),
                    connection_type = table.Column<int>(type: "integer", nullable: false),
                    character_id = table.Column<long>(type: "bigint", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_connections", x => x.id);
                    table.CheckConstraint("ck_event_connection_no_self", "source_event_id <> target_event_id");
                    table.ForeignKey(
                        name: "fk_event_connections_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_event_connections_events_source_event_id",
                        column: x => x.source_event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_event_connections_events_target_event_id",
                        column: x => x.target_event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fables",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    grade = table.Column<int>(type: "integer", nullable: false),
                    legend = table.Column<string>(type: "text", nullable: true),
                    origin_character_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fables", x => x.id);
                    table.ForeignKey(
                        name: "fk_fables_characters_origin_character_id",
                        column: x => x.origin_character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_fables_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scenario_participants",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scenario_id = table.Column<long>(type: "bigint", nullable: false),
                    character_id = table.Column<long>(type: "bigint", nullable: false),
                    outcome = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scenario_participants", x => x.id);
                    table.ForeignKey(
                        name: "fk_scenario_participants_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_scenario_participants_scenarios_scenario_id",
                        column: x => x.scenario_id,
                        principalTable: "scenarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_fables",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    character_id = table.Column<long>(type: "bigint", nullable: false),
                    fable_id = table.Column<long>(type: "bigint", nullable: false),
                    acquired_chapter = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_fables", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_fables_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_character_fables_fables_fable_id",
                        column: x => x.fable_id,
                        principalTable: "fables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_constellations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    character_id = table.Column<long>(type: "bigint", nullable: false),
                    constellation_id = table.Column<long>(type: "bigint", nullable: false),
                    relationship_type = table.Column<int>(type: "integer", nullable: false),
                    since_chapter = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_constellations", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_constellations_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_stigmas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    character_id = table.Column<long>(type: "bigint", nullable: false),
                    stigma_id = table.Column<long>(type: "bigint", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    acquired_chapter = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_stigmas", x => x.id);
                    table.ForeignKey(
                        name: "fk_character_stigmas_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "constellations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    modifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    true_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    nebula_id = table.Column<long>(type: "bigint", nullable: true),
                    grade = table.Column<int>(type: "integer", nullable: false),
                    origin_character_id = table.Column<long>(type: "bigint", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_constellations", x => x.id);
                    table.ForeignKey(
                        name: "fk_constellations_characters_origin_character_id",
                        column: x => x.origin_character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_constellations_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nebulae",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    founder_constellation_id = table.Column<long>(type: "bigint", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    power_rank = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nebulae", x => x.id);
                    table.ForeignKey(
                        name: "fk_nebulae_constellations_founder_constellation_id",
                        column: x => x.founder_constellation_id,
                        principalTable: "constellations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_nebulae_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stigmas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    page_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    provider_constellation_id = table.Column<long>(type: "bigint", nullable: false),
                    activation_cost = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    effect = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stigmas", x => x.id);
                    table.ForeignKey(
                        name: "fk_stigmas_constellations_provider_constellation_id",
                        column: x => x.provider_constellation_id,
                        principalTable: "constellations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stigmas_pages_page_id",
                        column: x => x.page_id,
                        principalTable: "pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_arcs_name",
                table: "arcs",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_arcs_order_number",
                table: "arcs",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_arcs_page_id",
                table: "arcs",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_attributes_name",
                table: "attributes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_attributes_page_id",
                table: "attributes",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bookmarks_page_id",
                table: "bookmarks",
                column: "page_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookmarks_user_id_page_id",
                table: "bookmarks",
                columns: new[] { "user_id", "page_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chapters_arc_id",
                table: "chapters",
                column: "arc_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_attributes_attribute_id",
                table: "character_attributes",
                column: "attribute_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_attributes_character_id_attribute_id",
                table: "character_attributes",
                columns: new[] { "character_id", "attribute_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_character_constellations_character_id_constellation_id_rela",
                table: "character_constellations",
                columns: new[] { "character_id", "constellation_id", "relationship_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_character_constellations_constellation_id",
                table: "character_constellations",
                column: "constellation_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_fables_character_id_fable_id",
                table: "character_fables",
                columns: new[] { "character_id", "fable_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_character_fables_fable_id",
                table: "character_fables",
                column: "fable_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_items_character_id",
                table: "character_items",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_items_item_id",
                table: "character_items",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_skills_character_id_skill_id",
                table: "character_skills",
                columns: new[] { "character_id", "skill_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_character_skills_skill_id",
                table: "character_skills",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_stigmas_character_id_stigma_id",
                table: "character_stigmas",
                columns: new[] { "character_id", "stigma_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_character_stigmas_stigma_id",
                table: "character_stigmas",
                column: "stigma_id");

            migrationBuilder.CreateIndex(
                name: "ix_characters_page_id",
                table: "characters",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_characters_portrait_media_id",
                table: "characters",
                column: "portrait_media_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_reactions_comment_id_user_id_reaction_type",
                table: "comment_reactions",
                columns: new[] { "comment_id", "user_id", "reaction_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_comment_reactions_user_id",
                table: "comment_reactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_page_id",
                table: "comments",
                column: "page_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_parent_comment_id",
                table: "comments",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_user_id",
                table: "comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_concepts_name",
                table: "concepts",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_concepts_page_id",
                table: "concepts",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_constellations_modifier",
                table: "constellations",
                column: "modifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_constellations_nebula_id",
                table: "constellations",
                column: "nebula_id");

            migrationBuilder.CreateIndex(
                name: "ix_constellations_origin_character_id",
                table: "constellations",
                column: "origin_character_id");

            migrationBuilder.CreateIndex(
                name: "ix_constellations_page_id",
                table: "constellations",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_demon_kings_name",
                table: "demon_kings",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_demon_kings_page_id",
                table: "demon_kings",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_demon_kings_ranking",
                table: "demon_kings",
                column: "ranking",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dokkaebi_channel_id",
                table: "dokkaebi",
                column: "channel_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dokkaebi_page_id",
                table: "dokkaebi",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_edit_suggestions_page_id",
                table: "edit_suggestions",
                column: "page_id");

            migrationBuilder.CreateIndex(
                name: "ix_edit_suggestions_reviewed_by_user_id",
                table: "edit_suggestions",
                column: "reviewed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_edit_suggestions_user_id",
                table: "edit_suggestions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_characters_character_id",
                table: "event_characters",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_characters_event_id_character_id_role",
                table: "event_characters",
                columns: new[] { "event_id", "character_id", "role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_connections_character_id",
                table: "event_connections",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_connections_source_event_id_target_event_id_connectio",
                table: "event_connections",
                columns: new[] { "source_event_id", "target_event_id", "connection_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_event_connections_target_event_id",
                table: "event_connections",
                column: "target_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_chapter_number",
                table: "events",
                column: "chapter_number");

            migrationBuilder.CreateIndex(
                name: "ix_events_location_id",
                table: "events",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_page_id",
                table: "events",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_events_worldline_id",
                table: "events",
                column: "worldline_id");

            migrationBuilder.CreateIndex(
                name: "ix_fables_origin_character_id",
                table: "fables",
                column: "origin_character_id");

            migrationBuilder.CreateIndex(
                name: "ix_fables_page_id",
                table: "fables",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fables_title",
                table: "fables",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_items_name",
                table: "items",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_items_page_id",
                table: "items",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_locations_page_id",
                table: "locations",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_locations_parent_location_id",
                table: "locations",
                column: "parent_location_id");

            migrationBuilder.CreateIndex(
                name: "ix_locations_worldline_id",
                table: "locations",
                column: "worldline_id");

            migrationBuilder.CreateIndex(
                name: "ix_media_uploaded_by_user_id",
                table: "media",
                column: "uploaded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_media_url",
                table: "media",
                column: "url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_nebulae_founder_constellation_id",
                table: "nebulae",
                column: "founder_constellation_id");

            migrationBuilder.CreateIndex(
                name: "ix_nebulae_name",
                table: "nebulae",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_nebulae_page_id",
                table: "nebulae",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_outer_gods_name",
                table: "outer_gods",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outer_gods_page_id",
                table: "outer_gods",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_page_tags_page_id_tag_id",
                table: "page_tags",
                columns: new[] { "page_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_page_tags_tag_id",
                table: "page_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_pages_slug",
                table: "pages",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scenario_locations_location_id",
                table: "scenario_locations",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_scenario_locations_scenario_id_location_id",
                table: "scenario_locations",
                columns: new[] { "scenario_id", "location_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scenario_participants_character_id",
                table: "scenario_participants",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "ix_scenario_participants_scenario_id_character_id",
                table: "scenario_participants",
                columns: new[] { "scenario_id", "character_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scenarios_code",
                table: "scenarios",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scenarios_page_id",
                table: "scenarios",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_skills_name",
                table: "skills",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_skills_page_id",
                table: "skills",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stigmas_name",
                table: "stigmas",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stigmas_page_id",
                table: "stigmas",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stigmas_provider_constellation_id",
                table: "stigmas",
                column: "provider_constellation_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_slug",
                table: "tags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_worldlines_line_number",
                table: "worldlines",
                column: "line_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_worldlines_page_id",
                table: "worldlines",
                column: "page_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_worldlines_parent_worldline_id",
                table: "worldlines",
                column: "parent_worldline_id");

            migrationBuilder.AddForeignKey(
                name: "fk_character_constellations_constellations_constellation_id",
                table: "character_constellations",
                column: "constellation_id",
                principalTable: "constellations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_character_stigmas_stigmas_stigma_id",
                table: "character_stigmas",
                column: "stigma_id",
                principalTable: "stigmas",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_constellations_nebulae_nebula_id",
                table: "constellations",
                column: "nebula_id",
                principalTable: "nebulae",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_characters_pages_page_id",
                table: "characters");

            migrationBuilder.DropForeignKey(
                name: "fk_constellations_pages_page_id",
                table: "constellations");

            migrationBuilder.DropForeignKey(
                name: "fk_nebulae_pages_page_id",
                table: "nebulae");

            migrationBuilder.DropForeignKey(
                name: "fk_media_users_uploaded_by_user_id",
                table: "media");

            migrationBuilder.DropForeignKey(
                name: "fk_constellations_characters_origin_character_id",
                table: "constellations");

            migrationBuilder.DropForeignKey(
                name: "fk_nebulae_constellations_founder_constellation_id",
                table: "nebulae");

            migrationBuilder.DropTable(
                name: "bookmarks");

            migrationBuilder.DropTable(
                name: "character_attributes");

            migrationBuilder.DropTable(
                name: "character_constellations");

            migrationBuilder.DropTable(
                name: "character_fables");

            migrationBuilder.DropTable(
                name: "character_items");

            migrationBuilder.DropTable(
                name: "character_skills");

            migrationBuilder.DropTable(
                name: "character_stigmas");

            migrationBuilder.DropTable(
                name: "comment_reactions");

            migrationBuilder.DropTable(
                name: "concepts");

            migrationBuilder.DropTable(
                name: "demon_kings");

            migrationBuilder.DropTable(
                name: "dokkaebi");

            migrationBuilder.DropTable(
                name: "edit_suggestions");

            migrationBuilder.DropTable(
                name: "event_characters");

            migrationBuilder.DropTable(
                name: "event_connections");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "outer_gods");

            migrationBuilder.DropTable(
                name: "page_tags");

            migrationBuilder.DropTable(
                name: "scenario_locations");

            migrationBuilder.DropTable(
                name: "scenario_participants");

            migrationBuilder.DropTable(
                name: "attributes");

            migrationBuilder.DropTable(
                name: "fables");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "skills");

            migrationBuilder.DropTable(
                name: "stigmas");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "scenarios");

            migrationBuilder.DropTable(
                name: "chapters");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "arcs");

            migrationBuilder.DropTable(
                name: "worldlines");

            migrationBuilder.DropTable(
                name: "pages");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "characters");

            migrationBuilder.DropTable(
                name: "media");

            migrationBuilder.DropTable(
                name: "constellations");

            migrationBuilder.DropTable(
                name: "nebulae");
        }
    }
}
