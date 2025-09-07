using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NightreignRevives.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace NightreignPatch
{
    public class NightNPCPatch : GlobalNPC
    {
        public short timeAutoRespawn;
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.type == ModContent.NPCType<NightreignRevives.Content.ReviveCircleNPC>();
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(timeAutoRespawn);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            timeAutoRespawn = binaryReader.ReadInt16();
        }

        public short GetRespawnTime()
        {
            if (!Main.CurrentFrameFlags.AnyActiveBossNPC)
            {
                // halfen if no boss active
                return (short)(NRPConfig.Get.AutoRespawnTime * 30);    
            }
            return (short)(NRPConfig.Get.AutoRespawnTime * 60);
        }

        public override void AI(NPC npc)
        {
            if (!NRPConfig.Get.AutoRespawn) return;
            if (timeAutoRespawn == -1) return;

            timeAutoRespawn++;

            if (timeAutoRespawn > GetRespawnTime())
            {
                npc.netUpdate = true;
                npc.ModNPC.CheckDead();
                timeAutoRespawn = -1;
                return;
            }

            // spawn dramatic countdown
            if (timeAutoRespawn % 60 == 0)
            {
                CombatText.NewText(npc.Hitbox, Color.Red, timeAutoRespawn / 60, true);
            }
        }

        public override void PostAI(NPC npc)
        {
            if (!Main.dedServ && NRPClientConfig.Get.DrawCursorToPlayer)
            {
                ModContent.GetInstance<NightreignUI>().npcToDraw.Add(npc);
            }
        }
    }
}