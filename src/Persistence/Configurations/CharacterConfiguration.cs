using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations
{
    public class CharacterConfiguration : IEntityTypeConfiguration<Character>
    {
        public void Configure(EntityTypeBuilder<Character> builder)
        {
            builder.HasIndex(c => new { c.UserId, c.Name }).IsUnique();
            builder.OwnsOne(c => c.Statistics, ConfigureCharacterStatistics);
            builder.OwnsOne(c => c.Items, ConfigureCharacterItems);
        }

        private static void ConfigureCharacterStatistics(OwnedNavigationBuilder<Character, CharacterStatistics> builder)
        {
            // https://github.com/efcore/EFCore.NamingConventions/issues/26
            builder.OwnsOne(cs => cs.Attributes, ConfigureCharacterAttributes);
            builder.OwnsOne(cs => cs.Skills, ConfigureCharacterSkills);
            builder.OwnsOne(cs => cs.WeaponProficiencies, ConfigureCharacterWeaponProficiencies);
        }

        private static void ConfigureCharacterAttributes(OwnedNavigationBuilder<CharacterStatistics, CharacterAttributes> builder)
        {
            builder.Property(a => a.Points).HasColumnName("attribute_points"); // default is CharacterAttributes_points
        }

        private static void ConfigureCharacterSkills(OwnedNavigationBuilder<CharacterStatistics, CharacterSkills> builder)
        {
            builder.Property(s => s.Points).HasColumnName("skill_points"); // default is CharacterSkills_points
        }

        private static void ConfigureCharacterWeaponProficiencies(OwnedNavigationBuilder<CharacterStatistics, CharacterWeaponProficiencies> builder)
        {
            builder.Property(wp => wp.Points).HasColumnName("weapon_proficiency_points"); // default is WeaponProficiencies_points
        }

        private static void ConfigureCharacterItems(OwnedNavigationBuilder<Character, CharacterItems> builder)
        {
            // defaults are weaponXitem_id (https://github.com/efcore/EFCore.NamingConventions/issues/27)
            builder.Property(a => a.Weapon1ItemId).HasColumnName("weapon1_item_id");
            builder.Property(a => a.Weapon2ItemId).HasColumnName("weapon2_item_id");
            builder.Property(a => a.Weapon3ItemId).HasColumnName("weapon3_item_id");
            builder.Property(a => a.Weapon4ItemId).HasColumnName("weapon4_item_id");
        }
    }
}