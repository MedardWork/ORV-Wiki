using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ORVWiki.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorldlineJumpsRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_connections");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attribute_rarity", "common,rare,epic,legend,mythic")
                .Annotation("Npgsql:Enum:character_constellation_rel", "main_sponsor,patron,subscriber,opposed")
                .Annotation("Npgsql:Enum:character_status", "unknown,alive,dead,transcended")
                .Annotation("Npgsql:Enum:comment_reaction_type", "like,dislike,heart,laugh,sad,star")
                .Annotation("Npgsql:Enum:concept_impact", "low,medium,high,core")
                .Annotation("Npgsql:Enum:constellation_grade", "historical,fable,myth,star_stream")
                .Annotation("Npgsql:Enum:dokkaebi_rank", "low,intermediate,great,grand,zen")
                .Annotation("Npgsql:Enum:edit_suggestion_status", "pending,approved,rejected")
                .Annotation("Npgsql:Enum:entity_type", "character,constellation,nebula,dokkaebi,demon_king,outer_god,worldline,fable,stigma,attribute,skill,item,location,scenario,arc,event,concept")
                .Annotation("Npgsql:Enum:event_character_role", "participant,observer,victim,perpetrator,mentor")
                .Annotation("Npgsql:Enum:event_importance", "minor,major,pivotal")
                .Annotation("Npgsql:Enum:fable_grade", "legendary,mythical,great,divine")
                .Annotation("Npgsql:Enum:gender", "male,female,unknown")
                .Annotation("Npgsql:Enum:item_grade", "common,uncommon,rare,epic,legendary,mythic,divine")
                .Annotation("Npgsql:Enum:notification_type", "comment_reply,edit_approved,edit_rejected,mention,system")
                .Annotation("Npgsql:Enum:scenario_difficulty", "f,e,d,c,b,a,s,ss,sss,unknown")
                .Annotation("Npgsql:Enum:scenario_outcome", "succeeded,failed,withdrew,pending")
                .Annotation("Npgsql:Enum:scenario_type", "main,sub,hidden,bounty,disaster")
                .Annotation("Npgsql:Enum:skill_type", "active,passive,general")
                .OldAnnotation("Npgsql:Enum:attribute_rarity", "common,rare,epic,legend,mythic")
                .OldAnnotation("Npgsql:Enum:character_constellation_rel", "main_sponsor,patron,subscriber,opposed")
                .OldAnnotation("Npgsql:Enum:character_status", "unknown,alive,dead,transcended")
                .OldAnnotation("Npgsql:Enum:comment_reaction_type", "like,dislike,heart,laugh,sad,star")
                .OldAnnotation("Npgsql:Enum:concept_impact", "low,medium,high,core")
                .OldAnnotation("Npgsql:Enum:constellation_grade", "historical,fable,myth,star_stream")
                .OldAnnotation("Npgsql:Enum:dokkaebi_rank", "low,intermediate,great,grand,zen")
                .OldAnnotation("Npgsql:Enum:edit_suggestion_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:entity_type", "character,constellation,nebula,dokkaebi,demon_king,outer_god,worldline,fable,stigma,attribute,skill,item,location,scenario,arc,event,concept")
                .OldAnnotation("Npgsql:Enum:event_character_role", "participant,observer,victim,perpetrator,mentor")
                .OldAnnotation("Npgsql:Enum:event_connection_type", "regression,migration,causality,prophecy,parallel")
                .OldAnnotation("Npgsql:Enum:event_importance", "minor,major,pivotal")
                .OldAnnotation("Npgsql:Enum:fable_grade", "legendary,mythical,great,divine")
                .OldAnnotation("Npgsql:Enum:gender", "male,female,unknown")
                .OldAnnotation("Npgsql:Enum:item_grade", "common,uncommon,rare,epic,legendary,mythic,divine")
                .OldAnnotation("Npgsql:Enum:notification_type", "comment_reply,edit_approved,edit_rejected,mention,system")
                .OldAnnotation("Npgsql:Enum:scenario_difficulty", "f,e,d,c,b,a,s,ss,sss,unknown")
                .OldAnnotation("Npgsql:Enum:scenario_outcome", "succeeded,failed,withdrew,pending")
                .OldAnnotation("Npgsql:Enum:scenario_type", "main,sub,hidden,bounty,disaster")
                .OldAnnotation("Npgsql:Enum:skill_type", "active,passive,general");

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "worldlines",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "worldlines",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "event_order",
                table: "events",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "length_estimate",
                table: "events",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "jumps",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    character_label = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    length_estimate = table.Column<string>(type: "text", nullable: true),
                    source_worldline_id = table.Column<long>(type: "bigint", nullable: false),
                    source_order = table.Column<double>(type: "double precision", nullable: false),
                    target_worldline_id = table.Column<long>(type: "bigint", nullable: false),
                    target_order = table.Column<double>(type: "double precision", nullable: false),
                    arc_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_jumps", x => x.id);
                    table.CheckConstraint("ck_jump_cross_worldline", "source_worldline_id <> target_worldline_id");
                    table.ForeignKey(
                        name: "fk_jumps_arcs_arc_id",
                        column: x => x.arc_id,
                        principalTable: "arcs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_jumps_worldlines_source_worldline_id",
                        column: x => x.source_worldline_id,
                        principalTable: "worldlines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_jumps_worldlines_target_worldline_id",
                        column: x => x.target_worldline_id,
                        principalTable: "worldlines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_worldlines_color",
                table: "worldlines",
                column: "color",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_worldline_color_hex",
                table: "worldlines",
                sql: "color IS NULL OR color ~ '^#[0-9A-Fa-f]{6}$'");

            migrationBuilder.CreateIndex(
                name: "ix_jumps_arc_id",
                table: "jumps",
                column: "arc_id");

            migrationBuilder.CreateIndex(
                name: "ix_jumps_source_worldline_id",
                table: "jumps",
                column: "source_worldline_id");

            migrationBuilder.CreateIndex(
                name: "ix_jumps_target_worldline_id",
                table: "jumps",
                column: "target_worldline_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jumps");

            migrationBuilder.DropIndex(
                name: "ix_worldlines_color",
                table: "worldlines");

            migrationBuilder.DropCheckConstraint(
                name: "ck_worldline_color_hex",
                table: "worldlines");

            migrationBuilder.DropColumn(
                name: "color",
                table: "worldlines");

            migrationBuilder.DropColumn(
                name: "display_order",
                table: "worldlines");

            migrationBuilder.DropColumn(
                name: "length_estimate",
                table: "events");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:attribute_rarity", "common,rare,epic,legend,mythic")
                .Annotation("Npgsql:Enum:character_constellation_rel", "main_sponsor,patron,subscriber,opposed")
                .Annotation("Npgsql:Enum:character_status", "unknown,alive,dead,transcended")
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
                .Annotation("Npgsql:Enum:skill_type", "active,passive,general")
                .OldAnnotation("Npgsql:Enum:attribute_rarity", "common,rare,epic,legend,mythic")
                .OldAnnotation("Npgsql:Enum:character_constellation_rel", "main_sponsor,patron,subscriber,opposed")
                .OldAnnotation("Npgsql:Enum:character_status", "unknown,alive,dead,transcended")
                .OldAnnotation("Npgsql:Enum:comment_reaction_type", "like,dislike,heart,laugh,sad,star")
                .OldAnnotation("Npgsql:Enum:concept_impact", "low,medium,high,core")
                .OldAnnotation("Npgsql:Enum:constellation_grade", "historical,fable,myth,star_stream")
                .OldAnnotation("Npgsql:Enum:dokkaebi_rank", "low,intermediate,great,grand,zen")
                .OldAnnotation("Npgsql:Enum:edit_suggestion_status", "pending,approved,rejected")
                .OldAnnotation("Npgsql:Enum:entity_type", "character,constellation,nebula,dokkaebi,demon_king,outer_god,worldline,fable,stigma,attribute,skill,item,location,scenario,arc,event,concept")
                .OldAnnotation("Npgsql:Enum:event_character_role", "participant,observer,victim,perpetrator,mentor")
                .OldAnnotation("Npgsql:Enum:event_importance", "minor,major,pivotal")
                .OldAnnotation("Npgsql:Enum:fable_grade", "legendary,mythical,great,divine")
                .OldAnnotation("Npgsql:Enum:gender", "male,female,unknown")
                .OldAnnotation("Npgsql:Enum:item_grade", "common,uncommon,rare,epic,legendary,mythic,divine")
                .OldAnnotation("Npgsql:Enum:notification_type", "comment_reply,edit_approved,edit_rejected,mention,system")
                .OldAnnotation("Npgsql:Enum:scenario_difficulty", "f,e,d,c,b,a,s,ss,sss,unknown")
                .OldAnnotation("Npgsql:Enum:scenario_outcome", "succeeded,failed,withdrew,pending")
                .OldAnnotation("Npgsql:Enum:scenario_type", "main,sub,hidden,bounty,disaster")
                .OldAnnotation("Npgsql:Enum:skill_type", "active,passive,general");

            migrationBuilder.AlterColumn<int>(
                name: "event_order",
                table: "events",
                type: "integer",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "event_connections",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    character_id = table.Column<long>(type: "bigint", nullable: true),
                    source_event_id = table.Column<long>(type: "bigint", nullable: false),
                    target_event_id = table.Column<long>(type: "bigint", nullable: false),
                    connection_type = table.Column<int>(type: "integer", nullable: false),
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
        }
    }
}
