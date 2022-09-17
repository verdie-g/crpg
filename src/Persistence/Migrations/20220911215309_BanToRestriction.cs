using System;
using Crpg.Domain.Entities.Restrictions;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crpg.Persistence.Migrations;

public partial class BanToRestriction : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:battle_fighter_application_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:battle_mercenary_application_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:battle_phase", "preparation,hiring,scheduled,live,end")
            .Annotation("Npgsql:Enum:battle_side", "attacker,defender")
            .Annotation("Npgsql:Enum:clan_invitation_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:clan_invitation_type", "request,offer")
            .Annotation("Npgsql:Enum:clan_member_role", "member,officer,leader")
            .Annotation("Npgsql:Enum:culture", "neutral,aserai,battania,empire,khuzait,looters,sturgia,vlandia")
            .Annotation("Npgsql:Enum:damage_type", "undefined,cut,pierce,blunt")
            .Annotation("Npgsql:Enum:item_slot", "head,shoulder,body,hand,leg,mount_harness,mount,weapon0,weapon1,weapon2,weapon3")
            .Annotation("Npgsql:Enum:item_type", "undefined,head_armor,shoulder_armor,body_armor,hand_armor,leg_armor,mount_harness,mount,shield,bow,crossbow,one_handed_weapon,two_handed_weapon,polearm,thrown,arrows,bolts,pistol,musket,bullets,banner")
            .Annotation("Npgsql:Enum:party_status", "idle,idle_in_settlement,recruiting_in_settlement,moving_to_point,following_party,moving_to_settlement,moving_to_attack_party,moving_to_attack_settlement,in_battle")
            .Annotation("Npgsql:Enum:platform", "steam,epic,gog")
            .Annotation("Npgsql:Enum:region", "europe,north_america,asia")
            .Annotation("Npgsql:Enum:restriction_type", "join,chat")
            .Annotation("Npgsql:Enum:role", "user,moderator,admin")
            .Annotation("Npgsql:Enum:settlement_type", "village,castle,town")
            .Annotation("Npgsql:Enum:weapon_class", "undefined,dagger,one_handed_sword,two_handed_sword,one_handed_axe,two_handed_axe,mace,pick,two_handed_mace,one_handed_polearm,two_handed_polearm,low_grip_polearm,arrow,bolt,cartridge,bow,crossbow,stone,boulder,throwing_axe,throwing_knife,javelin,pistol,musket,small_shield,large_shield,banner")
            .Annotation("Npgsql:PostgresExtension:postgis", ",,")
            .OldAnnotation("Npgsql:Enum:battle_fighter_application_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:battle_mercenary_application_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:battle_phase", "preparation,hiring,scheduled,live,end")
            .OldAnnotation("Npgsql:Enum:battle_side", "attacker,defender")
            .OldAnnotation("Npgsql:Enum:clan_invitation_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:clan_invitation_type", "request,offer")
            .OldAnnotation("Npgsql:Enum:clan_member_role", "member,officer,leader")
            .OldAnnotation("Npgsql:Enum:culture", "neutral,aserai,battania,empire,khuzait,looters,sturgia,vlandia")
            .OldAnnotation("Npgsql:Enum:damage_type", "undefined,cut,pierce,blunt")
            .OldAnnotation("Npgsql:Enum:item_slot", "head,shoulder,body,hand,leg,mount_harness,mount,weapon0,weapon1,weapon2,weapon3")
            .OldAnnotation("Npgsql:Enum:item_type", "undefined,head_armor,shoulder_armor,body_armor,hand_armor,leg_armor,mount_harness,mount,shield,bow,crossbow,one_handed_weapon,two_handed_weapon,polearm,thrown,arrows,bolts,pistol,musket,bullets,banner")
            .OldAnnotation("Npgsql:Enum:party_status", "idle,idle_in_settlement,recruiting_in_settlement,moving_to_point,following_party,moving_to_settlement,moving_to_attack_party,moving_to_attack_settlement,in_battle")
            .OldAnnotation("Npgsql:Enum:platform", "steam,epic,gog")
            .OldAnnotation("Npgsql:Enum:region", "europe,north_america,asia")
            .OldAnnotation("Npgsql:Enum:role", "user,moderator,admin")
            .OldAnnotation("Npgsql:Enum:settlement_type", "village,castle,town")
            .OldAnnotation("Npgsql:Enum:weapon_class", "undefined,dagger,one_handed_sword,two_handed_sword,one_handed_axe,two_handed_axe,mace,pick,two_handed_mace,one_handed_polearm,two_handed_polearm,low_grip_polearm,arrow,bolt,cartridge,bow,crossbow,stone,boulder,throwing_axe,throwing_knife,javelin,pistol,musket,small_shield,large_shield,banner")
            .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

        migrationBuilder.CreateTable(
            name: "restrictions",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                restricted_user_id = table.Column<int>(type: "integer", nullable: false),
                duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                type = table.Column<RestrictionType>(type: "restriction_type", nullable: false),
                reason = table.Column<string>(type: "text", nullable: false),
                restricted_by_user_id = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_restrictions", x => x.id);
                table.ForeignKey(
                    name: "fk_restrictions_users_restricted_by_user_id",
                    column: x => x.restricted_by_user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_restrictions_users_restricted_user_id",
                    column: x => x.restricted_user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql(@"
INSERT INTO restrictions
SELECT id, banned_user_id, duration, 'join', reason, banned_by_user_id, updated_at, created_at
FROM bans;
");

        migrationBuilder.DropTable(
            name: "bans");

        migrationBuilder.CreateIndex(
            name: "ix_restrictions_restricted_by_user_id",
            table: "restrictions",
            column: "restricted_by_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_restrictions_restricted_user_id",
            table: "restrictions",
            column: "restricted_user_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:battle_fighter_application_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:battle_mercenary_application_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:battle_phase", "preparation,hiring,scheduled,live,end")
            .Annotation("Npgsql:Enum:battle_side", "attacker,defender")
            .Annotation("Npgsql:Enum:clan_invitation_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:clan_invitation_type", "request,offer")
            .Annotation("Npgsql:Enum:clan_member_role", "member,officer,leader")
            .Annotation("Npgsql:Enum:culture", "neutral,aserai,battania,empire,khuzait,looters,sturgia,vlandia")
            .Annotation("Npgsql:Enum:damage_type", "undefined,cut,pierce,blunt")
            .Annotation("Npgsql:Enum:item_slot", "head,shoulder,body,hand,leg,mount_harness,mount,weapon0,weapon1,weapon2,weapon3")
            .Annotation("Npgsql:Enum:item_type", "undefined,head_armor,shoulder_armor,body_armor,hand_armor,leg_armor,mount_harness,mount,shield,bow,crossbow,one_handed_weapon,two_handed_weapon,polearm,thrown,arrows,bolts,pistol,musket,bullets,banner")
            .Annotation("Npgsql:Enum:party_status", "idle,idle_in_settlement,recruiting_in_settlement,moving_to_point,following_party,moving_to_settlement,moving_to_attack_party,moving_to_attack_settlement,in_battle")
            .Annotation("Npgsql:Enum:platform", "steam,epic,gog")
            .Annotation("Npgsql:Enum:region", "europe,north_america,asia")
            .Annotation("Npgsql:Enum:role", "user,moderator,admin")
            .Annotation("Npgsql:Enum:settlement_type", "village,castle,town")
            .Annotation("Npgsql:Enum:weapon_class", "undefined,dagger,one_handed_sword,two_handed_sword,one_handed_axe,two_handed_axe,mace,pick,two_handed_mace,one_handed_polearm,two_handed_polearm,low_grip_polearm,arrow,bolt,cartridge,bow,crossbow,stone,boulder,throwing_axe,throwing_knife,javelin,pistol,musket,small_shield,large_shield,banner")
            .Annotation("Npgsql:PostgresExtension:postgis", ",,")
            .OldAnnotation("Npgsql:Enum:battle_fighter_application_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:battle_mercenary_application_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:battle_phase", "preparation,hiring,scheduled,live,end")
            .OldAnnotation("Npgsql:Enum:battle_side", "attacker,defender")
            .OldAnnotation("Npgsql:Enum:clan_invitation_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:clan_invitation_type", "request,offer")
            .OldAnnotation("Npgsql:Enum:clan_member_role", "member,officer,leader")
            .OldAnnotation("Npgsql:Enum:culture", "neutral,aserai,battania,empire,khuzait,looters,sturgia,vlandia")
            .OldAnnotation("Npgsql:Enum:damage_type", "undefined,cut,pierce,blunt")
            .OldAnnotation("Npgsql:Enum:item_slot", "head,shoulder,body,hand,leg,mount_harness,mount,weapon0,weapon1,weapon2,weapon3")
            .OldAnnotation("Npgsql:Enum:item_type", "undefined,head_armor,shoulder_armor,body_armor,hand_armor,leg_armor,mount_harness,mount,shield,bow,crossbow,one_handed_weapon,two_handed_weapon,polearm,thrown,arrows,bolts,pistol,musket,bullets,banner")
            .OldAnnotation("Npgsql:Enum:party_status", "idle,idle_in_settlement,recruiting_in_settlement,moving_to_point,following_party,moving_to_settlement,moving_to_attack_party,moving_to_attack_settlement,in_battle")
            .OldAnnotation("Npgsql:Enum:platform", "steam,epic,gog")
            .OldAnnotation("Npgsql:Enum:region", "europe,north_america,asia")
            .OldAnnotation("Npgsql:Enum:restriction_type", "join,chat")
            .OldAnnotation("Npgsql:Enum:role", "user,moderator,admin")
            .OldAnnotation("Npgsql:Enum:settlement_type", "village,castle,town")
            .OldAnnotation("Npgsql:Enum:weapon_class", "undefined,dagger,one_handed_sword,two_handed_sword,one_handed_axe,two_handed_axe,mace,pick,two_handed_mace,one_handed_polearm,two_handed_polearm,low_grip_polearm,arrow,bolt,cartridge,bow,crossbow,stone,boulder,throwing_axe,throwing_knife,javelin,pistol,musket,small_shield,large_shield,banner")
            .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

        migrationBuilder.CreateTable(
            name: "bans",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                banned_by_user_id = table.Column<int>(type: "integer", nullable: false),
                banned_user_id = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                reason = table.Column<string>(type: "text", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_bans", x => x.id);
                table.ForeignKey(
                    name: "fk_bans_users_banned_by_user_id",
                    column: x => x.banned_by_user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_bans_users_banned_user_id",
                    column: x => x.banned_user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql(@"
INSERT INTO bans
SELECT id, banned_user_id, duration, reason, banned_by_user_id, updated_at, created_at
FROM restrictions;
");

        migrationBuilder.DropTable(
            name: "restrictions");

        migrationBuilder.CreateIndex(
            name: "ix_bans_banned_by_user_id",
            table: "bans",
            column: "banned_by_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_bans_banned_user_id",
            table: "bans",
            column: "banned_user_id");
    }
}
