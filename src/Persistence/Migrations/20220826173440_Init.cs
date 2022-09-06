using System;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crpg.Persistence.Migrations;

public partial class Init : Migration
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
            .Annotation("Npgsql:Enum:role", "user,moderator,admin")
            .Annotation("Npgsql:Enum:settlement_type", "village,castle,town")
            .Annotation("Npgsql:Enum:weapon_class", "undefined,dagger,one_handed_sword,two_handed_sword,one_handed_axe,two_handed_axe,mace,pick,two_handed_mace,one_handed_polearm,two_handed_polearm,low_grip_polearm,arrow,bolt,cartridge,bow,crossbow,stone,boulder,throwing_axe,throwing_knife,javelin,pistol,musket,small_shield,large_shield,banner")
            .Annotation("Npgsql:PostgresExtension:postgis", ",,");

        migrationBuilder.CreateTable(
            name: "battles",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                phase = table.Column<BattlePhase>(type: "battle_phase", nullable: false),
                region = table.Column<Region>(type: "region", nullable: false),
                position = table.Column<Point>(type: "geometry", nullable: false),
                scheduled_for = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_battles", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "clans",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                tag = table.Column<string>(type: "text", nullable: false),
                color = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_clans", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "items",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                culture = table.Column<Culture>(type: "culture", nullable: false),
                type = table.Column<ItemType>(type: "item_type", nullable: false),
                price = table.Column<int>(type: "integer", nullable: false),
                weight = table.Column<float>(type: "real", nullable: false),
                armor_head = table.Column<int>(type: "integer", nullable: true),
                armor_body = table.Column<int>(type: "integer", nullable: true),
                armor_arm = table.Column<int>(type: "integer", nullable: true),
                armor_leg = table.Column<int>(type: "integer", nullable: true),
                armor_material_type = table.Column<int>(type: "integer", nullable: true),
                mount_body_length = table.Column<int>(type: "integer", nullable: true),
                mount_charge_damage = table.Column<int>(type: "integer", nullable: true),
                mount_maneuver = table.Column<int>(type: "integer", nullable: true),
                mount_speed = table.Column<int>(type: "integer", nullable: true),
                mount_hit_points = table.Column<int>(type: "integer", nullable: true),
                primary_class = table.Column<WeaponClass>(type: "weapon_class", nullable: true),
                primary_accuracy = table.Column<int>(type: "integer", nullable: true),
                primary_missile_speed = table.Column<int>(type: "integer", nullable: true),
                primary_stack_amount = table.Column<int>(type: "integer", nullable: true),
                primary_length = table.Column<int>(type: "integer", nullable: true),
                primary_balance = table.Column<float>(type: "real", nullable: true),
                primary_handling = table.Column<int>(type: "integer", nullable: true),
                primary_body_armor = table.Column<int>(type: "integer", nullable: true),
                primary_flags = table.Column<long>(type: "bigint", nullable: true),
                primary_thrust_damage = table.Column<int>(type: "integer", nullable: true),
                primary_thrust_damage_type = table.Column<DamageType>(type: "damage_type", nullable: true),
                primary_thrust_speed = table.Column<int>(type: "integer", nullable: true),
                primary_swing_damage = table.Column<int>(type: "integer", nullable: true),
                primary_swing_damage_type = table.Column<DamageType>(type: "damage_type", nullable: true),
                primary_swing_speed = table.Column<int>(type: "integer", nullable: true),
                secondary_class = table.Column<WeaponClass>(type: "weapon_class", nullable: true),
                secondary_accuracy = table.Column<int>(type: "integer", nullable: true),
                secondary_missile_speed = table.Column<int>(type: "integer", nullable: true),
                secondary_stack_amount = table.Column<int>(type: "integer", nullable: true),
                secondary_length = table.Column<int>(type: "integer", nullable: true),
                secondary_balance = table.Column<float>(type: "real", nullable: true),
                secondary_handling = table.Column<int>(type: "integer", nullable: true),
                secondary_body_armor = table.Column<int>(type: "integer", nullable: true),
                secondary_flags = table.Column<long>(type: "bigint", nullable: true),
                secondary_thrust_damage = table.Column<int>(type: "integer", nullable: true),
                secondary_thrust_damage_type = table.Column<DamageType>(type: "damage_type", nullable: true),
                secondary_thrust_speed = table.Column<int>(type: "integer", nullable: true),
                secondary_swing_damage = table.Column<int>(type: "integer", nullable: true),
                secondary_swing_damage_type = table.Column<DamageType>(type: "damage_type", nullable: true),
                secondary_swing_speed = table.Column<int>(type: "integer", nullable: true),
                tertiary_class = table.Column<WeaponClass>(type: "weapon_class", nullable: true),
                tertiary_accuracy = table.Column<int>(type: "integer", nullable: true),
                tertiary_missile_speed = table.Column<int>(type: "integer", nullable: true),
                tertiary_stack_amount = table.Column<int>(type: "integer", nullable: true),
                tertiary_length = table.Column<int>(type: "integer", nullable: true),
                tertiary_balance = table.Column<float>(type: "real", nullable: true),
                tertiary_handling = table.Column<int>(type: "integer", nullable: true),
                tertiary_body_armor = table.Column<int>(type: "integer", nullable: true),
                tertiary_flags = table.Column<long>(type: "bigint", nullable: true),
                tertiary_thrust_damage = table.Column<int>(type: "integer", nullable: true),
                tertiary_thrust_damage_type = table.Column<DamageType>(type: "damage_type", nullable: true),
                tertiary_thrust_speed = table.Column<int>(type: "integer", nullable: true),
                tertiary_swing_damage = table.Column<int>(type: "integer", nullable: true),
                tertiary_swing_damage_type = table.Column<DamageType>(type: "damage_type", nullable: true),
                tertiary_swing_speed = table.Column<int>(type: "integer", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_items", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                platform = table.Column<Platform>(type: "platform", nullable: false),
                platform_user_id = table.Column<string>(type: "text", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                gold = table.Column<int>(type: "integer", nullable: false),
                heirloom_points = table.Column<int>(type: "integer", nullable: false),
                role = table.Column<Role>(type: "role", nullable: false),
                avatar_small = table.Column<string>(type: "text", nullable: true),
                avatar_medium = table.Column<string>(type: "text", nullable: true),
                avatar_full = table.Column<string>(type: "text", nullable: true),
                deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "bans",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                banned_user_id = table.Column<int>(type: "integer", nullable: false),
                duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                reason = table.Column<string>(type: "text", nullable: false),
                banned_by_user_id = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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

        migrationBuilder.CreateTable(
            name: "characters",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                user_id = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                generation = table.Column<int>(type: "integer", nullable: false),
                level = table.Column<int>(type: "integer", nullable: false),
                experience = table.Column<int>(type: "integer", nullable: false),
                experience_multiplier = table.Column<float>(type: "real", nullable: false),
                skipped_the_fun = table.Column<bool>(type: "boolean", nullable: false),
                auto_repair = table.Column<bool>(type: "boolean", nullable: false),
                attribute_points = table.Column<int>(type: "integer", nullable: false),
                strength = table.Column<int>(type: "integer", nullable: false),
                agility = table.Column<int>(type: "integer", nullable: false),
                skill_points = table.Column<int>(type: "integer", nullable: false),
                iron_flesh = table.Column<int>(type: "integer", nullable: false),
                power_strike = table.Column<int>(type: "integer", nullable: false),
                power_draw = table.Column<int>(type: "integer", nullable: false),
                power_throw = table.Column<int>(type: "integer", nullable: false),
                athletics = table.Column<int>(type: "integer", nullable: false),
                riding = table.Column<int>(type: "integer", nullable: false),
                weapon_master = table.Column<int>(type: "integer", nullable: false),
                mounted_archery = table.Column<int>(type: "integer", nullable: false),
                shield = table.Column<int>(type: "integer", nullable: false),
                weapon_proficiency_points = table.Column<int>(type: "integer", nullable: false),
                one_handed = table.Column<int>(type: "integer", nullable: false),
                two_handed = table.Column<int>(type: "integer", nullable: false),
                polearm = table.Column<int>(type: "integer", nullable: false),
                bow = table.Column<int>(type: "integer", nullable: false),
                throwing = table.Column<int>(type: "integer", nullable: false),
                crossbow = table.Column<int>(type: "integer", nullable: false),
                kills = table.Column<int>(type: "integer", nullable: false),
                deaths = table.Column<int>(type: "integer", nullable: false),
                assists = table.Column<int>(type: "integer", nullable: false),
                statistics_play_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_characters", x => x.id);
                table.ForeignKey(
                    name: "fk_characters_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "clan_invitations",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                clan_id = table.Column<int>(type: "integer", nullable: false),
                invitee_id = table.Column<int>(type: "integer", nullable: false),
                inviter_id = table.Column<int>(type: "integer", nullable: false),
                type = table.Column<ClanInvitationType>(type: "clan_invitation_type", nullable: false),
                status = table.Column<ClanInvitationStatus>(type: "clan_invitation_status", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_clan_invitations", x => x.id);
                table.ForeignKey(
                    name: "fk_clan_invitations_clans_clan_id",
                    column: x => x.clan_id,
                    principalTable: "clans",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_clan_invitations_users_invitee_id",
                    column: x => x.invitee_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_clan_invitations_users_inviter_id",
                    column: x => x.inviter_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "clan_members",
            columns: table => new
            {
                user_id = table.Column<int>(type: "integer", nullable: false),
                clan_id = table.Column<int>(type: "integer", nullable: false),
                role = table.Column<ClanMemberRole>(type: "clan_member_role", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_clan_members", x => x.user_id);
                table.ForeignKey(
                    name: "fk_clan_members_clans_clan_id",
                    column: x => x.clan_id,
                    principalTable: "clans",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_clan_members_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_items",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                user_id = table.Column<int>(type: "integer", nullable: false),
                base_item_id = table.Column<string>(type: "text", nullable: false),
                rank = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_items", x => x.id);
                table.ForeignKey(
                    name: "fk_user_items_items_base_item_id",
                    column: x => x.base_item_id,
                    principalTable: "items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_items_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "battle_mercenary_applications",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                battle_id = table.Column<int>(type: "integer", nullable: false),
                character_id = table.Column<int>(type: "integer", nullable: false),
                side = table.Column<BattleSide>(type: "battle_side", nullable: false),
                wage = table.Column<int>(type: "integer", nullable: false),
                note = table.Column<string>(type: "text", nullable: false),
                status = table.Column<BattleMercenaryApplicationStatus>(type: "battle_mercenary_application_status", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_battle_mercenary_applications", x => x.id);
                table.ForeignKey(
                    name: "fk_battle_mercenary_applications_battles_battle_id",
                    column: x => x.battle_id,
                    principalTable: "battles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_battle_mercenary_applications_characters_character_id",
                    column: x => x.character_id,
                    principalTable: "characters",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "equipped_items",
            columns: table => new
            {
                character_id = table.Column<int>(type: "integer", nullable: false),
                slot = table.Column<ItemSlot>(type: "item_slot", nullable: false),
                user_item_id = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_equipped_items", x => new { x.character_id, x.slot });
                table.ForeignKey(
                    name: "fk_equipped_items_characters_character_id",
                    column: x => x.character_id,
                    principalTable: "characters",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_equipped_items_user_items_user_item_id",
                    column: x => x.user_item_id,
                    principalTable: "user_items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "battle_fighter_applications",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                battle_id = table.Column<int>(type: "integer", nullable: false),
                party_id = table.Column<int>(type: "integer", nullable: false),
                side = table.Column<BattleSide>(type: "battle_side", nullable: false),
                status = table.Column<BattleFighterApplicationStatus>(type: "battle_fighter_application_status", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_battle_fighter_applications", x => x.id);
                table.ForeignKey(
                    name: "fk_battle_fighter_applications_battles_battle_id",
                    column: x => x.battle_id,
                    principalTable: "battles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "battle_fighters",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                battle_id = table.Column<int>(type: "integer", nullable: false),
                party_id = table.Column<int>(type: "integer", nullable: true),
                settlement_id = table.Column<int>(type: "integer", nullable: true),
                side = table.Column<BattleSide>(type: "battle_side", nullable: false),
                commander = table.Column<bool>(type: "boolean", nullable: false),
                mercenary_slots = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_battle_fighters", x => x.id);
                table.ForeignKey(
                    name: "fk_battle_fighters_battles_battle_id",
                    column: x => x.battle_id,
                    principalTable: "battles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "battle_mercenaries",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                battle_id = table.Column<int>(type: "integer", nullable: false),
                character_id = table.Column<int>(type: "integer", nullable: false),
                side = table.Column<BattleSide>(type: "battle_side", nullable: false),
                captain_fighter_id = table.Column<int>(type: "integer", nullable: false),
                application_id = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_battle_mercenaries", x => x.id);
                table.ForeignKey(
                    name: "fk_battle_mercenaries_battle_fighters_captain_fighter_id",
                    column: x => x.captain_fighter_id,
                    principalTable: "battle_fighters",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_battle_mercenaries_battle_mercenary_applications_applicatio",
                    column: x => x.application_id,
                    principalTable: "battle_mercenary_applications",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_battle_mercenaries_battles_battle_id",
                    column: x => x.battle_id,
                    principalTable: "battles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_battle_mercenaries_characters_character_id",
                    column: x => x.character_id,
                    principalTable: "characters",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "parties",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false),
                region = table.Column<Region>(type: "region", nullable: false),
                gold = table.Column<int>(type: "integer", nullable: false),
                troops = table.Column<float>(type: "real", nullable: false),
                position = table.Column<Point>(type: "geometry", nullable: false),
                status = table.Column<PartyStatus>(type: "party_status", nullable: false),
                waypoints = table.Column<MultiPoint>(type: "geometry", nullable: false),
                targeted_party_id = table.Column<int>(type: "integer", nullable: true),
                targeted_settlement_id = table.Column<int>(type: "integer", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_parties", x => x.id);
                table.ForeignKey(
                    name: "fk_parties_parties_targeted_party_id",
                    column: x => x.targeted_party_id,
                    principalTable: "parties",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_parties_users_user_id",
                    column: x => x.id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "party_items",
            columns: table => new
            {
                party_id = table.Column<int>(type: "integer", nullable: false),
                item_id = table.Column<string>(type: "text", nullable: false),
                count = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_party_items", x => new { x.party_id, x.item_id });
                table.ForeignKey(
                    name: "fk_party_items_items_item_id",
                    column: x => x.item_id,
                    principalTable: "items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_party_items_parties_party_id",
                    column: x => x.party_id,
                    principalTable: "parties",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "settlements",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "text", nullable: false),
                type = table.Column<SettlementType>(type: "settlement_type", nullable: false),
                culture = table.Column<Culture>(type: "culture", nullable: false),
                region = table.Column<Region>(type: "region", nullable: false),
                position = table.Column<Point>(type: "geometry", nullable: false),
                scene = table.Column<string>(type: "text", nullable: false),
                troops = table.Column<int>(type: "integer", nullable: false),
                owner_id = table.Column<int>(type: "integer", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_settlements", x => x.id);
                table.ForeignKey(
                    name: "fk_settlements_parties_owner_id",
                    column: x => x.owner_id,
                    principalTable: "parties",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "settlement_items",
            columns: table => new
            {
                settlement_id = table.Column<int>(type: "integer", nullable: false),
                item_id = table.Column<string>(type: "text", nullable: false),
                count = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_settlement_items", x => new { x.settlement_id, x.item_id });
                table.ForeignKey(
                    name: "fk_settlement_items_items_item_id",
                    column: x => x.item_id,
                    principalTable: "items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_settlement_items_settlements_settlement_id",
                    column: x => x.settlement_id,
                    principalTable: "settlements",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_bans_banned_by_user_id",
            table: "bans",
            column: "banned_by_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_bans_banned_user_id",
            table: "bans",
            column: "banned_user_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_fighter_applications_battle_id",
            table: "battle_fighter_applications",
            column: "battle_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_fighter_applications_party_id",
            table: "battle_fighter_applications",
            column: "party_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_fighters_battle_id",
            table: "battle_fighters",
            column: "battle_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_fighters_party_id",
            table: "battle_fighters",
            column: "party_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_fighters_settlement_id",
            table: "battle_fighters",
            column: "settlement_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_mercenaries_application_id",
            table: "battle_mercenaries",
            column: "application_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_mercenaries_battle_id",
            table: "battle_mercenaries",
            column: "battle_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_mercenaries_captain_fighter_id",
            table: "battle_mercenaries",
            column: "captain_fighter_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_mercenaries_character_id",
            table: "battle_mercenaries",
            column: "character_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_mercenary_applications_battle_id",
            table: "battle_mercenary_applications",
            column: "battle_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_mercenary_applications_character_id",
            table: "battle_mercenary_applications",
            column: "character_id");

        migrationBuilder.CreateIndex(
            name: "ix_characters_user_id_name",
            table: "characters",
            columns: new[] { "user_id", "name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_clan_invitations_clan_id",
            table: "clan_invitations",
            column: "clan_id");

        migrationBuilder.CreateIndex(
            name: "ix_clan_invitations_invitee_id",
            table: "clan_invitations",
            column: "invitee_id");

        migrationBuilder.CreateIndex(
            name: "ix_clan_invitations_inviter_id",
            table: "clan_invitations",
            column: "inviter_id");

        migrationBuilder.CreateIndex(
            name: "ix_clan_members_clan_id",
            table: "clan_members",
            column: "clan_id");

        migrationBuilder.CreateIndex(
            name: "ix_clans_name",
            table: "clans",
            column: "name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_clans_tag",
            table: "clans",
            column: "tag",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_equipped_items_user_item_id",
            table: "equipped_items",
            column: "user_item_id");

        migrationBuilder.CreateIndex(
            name: "ix_parties_targeted_party_id",
            table: "parties",
            column: "targeted_party_id");

        migrationBuilder.CreateIndex(
            name: "ix_parties_targeted_settlement_id",
            table: "parties",
            column: "targeted_settlement_id");

        migrationBuilder.CreateIndex(
            name: "ix_party_items_item_id",
            table: "party_items",
            column: "item_id");

        migrationBuilder.CreateIndex(
            name: "ix_settlement_items_item_id",
            table: "settlement_items",
            column: "item_id");

        migrationBuilder.CreateIndex(
            name: "ix_settlements_owner_id",
            table: "settlements",
            column: "owner_id");

        migrationBuilder.CreateIndex(
            name: "ix_settlements_region_name",
            table: "settlements",
            columns: new[] { "region", "name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_items_base_item_id",
            table: "user_items",
            column: "base_item_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_items_user_id_base_item_id_rank",
            table: "user_items",
            columns: new[] { "user_id", "base_item_id", "rank" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_platform_platform_user_id",
            table: "users",
            columns: new[] { "platform", "platform_user_id" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "fk_battle_fighter_applications_parties_party_id",
            table: "battle_fighter_applications",
            column: "party_id",
            principalTable: "parties",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_battle_fighters_parties_party_id",
            table: "battle_fighters",
            column: "party_id",
            principalTable: "parties",
            principalColumn: "id");

        migrationBuilder.AddForeignKey(
            name: "fk_battle_fighters_settlements_settlement_id",
            table: "battle_fighters",
            column: "settlement_id",
            principalTable: "settlements",
            principalColumn: "id");

        migrationBuilder.AddForeignKey(
            name: "fk_parties_settlements_targeted_settlement_id",
            table: "parties",
            column: "targeted_settlement_id",
            principalTable: "settlements",
            principalColumn: "id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_parties_users_user_id",
            table: "parties");

        migrationBuilder.DropForeignKey(
            name: "fk_settlements_parties_owner_id",
            table: "settlements");

        migrationBuilder.DropTable(
            name: "bans");

        migrationBuilder.DropTable(
            name: "battle_fighter_applications");

        migrationBuilder.DropTable(
            name: "battle_mercenaries");

        migrationBuilder.DropTable(
            name: "clan_invitations");

        migrationBuilder.DropTable(
            name: "clan_members");

        migrationBuilder.DropTable(
            name: "equipped_items");

        migrationBuilder.DropTable(
            name: "party_items");

        migrationBuilder.DropTable(
            name: "settlement_items");

        migrationBuilder.DropTable(
            name: "battle_fighters");

        migrationBuilder.DropTable(
            name: "battle_mercenary_applications");

        migrationBuilder.DropTable(
            name: "clans");

        migrationBuilder.DropTable(
            name: "user_items");

        migrationBuilder.DropTable(
            name: "battles");

        migrationBuilder.DropTable(
            name: "characters");

        migrationBuilder.DropTable(
            name: "items");

        migrationBuilder.DropTable(
            name: "users");

        migrationBuilder.DropTable(
            name: "parties");

        migrationBuilder.DropTable(
            name: "settlements");
    }
}
