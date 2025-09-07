using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace NightreignPatch
{
    public abstract class AccesoryPatch : GlobalItem
    {
        public virtual int AccType => 0;
        public virtual float DamageBonus => 0f;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.accessory && entity.type == AccType;
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (player.TryGetModPlayer<Nightplayer>(out Nightplayer ng))
            {
                ng.reviveDamage += DamageBonus;
            }
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (DamageBonus <= 0f) return;

            // turn into 10.0001% for some fucking reason hell yeah
            tooltips.Add(new TooltipLine(Mod, "NRPatch", $"Increased damage by {(int)(DamageBonus * 100)}% on Revive circle "));
        }
    }

    public abstract class AccesoryPatchMod : GlobalItem
    {
        public virtual string AccName => "";
        public virtual string AccMod => "";
        public virtual float DamageBonus => 0f;

        public override bool IsLoadingEnabled(Mod mod)
        {
            return ModLoader.HasMod(AccMod);
        }
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.accessory && entity.ModItem != null && entity.ModItem.Name == AccName && entity.ModItem.Mod.Name == AccMod;
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (player.TryGetModPlayer<Nightplayer>(out Nightplayer ng))
            {
                ng.reviveDamage += DamageBonus;
            }
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (DamageBonus <= 0f) return;

            tooltips.Add(new TooltipLine(Mod, "NRPatch", $"Increased damage by {DamageBonus * 100}% on Revive circle "));
        }
    }

    public class LifeRegen : AccesoryPatch { public override int AccType => ItemID.BandofRegeneration; public override float DamageBonus => 0.1f; }
    public class PhilosophersStone : AccesoryPatch { public override int AccType => ItemID.PhilosophersStone; public override float DamageBonus => 0.05f; }
    public class CharmMyth : AccesoryPatch { public override int AccType => 860; public override float DamageBonus => 0.15f; }

    // Calamity shit
    public class Affliction : AccesoryPatchMod
    {
        public override string AccName => "Affliction"; public override string AccMod => "CalamityMod";
        public override float DamageBonus => 0.2f;
    }

    public class ChaliceOfTheBloodGod : AccesoryPatchMod
    {
        public override string AccName => "ChaliceOfTheBloodGod"; public override string AccMod => "CalamityMod";
        public override float DamageBonus => 0.35f;
    }

    public class TrinketofChi : AccesoryPatchMod
    {
        public override string AccName => "TrinketofChi"; public override string AccMod => "CalamityMod";
        public override float DamageBonus => 0.1f;
    }

    public class TheCommunity : AccesoryPatchMod
    {
        public override string AccName => "TheCommunity"; public override string AccMod => "CalamityMod";
        public override float DamageBonus => 0.2f;
    }

    public class SilvaWings : AccesoryPatchMod
    {
        public override string AccName => "SilvaWings"; public override string AccMod => "CalamityMod";
        public override float DamageBonus => 0.35f;
    }

    public class WingsofRebirth : AccesoryPatchMod
    {
        public override string AccName => "WingsofRebirth"; public override string AccMod => "CalamityMod";
        public override float DamageBonus => 0.5f;
    }

    public class TracersSeraph : AccesoryPatchMod
    {
        public override string AccName => "TracersSeraph"; public override string AccMod => "CalamityMod";
        public override float DamageBonus => 0.55f;
    }
    
    public class Regenator : AccesoryPatchMod
    {
        public override string AccName => "Regenator"; public override string AccMod => "CalamityMod";
        public override float DamageBonus => 0.6f;
    }
}