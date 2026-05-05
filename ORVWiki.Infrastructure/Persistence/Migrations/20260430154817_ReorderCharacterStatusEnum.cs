using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORVWiki.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReorderCharacterStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                .OldAnnotation("Npgsql:Enum:character_status", "alive,dead,unknown,transcended")
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

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "characters",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "characters",
                type: "integer",
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);
        }
    }
}
