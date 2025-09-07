using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace NightreignPatch
{
    public class NRPConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;
        public static NRPConfig Get => ModContent.GetInstance<NRPConfig>();

        [Header("Revive")]

        [DefaultValue(false)]
        public bool AutoRespawn;

        [DefaultValue(20)]
        [Range(5, 80)]
        [Increment(5)]
        [DrawTicks]
        // [DefaultValue(2f)]
        public int AutoRespawnTime;
    }

    public class NRPClientConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        public static NRPClientConfig Get => ModContent.GetInstance<NRPClientConfig>();

        [Header("Visual")]

        // [DefaultValue(true)]
        // [ReloadRequired]
        // public bool DrawPlayerHead;

        [DefaultValue(true)]
        public bool DrawCursorToPlayer;

        [DefaultValue(true)]
        public bool HideCursorWhenClose;
        
        //[Label("Cursor distance")]
        //[Tooltip("The distance the cursor is from the player.")]
        [Range(0,500)]
        [Increment(10)]
        [Slider]
        [DefaultValue(180)]
        public int CursorDistance;
        
        //[Label("Cursor size scale")]
        //[Tooltip("The scaling factor of the cursor.")]
        [Range(.1f, 2f)]
        [Increment(.1f)]
        [DrawTicks]
        [DefaultValue(0.8f)]
        public float CursorSize;
	}
}