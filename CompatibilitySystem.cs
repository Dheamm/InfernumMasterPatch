using System.Reflection;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace InfernumMasterPatch
{
    public class CompatibilitySystem : ModSystem
    {
        public static bool IsActive { get; private set; }

        private static PropertyInfo _infernumDisableModes;
        private static FieldInfo _infernumActiveField;
        private static FieldInfo _calamityDeathField;
        private static bool _initialized;

        public static void TogglePatch(bool enable)
        {
            IsActive = enable;
            
            if (enable)
            {
                Announce("Mods.InfernumMasterPatch.Messages.Enabled");
                EnforceState(); 
            }
            else
            {
                Announce("Mods.InfernumMasterPatch.Messages.Disabled");
            }
        }

        public override void PostUpdateWorld()
        {
            if (Main.gameMenu) { IsActive = false; return; }
            if (!_initialized) Initialize();

            if (IsActive) EnforceState();
        }

        private static void EnforceState()
        {
            if (!_initialized) return;

            if (Main.GameMode != GameModeID.Master)
                Main.GameMode = GameModeID.Master;

            _calamityDeathField?.SetValue(null, true);

            _infernumDisableModes?.SetValue(null, false); 
            _infernumActiveField?.SetValue(null, true);     
        }

        private static void Announce(string key)
        {
            string text = Language.GetTextValue(key);
            if (text == key) text = key.Contains("Enabled") ? "Infernum Master Patch: ENABLED" : "Infernum Master Patch: DISABLED";

            Color color = Color.White;

            if (Main.netMode == NetmodeID.Server)
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), color);
            else if (Main.netMode == NetmodeID.SinglePlayer)
                Main.NewText(text, color);
        }

        private static void Initialize()
        {
            if (ModLoader.TryGetMod("InfernumMode", out Mod inf))
            {
                _infernumDisableModes = inf.Code.GetType("InfernumMode.Core.GlobalInstances.Systems.DifficultyManagementSystem")?.GetProperty("DisableDifficultyModes");
                _infernumActiveField = inf.Code.GetType("InfernumMode.Core.GlobalInstances.Systems.WorldSaveSystem")?.GetField("infernumModeEnabled", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            }
            
            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
            {
                _calamityDeathField = cal.Code.GetType("CalamityMod.World.CalamityWorld")?.GetField("death");
            }
            
            _initialized = true;
        }
    }
}