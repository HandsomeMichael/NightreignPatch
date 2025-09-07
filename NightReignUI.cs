using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NightreignRevives.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace NightreignPatch
{
    public class NightreignUI : ModSystem
    {
        private const string CursorTexturePath = "NightreignPatch/Cursor";
         private const string VanillaInterfaceLayer = "Vanilla: Entity Health Bars";
        private const float HeadDistance = 20f;
        public List<NPC> npcToDraw;
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals(VanillaInterfaceLayer)) + 1;
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer("NightreignPatch" + ": UI",
                    delegate
                    {
                        DrawPlayerCursor(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
        
        public override void Load()
        {
            npcToDraw = new List<NPC>();
        }

        public override void Unload()
        {
            npcToDraw = null;
        }

        private static bool IsValidNRP(NPC npc)
        {
            return npc.active && npc.ModNPC is ReviveCircleNPC;
        }

        public void DrawPlayerCursor(SpriteBatch spriteBatch)
        {

            var config = NRPClientConfig.Get;

            // Draw nothing if cursor is disabled
            if (npcToDraw.Count <= 0 || !config.DrawCursorToPlayer)
            {
                return;
            }

            // Get the player position
            var playerPos = Main.LocalPlayer.Center;

            //Get UI scale and prepare scaling factor
            var posScaleFactor = 1f / Main.UIScale;

            // Draw an arrow for each boss
            foreach (var revNPC in npcToDraw.Where(IsValidNRP))
            {
                ReviveCircleNPC modNPC = (ReviveCircleNPC)revNPC.ModNPC;

                if (config.HideCursorWhenClose || modNPC.ForClient == Main.myPlayer)
                {
                    // Hide cursor if boss is on screen
                    var p = revNPC.Center - Main.screenPosition;
                    if (!(p.X < 0 || p.Y < 0 || p.X > Main.screenWidth * Main.UIScale || p.Y > Main.screenHeight * Main.UIScale))
                    {
                        continue;
                    }
                }

                // Get the vector pointing towards the boss
                var npcVector = revNPC.Center - playerPos;

                // reverse arrow if gravitation potion effect is active
                npcVector.Y *= Main.LocalPlayer.gravDir;

                // Defines variables used to for drawing
                var modifier = Utils.Clamp(1.15f - 1 / (2f * Main.screenWidth) * npcVector.Length(), 0.02f, 1f);
                var alpha = modifier * 0.9f;
                var scale = modifier * 1.2f;
                npcVector.Normalize();
                var arrowPos = playerPos + npcVector * config.CursorDistance - Main.screenPosition;
                arrowPos *= posScaleFactor;

                var rotation = (float)Math.Atan2(npcVector.Y, npcVector.X);

                // Draw the arrow
                var tex = GetCursorTexture();
                spriteBatch.Draw(tex, arrowPos, null, Color.White * alpha, rotation, tex.Size() / 2f, 1.2f * config.CursorSize, SpriteEffects.None, 1);

                // Draw the boss head
                //var headTex = GetHeadTexture(revNPC);
                var headPos = playerPos + npcVector * (config.CursorDistance - (HeadDistance * Main.UIScale) * config.CursorSize) - Main.screenPosition;
                headPos *= posScaleFactor;

                NightreignPatch.DrawPlayerHead(Main.player[modNPC.ForClient], headPos, alpha, scale * config.CursorSize);
                
                //spriteBatch.Draw(headTex,headPos,null,Color.White * alpha,0f,headTex.Size() * 0.5f,scale * _config.CursorSize,boss.GetBossHeadSpriteEffects(),0);
            }

            npcToDraw.Clear();
        }
        private static Texture2D GetCursorTexture()
        {
            return ModContent.Request<Texture2D>(CursorTexturePath).Value;
        }
    }
}