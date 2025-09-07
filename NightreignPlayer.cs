using NightreignRevives.Content;
using Terraria;
using Terraria.ModLoader;

namespace NightreignPatch
{
    public class Nightplayer : ModPlayer
    {
        public float reviveDamage;

        public override void ResetEffects()
        {
            reviveDamage = 0f;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (target.ModNPC is ReviveCircleNPC)
            {
                modifiers.ScalingBonusDamage += reviveDamage;
            }
        }
    }
}