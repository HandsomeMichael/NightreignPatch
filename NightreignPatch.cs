using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FishUtils.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using NightreignRevives.Content;
using NightreignRevives.Core;
using NightreignRevives.Core.UI;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace NightreignPatch
{
	public class NightreignPatch : Mod
	{

		public const BindingFlags defaultBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
		public delegate void orig_DrawReviveCircle(NPC reviveNPC, ReviveCircleNPC reviveCircleNPC, Player player);
		private static readonly MethodInfo DrawReviveCircleMethod = typeof(NightreignRevives.Core.UI.PlayerReviveCirclesUIState).GetMethod("DrawReviveCircle", defaultBindingFlags);
		Hook DrawReviveCircleHook;

		public override void Load()
		{
			typeof(NightreignRevives.Core.UI.PlayerReviveCirclesUIState).GetMethod("DrawReviveCircle", defaultBindingFlags);
			DrawReviveCircleHook = new(DrawReviveCircleMethod, DrawReviveCirclePatch);
			DrawReviveCircleHook.Apply();
		}

		public override void PostSetupContent()
		{
			if (Main.dedServ) return;

			LoadAssets();
		}

		public override void Unload()
		{
			DrawReviveCircleHook.Undo();
		}

		private static Asset<Texture2D> _outlineTexture;
		private static Asset<Texture2D> _fillTexture;
		private static Asset<Effect> _radialFillEffect;

		public static void LoadAssets()
		{
			// shit loaded twice gng
			// shouldve make a public getter
			_outlineTexture = ModContent.Request<Texture2D>($"{nameof(NightreignRevives)}/Assets/ReviveUIOutline");
			_fillTexture = ModContent.Request<Texture2D>($"{nameof(NightreignRevives)}/Assets/ReviveUIFill");
			_radialFillEffect = ModContent.Request<Effect>($"{nameof(NightreignRevives)}/Assets/Shaders/RadialMask");
		}

		// idk if i should

		// public void TryDisposingAssets()
		// {
		// 	var uiStateType = typeof(NightreignRevives.Core.UI.PlayerReviveCirclesUIState);

		// 	var outlineTextureField = uiStateType.GetField("_outlineTexture", BindingFlags.NonPublic | BindingFlags.Static);
		// 	var fillTextureField = uiStateType.GetField("_fillTexture", BindingFlags.NonPublic | BindingFlags.Static);
		// 	var radialFillEffectField = uiStateType.GetField("_radialFillEffect", BindingFlags.NonPublic | BindingFlags.Static);

		// 	if (outlineTextureField != null) (outlineTextureField.GetValue(null) as Asset<Texture2D>)?.Dispose();
		// 	if (fillTextureField != null) (fillTextureField.GetValue(null) as Asset<Texture2D>)?.Dispose();
		// 	if (radialFillEffectField != null) (radialFillEffectField.GetValue(null) as Asset<Effect>)?.Dispose();
		// }

		public static void DrawReviveCirclePatch(orig_DrawReviveCircle orig, NPC reviveNPC, ReviveCircleNPC reviveCircleNPC, Player player)
		{
			DrawData drawData = new()
			{
				texture = _outlineTexture.Value,
				position = (reviveNPC.Center - Main.screenPosition).Floor(),
				sourceRect = _fillTexture.Frame(),
				origin = _fillTexture.Size() / 2f,
				color = Color.White * reviveCircleNPC.Opacity,
				scale = new Vector2(1f),
			};

			if (player.whoAmI == Main.myPlayer)
			{
				// drawData.position = (Main.ScreenSize.ToVector2() / 2f).Floor();
				drawData.scale = new Vector2(2f);
			}

			SpriteBatchParams circleSBParams = SpriteBatchParams.Default;

			Main.spriteBatch.Begin(circleSBParams);

			//DrawPlayerHead(player, reviveNPC.Center,1f, player.whoAmI == Main.myPlayer ? 2f : 1f);
			drawData.Draw(Main.spriteBatch);

			var globeNPC = reviveNPC.GetGlobalNPC<NightNPCPatch>();
			int time = globeNPC.timeAutoRespawn;
			int maxTime = globeNPC.GetRespawnTime();

			DynamicSpriteFont font = FontAssets.DeathText.Value;

			var size = Vector2.One;
			if (Main.myPlayer != player.whoAmI)
			{
				size /= 2f;
			}

			string text = ((maxTime - time)/60).ToString();
			var parsed = ChatManager.ParseMessage(text, Color.Purple * reviveCircleNPC.Opacity).ToArray();
			var parsedSize = ChatManager.GetStringSize(font, parsed, size);
			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch,font, parsed,drawData.position - new Vector2(5,5),0f,parsedSize / 2f,size,out _);

			Main.spriteBatch.End();

			// this doesnt sync foreach client, too lazy to patch it thought
			float minProgress = player.GetModPlayer<NightreignRevivePlayer>().NumDownsThisFight switch
			{
				1 => 0.66f,
				2 => 0.33f,
				_ => 0,
			};

			float fillProgress = Utils.Remap(reviveNPC.life, 0f, reviveNPC.lifeMax, 1f, minProgress, false);
			_radialFillEffect.Value.Parameters["progress"].SetValue(fillProgress);
			_radialFillEffect.Value.Parameters["textureSize"].SetValue(_fillTexture.Size());

			Main.spriteBatch.Begin(circleSBParams with { Effect = _radialFillEffect.Value });

			DrawData fillDrawData = drawData with
			{
				texture = _fillTexture.Value,
			};

			fillDrawData.Draw(Main.spriteBatch);

			Main.spriteBatch.End();
			//orig(reviveNPC,  reviveCircleNPC, player);
		}

		public static void DrawPlayerHead(Player drawPlayer, Vector2 position, float alpha = 1f, float scale = 1f)
		{

			// since i cant draw player head i'd draw the last held item lol
			if (drawPlayer.HeldItem != null && !drawPlayer.HeldItem.IsAir)
			{
				ItemSlot.DrawItemIcon(drawPlayer.HeldItem, 31, Main.spriteBatch, position, scale, 32f, Color.White * alpha);
			}
			
			// No method work due to reasons i cant even explain

			// Player newDp = (Player)drawPlayer.Clone(); // tried cloning and doesnt work too
			// newDp.Center = position;
			// newDp.dead = false;
			// newDp.invis = false;
			// newDp.active = true;
			// newDp.statLife = 100;


			// Main.PlayerRenderer.DrawPlayer(Main.Camera, newDp, position, 0f, Vector2.Zero);
			// Main.PlayerRenderer.DrawPlayerHead(Main.Camera, newDp, position, 1f);
			// Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, newDp, position);

		}
	}
}
